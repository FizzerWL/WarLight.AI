using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    static class Commanders
    {
        /// <summary>
        /// Runs the commander away from opponents
        /// </summary>
        public static void Go(BotMain bot)
        {
            if (!bot.Settings.Commanders)
                return;

            var cmdrTerritory = bot.Standing.Territories.Values.SingleOrDefault(o => o.NumArmies.SpecialUnits.OfType<Commander>().Any(t => t.OwnerID == bot.PlayerID));

            if (cmdrTerritory == null)
                return;

            //Consider this territory and all adjacent territories.  Which is the furthest from any enemy?
            var terrDistances = bot.Map.Territories[cmdrTerritory.ID].ConnectedTo.Keys.ConcatOne(cmdrTerritory.ID)
                .Where(o => bot.Standing.Territories[o].OwnerPlayerID == bot.PlayerID || bot.Standing.Territories[o].NumArmies.DefensePower <= 4) //don't go somewhere that's defended heavily
                .ToDictionary(o => o, o => bot.DistanceFromEnemy(o));

            var sorted = terrDistances.OrderByDescending(o => o.Value).ToList();
            sorted.RemoveWhere(o => o.Value < sorted[0].Value);

            var runTo = bot.UseRandomness ? sorted.Random().Key : sorted[0].Key;

            if (runTo == cmdrTerritory.ID)
                return; //already there

            AILog.Log("Commanders", "Moving commander from " + bot.TerrString(cmdrTerritory.ID) + " to " + bot.TerrString(runTo));
            bot.Orders.AddAttack(cmdrTerritory.ID, runTo, AttackTransferEnum.AttackTransfer, cmdrTerritory.NumArmies.NumArmies, false);
        }
    }
}
