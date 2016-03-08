using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class CardInstance
    {
        public CardInstanceIDType ID;
        public CardIDType CardID;

        public override string ToString()
        {
            return GetType().Name + " " + ID;
        }
    }

    public class ReinforcementCardInstance : CardInstance
    {
        public int Armies;
    }
}
