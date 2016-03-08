using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardAbandon : GameOrderPlayCard
    {
        public TerritoryIDType TargetTerritoryID;
        public static GameOrderPlayCardAbandon Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, TerritoryIDType targetTerritoryID)
        {
            var o = new GameOrderPlayCardAbandon();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = (PlayerIDType)playerID;
            o.TargetTerritoryID = targetTerritoryID;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.AbandonCards; }
        }

    }
}
