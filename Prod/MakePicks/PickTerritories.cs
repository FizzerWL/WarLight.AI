using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakePicks
{
    static class PickTerritories
    {
        public static List<TerritoryIDType> MakePicks(BotMain bot)
        {
            if (bot.Map.IsScenarioDistribution(bot.Settings.DistributionModeID))
            {
                var us = bot.Map.GetTerritoriesForScenario(bot.Settings.DistributionModeID, bot.GamePlayerReference.ScenarioID);

                if (bot.UseRandomness)
                    return us.OrderByRandom().ToList();
                else
                    return us.OrderBy(o => (int)o).ToList();
            }
            else
            {
                int maxPicks = bot.Settings.LimitDistributionTerritories == 0 ? bot.Map.Territories.Count : (bot.Settings.LimitDistributionTerritories * bot.Players.Values.Count(o => o.State == GamePlayerState.Playing));

                var allAvailable = bot.DistributionStandingOpt.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToHashSet(true);

                var expansionWeights = allAvailable.ToDictionary(o => o, o => GetExpansionWeight(bot, o));

                var ordered = expansionWeights.OrderByDescending(o => o.Value).ToList();

                AILog.Log("PickTerritories", "Came up with " + expansionWeights.Count + " expansion weights: ");
                foreach (var e in ordered.Take(30))
                    AILog.Log("PickTerritories", " - " + bot.TerrString(e.Key) + ": " + e.Value);

                if (!bot.UseRandomness)
                    return ordered.Select(o => o.Key).Take(maxPicks).ToList();

                //Normalize weights
                var top = ordered.Take(maxPicks * 2);
                var sub = top.Min(o => o.Value) - 1;
                var normalized = top.ToDictionary(o => o.Key, o => o.Value - sub);

                var picks = new List<TerritoryIDType>();
                while (picks.Count < maxPicks)
                {
                    var pick = RandomUtility.WeightedRandom(normalized.Keys, o => normalized[o]);
                    picks.Add(pick);
                    normalized.Remove(pick);
                }
                return picks;

                //var picks = new List<TerritoryIDType>();

                //picks.Add();
                //if (picks.Count < maxPicks)
                //{
                //    var usedSpots = new HashSet<TerritoryIDType>();
                //    usedSpots.Add(picks[0]);

                //    foreach (var pick1 in ClusterOut(usedSpots, picks[0], bot.DistributionStandingOpt, new HashSet<TerritoryIDType>(), bot.Map, maxPicks, 1))
                //    {
                //        picks.Add(pick1);
                //        if (picks.Count >= maxPicks)
                //            break;
                //    }

                //    if (picks.Count < maxPicks)
                //    {
                //        //Due to stack depth, we may not traverse everything.  Finish it out.
                //        foreach (var terr in bot.Map.Territories.Keys)
                //            if (!usedSpots.Contains(terr) && bot.DistributionStandingOpt.Territories[terr].OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                //            {
                //                picks.Add(terr);
                //                if (picks.Count >= maxPicks)
                //                    break;
                //            }
                //    }
                //}

                //return picks.ToArray();
            }
        }

        private static float GetExpansionWeight(BotMain bot, TerritoryIDType terrID)
        {
            var td = bot.Map.Territories[terrID];

            var bonusPaths = td.PartOfBonuses.Where(o => bot.BonusValue(o) > 0).ToDictionary(o => o, o => new BonusPath(bot, o, ts => ts.ID == terrID));
            var turnsToTake = bonusPaths.Keys.ToDictionary(o => o, o => TurnsToTake(bot, td.ID, o, bonusPaths[o]));
            var bonusWeights = bonusPaths.Keys.ToDictionary(o => o, o => ExpansionHelper.WeighBonus(bot, o, bonusPaths[o], ts => ts.ID == terrID, turnsToTake[o]));

            var weight = 0.0f;

            weight += td.PartOfBonuses.Count == 0 ? 0 : td.PartOfBonuses.Max(o => bonusWeights.ContainsKey(o) ? bonusWeights[o] : 0);

            AILog.Log("PickTerritories", "Expansion weight for terr " + bot.TerrString(terrID) + " is " + weight + ". " + td.PartOfBonuses.Select(b => "Bonus " + bot.BonusString(b) + " Weight=" + bonusWeights[b] + " TurnsToTake=" + turnsToTake[b] + " (infinite armies=" + bonusPaths[b].TurnsToTakeByDistance + ")").JoinStrings(", "));

            return weight;
        }

        //private static IEnumerable<TerritoryIDType> ClusterOut(HashSet<TerritoryIDType> usedSpots, TerritoryIDType startingSpot, GameStanding standing, HashSet<TerritoryIDType> traversedSpots, MapDetails map, int maxPicks, int depth)
        //{
        //    var ret = new List<TerritoryIDType>();

        //    if (usedSpots.Count >= maxPicks) //just for perf, we don't need to keep clustering if we've reached the max.  Our caller does the authoritive limit check.
        //        return ret;

        //    traversedSpots.Add(startingSpot);
        //    foreach (var connected1 in map.Territories[startingSpot].ConnectedTo)
        //        if (!usedSpots.Contains(connected1) && standing.Territories[connected1].OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
        //        {
        //            usedSpots.Add(connected1);
        //            ret.Add(connected1);

        //            if (usedSpots.Count >= maxPicks) //same comment as above
        //                return ret;
        //        }

        //    if (depth >= 30) //prevent stack overflows
        //        return ret;

        //    foreach (var connected2 in map.Territories[startingSpot].ConnectedTo)
        //        if (!traversedSpots.Contains(connected2))
        //            foreach (var o in ClusterOut(usedSpots, connected2, standing, traversedSpots, map, maxPicks, depth + 1))
        //                ret.Add(o);

        //    return ret;
        //}

        /// <summary>
        /// Estimate how many turns it would take us to complete the bonus, assuming we start in the passed territory and can use full deployment every turn
        /// </summary>
        /// <param name="bonusID"></param>
        /// <param name="bonusPath"></param>
        /// <returns></returns>
        private static CaptureTerritories TurnsToTake(BotMain bot, TerritoryIDType terrID, BonusIDType bonusID, BonusPath path)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            var terrsToTake = bonus.Territories.ExceptOne(terrID).ToHashSet(true);

            return CaptureTerritories.TryFindTurnsToTake(bot, path, bot.Settings.InitialPlayerArmiesPerTerritory, bot.Settings.MinimumArmyBonus, terrsToTake, o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution ? bot.Settings.InitialNeutralsInDistribution : o.NumArmies.DefensePower);
        }

    }
}
