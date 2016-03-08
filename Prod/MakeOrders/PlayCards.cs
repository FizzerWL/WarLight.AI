using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    static class PlayCards
    {

        public static void Go(BotMain bot)
        {
            if (bot.GamePlayerReference.Team != PlayerInvite.NoTeam && bot.Players.Values.Any(o => o.ID != bot.PlayerID && !o.IsAIOrHumanTurnedIntoAI && o.Team == bot.GamePlayerReference.Team && o.State == GamePlayerState.Playing && !o.HasCommittedOrders))
                return; //If there are any humans on our team that have yet to take their turn, do not play cards.

            //For now, just play all reinforcement cards, and discard if we must use any others.

            var cardsPlayedByTeammate =
                bot.TeammatesSubmittedOrders.OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID)
                .Concat(bot.TeammatesSubmittedOrders.OfType<GameOrderDiscard>().Select(o => o.CardInstanceID))
                .ToHashSet(true);

            int numMustPlay = bot.CardsMustPlay;

            foreach (var card in bot.Cards)
            {
                if (cardsPlayedByTeammate.Contains(card.ID))
                    continue; //Teammate played it

                if (card is ReinforcementCardInstance)
                {
                    var numArmies = card.As<ReinforcementCardInstance>().Armies;
                    AILog.Log("PlayCards", "Playing reinforcement card for " + numArmies);
                    bot.Orders.AddOrder(GameOrderPlayCardReinforcement.Create(card.ID, bot.PlayerID));
                    bot.EffectiveIncome.FreeArmies += numArmies;
                    numMustPlay--;
                }
                else if (numMustPlay > 0) //For now, just discard all non-reinforcement cards if we must use the card
                {
                    AILog.Log("PlayCards", "Must discard card " + card);
                    bot.Orders.AddOrder(GameOrderDiscard.Create(bot.PlayerID, card.ID));
                    numMustPlay--;
                }
            }
        }
    }
}
