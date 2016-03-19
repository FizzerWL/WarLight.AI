using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;
using WarLight.Shared.AI.Wunderwaffe.Bot.Cards;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    public static class PlayCardsTask
    {
        public static void PlayCards(BotMain state, Moves moves)
        {




            if (state.Me.Team != PlayerInvite.NoTeam && state.Players.Values.Any(o => state.IsTeammate(o.ID) && !o.IsAIOrHumanTurnedIntoAI && o.State == GamePlayerState.Playing && !o.HasCommittedOrders))
                return; //If there are any humans on our team that have yet to take their turn, do not play cards.

            //For now, just play all reinforcement cards, and discard if we must use any others.
            var teammatesOrders = state.TeammatesOrders.Values.Where(o => o.Orders != null).SelectMany(o => o.Orders);
            var cardsPlayedByTeammate = teammatesOrders.OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID).Concat(teammatesOrders.OfType<GameOrderDiscard>().Select(o => o.CardInstanceID)).ToHashSet(true);

            int numMustPlay = state.CardsMustPlay;


            foreach (ReinforcementCard reinforcementCard in state.CardsHandler.GetReinforcementCards())
            {
                moves.AddOrder(new BotOrderGeneric(GameOrderPlayCardReinforcement.Create(reinforcementCard.CardInstanceId, state.Me.ID)));
                state.MyIncome.FreeArmies += reinforcementCard.Armies;
                numMustPlay--;
            }




            foreach (var card in state.Cards)
            {
                if (cardsPlayedByTeammate.Contains(card.ID))
                {
                    continue;
                }

                //if (card is ReinforcementCardInstance)
                //{
                //    moves.AddOrder(new BotOrderGeneric(GameOrderPlayCardReinforcement.Create(card.ID, state.Me.ID)));
                //    state.MyIncome.FreeArmies += card.As<ReinforcementCardInstance>().Armies;
                //    numMustPlay--;
                //}
                //else
                if (numMustPlay > 0) //For now, just discard all non-reinforcement cards if we must use the card
                {
                    moves.AddOrder(new BotOrderGeneric(GameOrderDiscard.Create(state.Me.ID, card.ID)));
                    numMustPlay--;
                }
            }

        }
    }
}
