using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Wunderwaffe.Move
{
    public class BotOrderGeneric : BotOrder
    {
        public GameOrder Order;
        public BotOrderGeneric(GameOrder order)
        {
            this.Order = order;
            this.PlayerID = order.PlayerID;
        }

        public override TurnPhase OccursInPhase
        {
            get { return Order.OccursInPhase.Value; }
        }

    }
}
