using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Move
{
    /// <summary>This class is responsible for committing moves.</summary>
    public class MovesCommitter
    {
        public static void CommittMoves(BotMain state, Moves moves)
        {
            foreach(var order in moves.Orders)
            {
                if (order is BotOrderAttackTransfer)
                    CommittAttackTransferMove(state, (BotOrderAttackTransfer)order);
                else if (order is BotOrderDeploy)
                    CommittPlaceArmiesMove((BotOrderDeploy)order);
            }
        }

        public static void CommittPlaceArmiesMoves(List<BotOrderDeploy> placeArmiesMoves)
        {
            foreach (BotOrderDeploy placeArmiesMove in placeArmiesMoves)
                CommittPlaceArmiesMove(placeArmiesMove);
        }

        public static void CommittAttackTransferMoves(BotMain state, List<BotOrderAttackTransfer> attackTransferMoves)
        {
            foreach (var attackTransferMove in attackTransferMoves)
                CommittAttackTransferMove(state, attackTransferMove);
        }

        public static void CommittPlaceArmiesMove(BotOrderDeploy placeArmiesMove)
        {
            placeArmiesMove.Territory.Deployment.Add(placeArmiesMove);
        }

        public static void CommittPlaceArmiesMove(BotOrderDeploy placeArmiesMove, BotTerritory.DeploymentType type)
        {
            if (type == BotTerritory.DeploymentType.Normal)
                placeArmiesMove.Territory.Deployment.Add(placeArmiesMove);
            else if (type == BotTerritory.DeploymentType.Conservative)
                placeArmiesMove.Territory.ConservativeDeployment.Add(placeArmiesMove);
        }

        public static void CommittAttackTransferMove(BotMain state, BotOrderAttackTransfer attackTransferMove)
        {
            attackTransferMove.From.OutgoingMoves.Add(attackTransferMove);
            attackTransferMove.To.IncomingMoves.Add(attackTransferMove);
            state.MapUpdater.UpdateMap(attackTransferMove, state.WorkingMap, BotTerritory.DeploymentType.Normal);
        }
    }
}
