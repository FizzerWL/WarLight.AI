using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod
{
    static class ExpansionHelper
    {
        public const float BaseBonusWeight = 1000f;
        public static float ArmyMultiplier(double defensiveKillRate)
        {
            return (float)defensiveKillRate + 0.8f;
        }

        public static float WeighBonus(BotMain bot, BonusIDType bonusID, Func<TerritoryStanding, bool> weOwn, int turnsToTake)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            int bonusValue = bot.BonusValue(bonusID);

            if (bonusValue <= 0)
                throw new Exception("Considered zero or negative bonuses"); //we should not even be considering zero or negative bonuses, ensure they're filtered out before we get here.

            var weight = BaseBonusWeight;

            //When randomness is enabled, modify the bonus by a fixed amount for this bonus.
            weight += bot.BonusFuzz(bonusID);

            weight += bonusValue * (bot.IsFFA ? 7 : 4);

            //Subtract value for each additional turn it takes to take over one
            weight -= bonusValue * (turnsToTake - 1);

            weight -= bonus.Territories.Count * bot.Settings.OneArmyMustStandGuardOneOrZero;

            float armyMult = ArmyMultiplier(bot.Settings.DefenseKillRate);

            //How many territories do we need to take to get it? Subtract one weight for each army standing in our way
            foreach (var terrInBonus in bonus.Territories)
            {
                var ts = bot.Standing.Territories[terrInBonus];

                if (weOwn(ts))
                    weight += ts.NumArmies.AttackPower * armyMult; //Already own it
                else if (ts.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                    weight -= GuessNumberOfArmies(bot, ts.ID).DefensePower * armyMult;
                else if (bot.IsTeammate(ts.OwnerPlayerID))
                    weight -= bot.Players[ts.OwnerPlayerID].IsAIOrHumanTurnedIntoAI ? 0 : ts.NumArmies.DefensePower * 4 * armyMult; //Human teammate in it. We'll defer to them since humans know best.
                else if (ts.OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                    weight -= Math.Max(bot.Settings.InitialNeutralsInDistribution, bot.Settings.InitialPlayerArmiesPerTerritory) * armyMult; //assume another player could start there
                else if (ts.IsNeutral)
                {
                    //Neutral in it
                    if (ts.NumArmies.Fogged == false)
                        weight -= ts.NumArmies.DefensePower * armyMult;
                    else
                        weight -= GuessNumberOfArmies(bot, ts.ID).DefensePower * armyMult;
                }
                else
                {
                    //Opponent in it - expansion less likely
                    if (ts.NumArmies.Fogged == false)
                        weight -= ts.NumArmies.DefensePower * 3 * armyMult;
                    else
                        weight -= GuessNumberOfArmies(bot, ts.ID).DefensePower * armyMult;
                }
            }

            return weight;

        }

        public static Armies GuessNumberOfArmies(BotMain bot, TerritoryIDType terrID)
        {
            return GuessNumberOfArmies(bot, terrID, bot.Standing);
        }

        public static Armies GuessNumberOfArmies(BotMain bot, TerritoryIDType terrID, GameStanding standing, Func<BotMain, TerritoryStanding, Armies> opponentFoggedTerritoryOpt = null)
        {
            var ts = standing.Territories[terrID];
            if (!ts.NumArmies.Fogged)
                return ts.NumArmies;

            if (bot.IsOpponent(ts.OwnerPlayerID))
            {
                //We can see it's an opponent, but the armies are fogged.  This can happen in light, dense, or heavy fog. We have no way of knowing what's there, so just assume the minimum
                if (opponentFoggedTerritoryOpt != null)
                    return opponentFoggedTerritoryOpt(bot, ts);
                else
                    return new Armies(bot.Settings.OneArmyMustStandGuardOneOrZero);
            }

            if (bot.DistributionStandingOpt != null)
            {
                var dist = bot.DistributionStandingOpt.Territories[terrID];
                if (dist.IsNeutral)
                    return dist.NumArmies;
                Assert.Fatal(dist.OwnerPlayerID == TerritoryStanding.AvailableForDistribution);

                return new Armies(Math.Max(bot.Settings.InitialNeutralsInDistribution, bot.Settings.InitialPlayerArmiesPerTerritory)); //TODO: If it's not a random distribution, we could check if this territory is in the distribution and be more accurate on whether a player started with it or not.
            }

            return new Armies(bot.Settings.InitialNonDistributionArmies);
        }

        /// <summary>
        /// The biggest bonus weight applies at 100%, and all other positive bonus weights get added in at a reduced weight. The idea is that the main bonus gets full weight, and super bonuses just help out a little.
        /// Negative super bonus bonus weights will never reduce the weight of the main bonus
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static float WeighMultipleBonuses(Dictionary<BonusIDType, float> weights)
        {
            if (weights.Count == 0)
                return 0;

            var maxID = weights.First().Key;
            foreach (var pair in weights)
                if (pair.Value > weights[maxID])
                    maxID = pair.Key;

            var ret = 0f;
            foreach(var pair in weights)
            {
                if (pair.Key == maxID)
                    ret += pair.Value;
                else if (pair.Value > 0)
                    ret += pair.Value / 10f;
            }

            return ret;
        }
    }
}
