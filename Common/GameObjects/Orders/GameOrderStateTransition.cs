using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderStateTransition : GameOrder
    {
        public GamePlayerState NewState;

        public override TurnPhase? OccursInPhase
        {
            get { return null; }
        }

    }
}
