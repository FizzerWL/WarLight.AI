using System;
using System.Collections.Generic;
using System.Linq;

namespace WarLight.Shared.AI.Wunderwaffe.Move
{
    /// <summary>Moves is a data structure to store all calculated moves.</summary>
    public class Moves
    {
        public List<BotOrder> Orders = new List<BotOrder>();

        public void AddOrder(BotOrder orderToAdd)
        {
            for (int i = 0; i < Orders.Count; i++)
            {
                if ((int)orderToAdd.OccursInPhase < (int)Orders[i].OccursInPhase)
                {
                    Orders.Insert(i, orderToAdd);
                    return;
                }
            }

            Orders.Add(orderToAdd);
        }


        public void MergeMoves(Moves newMoves)
        {
            foreach (var order in newMoves.Orders)
                AddOrder(order);
        }
        
        public int GetTotalDeployment()
        {
            return Orders.OfType<BotOrderDeploy>().Sum(o => o.Armies);
        }

        public List<GameOrder> Convert()
        {
            var ret = new List<GameOrder>();
            foreach (var order in this.Orders)
            {
                if (order is BotOrderDeploy)
                    ret.Add(Convert((BotOrderDeploy)order));
                else if (order is BotOrderAttackTransfer)
                    ret.Add(Convert((BotOrderAttackTransfer)order));
                else if (order is BotOrderGeneric)
                    ret.Add(order.As<BotOrderGeneric>().Order);
                else
                    throw new Exception("Unexpected order type");
            }

            return ret;
        }

        private GameOrder Convert(BotOrderDeploy o)
        {
            return GameOrderDeploy.Create(o.PlayerID, o.Armies, o.Territory.ID, false);
        }

        private GameOrder Convert(BotOrderAttackTransfer o)
        {
            return GameOrderAttackTransfer.Create(o.PlayerID, o.From.ID, o.To.ID, AttackTransferEnum.AttackTransfer, false, o.Armies, false);
        }

        /// <summary>Creates a copy of this object.</summary>
        /// <returns>a copy where the attackTransferMoves and placeArmiesMoves point to the same objects.
        /// </returns>
        public Moves Copy()
        {
            var copy = new Moves();
            copy.Orders.AddRange(this.Orders);
            return copy;
        }

        public void DumpToLog()
        {
            AILog.Log("MovesCalculator", "Final " + Orders.Count + " orders:");
                
            foreach (var order in Orders)
            {
                if (order is BotOrderDeploy)
                {
                    var dep = (BotOrderDeploy)order;
                    AILog.Log("MovesCalculator", " - " + dep.Armies + " on " + dep.Territory.Details.Name + " " + dep.Territory.ToString());
                }
                else if (order is BotOrderAttackTransfer)
                {
                    var attack = (BotOrderAttackTransfer)order;

                    AILog.Log("MovesCalculator", " - " + attack.From.Details.Name + " -> " + attack.To.Details.Name + " " + attack.Armies + " Message=" + attack.Message + ", Source=" + attack.Source);
                }
                else if (order is BotOrderGeneric)
                {
                    AILog.Log("MovesCalculator", " - " + order.As<BotOrderGeneric>().Order.ToString());
                }
                else
                    throw new Exception("Unexpected order type");

            }

        }
    }
}
