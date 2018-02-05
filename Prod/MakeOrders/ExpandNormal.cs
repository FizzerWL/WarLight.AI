using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public class ExpandNormal : Expand
    {
        BotMain Bot;
        public Dictionary<TerritoryIDType, PossibleExpandTarget> AttackableNeutrals;

        public ExpandNormal(BotMain bot)
        {
            this.Bot = bot;

            var attackableTerritories = Bot.Standing.Territories.Values.
                Where(o => bot.Map.Territories[o.ID].ConnectedTo.Keys.Any(c => bot.Standing.Territories[c].OwnerPlayerID == bot.PlayerID && !bot.AvoidTerritories.Contains(c)));

            var terrs = attackableTerritories.Where(o => o.IsNeutral || o.OwnerPlayerID == TerritoryStanding.FogPlayerID).Select(o => o.ID).ToHashSet(false);
            AttackableNeutrals = GetExpansionWeights(terrs);
        }

        public override void Go(int useArmies, bool highlyValuedOnly)
        {
            //In FFA, focus on expansion even moreso than in headsup
            float minWeight = !highlyValuedOnly ? float.MinValue : ExpansionHelper.BaseBonusWeight + (Bot.IsFFA ? -10 : 0) + (Bot.UseRandomness ? (float)RandomUtility.BellRandom(-9, 9) : 0);

            AILog.Log("Expand", "Expand called with useArmies=" + useArmies + ", minWeight=" + minWeight);
            Assert.Fatal(useArmies >= 0, "useArmies negative");

            var meetsFilter = AttackableNeutrals.Where(o => o.Value.Weight > minWeight).ToDictionary(o => o.Key, o => o.Value);  //Don't bother with anything less than the min weight

            AILog.Log("Expand", meetsFilter.Count + " items over weight " + minWeight + " (" + AttackableNeutrals.Count + " total), top:");
            foreach (var spot in meetsFilter.OrderByDescending(o => o.Value.Weight).Take(10))
                AILog.Log("Expand", " - " + spot.Value);

            int armiesToExpandWithRemaining = useArmies;
            while (meetsFilter.Count > 0)
            {
                var expandTo = meetsFilter.OrderByDescending(o => o.Value.Weight).First().Key;
                meetsFilter.Remove(expandTo);

                if (Bot.Orders.Orders.OfType<GameOrderAttackTransfer>().Any(o => o.To == expandTo))
                    continue; //we've already attacked it

                int attackWith = Bot.ArmiesToTake(Bot.Standing.Territories[expandTo].NumArmies.Fogged ? ExpansionHelper.GuessNumberOfArmies(Bot, expandTo) : Bot.Standing.Territories[expandTo].NumArmies);

                var attackFromList = Bot.Map.Territories[expandTo].ConnectedTo.Keys
                    .Select(o => Bot.Standing.Territories[o])
                    .Where(o => o.OwnerPlayerID == Bot.PlayerID && !Bot.AvoidTerritories.Contains(o.ID))
                    .ToDictionary(o => o.ID, o => Bot.MakeOrders.GetArmiesAvailable(o.ID))
                    .OrderByDescending(o => o.Value).ToList();

                if (attackFromList.Count == 0)
                    continue; //nowhere to attack from
                var attackFrom = attackFromList[0];

                int armiesNeedToDeploy = Math.Max(0, attackWith - attackFrom.Value);

                if (armiesToExpandWithRemaining >= armiesNeedToDeploy)
                {
                    //Deploy if needed
                    if (armiesNeedToDeploy > 0)
                    {
                        armiesToExpandWithRemaining -= armiesNeedToDeploy;
                        if (!Bot.Orders.TryDeploy(attackFrom.Key, armiesNeedToDeploy))
                            continue;
                        else
                        {
                            //Remember that we deployed armies towards the capture of this bonus
                            foreach (var bonusID in AttackableNeutrals[expandTo].Bonuses.Keys)
                                foreach (var an in AttackableNeutrals.Values)
                                    if (an.Bonuses.ContainsKey(bonusID))
                                        an.Bonuses[bonusID].DeployedTowardsCapturing += armiesNeedToDeploy;
                                
                        }
                    }

                    AILog.Log("Expand", "Expanding into " + Bot.TerrString(expandTo) + " from " + Bot.TerrString(attackFrom.Key) + " with " + attackWith + " by deploying " + armiesNeedToDeploy + ", already had " + attackFrom.Value + " available");

                    //Attack
                    Bot.Orders.AddAttack(attackFrom.Key, expandTo, AttackTransferEnum.AttackTransfer, attackWith, false);
                }
            }
        }

        private Dictionary<TerritoryIDType, PossibleExpandTarget> GetExpansionWeights(HashSet<TerritoryIDType> terrs)
        {
            var bonusPaths = terrs
                .SelectMany(o => Bot.Map.Territories[o].PartOfBonuses)
                .Where(o => Bot.BonusValue(o) > 0)
                .Distinct()
                .Select(o =>
                {
                    if (Bot.PastTime(7))
                        return null; //stop trying to expand if we're slow

                    return BonusPath.TryCreate(Bot, o, ts => ts.OwnerPlayerID == Bot.PlayerID);
                    
                })
                .Where(o => o != null)
                .ToDictionary(o => o.BonusID, o => o);

            var turnsToTake = bonusPaths.Keys.ToDictionary(o => o, o => TurnsToTake(o, bonusPaths[o]));
            foreach(var cannotTake in turnsToTake.Where(o => o.Value == null).ToList())
            {
                turnsToTake.Remove(cannotTake.Key);
                bonusPaths.Remove(cannotTake.Key);
            }

            var bonusWeights = bonusPaths.Keys.ToDictionary(o => o, o => ExpansionHelper.WeighBonus(Bot, o, ts => ts.OwnerPlayerID == Bot.PlayerID, turnsToTake[o].NumTurns));

            AILog.Log("Expand", "GetExpansionWeights called with " + terrs.Count + " territories.  Weighted " + bonusWeights.Count + " bonuses:");
            foreach (var bw in bonusWeights.OrderByDescending(o => o.Value).Take(10))
                AILog.Log("Expand", " - " + Bot.BonusString(bw.Key) + " Weight=" + bw.Value + " " + turnsToTake[bw.Key] + " TurnsToTakeByDistance=" + bonusPaths[bw.Key].TurnsToTakeByDistance + " CriticalPath=" + bonusPaths[bw.Key].TerritoriesOnCriticalPath.Select(o => Bot.TerrString(o)).JoinStrings(", "));

            var ret = new Dictionary<TerritoryIDType, PossibleExpandTarget>();

            foreach (var terr in terrs)
                ret[terr] = new PossibleExpandTarget(Bot, terr, Bot.Map.Territories[terr].PartOfBonuses.Where(b => bonusPaths.ContainsKey(b)).ToDictionary(b => b, b => new PossibleExpandTargetBonus(bonusWeights[b], bonusPaths[b], turnsToTake[b])));

            AILog.Log("Expand", "Finished weighing " + terrs.Count + " territories:");
            foreach (var terr in ret.OrderByDescending(o => o.Value.Weight).Take(10))
                AILog.Log("Expand", " - " + Bot.TerrString(terr.Key) + " Weight=" + terr.Value);

            return ret;
        }

        
        /// <summary>
        /// Estimate how many turns it would take us to complete the bonus, assuming full deployment and usage of armies in or next to it.
        /// </summary>
        /// <param name="bonusID"></param>
        /// <param name="bonusPath"></param>
        /// <returns></returns>
        private CaptureTerritories TurnsToTake(BonusIDType bonusID, BonusPath path)
        {
            var bonus = Bot.Map.Bonuses[bonusID];
            var terrsToTake = bonus.Territories.Where(o => Bot.Standing.Territories[o].OwnerPlayerID != Bot.PlayerID).ToHashSet(true);

            var terrsWeOwnInOrAroundBonus = bonus.Territories.Concat(bonus.Territories.SelectMany(o => Bot.Map.Territories[o].ConnectedTo.Keys)).Where(o => Bot.Standing.Territories[o].OwnerPlayerID == Bot.PlayerID).ToHashSet(false);
            var armiesWeHaveInOrAroundBonus = terrsWeOwnInOrAroundBonus.Sum(o => Bot.MakeOrders.GetArmiesAvailable(o));

            var armiesPerTurn = Bot.BaseIncome.FreeArmies;

            return CaptureTerritories.TryFindTurnsToTake(Bot, path, armiesWeHaveInOrAroundBonus, armiesPerTurn, terrsToTake, o => o.NumArmies.Fogged ? ExpansionHelper.GuessNumberOfArmies(Bot, o.ID).DefensePower : o.NumArmies.DefensePower);
        }

        public override bool HelpExpansion(IEnumerable<TerritoryIDType> terrs, int armies, Action<TerritoryIDType, int> deploy)
        {
            foreach (var helpExpansionTo in terrs.SelectMany(o => Bot.Map.Territories[o].ConnectedTo.Keys).Where(o => this.AttackableNeutrals.ContainsKey(o)).OrderByDescending(o => this.AttackableNeutrals[o].Weight))
            {
                var attackOptions = Bot.Orders.Orders.OfType<GameOrderAttackTransfer>().Where(o => terrs.Contains(o.From) && o.To == helpExpansionTo).ToList();

                if (attackOptions.Count > 0)
                {
                    var attack = Bot.UseRandomness ? attackOptions.Random() : attackOptions[0];

                    deploy(attack.From, armies);
                    attack.NumArmies = attack.NumArmies.Add(new Armies(armies));
                    return true;
                }
            }

            return false;
        }
    }
}
