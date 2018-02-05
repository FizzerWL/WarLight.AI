/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarLight.Shared.AI.Cowzow.Move
{
    public class BotOrder
    {
        public PlayerIDType PlayerID;

        public static List<GameOrder> Convert(IEnumerable<BotOrder> orders)
        {
            var ret = new List<GameOrder>();
            foreach (var order in orders)
            {
                if (order is BotOrderDeploy)
                    ret.Add(Convert((BotOrderDeploy)order));
                else if (order is BotOrderAttackTransfer)
                    ret.Add(Convert((BotOrderAttackTransfer)order));
                //else if (order is BotOrderGeneric)
                //    ret.Add(order.As<BotOrderGeneric>().Order);
                else
                    throw new Exception("Unexpected order type");
            }

            return ret;
        }


        private static GameOrder Convert(BotOrderDeploy o)
        {
            return GameOrderDeploy.Create(o.PlayerID, o.Armies, o.Territory.ID, false);
        }

        private static GameOrder Convert(BotOrderAttackTransfer o)
        {
            return GameOrderAttackTransfer.Create(o.PlayerID, o.FromTerritory.ID, o.ToTerritory.ID, AttackTransferEnum.AttackTransfer, false, new Armies(o.Armies), false);
        }

        public override string ToString()
        {
            return "BotOrder";
        }
    }
}
