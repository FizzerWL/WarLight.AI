
using System;

namespace WarLight.Shared.AI
{


    public class GameOrderAttackTransfer : GameOrder
    {
        public TerritoryIDType From;
        public TerritoryIDType To;

        public Armies NumArmies;
        public bool AttackTeammates;
        public AttackTransferEnum AttackTransfer;
        public bool ByPercent;

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Attacks; }
        }

        public static GameOrderAttackTransfer Create(PlayerIDType playerID, TerritoryIDType from, TerritoryIDType to, AttackTransferEnum attackTransfer, bool byPercent, Armies armies, bool attackTeammates)
        {
            var r = new GameOrderAttackTransfer();
            r.PlayerID = playerID;
            r.From = from;
            r.To = to;
            r.AttackTransfer = attackTransfer;
            r.ByPercent = byPercent;
            r.NumArmies = armies;
            r.AttackTeammates = attackTeammates;
            return r;
        }

        public override string ToString()
        {
            return AttackTransfer + " by " + this.PlayerID + " from " + this.From + " to " + this.To + ", ByPercent=" + ByPercent + ", NumArmies = " + NumArmies;
        }


    }
}
