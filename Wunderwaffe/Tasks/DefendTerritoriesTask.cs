using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>
    /// DefendTerritoriesTask is responsible for calculating a defense plan so that the opponent can't take any of the specified territories.
    /// </summary>
    public class DefendTerritoriesTask
    {
        public BotMain BotState;
        public DefendTerritoriesTask(BotMain state)
        {
            this.BotState = state;
        }

        /// <summary>Returns null if not possible to defend all territories and not the acceptNotAllDefense flag is set.
        /// </summary>
        public Moves CalculateDefendTerritoriesTask(List<BotTerritory> territoriesToDefend, int maxDeployment, bool acceptNotAllDefense, BotTerritory.DeploymentType lowerConservativeLevel, BotTerritory.DeploymentType upperConservativeLevel)
        {
            var outvar = new Moves();
            var sortedDefenceTerritories = BotState.TerritoryValueCalculator.SortDefenseValue(territoriesToDefend);
            foreach (var territory in sortedDefenceTerritories)
            {
                var stillAvailableArmies = maxDeployment - outvar.GetTotalDeployment();
                var defendTerritoryMoves = BotState.DefendTerritoryTask.CalculateDefendTerritoryTask(territory, stillAvailableArmies, true, lowerConservativeLevel, upperConservativeLevel);
                if (defendTerritoryMoves != null)
                    outvar.MergeMoves(defendTerritoryMoves);
                else if (!acceptNotAllDefense)
                    return null;
            }
            return outvar;
        }
    }
}
