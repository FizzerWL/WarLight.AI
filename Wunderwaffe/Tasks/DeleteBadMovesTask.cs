using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>DeleteBadMovesTask is responsible for deleting bad moves from previous steps.
    /// </summary>
    /// <remarks>
    /// DeleteBadMovesTask is responsible for deleting bad moves from previous steps. If we deploy in territory B and transfer
    /// to Territory A then we should directly deploy at A. If we Attack from A O1 and O2 and also from B O1 and O2 then we
    /// should go for lesser stronger attacks.
    /// </remarks>
    public class DeleteBadMovesTask
    {
        public BotMain BotState;
        public DeleteBadMovesTask(BotMain state)
        {
            this.BotState = state;
        }
        public void CalculateDeleteBadMovesTask(Moves movesSoFar)
        {
            DeleteBadTransferMoves(movesSoFar);
            DeleteBadAttacks(movesSoFar);
        }

        private void DeleteBadAttacks(Moves movesSoFar)
        {
            var interestingAttacks = new List<BotOrderAttackTransfer>();
            foreach (var atm in movesSoFar.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (atm.Armies.AttackPower > 1 && BotState.IsOpponent(atm.To.OwnerPlayerID))
                {
                    var isInteresting = false;
                    var attackTerritory = atm.To.IncomingMoves[0].From;
                    foreach (var incomingMove in atm.To.IncomingMoves)
                    {
                        if (incomingMove.From != attackTerritory)
                            isInteresting = true;
                    }
                    if (isInteresting)
                        interestingAttacks.Add(atm);
                }
            }
            var territoriesWithInterestingAttacks = new HashSet<BotTerritory>();
            foreach (BotOrderAttackTransfer atm_1 in interestingAttacks)
                territoriesWithInterestingAttacks.Add(atm_1.From);
        }

        // TODO weitermachen
        private void DeleteBadTransferMoves(Moves movesSoFar)
        {
            var interestingTransfers = new List<BotOrderAttackTransfer>();
            foreach (var atm in movesSoFar.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (atm.Armies.AttackPower > 1 && atm.To.OwnerPlayerID == BotState.Me.ID && atm.From.GetTotalDeployment(BotTerritory.DeploymentType.Normal) > 0)
                    interestingTransfers.Add(atm);
            }
            foreach (var atm_1 in interestingTransfers)
            {
                var deploymentToShift = Math.Min(atm_1.From.GetTotalDeployment(BotTerritory.DeploymentType.Normal), atm_1.Armies.AttackPower);
                atm_1.Armies = atm_1.Armies.Subtract(new Armies(deploymentToShift));
                var pam = new BotOrderDeploy(BotState.Me.ID, atm_1.To, deploymentToShift);
                MovesCommitter.CommittPlaceArmiesMove(pam);
                movesSoFar.AddOrder(pam);
                foreach (var oldDeployment in atm_1.From.GetDeployment(BotTerritory.DeploymentType.Normal))
                {
                    while (deploymentToShift > 0)
                    {
                        if (oldDeployment.Armies > 0)
                        {
                            deploymentToShift--;
                            oldDeployment.Armies = oldDeployment.Armies - 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
