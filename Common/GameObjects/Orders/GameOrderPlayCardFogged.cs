using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardFogged : GameOrderPlayCard
    {
        public static GameOrderPlayCardFogged Create(PlayerIDType pid, CardInstanceIDType cid)
        {
            var ret = new GameOrderPlayCardFogged();
            ret.PlayerID = pid;
            ret.CardInstanceID = cid;
            return ret;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return null; }
        }

    }
}
