/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;
using WarLight.Shared.AI.Cowzow.Map;
using WarLight.Shared.AI;

namespace WarLight.Shared.AI.Cowzow.Comparators
{
    public class StartingTerritoryComparator : IComparer<BotTerritory>
    {
        private readonly Dictionary<TerritoryIDType, double> TerritoryScoresCache;
        public CowzowBot Bot;

        public StartingTerritoryComparator(CowzowBot bot)
        {
            this.Bot = bot;
            TerritoryScoresCache = new Dictionary<TerritoryIDType, double>();
        }

        public int Compare(BotTerritory a, BotTerritory b)
        {
            var aScore = GetScore(a);
            var bScore = GetScore(b);
            if (aScore > bScore)
                return -1;
            if (aScore < bScore)
                return 1;
            return 0;
        }

        // Does this choice of territory secure the Bonus
        public bool IsSecure(BotBonus bonus, BotTerritory startingPoint)
        {
            var threats = Bot.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToHashSet(true);

            foreach (var r in bonus.Territories)
            {
                if (threats.Contains(r.ID))
                    return false;
                foreach (var n in r.Neighbors)
                    if (threats.Contains(n.ID))
                        return false;
            }
            return true;
        }

        public double GetScore(BotTerritory territory)
        {
            if (TerritoryScoresCache.ContainsKey(territory.ID))
                return TerritoryScoresCache[territory.ID];

            Assert.Fatal(territory.OwnerPlayerID == TerritoryStanding.AvailableForDistribution);
            territory.OwnerPlayerID = Bot.Me.ID; //assume we have it for the duration of this function, we'll re-set it at the bottom

            var visitedBonuses = new HashSet<BonusIDType>();
            var visited = new HashSet<TerritoryIDType>();
            var count = 1;
            double depth = 0;
            double score = 0;
            var stack = new Queue<BotTerritory>();
            stack.Enqueue(territory);
            while (stack.Count > 0 && depth < 4)
            {
                var curr = stack.Dequeue();
                count--;
                if (!visited.Contains(curr.ID))
                {
                    visited.Add(curr.ID);
                    visitedBonuses.AddRange(curr.Bonuses.Select(o => o.ID));
                    foreach (var n in curr.Neighbors)
                        if (!visited.Contains(n.ID))
                            stack.Enqueue(n);

                }
                if (count == 0)
                {
                    count = stack.Count;

                    foreach (var bonusID in visitedBonuses.ToList())
                    {
                        var bonus = Bot.BotMap.Bonuses[bonusID];

                        var subTerritories = bonus.Territories.Select(o => o.ID).ToHashSet(true);
                        // subTerritories.removeAll(myTerritories);
                        subTerritories.Remove(territory.ID);
                        if (visited.ContainsAll(subTerritories))
                        {
                            var neutralArmies = subTerritories.Sum(o => Bot.BotMap.Territories[o].GuessedArmiesNotOwnedByUs);
                            var divisor = Math.Max(1, Math.Pow(depth, 1.3)) * Math.Max(1, neutralArmies + subTerritories.Count * Bot.Settings.OneArmyMustStandGuardOneOrZero);
                            var result = Math.Pow(bonus.ArmiesReward, 1.2) / divisor;

                            //ArmiesToNeutralsRatio range: 0.2 is terrible, 0.5 is great
                            //result += ((bonus.ArmiesToNeutralsRatio - 0.2) / 0.3) * 1.5;

                            if (bonus.ArmiesReward < 2)
                                result *= 0.5;

                            AILog.Log("StartingTerritoryComparator", " - " + bonus.Details.Name + " depth=" + depth + " neutrals=" + neutralArmies + " ratio=" + bonus.ArmiesToNeutralsRatio + " divisor=" + divisor + " result=" + result);
                            score += result;

                            visitedBonuses.Remove(bonusID);
                        }
                    }
                    depth++;
                }
            }

            foreach (var bonus_1 in Bot.BotMap.Bonuses.Values)
                if (IsSecure(bonus_1, territory) 
                    //&& !bonus_1.HasWasteland() 
                    && bonus_1.ArmiesToNeutralsRatio >= 0.25 
                    && bonus_1.Territories.Count < 5)
                    score += bonus_1.ArmiesToNeutralsRatio;

            AILog.Log("StartingTerritoryComparator", "Score for " + territory + " is " + score);

            TerritoryScoresCache[territory.ID] = score;

            territory.OwnerPlayerID = TerritoryStanding.AvailableForDistribution; //reset
            return score;
        }
    }
}
