using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardDiplomacy : GameOrderPlayCard
    {
        public PlayerIDType PlayerOne;
        public PlayerIDType PlayerTwo;

        public static GameOrderPlayCardDiplomacy Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, PlayerIDType playerOne, PlayerIDType playerTwo)
        {
            var o = new GameOrderPlayCardDiplomacy();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = (PlayerIDType)playerID;
            o.PlayerOne = playerOne;
            o.PlayerTwo = playerTwo;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.DiplomacyCards; }
        }

        public bool AffectsPlayer(PlayerIDType p)
        {
            return p == PlayerOne || p == PlayerTwo;
        }

    }
}
