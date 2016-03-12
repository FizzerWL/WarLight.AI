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

            var cardsPlayedByTeammate =
                bot.TeammatesSubmittedOrders.OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID)
                .Concat(bot.TeammatesSubmittedOrders.OfType<GameOrderDiscard>().Select(o => o.CardInstanceID))
                .ToHashSet(true);

            int numMustPlay = bot.CardsMustPlay;

            foreach (var card in bot.Cards)
            {
                if (cardsPlayedByTeammate.Contains(card.ID))
                    continue; //Teammate played it

                if (card.CardID == CardType.Reinforcement.CardID)
                {
                    var numArmies = card.As<ReinforcementCardInstance>().Armies;
                    AILog.Log("PlayCards", "Playing reinforcement card for " + numArmies);
                    bot.Orders.AddOrder(GameOrderPlayCardReinforcement.Create(card.ID, bot.PlayerID));
                    bot.EffectiveIncome.FreeArmies += numArmies;
                    numMustPlay--;
                }
                else if (card.CardID == CardType.Sanctions.CardID)
                {
                    var sanction = bot.UseRandomness ? RandomUtility.WeightedRandom(bot.WeightedNeighbors.Keys, o => bot.WeightedNeighbors[o]) : bot.WeightedNeighbors.OrderByDescending(o => o.Value).First().Key;
                    AILog.Log("PlayCards", "Sanctioning " + sanction);
                    bot.Orders.AddOrder(GameOrderPlayCardSanctions.Create(card.ID, bot.PlayerID, sanction));
                }
                else if (card.CardID == CardType.Bomb.CardID)
                {
                    var allBombableEnemyTerritories = bot.Standing.Territories.Values
                        .Where(o => o.OwnerPlayerID == bot.PlayerID)
                        .SelectMany(o => bot.Map.Territories[o.ID].ConnectedTo.Keys)
                        .Distinct()
                        .Select(o => bot.Standing.Territories[o])
                        .Where(o => bot.IsOpponent(o.OwnerPlayerID) && o.NumArmies.Fogged == false)
                        .ToList();

                    var minArmies = !bot.UseRandomness ? bot.BaseIncome.Total * 2 : SharedUtility.Round(bot.BaseIncome.Total * RandomUtility.BellRandom(1, 3));

                    var weights = allBombableEnemyTerritories.Where(o => o.NumArmies.NumArmies > minArmies).ToDictionary(o => o.ID, o => o.NumArmies.NumArmies - minArmies);
                    if (weights.Count > 0)
                    {
                        var bomb = bot.UseRandomness ? RandomUtility.WeightedRandom(weights.Keys, o => weights[o]) : weights.OrderByDescending(o => o.Value).First().Key;
                        bot.Orders.AddOrder(GameOrderPlayCardBomb.Create(card.ID, bot.PlayerID, bomb));
                    }
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
