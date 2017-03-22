using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    static class ResolveTeamBonuses
    {
        public static void Go(BotMain bot)
        {

            var terrs = bot.Standing.Territories.Values.Where(o => bot.IsTeammateOrUs(o.OwnerPlayerID) && o.NumArmies.NumArmies == bot.Settings.OneArmyMustStandGuardOneOrZero && o.NumArmies.SpecialUnits.Length == 0).Select(o => o.ID).ToHashSet(true);

            foreach (var bonus in bot.Map.Bonuses.Values)
            {
                if (bonus.Territories.All(o => terrs.Contains(o)) && bonus.ControlsBonus(bot.Standing).HasValue == false)
                {
                    //bot bonus is entirely controlled by our team with 1s, but not by a single player. The player with the most territories should take it.
                    var owners = bonus.Territories.GroupBy(o => bot.Standing.Territories[o].OwnerPlayerID).ToList();
                    owners.Sort((f, s) => SharedUtility.CompareInts(s.Count(), f.Count()));

                    Assert.Fatal(owners.Count >= 2);

                    var attacks = bonus.Territories
                        .Where(o => bot.Standing.Territories[o].OwnerPlayerID != bot.PlayerID) //Territories in the bonus by our teammate
                        .Where(o => bot.Map.Territories[o].ConnectedTo.Keys.Any(c => bot.Standing.Territories[c].OwnerPlayerID == bot.PlayerID)) //Where we control an adjacent
                        .Select(o => new PossibleAttack(bot, bot.Map.Territories[o].ConnectedTo.Keys.First(c => bot.Standing.Territories[c].OwnerPlayerID == bot.PlayerID), o));

                    if (owners[0].Count() == owners[1].Count())
                    {
                        //The top two players have the same number of terrs.  50% chance we should try taking one.
                        if (attacks.Any() && RandomUtility.RandomNumber(2) == 0)
                        {
                            var doAttack1 = bot.UseRandomness ? attacks.Random() : attacks.First();
                            var numArmies = bot.ArmiesToTake(bot.Standing.Territories[doAttack1.To].NumArmies);
                            if (bot.Orders.TryDeploy(doAttack1.From, numArmies))
                            {
                                AILog.Log("ResolveTeamBonuses", "Detected a split bonus " + bot.BonusString(bonus) + ", and we're attempting to break the split by doing a small attack from " + bot.TerrString(doAttack1.From) + " to " + bot.TerrString(doAttack1.To) + " with " + numArmies);
                                bot.Orders.AddAttack(doAttack1.From, doAttack1.To, AttackTransferEnum.Attack, numArmies, true);
                            }
                        }
                    }
                    else if (owners[0].Key == bot.PlayerID)
                    {
                        //We should take the bonus
                        foreach (var doAttack2 in attacks)
                        {
                            var numArmies = bot.ArmiesToTake(bot.Standing.Territories[doAttack2.To].NumArmies);

                            if (bot.Orders.TryDeploy(doAttack2.From, numArmies))
                            {
                                AILog.Log("ResolveTeamBonuses", "Detected we should take bonus " + bot.BonusString(bonus) + ", so we're attacking from " + bot.TerrString(doAttack2.From) + " to " + bot.TerrString(doAttack2.To) + " with " + numArmies);
                                bot.Orders.AddAttack(doAttack2.From, doAttack2.To, AttackTransferEnum.Attack, 2, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
