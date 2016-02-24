using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI
{
    public class ActiveCardWoreOff : GameOrder
    {
        public CardInstanceIDType CardInstanceID;

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.CardsWearOff; }
        }
    }
}
