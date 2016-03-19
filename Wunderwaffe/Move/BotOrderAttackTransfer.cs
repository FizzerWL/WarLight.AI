using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Move
{

    public enum AttackMessage
    {
        None, EarlyAttack, TryoutAttack, Snipe
    }

    /// <summary>This Move is used in the second part of each round.</summary>
    /// <remarks>
    /// This Move is used in the second part of each round. It represents the attack or transfer of armies from fromTerritory to
    /// toTerritory. If toTerritory is owned by the player himself, it's a transfer. If toTerritory is owned by the opponent, this
    /// Move is an attack.
    /// </remarks>
    public class BotOrderAttackTransfer : BotOrder
    {
        public BotTerritory From;
        public BotTerritory To;

        public Armies Armies;

        public AttackMessage Message;
        public string Source; //just for debugging, never used for logic decisions

        public BotOrderAttackTransfer(PlayerIDType playerID, BotTerritory from, BotTerritory to, Armies armies, string source)
        {
            this.PlayerID = playerID;
            this.From = from;
            this.To = to;
            this.Armies = armies;
            this.Source = source;
        }

        public override string ToString()
        {
            return this.From + " -[" + this.Armies + "]-> " + this.To + " " + this.To.OwnerPlayerID + " (" + this.Source + ")";
        }

        public override TurnPhase OccursInPhase
        {
            get { return TurnPhase.Attacks; }
        }
    }
}
