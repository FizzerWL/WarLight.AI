using System.Collections.Generic;
using System.Linq;


namespace WarLight.Shared.AI.Wunderwaffe.Bot.Cards
{
    public class CardsHandler
    {
        public List<Card> cards = new List<Card>();
        private BotMain BotState;


        public CardsHandler(BotMain botState)
        {
            this.BotState = botState;
        }

        public List<Card> GetCards(CardTypes cardType)
        {
            return cards.Where(o => o.CardType == cardType && !o.PlayedByTeammate).ToList();
        }


        public void initCards()
        {
            foreach (CardInstance cardInstance in BotState.Cards)
            {
                AILog.Log("CardsHandler", "Have card " + cardInstance.ID + " of type " + cardInstance.CardID);
                Card card = null;
                if (cardInstance.CardID == CardType.Reinforcement.CardID)
                {
                    int armies = cardInstance.As<ReinforcementCardInstance>().Armies;
                    card = new ReinforcementCard(CardTypes.Reinforcement, cardInstance.ID, armies);
                }
                else if (cardInstance.CardID == CardType.Spy.CardID)
                {
                    card = new Card(CardTypes.Spy, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.EmergencyBlockade.CardID)
                {
                    card = new Card(CardTypes.EmergencyBlockade, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.OrderPriority.CardID)
                {
                    card = new Card(CardTypes.OrderPriority, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.OrderDelay.CardID)
                {
                    card = new Card(CardTypes.OrderDelay, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Airlift.CardID)
                {
                    card = new Card(CardTypes.Airlift, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Gift.CardID)
                {
                    card = new Card(CardTypes.Gift, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Diplomacy.CardID)
                {
                    card = new Card(CardTypes.Diplomacy, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Sanctions.CardID)
                {
                    card = new Card(CardTypes.Sanctions, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Reconnaissance.CardID)
                {
                    card = new Card(CardTypes.Reconnaissance, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Surveillance.CardID)
                {
                    card = new Card(CardTypes.Surveilance, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Blockade.CardID)
                {
                    card = new Card(CardTypes.Blockade, cardInstance.ID);
                }
                else if (cardInstance.CardID == CardType.Bomb.CardID)
                {
                    card = new Card(CardTypes.Bomb, cardInstance.ID);
                }

                if (BotState.CardsPlayedByTeammates.Contains(cardInstance.ID))
                    card.PlayedByTeammate = true;

                cards.Add(card);
            }
        }



    }
}
