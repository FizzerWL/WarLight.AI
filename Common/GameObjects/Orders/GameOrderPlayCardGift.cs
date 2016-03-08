using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardGift : GameOrderPlayCard
    {
        public TerritoryIDType TerritoryID;
        public PlayerIDType GiftTo;

        public static GameOrderPlayCardGift Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, TerritoryIDType terrID, PlayerIDType giftTo)
        {
            var o = new GameOrderPlayCardGift();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            o.TerritoryID = terrID;
            o.GiftTo = giftTo;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Gift; }
        }

    }
}
