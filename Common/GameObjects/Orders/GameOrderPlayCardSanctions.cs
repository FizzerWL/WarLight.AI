using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardSanctions : GameOrderPlayCard
    {
        public PlayerIDType SanctionedPlayerID;
        public static GameOrderPlayCardSanctions Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, PlayerIDType sanctionPlayer)
        {
            var o = new GameOrderPlayCardSanctions();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = (PlayerIDType)playerID;
            o.SanctionedPlayerID = sanctionPlayer;
            return o;
        }
        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.SanctionCards; }
        }

    }
}
