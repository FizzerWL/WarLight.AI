using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;
using WarLight.Shared.AI.Wunderwaffe.Bot.Cards;
using System.Collections.Generic;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    public static class PlayCardsTask
    {
        public static void PlayCardsBeginTurn(BotMain state, Moves moves)
        {
            //If there are any humans on our team that have yet to take their turn, do not play cards.
            if (state.Me.Team != PlayerInvite.NoTeam && state.Players.Values.Any(o => state.IsTeammate(o.ID) && !o.IsAIOrHumanTurnedIntoAI && o.State == GamePlayerState.Playing && !o.HasCommittedOrders))
            {
                return;
            }

            foreach (var reinforcementCard in state.CardsHandler.GetCards(CardTypes.Reinforcement))
            {
                var numArmies = reinforcementCard.As<ReinforcementCard>().Armies;
                AILog.Log("PlayCardsTask", "Playing reinforcement card " + reinforcementCard.CardInstanceId + " for " + numArmies + " armies");
                moves.AddOrder(new BotOrderGeneric(GameOrderPlayCardReinforcement.Create(reinforcementCard.CardInstanceId, state.Me.ID)));
                state.MyIncome.FreeArmies += numArmies;
            }
        }

        public static void DiscardCardsEndTurn(BotMain state, Moves moves)
        {

            //If there are players on our team that have yet to take their turn, do not discard cards
            if (state.Me.Team != PlayerInvite.NoTeam && state.Players.Values.Any(o => state.IsTeammate(o.ID) && o.State == GamePlayerState.Playing && !o.HasCommittedOrders))
            {
                return;
            }

            // Discard as many cards as needed
            var cardsWePlayed = moves.Convert().OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID).ToHashSet(true);
            var cardsPlayedByAnyone = state.CardsPlayedByTeammates.Concat(cardsWePlayed).ToHashSet(true);

            int numMustPlay = state.CardsMustPlay;

            foreach (var card in state.Cards)
            {
                if (numMustPlay > 0 && !cardsPlayedByAnyone.Contains(card.ID))
                {
                    AILog.Log("PlayCardsTask", "Discarding card " + card.ID);
                    moves.AddOrder(new BotOrderGeneric(GameOrderDiscard.Create(state.Me.ID, card.ID)));
                    numMustPlay--;
                }
            }

        }
    }
}
