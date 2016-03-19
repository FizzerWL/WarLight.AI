/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Bot
{
    public class BonusAnalyzer
    {
        private readonly CowzowBot Bot;
        public readonly Dictionary<BonusIDType, int> BonusBuckets;
        public readonly Dictionary<BonusIDType, int> BonusCosts;

        public int TroopEstimate;

        public BonusAnalyzer(CowzowBot bot)
        {
            this.Bot = bot;
            BonusBuckets = new Dictionary<BonusIDType, int>();
            BonusCosts = new Dictionary<BonusIDType, int>();
            TroopEstimate = Bot.Settings.MinimumArmyBonus;
            foreach (var bonus in Bot.BotMap.Bonuses.Values)
            {
                BonusBuckets[bonus.ID] = 0;
                var cost = 0;
                foreach (var r in bonus.Territories)
                {
                    if (Bot.DistributionStanding != null)
                        cost += Bot.DistributionStanding.Territories[r.ID].NumArmies.NumArmies;
                    else if (!Bot.LatestStanding.Territories[r.ID].NumArmies.Fogged)
                        cost += Bot.LatestStanding.Territories[r.ID].NumArmies.NumArmies;
                    else
                        cost += Bot.Settings.InitialNonDistributionArmies;
                }

                BonusCosts[bonus.ID] = cost;
            }

            //foreach (var r_1 in State.OpponentStartingTerritories)
            //    BonusBuckets[r_1.Bonus] += 2;
        }

        public void Process()
        {
            var troopsUsed = 0;
            var placedTerritories = new HashSet<TerritoryIDType>();
            foreach (var move in Bot.OpponentOrders.OfType<GameOrderDeploy>())
            {
                troopsUsed += move.NumArmies;
                placedTerritories.Add(move.DeployOn);
            }

            foreach (var move_1 in this.Bot.OpponentOrders.OfType<GameOrderAttackTransfer>())
                if (placedTerritories.Contains(move_1.From))
                {
                    foreach(var bonusID in Bot.Map.Territories[move_1.To].PartOfBonuses)
                        BonusBuckets[bonusID] = BonusBuckets[bonusID] + move_1.NumArmies.ArmiesOrZero;
                }

            if (TroopEstimate < troopsUsed)
                TroopEstimate = troopsUsed;

            var hiddenTroops = TroopEstimate - troopsUsed;

            var bonusList = new List<BotBonus>();
            foreach (var bonus in Bot.BotMap.Bonuses.Values)
                if (UnvisCount(bonus) >= 1 && 2 * VisCount(bonus) <= TroopEstimate)
                    bonusList.Add(bonus);

            var bc = new BonusComparator(this);
            bonusList.Sort((f,s) => bc.Compare(f, s));
            // System.out.println(bonusList);
            foreach (var bonus_1 in bonusList)
            {
                var guess = BonusBuckets[bonus_1.ID];
                var cost = BonusCosts[bonus_1.ID];
                if (guess < cost)
                {
                    var diff = cost - guess;
                    var spend = Math.Min(diff, hiddenTroops);
                    BonusBuckets[bonus_1.ID] = guess + spend;
                    hiddenTroops -= spend;
                }
            }
            var bonuses = 0;
            foreach (var s in Bot.BotMap.Bonuses.Values)
                if (MightBeOwned(s))
                    bonuses += s.ArmiesReward;
            TroopEstimate = Math.Max(troopsUsed, bonuses + 5);
        }

        // Gets the count of troops that we cannot account for from bonuses that he will take
        public int GetEffectiveTroops()
        {
            var effectiveTroops = TroopEstimate;
            var bonusList = new List<BotBonus>();
            foreach (var bonus in Bot.BotMap.Bonuses.Values)
                if (UnvisCount(bonus) >= 1 && 2 * VisCount(bonus) <= TroopEstimate)
                    bonusList.Add(bonus);
            var bc = new BonusComparator(this);
            bonusList.Sort((a,b) => bc.Compare(a, b));
            foreach (var s in bonusList)
            {
                var score = (double)s.ArmiesReward / BonusCosts[s.ID];
                if (score > 0.3 && s.Territories.Count <= 5 || BonusCosts[s.ID] - BonusBuckets[s.ID] <= TroopEstimate)
                    effectiveTroops -= Math.Min(TroopEstimate, BonusCosts[s.ID] - BonusBuckets[s.ID]);
            }
            return Math.Max(0, effectiveTroops);
        }

        public void PrintReport()
        {
            var bonusIds = new List<BonusIDType>(Bot.BotMap.Bonuses.Keys);
            bonusIds.Sort();
            AILog.Log("BonusAnalyzer", "Expected troops: " + TroopEstimate);
            foreach (var i in bonusIds)
            {
                var s = Bot.BotMap.GetBonus(i);
                var msg = s.ToString();
                if (MightBeOwned(s))
                    msg += "YES:";
                else
                    msg += "NO:";

                msg += " " + BonusBuckets[s.ID] + " / " + BonusCosts[s.ID];
                if (SoonBeOwned(s))
                    msg += " SOON";

                AILog.Log("BonusAnalyzer", msg);
            }
        }

        public bool MightBeOwned(BotBonus bonus)
        {
            var terrs = bonus.Territories.Where(o => o.IsVisible).ToList();

            if (terrs.Count == 0)
                return true;

            var oppID = terrs[0].OwnerPlayerID;
            if (!Bot.IsOpponent(oppID))
                return false;

            foreach (var r in terrs.Skip(1))
                if (r.OwnerPlayerID != oppID)
                    return false;

            if (BonusBuckets[bonus.ID] + 2 < BonusCosts[bonus.ID])
                return false; //???

            return true;
        }

        public bool SoonBeOwned(BotBonus bonus)
        {
            var mine = 0;
            var his = 0;
            var neutral = 0;
            var unvis = 0;
            foreach (var r in bonus.Territories)
            {
                if (r.IsVisible)
                {
                    if (r.OwnerPlayerID == Bot.Me.ID)
                        mine++;
                    else
                    {
                        if (Bot.IsOpponent(r.OwnerPlayerID))
                            his++;
                        else
                            neutral++;
                    }
                }
                else
                    unvis++;
            }


            if (mine > 0)
                return false; //???
            if (neutral > 2 || neutral > bonus.Territories.Count / 2)
                return false;
            if (BonusBuckets[bonus.ID] + 4 < BonusCosts[bonus.ID])
                return false;

            // EXPERIMENTAL:
            var bonusList = new List<BotBonus>();
            foreach (var tmp in Bot.BotMap.Bonuses.Values)
                if (VisCount(tmp) <= 1 && !MightBeOwned(tmp))
                    bonusList.Add(tmp);

            var eval = new BonusComparator(this);
            foreach (var tmp_1 in bonusList)
                if (eval.Compare(bonus, tmp_1) > 0)
                    return false;
            return true;
        }

        private int VisCount(BotBonus s)
        {
            return s.Territories.Count - UnvisCount(s);
        }

        public int UnvisCount(BotBonus s)
        {
            var count = 0;
            foreach (var r in s.Territories)
                if (!r.IsVisible)
                    count++;
            return count;
        }

        internal class BonusComparator : IComparer<BotBonus>
        {
            private readonly BonusAnalyzer _enclosing;

            internal BonusComparator(BonusAnalyzer enclosing)
            {
                this._enclosing = enclosing;
            }

            public int Compare(BotBonus a, BotBonus b)
            {
                var aScore = (double)a.ArmiesReward / _enclosing.BonusCosts[a.ID];
                var bScore = (double)b.ArmiesReward / _enclosing.BonusCosts[b.ID];
                //if (a.IsUnvisible() && !b.IsUnvisible())
                //    aScore *= 1.2;

                if (aScore > bScore)
                    return -1;
                if (aScore < bScore)
                    return 1;
                if (a.Territories.Count < b.Territories.Count)
                    return -1;
                if (a.Territories.Count > b.Territories.Count)
                    return 1;
                if (_enclosing.UnvisCount(a) > _enclosing.UnvisCount(b))
                    return -1;
                if (_enclosing.UnvisCount(a) < _enclosing.UnvisCount(b))
                    return 1;
                return 0;
            }
        }
    }
}
