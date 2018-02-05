namespace WarLight.Shared.AI
{
    public enum TurnPhase
    {
        Purchase = 0,
        CardsWearOff = 5,
        Discards = 10,
        ReinforcementAndSpyCards = 20,
        Deploys = 25,
        BombCards = 30,
        AbandonCards = 37,
        Airlift = 43,
        Gift = 47,
        Attacks = 50,
        BlockadeCards = 60,
        DiplomacyCards = 75,
        SanctionCards = 80,
        ReceiveCards = 100
    }

    public abstract class GameOrder
    {
        public PlayerIDType PlayerID;

        public abstract TurnPhase? OccursInPhase { get; }

    }
}
