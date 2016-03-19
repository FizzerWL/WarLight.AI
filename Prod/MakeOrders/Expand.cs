using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public abstract class Expand
    {
        public abstract void Go(int remainingUndeployed, bool highlyWeightedOnly);
        public abstract bool HelpExpansion(IEnumerable<TerritoryIDType> terrs, int armies, Action<TerritoryIDType, int> deploy);
    }
}
