using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>This class is responsible for flanking a Bonus owned by the opponent.
    /// </summary>
    /// <remarks>This class is responsible for flanking a Bonus owned by the opponent.
    /// </remarks>
    public class FlankBonusTask
    {
        /// <summary>Calculates the best flanking moves for opponent Bonuses.</summary>
        /// <remarks>Calculates the best flanking moves for opponent Bonuses.</remarks>
        /// <param name="maxDeployment">the max deployment constraint</param>
        /// <returns>the best flanking moves and null if no such moves were found</returns>
        public static Moves CalculateFlankBonusTask(BotMain state, int maxDeployment)
        {
            Moves outvar = null;
            var sortedFlankingTerritories = state.TerritoryValueCalculator.GetSortedFlankingValueTerritories();
            foreach (var flankableTerritory in sortedFlankingTerritories)
            {
                if (flankableTerritory.FlankingTerritoryValue <= 2)
                {
                    break;
                }
                List<BotTerritory> territoryToTakeAsList = new List<BotTerritory>();
                territoryToTakeAsList.Add(flankableTerritory);
                outvar = state.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(maxDeployment, territoryToTakeAsList, 0, "FlankBonusTask");
                if (outvar != null)
                {
                    break;
                }
            }
            return outvar;
        }
    }
}
