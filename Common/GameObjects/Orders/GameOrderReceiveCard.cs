using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderReceiveCard : GameOrder
    {
        public List<CardInstance> InstancesCreated;
        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.ReceiveCards; }
        }
    }

}
