using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardOrderDelay : GameOrderPlayCard
    {
        public static GameOrderPlayCardOrderDelay Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID)
        {
            var o = new GameOrderPlayCardOrderDelay();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Attacks; }
        }

    }
}
