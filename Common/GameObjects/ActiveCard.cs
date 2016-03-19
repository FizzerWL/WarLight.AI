using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class ActiveCard
    {
        public int ExpiresAfterTurn;
        public GameOrderPlayCard Card;

        public ActiveCard Clone()
        {
            var clone = new ActiveCard();
            clone.ExpiresAfterTurn = ExpiresAfterTurn;
            clone.Card = this.Card;
            return clone;
        }
    }
}