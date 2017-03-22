using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakePicks
{
    static class PickCluster
    {
        public static List<TerritoryIDType> Go(BotMain bot, HashSet<TerritoryIDType> allAvailable, int maxPicks)
        {
            var picks = new List<TerritoryIDType>();

            if (bot.UseRandomness)
                picks.Add(allAvailable.Random());
            else
                picks.Add(allAvailable.First());

            if (picks.Count < maxPicks)
            {
                var usedSpots = new HashSet<TerritoryIDType>();
                usedSpots.Add(picks[0]);

                foreach (var pick1 in ClusterOut(usedSpots, picks[0], bot.DistributionStandingOpt, new HashSet<TerritoryIDType>(), bot.Map, maxPicks, 1))
                {
                    picks.Add(pick1);
                    if (picks.Count >= maxPicks)
                        break;
                }

                if (picks.Count < maxPicks)
                {
                    //Due to stack depth, we may not traverse everything.  Finish it out.
                    foreach (var terr in allAvailable.OrderByRandom().Where(o => !usedSpots.Contains(o)))
                    {
                        picks.Add(terr);
                        if (picks.Count >= maxPicks)
                            break;
                    }
                }
            }

            return picks;

        }


        private static IEnumerable<TerritoryIDType> ClusterOut(HashSet<TerritoryIDType> usedSpots, TerritoryIDType startingSpot, GameStanding standing, HashSet<TerritoryIDType> traversedSpots, MapDetails map, int maxPicks, int depth)
        {
            var ret = new List<TerritoryIDType>();

            if (usedSpots.Count >= maxPicks) //just for perf, we don't need to keep clustering if we've reached the max.  Our caller does the authoritive limit check.
                return ret;

            traversedSpots.Add(startingSpot);
            foreach (var connected1 in map.Territories[startingSpot].ConnectedTo.Keys)
                if (!usedSpots.Contains(connected1) && standing.Territories[connected1].OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                {
                    usedSpots.Add(connected1);
                    ret.Add(connected1);

                    if (usedSpots.Count >= maxPicks) //same comment as above
                        return ret;
                }

            if (depth >= 30) //prevent stack overflows
                return ret;

            foreach (var connected2 in map.Territories[startingSpot].ConnectedTo.Keys)
                if (!traversedSpots.Contains(connected2))
                    foreach (var o in ClusterOut(usedSpots, connected2, standing, traversedSpots, map, maxPicks, depth + 1))
                        ret.Add(o);

            return ret;
        }

    }
}
