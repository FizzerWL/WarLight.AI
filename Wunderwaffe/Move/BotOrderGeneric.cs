namespace WarLight.Shared.AI.Wunderwaffe.Move
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
