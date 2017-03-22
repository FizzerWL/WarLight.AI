using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{

    public class MultiAttackExpand : Expand
    {
        BotMain Bot;
        GameStanding MultiAttackStanding;
        ExpandNormal Normal;

        /// <summary>
        /// We'll add to StackStartedFrom every time we try to take a territory with a stack.  The key is the territory we're taking, and the value is where we originally started the stack from.
        /// This is used so we know where to deploy if we want to feed a stack.
        /// </summary>
        Dictionary<TerritoryIDType, TerritoryIDType> StackStartedFrom = new Dictionary<TerritoryIDType, TerritoryIDType>();

        public MultiAttackExpand(BotMain bot)
        {
            this.Bot = bot;
            this.Normal = new ExpandNormal(bot);
            this.MultiAttackStanding = (GameStanding)bot.Standing.Clone();
        }

        public override void Go(int remainingUndeployed, bool highlyWeightedOnly)
        {
            //don't search more than 10% of the map. We won't cross the entire map to get a bonus, we're looking for ones near us.  For small maps, never search fewer than 5.
            var maxDistance = highlyWeightedOnly ? 1 : Math.Max(5, (int)(Bot.Map.Territories.Count / 10));

            var armyMult = ExpansionHelper.ArmyMultiplier(Bot.Settings.DefenseKillRate);
            var bonusWeights = Bot.Map.Bonuses.Keys.Where(o => Bot.BonusValue(o) > 0).ToDictionary(o => o, o => ExpansionHelper.WeighBonus(Bot, o, ts => ts.OwnerPlayerID == Bot.PlayerID, 1)); //assume 1 turn to take for now, which is wrong, but it gives us an even baseline. We'll adjust for turnsToTake in adjustedBonusWeights.

            AILog.Log("ExpandMultiAttack", "remainingUndeployed=" + remainingUndeployed + ". " + bonusWeights.Count + " base weights: ");
            foreach (var bw in bonusWeights.OrderByDescending(o => o.Value).Take(10))
                AILog.Log("ExpandMultiAttack", " - " + Bot.BonusString(bw.Key) + " Weight=" + bw.Value);
            var armiesLeft = remainingUndeployed;

            TryExpand(ref armiesLeft, maxDistance, armyMult, bonusWeights);

            if (!highlyWeightedOnly) //If we have anything left after the second pass, revert to normal expansion.  Do it even if armiesLeft is 0, as it will use normal border armies
                Normal.Go(armiesLeft, highlyWeightedOnly);

        }

        private void TryExpand(ref int armiesLeft, int maxDistanceArg, float armyMult, Dictionary<BonusIDType, float> bonusWeights)
        {
            var maxDistance = maxDistanceArg;

            foreach (var borderTerritory in MultiAttackStanding.Territories.Values.Where(o => Bot.IsBorderTerritory(MultiAttackStanding, o.ID)).OrderByDescending(o => o.NumArmies.NumArmies).ToList())
            {
                if (Bot.PastTime(10))
                    return;

                var stackSize = Math.Max(0, MultiAttackStanding.Territories[borderTerritory.ID].NumArmies.NumArmies - Bot.Settings.OneArmyMustStandGuardOneOrZero);
                var canDeployOnBorderTerritory = Bot.Standing.Territories[borderTerritory.ID].OwnerPlayerID == Bot.PlayerID;

                if (stackSize == 0 && !canDeployOnBorderTerritory)
                    continue;

                var bonusPaths = Bot.Map.Bonuses.Keys
                    .Where(o => Bot.BonusValue(o) > 0)
                    .Select(o =>
                    {
                        if (maxDistance > 1 && Bot.PastTime(7))
                        {
                            AILog.Log("MultiAttackExpand", "Due to slow speed, reducing bonus search distance from " + maxDistance + " to 1");
                            maxDistance = 1; //if we're taking too long, give up on far away bonuses.  Otherwise this algorithm can take forever on large maps
                        }
                        if (Bot.PastTime(10))
                            return null;

                        return MultiAttackPathToBonus.TryCreate(Bot, borderTerritory.ID, o, MultiAttackStanding, maxDistance);
                    })
                    .Where(o => o != null)
                    .ToDictionary(o => o.BonusID, o => o);

                var adjustedWeights = bonusPaths.Values.ToDictionary(o => o.BonusID, o => bonusWeights[o.BonusID] - o.ArmiesNeedToKillToGetThere * armyMult);

                //AILog.Log("ExpandMultiAttack", "Found " + bonusPaths.Count + " bonuses in range of " + Bot.TerrString(borderTerritory.ID) + ": " + bonusPaths.Values.OrderByDescending(o => adjustedWeights[o.BonusID]).Select(o => Bot.BonusString(o.BonusID)).JoinStrings(", "));


                foreach(var bonus in bonusPaths.Values.OrderByDescending(o => adjustedWeights[o.BonusID]))
                {
                    var estimatedArmiesNeedToDeploy = Math.Max(0, bonus.EstArmiesNeededToCapture - stackSize);
                    if (estimatedArmiesNeedToDeploy > armiesLeft)
                        continue;
                    if (estimatedArmiesNeedToDeploy > 0 && !canDeployOnBorderTerritory)
                        continue;

                    AILog.Log("ExpandMultiAttack", "Considering expansion to bonus " + Bot.BonusString(bonus.BonusID) + " from " + Bot.TerrString(borderTerritory.ID) + ".  stackSize=" + stackSize + " estimatedArmiesNeedToDeploy=" + estimatedArmiesNeedToDeploy + " weight=" + adjustedWeights[bonus.BonusID] + " ArmiesNeedToKillToGetThere=" + bonus.ArmiesNeedToKillToGetThere + " EstArmiesNeededToCapture=" + bonus.EstArmiesNeededToCapture + " armiesLeft=" + armiesLeft + " PathToGetThere=" + bonus.PathToGetThere.Select(o => Bot.TerrString(o)).JoinStrings(" -> "));

                    var plan = MultiAttackPlan.TryCreate(Bot, bonus, MultiAttackStanding, borderTerritory.ID);

                    if (plan == null)
                    {
                        AILog.Log("ExpandMultiAttack", " - Could not find a plan");
                        continue;
                    }

                    var actualArmiesNeedToCapture = Bot.ArmiesToTakeMultiAttack(plan.Select(o => ExpansionHelper.GuessNumberOfArmies(Bot, o.To, MultiAttackStanding, GuessOpponentNumberOfArmiesInFog)));
                    var actualArmiesNeedToDeploy = Math.Max(0, actualArmiesNeedToCapture - stackSize);
                    if (actualArmiesNeedToDeploy > armiesLeft)
                    {
                        AILog.Log("ExpandMultiAttack", " - actualArmiesNeedToDeploy=" + actualArmiesNeedToDeploy + " is not enough, have " + armiesLeft);
                        continue;
                    }

                    var stackStartedFrom = this.StackStartedFrom.ContainsKey(borderTerritory.ID) ? this.StackStartedFrom[borderTerritory.ID] : borderTerritory.ID;
                    if (!Bot.Orders.TryDeploy(stackStartedFrom, actualArmiesNeedToDeploy))
                    {
                        AILog.Log("ExpandMultiAttack", " - Could not deploy armies");
                        continue;
                    }
                    armiesLeft -= actualArmiesNeedToDeploy;


                    AILog.Log("ExpandMultiAttack", " - Attempting to capture. actualArmiesNeedToDeploy=" + actualArmiesNeedToDeploy + " plan=" + plan.Select(o => o.ToString()).JoinStrings(" -> "));

                    var terr = borderTerritory.ID;
                    foreach(var planStep in plan)
                    {
                        Assert.Fatal(Bot.Map.Territories[terr].ConnectedTo.ContainsKey(planStep.To), terr + " does not connect to " + planStep.To);

                        var defendersKill = SharedUtility.Round(ExpansionHelper.GuessNumberOfArmies(Bot, planStep.To).DefensePower * Bot.Settings.DefenseKillRate);
                        if (planStep.Type == MultiAttackPlanType.MainStack)
                        {
                            Bot.Orders.AddAttack(terr, planStep.To, AttackTransferEnum.AttackTransfer, 100, false, true);
                            MultiAttackStanding.Territories[planStep.To] = TerritoryStanding.Create(planStep.To, Bot.PlayerID, MultiAttackStanding.Territories[terr].NumArmies.Subtract(new Armies(defendersKill)));
                            MultiAttackStanding.Territories[terr].NumArmies = new Armies(Bot.Settings.OneArmyMustStandGuardOneOrZero);

                            terr = planStep.To;
                        }
                        else if (planStep.Type == MultiAttackPlanType.OneTerritoryOffshoot)
                        {
                            var attackWith = Bot.ArmiesToTake(ExpansionHelper.GuessNumberOfArmies(Bot, planStep.To));
                            Bot.Orders.AddAttack(terr, planStep.To, AttackTransferEnum.AttackTransfer, attackWith, false, false);
                            
                            MultiAttackStanding.Territories[planStep.To] = TerritoryStanding.Create(planStep.To, Bot.PlayerID, new Armies(attackWith - defendersKill));
                            MultiAttackStanding.Territories[terr].NumArmies = MultiAttackStanding.Territories[terr].NumArmies.Subtract(new Armies(attackWith));
                        }

                        this.StackStartedFrom.Add(planStep.To, stackStartedFrom);
                        
                    }

                    //After taking a bonus, we changed MultiAttackStanding. Therefore, we need to re-calc everything.
                    TryExpand(ref armiesLeft, maxDistance, armyMult, bonusWeights);
                    return;
                }
            }
        }

        public static Armies GuessOpponentNumberOfArmiesInFog(BotMain bot, TerritoryStanding ts)
        {
            //most enemy territories will have the minimum, but we'll add a few more to account for deployments and unexpected surprises
            if (!bot.UseRandomness)
                return new Armies(bot.Settings.OneArmyMustStandGuardOneOrZero + 1);

            var r = RandomUtility.RandomNumber(6) + RandomUtility.RandomNumber(6);
            if (r < 1)
                return new Armies(bot.Settings.OneArmyMustStandGuardOneOrZero);
            else
                return new Armies(bot.Settings.OneArmyMustStandGuardOneOrZero + Math.Max(1, r - 6));
            
        }

        public override bool HelpExpansion(IEnumerable<TerritoryIDType> terrs, int armies, Action<TerritoryIDType, int> deploy)
        {
            return Normal.HelpExpansion(terrs, armies, deploy);
        }
    }
}
