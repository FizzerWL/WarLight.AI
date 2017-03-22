using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public class UtilizeSpareArmies
    {
        public GameOrderAttackTransfer Order;
        public int Available;
        public UtilizeSpareArmies(GameOrderAttackTransfer order, int available)
        {
            Order = order;
            Available = available;
        }

        public static void Go(BotMain bot)
        {
            var attacks = bot.Orders.Orders.OfType<GameOrderAttackTransfer>();

            foreach (var orders in attacks
                .Where(o => bot.Standing.Territories[o.From].OwnerPlayerID == bot.PlayerID)
                .Select(o => new UtilizeSpareArmies(o, bot.MakeOrders.GetArmiesAvailable(o.From)))
                .Where(o => o.Available > 0)
                .GroupBy(o => o.Order.From))
            {
                var order = bot.UseRandomness ? orders.Random() : orders.First();

                AILog.Log("UtilizeSpareArmies", "Adding " + order.Available + " available armies into attack from " + bot.TerrString(order.Order.From) + " to " + bot.TerrString(order.Order.To) + ", originally had " + order.Order.NumArmies.NumArmies);
                order.Order.NumArmies = order.Order.NumArmies.Add(new Armies(order.Available));
            }
        }
    }
}
