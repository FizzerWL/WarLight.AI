using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Prod.MakeOrders
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

            foreach (var o in attacks.Select(o => new UtilizeSpareArmies(o, bot.MakeOrders.GetArmiesAvailable(o.From)))
                .Where(o => o.Available > 0)
                .GroupBy(o => o.Order.From)
                .Select(o => o.Random()))
            {
                AILog.Log("UtilizeSpareArmies", "Adding " + o.Available + " available armies into attack from " + bot.TerrString(o.Order.From) + " to " + bot.TerrString(o.Order.To) + ", originally had " + o.Order.NumArmies.NumArmies);
                o.Order.NumArmies = o.Order.NumArmies.Add(new Armies(o.Available));
            }
        }
    }
}
