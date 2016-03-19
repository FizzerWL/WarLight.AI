using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;


namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>
    /// MoveIdleArmiesTask is responsible for calculating armies to join in to the
    /// already calculated moves.
    /// </summary>
    /// <remarks>
    /// MoveIdleArmiesTask is responsible for calculating armies to join in to the
    /// already calculated moves.
    /// </remarks>
    public class MoveIdleArmiesTask
    {
        public static Moves CalculateMoveIdleArmiesTask(BotMain state)
        {
            var outvar = new Moves();
            foreach (var ownedTerritory in state.VisibleMap.GetOwnedTerritories())
            {
                var outgoingMoves = ownedTerritory.OutgoingMoves;
                BotOrderAttackTransfer mostImportantMove = null;
                var currentHighestValue = -1;
                foreach (var atm in outgoingMoves)
                {
                    if (state.IsOpponent(atm.To.OwnerPlayerID) && atm.Armies.AttackPower > 1 && atm.Message != AttackMessage.TryoutAttack)
                    {
                        var attackValue = atm.To.AttackTerritoryValue;
                        if (attackValue > currentHighestValue)
                        {
                            currentHighestValue = attackValue;
                            mostImportantMove = atm;
                        }
                    }
                }
                var idleArmies = ownedTerritory.GetIdleArmies();

                if (mostImportantMove != null && idleArmies.IsEmpty == false)
                {
                    var atm_1 = new BotOrderAttackTransfer(state.Me.ID, ownedTerritory, mostImportantMove.To, ownedTerritory.GetIdleArmies(), "MoveIdleArmiesTask1");
                    outvar.AddOrder(atm_1);
                }
            }
            return outvar;
        }

        /// <summary>Calculates the movement of idle armies to join in expansion steps.</summary>
        /// <remarks>Calculates the movement of idle armies to join in expansion steps.</remarks>
        /// <returns></returns>
        public static Moves CalculateMoveIdleExpansionArmiesTask(BotMain state)
        {
            var outvar = new Moves();
            foreach (var ourTerritory in state.VisibleMap.GetNonOpponentBorderingBorderTerritories())
            {
                if (ourTerritory.GetIdleArmies().IsEmpty == false && ourTerritory.GetExpansionMoves().Count > 0)
                {
                    var bestMove = GetBestExpansionMoveToAddArmies(ourTerritory.GetExpansionMoves());
                    if (IsAddingArmiesBeneficial(bestMove))
                        outvar.AddOrder(new BotOrderAttackTransfer(state.Me.ID, ourTerritory, bestMove.To, ourTerritory.GetIdleArmies(), "MoveIdleArmiesTask2"));
                }
            }
            return outvar;
        }

        private static bool IsAddingArmiesBeneficial(BotOrderAttackTransfer expansionMove)
        {
            var isBeneficial = true;
            var containsToTerritoryOpponentNeighbor = expansionMove.To.GetOpponentNeighbors
                ().Count > 0 ? true : false;
            var containsFromTerritoryOpponentBorderingNeighbor = false;
            foreach (var ownedNeighbor in expansionMove.From.GetOwnedNeighbors())
            {
                if (ownedNeighbor.GetOpponentNeighbors().Count > 0)
                {
                    containsFromTerritoryOpponentBorderingNeighbor = true;
                }
            }
            if (containsToTerritoryOpponentNeighbor == false && containsFromTerritoryOpponentBorderingNeighbor
                 == true)
            {
                isBeneficial = false;
            }
            return isBeneficial;
        }

        /// <param name="expansionMoves">only moves where the toTerritory is neutral are allowed
        /// </param>
        /// <returns></returns>
        private static BotOrderAttackTransfer GetBestExpansionMoveToAddArmies(List<BotOrderAttackTransfer> expansionMoves)
        {
            BotOrderAttackTransfer bestExpansionMove = null;
            List<BotOrderAttackTransfer> expansionMovesToOpponent = new List<BotOrderAttackTransfer
                >();
            foreach (BotOrderAttackTransfer atm in expansionMoves)
            {
                if (atm.To.GetOpponentNeighbors().Count > 0)
                    expansionMovesToOpponent.Add(atm);
            }
            if (expansionMovesToOpponent.Count > 0)
            {
                bestExpansionMove = expansionMovesToOpponent[0];
                foreach (BotOrderAttackTransfer atm_1 in expansionMovesToOpponent)
                {
                    if (atm_1.To.AttackTerritoryValue > bestExpansionMove.To.AttackTerritoryValue)
                        bestExpansionMove = atm_1;
                }
            }
            else
            {
                if (expansionMoves.Count > 0)
                {
                    bestExpansionMove = expansionMoves[0];
                    foreach (BotOrderAttackTransfer atm_1 in expansionMoves)
                    {
                        if (atm_1.To.GetNonOwnedNeighbors().Count > bestExpansionMove.To.GetNonOwnedNeighbors().Count)
                            bestExpansionMove = atm_1;
                    }
                }
            }
            return bestExpansionMove;
        }
    }
}
