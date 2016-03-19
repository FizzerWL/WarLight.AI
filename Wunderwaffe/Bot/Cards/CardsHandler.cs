using System.Collections.Generic;
using System.Linq;


namespace WarLight.Shared.AI.Wunderwaffe.Bot.Cards
{
    public class CardsHandler
    {
        public List<Card> cards { get; private set; } = new List<Card>();
        private BotMain BotState;


        public CardsHandler(BotMain botState)
        {
            this.BotState = botState;
        }

        public List<ReinforcementCard> GetReinforcementCards()
        {
            return cards.Where(o => o.CardType == CardTypes.Reinforcement).Cast<ReinforcementCard>().ToList();
        }

        public List<Card> GetCards(CardTypes cardType)
        {
            return cards.Where(o => o.CardType == cardType).ToList();
        }


        public void initCards()
        {
            foreach (CardInstance cardInstance in BotState.Cards)
            {
                Card card = null;
                if (cardInstance.CardID == WarLight.Shared.AI.CardType.Reinforcement.CardID)
                {
                    int armies = cardInstance.As<ReinforcementCardInstance>().Armies;
                    card = new ReinforcementCard(CardTypes.Reinforcement, cardInstance.ID, armies);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Spy.CardID)
                {
                    card = new Card(CardTypes.Spy, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.EmergencyBlockade.CardID)
                {
                    card = new Card(CardTypes.EmergencyBlockade, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.OrderPriority.CardID)
                {
                    card = new Card(CardTypes.OrderPriority, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.OrderDelay.CardID)
                {
                    card = new Card(CardTypes.OrderDelay, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Airlift.CardID)
                {
                    card = new Card(CardTypes.Airlift, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Gift.CardID)
                {
                    card = new Card(CardTypes.Gift, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Diplomacy.CardID)
                {
                    card = new Card(CardTypes.Diplomacy, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Sanctions.CardID)
                {
                    card = new Card(CardTypes.Sanctions, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Reconnaissance.CardID)
                {
                    card = new Card(CardTypes.Reconnaissance, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Surveillance.CardID)
                {
                    card = new Card(CardTypes.Surveilance, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Blockade.CardID)
                {
                    card = new Card(CardTypes.Blockade, cardInstance.ID);
                }
                else if (cardInstance.CardID == WarLight.Shared.AI.CardType.Bomb.CardID)
                {
                    card = new Card(CardTypes.Bomb, cardInstance.ID);
                }
                cards.Add(card);
            }
        }



    }
}
