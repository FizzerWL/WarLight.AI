using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardSurveillance : GameOrderPlayCard
    {
        public BonusIDType TargetBonus;

        public static GameOrderPlayCardSurveillance Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, BonusIDType targetBonus)
        {
            var o = new GameOrderPlayCardSurveillance();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            o.TargetBonus = targetBonus;
            return o;
        }
        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.ReinforcementAndSpyCards; }
        }

    }
}
