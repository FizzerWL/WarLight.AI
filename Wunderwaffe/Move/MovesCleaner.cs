using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Move
{
    /// <summary>This class is responsible for cleaning up moves.</summary>
    /// <remarks>
    /// This class is responsible for cleaning up moves. This means that the deployment gets embellished and if there are
    /// intern more than one move from Territory A to Territory B then those moves get joined.
    /// </remarks>
    public class MovesCleaner
    {
        public static void CleanupMoves(BotMain state, Moves moves)
        {
            Debug.Debug.PrintMoves(state, moves);
            DeleteOldMovesFromMap(state);
            MergeMoves(state, moves);
            state.MapUpdater.UpdateMap(state.WorkingMap);
        }

        private static void MergeMoves(BotMain state, Moves moves)
        {
            var deployedTo = new HashSet<TerritoryIDType>();
            var attackedBetween = new HashSet<KeyValuePair<TerritoryIDType, TerritoryIDType>>();

            for (int i = 0; i < moves.Orders.Count; i++)
            {
                var order = moves.Orders[i];

                if (order is BotOrderDeploy)
                {
                    var deploy = (BotOrderDeploy)order;
                    if (deploy.Armies <= 0)
                    {
                        moves.Orders.RemoveAt(i);
                        i--;
                    }
                    else if (deployedTo.Contains(deploy.Territory.ID))
                    {
                        moves.Orders.OfType<BotOrderDeploy>().First(o => o.Territory.ID == deploy.Territory.ID).Armies += deploy.Armies;
                        moves.Orders.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        deployedTo.Add(deploy.Territory.ID);
                    }
                }
                else if (order is BotOrderAttackTransfer)
                {
                    var attack = (BotOrderAttackTransfer)order;
                    var key = new KeyValuePair<TerritoryIDType, TerritoryIDType>(attack.From.ID, attack.To.ID);

                    if (attack.Armies.IsEmpty)
                    {
                        moves.Orders.RemoveAt(i);
                        i--;
                    }
                    else if (attackedBetween.Contains(key))
                    {
                        var existing = moves.Orders.OfType<BotOrderAttackTransfer>().First(o => o.From == attack.From && o.To == attack.To);

                        existing.Armies = existing.Armies.Add(attack.Armies);
                        if (existing.Message == AttackMessage.None)
                            existing.Message = attack.Message;

                        moves.Orders.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        attackedBetween.Add(key);
                    }
                }
            }

            MovesCommitter.CommittMoves(state, moves);
        }

        /// <summary>Deletes all of our moves from the visible map.</summary>
        /// <remarks>Deletes all of our moves from the visible map.</remarks>
        private static void DeleteOldMovesFromMap(BotMain state)
        {
            foreach (var territory in state.VisibleMap.Territories.Values)
            {
                territory.OutgoingMoves.Clear();
                territory.IncomingMoves.Clear();
                if (territory.OwnerPlayerID == state.Me.ID)
                {
                    territory.GetDeployment(BotTerritory.DeploymentType.Normal).Clear();
                }
            }
        }
    }
}
