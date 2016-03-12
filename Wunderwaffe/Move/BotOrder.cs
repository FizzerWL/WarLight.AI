using WarLight.Shared.AI;

namespace WarLight.AI.Wunderwaffe.Move
{
    public abstract class BotOrder
    {
        public PlayerIDType PlayerID;

        public abstract TurnPhase OccursInPhase { get; }

    }
}
