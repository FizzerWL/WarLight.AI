using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Wunderwaffe.Bot.Cards
{
    public class Card
    {
        public CardTypes CardType;
        public CardInstanceIDType CardInstanceId { get; private set; }
        private bool played = false;
        private bool playedByTeammate = false;

        public Card(CardTypes cardType, CardInstanceIDType cardInstanceId)
        {
            CardType = cardType;
            CardInstanceId = cardInstanceId;
        }

        public bool Played
        {
            get
            {
                return played;
            }

            set
            {
                played = value;
            }
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
