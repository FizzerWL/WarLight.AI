namespace WarLight.Shared.AI.Wunderwaffe.Move
{
    public abstract class BotOrder
    {
        public PlayerIDType PlayerID;

        public abstract TurnPhase OccursInPhase { get; }

        public override string ToString()
        {
            return "BotOrder";
        }

    }
}
