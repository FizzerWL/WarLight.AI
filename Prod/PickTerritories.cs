using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.AI.Prod
{
    static class GameAI
    {
        public static TerritoryIDType[] MakePicks(Dictionary<PlayerIDType, GamePlayer> players, GameStanding standing, GameSettings settings, MapDetails map, ushort scenarioID)
        {
            //Assert.Fatal(game.State == GameState.DistributingTerritories, "Called MakeClusterPicks in " + game.State);

            if (map.IsScenarioDistribution(settings.DistributionModeID))
            {
                var us = map.GetTerritoriesForScenario(settings.DistributionModeID, scenarioID);

                us.RandomizeOrder();
                return us.ToArray();
            }
            else
            {
                int maxPicks = settings.LimitDistributionTerritories == 0 ? map.Territories.Count : (settings.LimitDistributionTerritories * players.Values.Count(o => o.State == GamePlayerState.Playing));

                var picks = new List<TerritoryIDType>();

                picks.Add(standing.Territories.Values.RandomWhere(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).ID);

                var usedSpots = new HashSet<TerritoryIDType>();
                usedSpots.Add(picks[0]);

                foreach (var pick1 in ClusterOut(usedSpots, picks[0], standing, new HashSet<TerritoryIDType>(), map, maxPicks, 1))
                {
                    if (picks.Count >= maxPicks)
                        break;
                    picks.Add(pick1);
                    if (picks.Count >= maxPicks)
                        break;
                }

                if (picks.Count < maxPicks)
                {
                    //Due to stack depth, we may not traverse everything.  Finish it out.
                    foreach (var terr in map.Territories.Keys)
                        if (!usedSpots.Contains(terr) && standing.Territories[terr].OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                        {
                            picks.Add(terr);
                            if (picks.Count >= maxPicks)
                                break;
                        }
                }

                return picks.ToArray();
            }
        }

        private static IEnumerable<TerritoryIDType> ClusterOut(HashSet<TerritoryIDType> usedSpots, TerritoryIDType startingSpot, GameStanding standing, HashSet<TerritoryIDType> traversedSpots, MapDetails map, int maxPicks, int depth)
        {
            var ret = new List<TerritoryIDType>();

            if (usedSpots.Count >= maxPicks) //just for perf, we don't need to keep clustering if we've reached the max.  Our caller does the authoritive limit check.
                return ret;

            traversedSpots.Add(startingSpot);
            foreach (var connected1 in map.Territories[startingSpot].ConnectedTo)
                if (!usedSpots.Contains(connected1) && standing.Territories[connected1].OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                {
                    usedSpots.Add(connected1);
                    ret.Add(connected1);

                    if (usedSpots.Count >= maxPicks) //same comment as above
                        return ret;
                }

            if (depth >= 30) //prevent stack overflows
                return ret;

            foreach (var connected2 in map.Territories[startingSpot].ConnectedTo)
                if (!traversedSpots.Contains(connected2))
                    foreach (var o in ClusterOut(usedSpots, connected2, standing, traversedSpots, map, maxPicks, depth + 1))
                        ret.Add(o);

            return ret;
        }

    }
}
