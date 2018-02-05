using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    static class DeployRemaining
    {

        public static void Go(BotMain bot)
        {
            //Must do locked before free, otherwise a free deployment could happen to go into a locked bonus and we'd come up short.
            DeployRemainingLockedArmies(bot);
            DeployRemainingFreeArmies(bot);
        }

        /// <summary>
        /// Used for LD games
        /// </summary>
        /// <param name="bot"></param>
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
            if (count <= 0)
                return;
            
            var ourTerritories = bot.Standing.Territories.Values.Where(o => o.OwnerPlayerID == bot.PlayerID).Select(o => o.ID).ToList();

            if (ourTerritories.Count == 0)
                return;

            if (bot.BorderTerritories.Any())
                Deploy(bot, "DeployRemainingFreeArmies borders", bot.BorderTerritories.Select(o => o.ID), count);
            else
                Deploy(bot, "DeployRemainingFreeArmies all", ourTerritories, count);
        }

        /// <summary>
        /// Figure out which one of the territories in terrs most needs deployments, and deploy there.  Will also add to attacks from that territory if necessary.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="source">Just for debugging</param>
        /// <param name="terrs"></param>
        /// <param name="armies">Number of armies to deploy</param>
        private static void Deploy(BotMain bot, string source, IEnumerable<TerritoryIDType> terrs, int armies)
        {
            var canDeployOn = terrs.Where(o => bot.AvoidTerritories.Contains(o) == false).ToList();

            if (canDeployOn.Count == 0)
                canDeployOn = terrs.ToList(); //if we can't deploy otherwise, ignore AvoidTerritories
            Assert.Fatal(canDeployOn.Count > 0, "No deploy options");

            if (!bot.UseRandomness)
                DeployExact(bot, source, canDeployOn, armies);
            else
            {
                //In randomness, break the deployment up into chunks (we could just call it on every army individually, but that's inefficient when our income gets very high)
                var chunkSize = Math.Max(1, (int)(armies / 10));
                var armiesDone = 0;
                while (armiesDone < armies)
                {
                    var deploy = Math.Min(armies - armiesDone, chunkSize);
                    DeployExact(bot, source, canDeployOn, deploy);
                    armiesDone += deploy;
                }
            }
        }

        private static void DeployExact(BotMain bot, string source, IEnumerable<TerritoryIDType> terrs, int armies)
        {
            var allAttacksOrDefenses = bot.MakeOrders.DefendAttack.WeightedMoves.Where(o => terrs.Contains(o.From)).ToList();

            if (!bot.UseRandomness) 
                allAttacksOrDefenses = allAttacksOrDefenses.OrderByDescending(o => o.HighestImportance).ToList();

            while (allAttacksOrDefenses.Count > 0)
            {
                PossibleAttack attackOrDefense;
                if (bot.UseRandomness)
                {
                    var i = RandomUtility.WeightedRandomIndex(allAttacksOrDefenses, o => o.HighestImportance);
                    attackOrDefense = allAttacksOrDefenses[i];
                    allAttacksOrDefenses.RemoveAt(i);
                }
                else
                {
                    attackOrDefense = allAttacksOrDefenses[0];
                    allAttacksOrDefenses.RemoveAt(0);
                }

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

            if (bot.MakeOrders.Expand.HelpExpansion(terrs, armies, (terrID, a) => Deploy(bot, source + " from TryHelpExpansion", terrID, a)))
                return;

            if (bot.UseRandomness)
                Deploy(bot, source + " random", terrs.Random(), armies);
            else
                Deploy(bot, source + " first", terrs.First(), armies);
        }

        private static void Deploy(BotMain bot, string source, TerritoryIDType terrID, int armies)
        {
            AILog.Log("DeployRemaining", source + " deploying " + armies + " to " + bot.TerrString(terrID));
            bot.Orders.Deploy(terrID, armies, true);
        }
    }
}
