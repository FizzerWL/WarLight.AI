using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>NoPlanBreakBestTerritoryTask is responsible for breaking the best opponent territory without following a specified plan.
    /// </summary>
    public class NoPlanBreakBestTerritoryTask
    {
        public static Moves CalculateNoPlanBreakBestTerritoryTask(BotMain state, int maxDeployment, List<BotTerritory> territoriesToConsider, BotMap visibleMap, BotMap workingMap, string source)
        {
            var wmOpponentTerritories = workingMap.AllOpponentTerritories.Where(o => o.IsVisible).ToList();
            var vmOpponentTerritories = visibleMap.CopyTerritories(wmOpponentTerritories);
            var sortedOpponentTerritories = state.TerritoryValueCalculator.SortAttackValue(vmOpponentTerritories);
            if (territoriesToConsider != null)
            {
                var territoriesToRemove = new List<BotTerritory>();
                foreach (var territory in sortedOpponentTerritories)
                {
                    if (!territoriesToConsider.Contains(territory))
                        territoriesToRemove.Add(territory);
                }
                territoriesToRemove.ForEach(o => sortedOpponentTerritories.Remove(o));
            }
            foreach (var territory_1 in sortedOpponentTerritories)
            {
                if (territory_1.IsVisible)
                {
                    var breakTerritoryMoves = state.BreakTerritoryTask.CalculateBreakTerritoryTask(territory_1, maxDeployment, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal, source);
                    if (breakTerritoryMoves != null)
                        return breakTerritoryMoves;
                }
            }
            return null;
        }

        public static List<BotTerritory> RemoveTerritoriesThatWeTook(BotMain state, List<BotTerritory> opponentTerritories)
        {
            var newOpponentTerritoriesIDs = state.WorkingMap.AllOpponentTerritories.Select(o => o.ID).ToHashSet(false);
            return opponentTerritories.Where(o => newOpponentTerritoriesIDs.Contains(o.ID)).ToList();
        }
    }
}
