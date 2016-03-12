using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class CardType
    {
        public CardIDType CardID;

        public CardType(int id)
        {
            this.CardID = (CardIDType)id;
        }

        public static CardType Reinforcement = new CardType(1);
        public static CardType Spy = new CardType(2);
        public static CardType EmergencyBlockade = new CardType(3);
        public static CardType OrderPriority = new CardType(4);
        public static CardType OrderDelay = new CardType(5);
        public static CardType Airlift = new CardType(6);
        public static CardType Gift = new CardType(7);
        public static CardType Diplomacy = new CardType(8);
        public static CardType Sanctions = new CardType(9);
        public static CardType Reconnaissance = new CardType(10);
        public static CardType Surveillance = new CardType(11);
        public static CardType Blockade = new CardType(12);
        public static CardType Bomb = new CardType(13);
    }
}
