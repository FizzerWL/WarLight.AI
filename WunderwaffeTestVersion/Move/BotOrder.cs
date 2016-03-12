using WarLight.Shared.AI;

namespace WarLight.AI.WunderwaffeTestVersion.Move
{
    public abstract class BotOrder
    {
        public PlayerIDType PlayerID;

        public abstract TurnPhase OccursInPhase { get; }

    }
}
