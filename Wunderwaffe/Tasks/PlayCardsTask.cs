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

            foreach (ReinforcementCard reinforcementCard in state.CardsHandler.GetReinforcementCards())
            {
                moves.AddOrder(new BotOrderGeneric(GameOrderPlayCardReinforcement.Create(reinforcementCard.CardInstanceId, state.Me.ID)));
                state.MyIncome.FreeArmies += reinforcementCard.Armies;
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
            var teammatesOrders = state.TeammatesOrders.Values.Where(o => o.Orders != null).SelectMany(o => o.Orders).ToList();
            var ownOrders = moves.Convert();
            List<CardInstanceIDType> playedCards = teammatesOrders.OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID).Concat(teammatesOrders.OfType<GameOrderDiscard>().Select(o => o.CardInstanceID)).ToList();
            playedCards.AddRange(ownOrders.OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID).ToList());


            int numMustPlay = state.CardsMustPlay - playedCards.Count;

            foreach (var card in state.Cards)
            {
                if (numMustPlay > 0 && !playedCards.Contains(card.ID))
                {
                    moves.AddOrder(new BotOrderGeneric(GameOrderDiscard.Create(state.Me.ID, card.ID)));
                    numMustPlay--;
                }
            }

        }
    }
}
