namespace WarLight.Shared.AI.Wunderwaffe.Bot.Cards
{
    public class Card
    {
        public CardTypes CardType;
        public CardInstanceIDType CardInstanceId { get; private set; }
        private bool playedByTeammate = false;

        public Card(CardTypes cardType, CardInstanceIDType cardInstanceId)
        {
            CardType = cardType;
            CardInstanceId = cardInstanceId;
        }

        public bool PlayedByTeammate
        {
            get
            {
                return playedByTeammate;
            }

            set
            {
                playedByTeammate = value;
            }
        }

    }
}
