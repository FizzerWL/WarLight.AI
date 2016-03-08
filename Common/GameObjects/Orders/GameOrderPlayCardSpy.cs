using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardSpy : GameOrderPlayCard
    {
        public PlayerIDType TargetPlayerID;

        public static GameOrderPlayCardSpy Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, PlayerIDType targetPlayerID)
        {
            var o = new GameOrderPlayCardSpy();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            o.TargetPlayerID = targetPlayerID;
            return o;
        }
        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.ReinforcementAndSpyCards; }
        }
    }
}
