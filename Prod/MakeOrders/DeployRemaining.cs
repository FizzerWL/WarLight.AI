using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Prod.MakeOrders
{
    static class DeployRemaining
    {

        public static void Go(BotMain bot)
        {
            //Must do locked before free, otherwise a free deployment could happen to go into a locked bonus and we'd come up short.
            DeployRemainingLockedArmies(bot);
            DeployRemainingFreeArmies(bot);
        }

        static void DeployRemainingLockedArmies(BotMain bot)
        {
            foreach (var deploy in bot.MakeOrders.IncomeTracker.RestrictedBonusProgress)
            {
                var count = deploy.TotalToDeploy - deploy.Deployed;
                if (count > 0)
                    Deploy(bot, "DeployRemainingLockedArmies", bot.Map.Bonuses[deploy.Restriction].Territories, count);
            }
        }

        static void DeployRemainingFreeArmies(BotMain bot)
        {
            var count = bot.MakeOrders.IncomeTracker.FreeArmiesUndeployed;
            if (count == 0)
                return;

            var ourTerritories = bot.Standing.Territories.Values.Where(o => o.OwnerPlayerID == bot.PlayerID).Select(o => o.ID).ToList();

            if (ourTerritories.Count == 0)
                return;

            if (bot.BorderTerritories.Any())
                Deploy(bot, "DeployRemainingFreeArmies borders", bot.BorderTerritories.Select(o => o.ID), count);
            else
                Deploy(bot, "DeployRemainingFreeArmies all", ourTerritories, count);
        }


        private static void Deploy(BotMain bot, string source, IEnumerable<TerritoryIDType> terrs, int armies)
        {
            var allMoves = bot.MakeOrders.DefendAttack.WeightedMoves;

            //Filter down unimportant ones.  Don't do this when there's only 1, since their weights get normallized and the smallest one will always be 1 even if it's super important
            if (allMoves.Count > 1)
                allMoves = allMoves.Where(o => o.HighestImportance > 10).ToList();

            foreach (var attackOrDefense in allMoves.OrderByDescending(o => o.HighestImportance))
            {
                var attacks = bot.MakeOrders.Orders.Orders.OfType<GameOrderAttackTransfer>().Where(o => o.From == attackOrDefense.From && o.To == attackOrDefense.To).ToList();

                if (attacks.Count > 0)
                {
                    Deploy(bot, source + " assisting attack", attacks[0].From, armies);
                    attacks[0].NumArmies = attacks[0].NumArmies.Add(new Armies(armies));
                    return;
                }

                if (attackOrDefense.DefenseImportance > attackOrDefense.OffenseImportance)
                {
                    Deploy(bot, source + " by defense weight", attackOrDefense.From, armies);
                    return;
                }
            }

            var expandWeights = bot.MakeOrders.Expand.AttackableNeutrals;

            foreach (var helpExpansionTo in terrs.SelectMany(o => bot.Map.Territories[o].ConnectedTo).Where(o => expandWeights.ContainsKey(o)).OrderByDescending(o => expandWeights[o].Weight))
            {
                var attack = bot.Orders.Orders.OfType<GameOrderAttackTransfer>().RandomOrDefault(o => o.To == helpExpansionTo);

                if (attack != null)
                {
                    Deploy(bot, source + " by expansion weight", attack.From, armies);
                    attack.NumArmies = attack.NumArmies.Add(new Armies(armies));
                    return;
                }
            }

            Deploy(bot, source + " random", terrs.Random(), armies);
        }

        private static void Deploy(BotMain bot, string source, TerritoryIDType terrID, int armies)
        {
            AILog.Log("DeployRemaining", source + " deploying " + armies + " to " + bot.TerrString(terrID));
            bot.Orders.Deploy(terrID, armies);
        }
    }
}
