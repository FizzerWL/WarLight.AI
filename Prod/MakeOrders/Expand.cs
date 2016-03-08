using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Prod.MakeOrders
{
    public class Expand
    {
        BotMain Bot;
        public Dictionary<TerritoryIDType, PossibleExpandTarget> AttackableNeutrals;

        public Expand(BotMain bot)
        {
            this.Bot = bot;

            AILog.Log("Expand", "Finding attackable neutrals");

            var terrs = Bot.AttackableTerritories.Where(o => o.IsNeutral).Select(o => o.ID).ToHashSet(false);
            AttackableNeutrals = GetExpansionWeights(terrs);
        }

        public void Go(int useArmies, float minWeight)
        {
            AILog.Log("Expand", "Expand called with useArmies=" + useArmies + ", minWeight=" + minWeight);

            if (useArmies == 0)
                return;

            var meetsFilter = AttackableNeutrals.Where(o => o.Value.Weight > minWeight).ToDictionary(o => o.Key, o => o.Value);  //Don't bother with anything less than the min weight

            AILog.Log("Expand", Bot.PlayerID + " got " + meetsFilter.Count + " items over weight " + minWeight + " (" + AttackableNeutrals.Count + " total), top:");
            foreach (var spot in meetsFilter.OrderByDescending(o => o.Value.Weight).Take(10))
                AILog.Log("Expand", " - " + spot.Value);

            int armiesToExpandWithRemaining = useArmies;
            while (meetsFilter.Count > 0)
            {
                var expandTo = meetsFilter.OrderByDescending(o => o.Value.Weight).First().Key;
                meetsFilter.Remove(expandTo);

                if (Bot.Orders.Orders.OfType<GameOrderAttackTransfer>().Any(o => o.To == expandTo))
                    continue; //we've already attacked it

                int attackWith = Bot.ArmiesToTake(Bot.Standing.Territories[expandTo].NumArmies);

                var attackFrom = Bot.Map.Territories[expandTo].ConnectedTo
                    .Select(o => Bot.Standing.Territories[o])
                    .Where(o => o.OwnerPlayerID == Bot.PlayerID)
                    .ToDictionary(o => o.ID, o => Bot.MakeOrders.GetArmiesAvailable(o.ID))
                    .OrderByDescending(o => o.Value).First();


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
            var bonusPaths = terrs.SelectMany(o => Bot.Map.Territories[o].PartOfBonuses).Where(o => Bot.BonusValue(o) > 0).Distinct().ToDictionary(o => o, o => new BonusPath(Bot, o, ts => ts.OwnerPlayerID == Bot.PlayerID));
            var turnsToTake = bonusPaths.Keys.ToDictionary(o => o, o => TurnsToTake(o, bonusPaths[o]));
            var bonusWeights = bonusPaths.Keys.ToDictionary(o => o, o => ExpansionHelper.WeighBonus(Bot, o, bonusPaths[o], ts => ts.OwnerPlayerID == Bot.PlayerID, turnsToTake[o]));

            AILog.Log("Expand", "GetExpansionWeights called with " + terrs.Count + " territories.  Weighted " + bonusWeights.Count + " bonuses:");
            foreach (var bw in bonusWeights.OrderByDescending(o => o.Value).Take(10))
                AILog.Log("Expand", " - " + Bot.BonusString(bw.Key) + " Weight=" + bw.Value + " " + turnsToTake[bw.Key] + " (infinite armies=" + bonusPaths[bw.Key].TurnsToTakeByDistance + ") CriticalPath=" + bonusPaths[bw.Key].TerritoriesOnCriticalPath.Select(o => Bot.TerrString(o)).JoinStrings(", "));

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

            var terrsWeOwnInOrAroundBonus = bonus.Territories.Concat(bonus.Territories.SelectMany(o => Bot.Map.Territories[o].ConnectedTo)).Where(o => Bot.Standing.Territories[o].OwnerPlayerID == Bot.PlayerID).ToHashSet(false);
            var armiesWeHaveInOrAroundBonus = terrsWeOwnInOrAroundBonus.Sum(o => Bot.MakeOrders.GetArmiesAvailable(o));

            var armiesPerTurn = Bot.BaseIncome.FreeArmies;

            return CaptureTerritories.TryFindTurnsToTake(Bot, path, armiesWeHaveInOrAroundBonus, armiesPerTurn, terrsToTake, o => o.OwnerPlayerID == TerritoryStanding.FogPlayerID ? ExpansionHelper.GuessNumberOfArmies(Bot, o.ID).DefensePower : o.NumArmies.DefensePower);
        }
    }
}
