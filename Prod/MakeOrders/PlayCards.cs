using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            if (numMustPlay > 0)
                AILog.Log("PlayCards", "Must play " + numMustPlay + " cards, have " + bot.Cards.Count + ", teammate played " + cardsPlayedByTeammate.Count);

            var availableCards = bot.Cards.ToDictionary(o => o.ID, o => o);
            

            foreach (var card in bot.Cards)
            {
                if (cardsPlayedByTeammate.Contains(card.ID))
                {
                    //Teammate played it
                    availableCards.Remove(card.ID);
                    continue; 
                }

                Action<CardType, Func<BotMain, CardInstance, bool>> tryPlay = (cardType, playFn) =>
                {
                    if (card.CardID == cardType.CardID && playFn(bot, card))
                    {
                        availableCards.Remove(card.ID);
                        numMustPlay--;
                    }
                };

                tryPlay(CardType.Reinforcement, PlayReinforcementCard);
                tryPlay(CardType.Sanctions, PlaySanctionsCard);
                tryPlay(CardType.Bomb, PlayBombCard);
                tryPlay(CardType.Blockade, PlayBlockadeCard);
                tryPlay(CardType.Diplomacy, PlayDiplomacyCard);
            }

            while (numMustPlay > 0)
            {
                var card = availableCards.First().Value;
                AILog.Log("PlayCards", "Discarding card " + card + ", type=" + card.CardID);
                bot.Orders.AddOrder(GameOrderDiscard.Create(bot.PlayerID, card.ID));
                numMustPlay--;
                availableCards.Remove(card.ID);
            }
        }

        private static bool PlayBlockadeCard(BotMain bot, CardInstance card)
        {
            //Look for bonuses that we can't hold and should blockade
            foreach (var bonus in bot.Map.Bonuses.Values)
            {
                var grouped = bonus.Territories.GroupBy(o => bot.Standing.Territories[o].OwnerPlayerID).ToDictionary(o => o.Key, o => o);
                if (!grouped.ContainsKey(bot.PlayerID))
                    continue; //we're not in it
                if (grouped.ContainsKey(TerritoryStanding.NeutralPlayerID))
                    continue; //only complete bonuses -- if it's never been taken, don't blockade
                var opps = grouped.Keys.Where(o => bot.IsOpponent(o)).ToList();
                if (opps.Count == 0)
                    continue; //no opponents in it
                if (bonus.Territories.Any(t => bot.AvoidTerritories.Contains(t)))
                    continue; //already doing something here, perhaps already blockading it.
                var oppTerrs = opps.SelectMany(o => grouped[o].ToList()).ToHashSet(false);
                var friendlyTerrs = grouped.Where(o => bot.IsTeammateOrUs(o.Key)).SelectMany(o => o.Value.ToList()).ToList();
                var friendlyArmies = friendlyTerrs.Sum(o => bot.Standing.Territories[o].NumArmies.DefensePower);
                var enemyArmies = oppTerrs.Sum(o => ExpansionHelper.GuessNumberOfArmies(bot, o).AttackPower);
                var ratio = bot.UseRandomness ? RandomUtility.BellRandom(1, 3) : 2;
                if (friendlyArmies * ratio > enemyArmies)
                    continue;

                var armies = SharedUtility.Round(bot.EffectiveIncome.FreeArmies * (bot.UseRandomness ? RandomUtility.BellRandom(.1, .4) : .25));
                if (armies < 5)
                    armies = 5;

                var canBlockade = friendlyTerrs.Where(o =>
                    bot.Standing.Territories[o].OwnerPlayerID == bot.PlayerID
                    && bot.Map.Territories[o].ConnectedTo.Keys.None(t => oppTerrs.Contains(t))
                    && bot.Standing.Territories[o].NumArmies.SpecialUnits.Length == 0
                    && bot.Standing.Territories[o].NumArmies.NumArmies < armies * 2
                    ).ToList();
                if (canBlockade.Count == 0)
                    continue;
                var blockade = bot.UseRandomness ? canBlockade.Random() : canBlockade.First();
                var deploy = Math.Max(0, armies - bot.Standing.Territories[blockade].NumArmies.NumArmies);

                if (!bot.Orders.TryDeploy(blockade, deploy))
                    continue;

                AILog.Log("PlayCards", "Blockading " + bot.TerrString(blockade) + " with " + armies + " (had to deploy " + deploy + ")");

                bot.Orders.AddOrder(GameOrderPlayCardBlockade.Create(card.ID, bot.PlayerID, blockade));
                bot.AvoidTerritories.Add(blockade);
                return true;
            }

            return false;
        }

        private static bool PlayBombCard(BotMain bot, CardInstance card)
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
            if (weights.Count == 0)
                return false;

            var bomb = bot.UseRandomness ? RandomUtility.WeightedRandom(weights.Keys, o => weights[o]) : weights.OrderByDescending(o => o.Value).First().Key;
            AILog.Log("PlayCards", "Bombing " + bot.TerrString(bomb));
            bot.Orders.AddOrder(GameOrderPlayCardBomb.Create(card.ID, bot.PlayerID, bomb));
            return true;
        }

        private static bool PlaySanctionsCard(BotMain bot, CardInstance card)
        {
            var canSanction = bot.Players.Values.Where(o => o.State == GamePlayerState.Playing && bot.IsOpponent(o.ID)).Select(o => o.ID).ToList();
            if (canSanction.Count == 0)
                return false;

            var sanction = bot.UseRandomness ? RandomUtility.WeightedRandom(canSanction, o => bot.WeightedNeighbors[o]) : canSanction.OrderByDescending(o => bot.WeightedNeighbors[o]).First();
            AILog.Log("PlayCards", "Sanctioning " + sanction);
            bot.Orders.AddOrder(GameOrderPlayCardSanctions.Create(card.ID, bot.PlayerID, sanction));
            return true;
        }

        private static bool PlayDiplomacyCard(BotMain bot, CardInstance card)
        {
            var canDip = bot.Players.Values.Where(o => 
                o.State == GamePlayerState.Playing  //only diplomacy people in the game
                && bot.IsOpponent(o.ID)  //only diplomacy opponents
                && bot.Orders.Orders.Concat(bot.Standing.ActiveCards.Select(z => (GameOrder)z.Card)).OfType<GameOrderPlayCardDiplomacy>().None(z => z.AffectsPlayer(o.ID) && z.AffectsPlayer(bot.PlayerID)) //don't diplomacy someone we're already in diplomacy with, or that we already played a diplomacy card on this turn
                ).ToList();
            if (canDip.Count == 0)
                return false;

            var dip = bot.UseRandomness ? RandomUtility.WeightedRandom(canDip, o => bot.WeightedNeighbors[o.ID]) : canDip.OrderByDescending(o => bot.WeightedNeighbors[o.ID]).First();
            AILog.Log("PlayCards", "Playing a diplomacy card between myself and " + dip);
            bot.Orders.AddOrder(GameOrderPlayCardDiplomacy.Create(card.ID, bot.PlayerID, bot.PlayerID, dip.ID));
            return true;
        }

        private static bool PlayReinforcementCard(BotMain bot, CardInstance card)
        {
            var numArmies = card.As<ReinforcementCardInstance>().Armies;
            AILog.Log("PlayCards", "Playing reinforcement card for " + numArmies);
            bot.Orders.AddOrder(GameOrderPlayCardReinforcement.Create(card.ID, bot.PlayerID));
            bot.Orders.ArmiesFromReinforcementCards += numArmies;
            bot.EffectiveIncome.FreeArmies += numArmies;
            return true;
        }
        
    }
}
