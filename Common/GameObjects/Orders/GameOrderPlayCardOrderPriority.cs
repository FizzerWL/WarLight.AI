using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardOrderPriority : GameOrderPlayCard
    {
        public static GameOrderPlayCardOrderPriority Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID)
        {
            var o = new GameOrderPlayCardOrderPriority();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.ReinforcementAndSpyCards; }
        }
    }
}
