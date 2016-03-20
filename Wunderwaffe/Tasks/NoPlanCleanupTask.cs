using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>NoPlanCleanupTask is responsible for calculating the remaining moves after the other tasks have been fulfilled.
    /// </summary>
    /// <remarks>
    /// If the other tasks don't use the full deployment then this task is responsible for deploying the remaining armies. Also if there are idle armies after the other tasks then in this task those armies can get used to perform an expansion step.
    /// </remarks>
    public class NoPlanCleanupTask
    {
        public static Moves CalculateNoPlanCleanupDeploymentTask(BotMain state, int armiesToDeploy, Moves movesSoFar)
        {
            var outvar = new Moves();
            if (armiesToDeploy > 0)
            {
                var bestDeploymentTerritory = GetBestDeploymentTerritory(state, movesSoFar);
                outvar.AddOrder(new BotOrderDeploy(state.Me.ID, bestDeploymentTerritory, armiesToDeploy));
            }
            return outvar;
        }

        public static Moves CalculateNoPlanCleanupExpansionTask(BotMain state, Moves movesSoFar)
        {
            var outvar = new Moves();
            foreach (var fromTerritory in state.VisibleMap.GetNonOpponentBorderingBorderTerritories())
            {
                var possibleToTerritories = new List<BotTerritory>();
                foreach (var nonOwnedNeighbor in fromTerritory.GetNonOwnedNeighbors())
                {
                    if (nonOwnedNeighbor.OwnerPlayerID == TerritoryStanding.NeutralPlayerID && IsUnplannedExpansionStepSmart(state, fromTerritory, nonOwnedNeighbor))
                        possibleToTerritories.Add(nonOwnedNeighbor);
                }
                if (possibleToTerritories.Count > 0)
                {
                    var territoryToAttack = possibleToTerritories.OrderByDescending(t => t.Bonuses.Sum(b => b.GetExpansionValue()) * 100 + t.ExpansionTerritoryValue).First();

                    outvar.AddOrder(new BotOrderAttackTransfer(state.Me.ID, fromTerritory, territoryToAttack, fromTerritory.GetIdleArmies(), "NoPlanCleanupTask"));
                }
            }
            return outvar;
        }

        /// <summary>Returns whether it makes sense to perform an unplanned expansion step from our territory to the neutral territory.
        /// </summary>
        /// <remarks>
        /// Returns whether it makes sense to perform an unplanned expansion step from our territory to the neutral territory. The
        /// parameters considered therefore are the distance to the opponent and whether we can take that territory.
        /// </remarks>
        /// <param name="fromTerritory">an owned territory, not bordering the opponent</param>
        /// <param name="toTerritory">a neutral territory</param>
        /// <returns></returns>
        private static bool IsUnplannedExpansionStepSmart(BotMain state, BotTerritory fromTerritory, BotTerritory toTerritory)
        {
            var isSmart = true;

            if (fromTerritory.GetIdleArmies().AttackPower <= 1)
                isSmart = false;

            if (toTerritory.Armies.DefensePower > toTerritory.getOwnKills(fromTerritory.GetIdleArmies().AttackPower,toTerritory.Armies.DefensePower))
                isSmart = false;
            var distanceCondition1 = fromTerritory.DistanceToOpponentBorder <= 4;
            var distanceCondition2 = toTerritory.GetOpponentNeighbors().Count == 0;
            if (distanceCondition1 && distanceCondition2)
            {
                isSmart = false;
            }
            return isSmart;
        }

        private static BotTerritory GetBestDeploymentTerritory(BotMain state, Moves movesSoFar)
        {
            // If we are bordering the opponent then the highest defense territory next
            // to the opponent is good
            var opponentBorderingTerritories = state.VisibleMap.GetOpponentBorderingTerritories();
            if (opponentBorderingTerritories.Count > 0)
            {
                var sortedTerritories = state.TerritoryValueCalculator.SortDefenseValue(opponentBorderingTerritories);
                var bestTerritory = sortedTerritories[0];
                return bestTerritory;
            }
            var expansionMoves = new List<BotOrderAttackTransfer>();
            foreach (var atm in movesSoFar.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (atm.To.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                    expansionMoves.Add(atm);
            }
            // If we aren't expanding at all then a random territory is good (strange
            // case)
            if (expansionMoves.Count == 0)
                return state.VisibleMap.GetOwnedTerritories()[0];
            // If we are expanding then look for the attack to the highest
            // ExpansionValueTerritory and deploy to the moving territory.
            var biggestExpansionValue = 0;
            var mostImportantMove = expansionMoves[0];
            foreach (var atm_1 in expansionMoves)
            {
                if (atm_1.To.ExpansionTerritoryValue > biggestExpansionValue)
                {
                    biggestExpansionValue = atm_1.To.ExpansionTerritoryValue;
                    mostImportantMove = atm_1;
                }
            }
            return mostImportantMove.From;
        }
    }
}
