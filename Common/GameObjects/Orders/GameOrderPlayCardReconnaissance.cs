using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardReconnaissance : GameOrderPlayCard
    {
        public TerritoryIDType TargetTerritory;
        public static GameOrderPlayCardReconnaissance Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, TerritoryIDType targetTerritory)
        {
            var o = new GameOrderPlayCardReconnaissance();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            o.TargetTerritory = targetTerritory;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.ReinforcementAndSpyCards; }
        }

    }
}
