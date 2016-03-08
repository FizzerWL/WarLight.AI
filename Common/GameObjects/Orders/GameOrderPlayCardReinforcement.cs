using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardReinforcement : GameOrderPlayCard
    {
        public static GameOrderPlayCardReinforcement Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID)
        {
            var o = new GameOrderPlayCardReinforcement();
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
