using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderDiscard : GameOrder
    {
        public CardInstanceIDType CardInstanceID;

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Discards; }
        }

        public static GameOrderDiscard Create(PlayerIDType playerID, CardInstanceIDType cardInstanceID)
        {
            var r = new GameOrderDiscard();
            r.PlayerID = playerID;
            r.CardInstanceID = cardInstanceID;
            return r;
        }
    }
}
