using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <remarks>
    /// This class is responsible for preventing the opponent from taking over a Bonus. This can be achieved in different ways. We can deploy to a territory there or we can hit the Bonus from outside.
    /// </remarks>
    public class PreventBonusTask
    {
        public static Moves CalculatePreventBonusTask(BotMain state, BotBonus bonusToPrevent, PlayerIDType opponentID, int maxDeployment, BotTerritory.DeploymentType conservativeLevel)
        {
            var territoriesToPrevent = bonusToPrevent.GetOwnedTerritories();

            var opponentAttacks = PreventTerritoriesTask.CalculateGuessedOpponentTakeOverMoves(state, territoriesToPrevent, opponentID, true, conservativeLevel);
            if (opponentAttacks == null)
                return new Moves();

            var preventTerritoryMovesByDeploying = PreventTerritoriesTask.CalculatePreventTerritoriesTask(state, territoriesToPrevent, opponentID, maxDeployment, conservativeLevel);
            var visibleOpponentTerritories = bonusToPrevent.GetVisibleOpponentTerritories();
            var breakBestTerritoryMoves = NoPlanBreakBestTerritoryTask.CalculateNoPlanBreakBestTerritoryTask(state, maxDeployment, visibleOpponentTerritories, state.VisibleMap, state.WorkingMap, "PreventBonusTask");

            if (breakBestTerritoryMoves == null)
                return preventTerritoryMovesByDeploying;
            else if (breakBestTerritoryMoves.GetTotalDeployment() <= preventTerritoryMovesByDeploying.GetTotalDeployment())
                return breakBestTerritoryMoves;
            else
                return preventTerritoryMovesByDeploying;
        }
    }
}
