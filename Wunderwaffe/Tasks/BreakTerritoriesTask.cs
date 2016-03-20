using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <remarks>
    /// BreakTerritoriesTask is responsible for breaking a stuff like an opponent Bonus. Therefore the goal is not to break
    /// all territories in the Bonus but to pick one and break this territory.
    /// </remarks>
    public class BreakTerritoriesTask
    {
        /// <remarks>Returns null if none of the specified territories can get broken.</remarks>
        /// <param name="territoriesToBreak"></param>
        /// <param name="maxDeployment"></param>
        /// <returns></returns>
        public static Moves CalculateBreakTerritoriesTask(BotMain state, List<BotTerritory> territoriesToBreak, int maxDeployment, BotTerritory.DeploymentType lowerConservativeLevel, BotTerritory.DeploymentType upperConservativeLevel)
        {
            var sortedTerritories = state.TerritoryValueCalculator.SortAttackValue(territoriesToBreak
                );
            foreach (var opponentTerritory in sortedTerritories)
            {
                var breakTerritoryMoves = state.BreakTerritoryTask.CalculateBreakTerritoryTask(opponentTerritory, maxDeployment, lowerConservativeLevel, upperConservativeLevel, "CalculateBreakTerritoriesTask");
                if (breakTerritoryMoves != null)
                {
                    return breakTerritoryMoves;
                }
            }
            return null;
        }
    }
}
