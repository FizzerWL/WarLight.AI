using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public class PicksEvaluator
    {
        public BotMain BotState;
        public PicksEvaluator(BotMain state)
        {
            this.BotState = state;
        }

        public List<TerritoryIDType> GetPicks()
        {
            if (BotState.Map.IsScenarioDistribution(BotState.Settings.DistributionModeID))
            {
                var us = BotState.Map.GetTerritoriesForScenario(BotState.Settings.DistributionModeID, BotState.Me.ScenarioID);
                us.RandomizeOrder();
                return us;
            }


            int maxPicks = BotState.Settings.LimitDistributionTerritories == 0 ? BotState.Map.Territories.Count : (BotState.Settings.LimitDistributionTerritories * BotState.Players.Count(o => o.Value.State == GamePlayerState.Playing));

            var pickableTerritories = BotState.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToList();


            var weights = pickableTerritories.ToDictionary(o => o, terrID =>
            {
                var map = BotMap.FromStanding(BotState, BotState.DistributionStanding);

                map.Territories[terrID].OwnerPlayerID = BotState.Me.ID;
                if(map.Territories[terrID].Bonuses.Count > 0)
                {
                    BotBonus bonus = map.Territories[terrID].Bonuses[0];
                    bonus.SetMyExpansionValueHeuristic();
                    double r = bonus.ExpansionValue;
                    return r;
                }
                else
                {
                    return 0;
                }
            });

            List<TerritoryIDType> picks = weights.OrderByDescending(o => o.Value).Take(maxPicks).Select(o => o.Key).Distinct().ToList();
            //StatefulFogRemover.PickedTerritories = picks;

            return picks;
        }

    }
}
