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

            this.UpdateEffectiveIncome();

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

            //Build cities
            BuildCities();

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

            //Verify we're fully deployed, except in commerce games where it's OK to under-deploy
            if (!Bot.Settings.CommerceGame)
                Assert.Fatal(IncomeTracker.FullyDeployed, "Not fully deployed");

            return Orders.Orders;
        }


        /// <summary>
        /// Fixes EffectiveIncome based on what we can afford in commerce games
        /// </summary>
        private void UpdateEffectiveIncome()
        {
            if (!Bot.Settings.CommerceGame)
                return;

            var income = Bot.EffectiveIncome;

            var goldForArmies = Bot.Standing.NumResources(Bot.PlayerID, ResourceType.Gold);

            //subtract what we've spent if we spent gold on something else
            if (Orders.HasPurchaseOrder) //avoid making it if it doesn't exist
                goldForArmies -= Orders.PurchaseOrder.Cost(ResourceType.Gold, Bot.Settings, 0, Bot.Standing);

            var armiesCanAfford = Bot.Settings.HowManyArmiesCanAfford(goldForArmies) + Orders.ArmiesFromReinforcementCards;


            //In a commerce game, we may not be able to deploy armies to all of our territories.  Reduce our effective income to what we can afford.
            while (income.Total > armiesCanAfford)
            {
                //First try takimg away bonus-restricted income, as free armies are more valuable.

                var bonusesCanRemove = income.BonusRestrictions.Keys.Where(bonusID => {
                    if (!income.BonusRestrictions.ContainsKey(bonusID))
                        return false;
                    int i = income.BonusRestrictions[bonusID];
                    if (i <= 0)
                        return false;
                    return i > this.IncomeTracker.ArmiesDeployedToBonus(bonusID);
                }).ToList();

                if (bonusesCanRemove.Count > 0)
                {

                    var bonusID = Bot.UseRandomness ? bonusesCanRemove.Random() : bonusesCanRemove.OrderBy(o => (int)o).First();
                    if (income.BonusRestrictions[bonusID] == 1)
                        income.BonusRestrictions.Remove(bonusID);
                    else
                        income.BonusRestrictions[bonusID] = income.BonusRestrictions[bonusID] - 1;
                }
                else
                {
                    //Take away free armies
                    income.FreeArmies -= income.Total - armiesCanAfford;
                    Assert.Fatal(income.FreeArmies >= 0, "FreeArmies went negative");
                }
            }

            //It's also possible we can afford more armies than our income.  Add those to free armies (TODO: Can we do this in LD?)
            if (income.Total < armiesCanAfford)
            {
                income.FreeArmies += armiesCanAfford - income.Total;

                //In LD, increase armies but not to a value higher than LD allows
                if (Bot.Settings.LocalDeployments && income.FreeArmies > Bot.BaseIncome.FreeArmies)
                    income.FreeArmies = Bot.BaseIncome.FreeArmies;
            }
        }


        private void BuildCities()
        {
            if (Bot.Settings.CommerceGame == false || Bot.Settings.CommerceCityBaseCost.HasValue == false)
                return; //can't build cities

            var totalGold = Bot.Standing.NumResources(Bot.PlayerID, ResourceType.Gold);
            var spentGold = Bot.Settings.CostOfBuyingArmies(IncomeTracker.TotalArmiesDeployed);
            var maxPercent = !Bot.UseRandomness ? 0.5 : RandomUtility.BellRandom(0, 0.9);
            int goldLeftToSpendOnCities = Math.Min(totalGold - spentGold, SharedUtility.Round(totalGold * maxPercent)); //limit our cities to about half our gold to ensure we don't over-build

            AILog.Log("BuildCities", "totalGold=" + totalGold + " spentGold=" + spentGold + " goldToSpendOnCities=" + goldLeftToSpendOnCities + " maxPercent=" + maxPercent);

            if (goldLeftToSpendOnCities < Bot.Settings.CommerceCityBaseCost.Value)
                return; //can't even afford one city

            //randomize the safe range.  This makes it 
            int acceptableRangeFromOpponent = !Bot.UseRandomness ? 4 : SharedUtility.Round(RandomUtility.BellRandom(2, 6));

            var eligibleTerritories = Bot.TerritoriesNotNearOpponent(acceptableRangeFromOpponent);
            eligibleTerritories.RemoveAll(Bot.AvoidTerritories);

            var numCitiesOn = eligibleTerritories.ToDictionary(o => o, o => Bot.Standing.Territories[o].NumStructures(StructureType.City));

            //while we might be able to afford a city...
            while (goldLeftToSpendOnCities > Bot.Settings.CommerceCityBaseCost.Value)
            {
                var fewestCities = numCitiesOn.Values.Min();
                var cheapestCityCost = fewestCities + Bot.Settings.CommerceCityBaseCost.Value;
                if (goldLeftToSpendOnCities < cheapestCityCost)
                    return; //can't afford any more, we must have one on every spot which increases the cost.

                //We can afford it, let's build a city
                var buildCityOn = Bot.UseRandomness ? numCitiesOn.Where(o => o.Value == fewestCities).Random().Key : numCitiesOn.Where(o => o.Value == fewestCities).OrderBy(o => (int)o.Key).First().Key;
                goldLeftToSpendOnCities -= cheapestCityCost; //remember that we spent it for the loop above.

                AILog.Log("BuildCities", "Building a city on " + Bot.TerrString(buildCityOn) + " for " + cheapestCityCost + " unspentGold=" + goldLeftToSpendOnCities);

                Orders.PurchaseOrder.BuildCities.AddTo(buildCityOn, 1);
                numCitiesOn.AddTo(buildCityOn, 1);

                //Since we spent gold, adjust the remaining deployable armies so we don't overdeploy later
                this.UpdateEffectiveIncome();
            }
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
            armies -= Orders.Orders.OfType<GameOrderPlayCardAirlift>().Where(o => o.FromTerritoryID == terrID).Sum(o => o.Armies.NumArmies);

            //Subtract 1, since one must remain
            if (Bot.Settings.OneArmyStandsGuard)
                armies -= 1;

            return Math.Max(0, armies);
        }

    }
}
