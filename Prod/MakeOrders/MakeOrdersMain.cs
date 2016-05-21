using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public class MakeOrdersMain
    {
        public BotMain Bot;
        public OrdersManager Orders;
        public PlayerIncomeTracker IncomeTracker;
        public Expand Expand;
        public DefendAttack DefendAttack;

        public MakeOrdersMain(BotMain bot)
        {
            this.Bot = bot;
            this.Orders = new OrdersManager(Bot);
            this.IncomeTracker = new PlayerIncomeTracker(Bot.EffectiveIncome, Bot.Map);
        }

        public List<GameOrder> Go()
        {
            this.Expand = Bot.Settings.MultiAttack ? (Expand)new MultiAttackExpand(Bot) : new ExpandNormal(Bot);
            this.DefendAttack = new DefendAttack(Bot);

            PlayCards.Go(Bot);

            //Special units movement
            SpecialUnits.Go(Bot);

            //Ensure teammates defragment bonuses
            ResolveTeamBonuses.Go(Bot);

            //Expand into good opportunities.  
            Expand.Go(IncomeTracker.RemainingUndeployed, true);

            //Defend/attack
            DefendAttack.Go(IncomeTracker.RemainingUndeployed);

            //Expand into anything remaining
            Expand.Go(IncomeTracker.RemainingUndeployed, false);

            //If there's still remaining income, deploy it
            DeployRemaining.Go(Bot);

            //Move any unused landlocked armies towards a border
            MoveLandlockedUp.Go(Bot);

            //If we're not using any armies, utilize them
            UtilizeSpareArmies.Go(Bot);

            //Verify we've deployed all armies
            //VerifyIncomeAccurate();

            Assert.Fatal(IncomeTracker.FullyDeployed, "Not fully deployed");

            return Orders.Orders;
        }


        /// <summary>
        /// Asserts that the orders built so far deploy all armies the AI is receiving
        /// </summary>
        //private void VerifyIncomeAccurate()
        //{

        //	var actualIncome = GamePlayerReference.IncomeFromStanding(Orders.OfType<GameOrderPlayCardReinforcement>().Select(o => ((ReinforcementCardInstance)Game.LatestTurnStanding_ReadOnly.FindCard(o.CardInstanceID)).Armies).SumInts(), Game.LatestTurnStanding_ReadOnly, false, false);
        //	var ordersDeploy = Orders.OfType<GameOrderDeploy>().Select(o => o.NumArmies).SumInts();

        //	if (actualIncome.Total != ordersDeploy)
        //	{
        //		//Throw some details in the error for debugging
        //		var sb = new StringBuilder();
        //		sb.AppendLine("Order incomes inaccurate. ActualIncome = " + actualIncome + ", OrdersDeploy=" + ordersDeploy + ", NumOrders=" + Orders.Count);
        //		foreach (var order in Orders)
        //			sb.AppendLine("Order: " + order.ToString());
        //		Assert.Fatal(false, sb.ToString());

        //	}
        //}



        /// <summary>
        /// Tells us how many armies of ours on our territory that we haven't committed to another action
        /// </summary>
        /// <param name="terrID"></param>
        /// <returns></returns>
        public int GetArmiesAvailable(TerritoryIDType terrID)
        {
            Assert.Fatal(Bot.Standing.Territories[terrID].OwnerPlayerID == Bot.PlayerID, "Not owned by us");

            if (Orders.Orders.OfType<GameOrderPlayCardAbandon>().Any(o => o.TargetTerritoryID == terrID))
                return 0; //we're abandoning it, no armies are available.

            int armies = Bot.Standing.Territories[terrID].NumArmies.NumArmies;

            //Add in armies we deployed
            armies += Orders.Orders.OfType<GameOrderDeploy>().Where(o => o.DeployOn == terrID).Sum(o => o.NumArmies);

            //Subtract armies we've attacked with
            foreach (var attack in Orders.Orders.OfType<GameOrderAttackTransfer>().Where(o => o.From == terrID))
            {
                if (attack.ByPercent)
                    return 0; //in multi-attack, we attack by percentages.  Just assume they're all used
                else
                    armies -= attack.NumArmies.NumArmies;
            }

            //Subtract airlift out's
            armies -= Orders.Orders.OfType<GameOrderPlayCardAirlift>().Where(o => o.FromTerritoryID == terrID).Sum(o => o.GetArmies().NumArmies);

            //Subtract 1, since one must remain
            if (Bot.Settings.OneArmyStandsGuard)
                armies -= 1;

            return Math.Max(0, armies);
        }

    }
}
