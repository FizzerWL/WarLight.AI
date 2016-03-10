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

                if (allAvailable.Count > 300) //if there are too many picks, it would take forever to run our weighting algorithm on all of them.  Therefore, revert to just picking a single cluster of the map.
                    return PickCluster.Go(bot, allAvailable, maxPicks);
                else
                    return PickByWeight.Go(bot, allAvailable, maxPicks);

            }
        }



    }
}
