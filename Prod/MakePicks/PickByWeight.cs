using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakePicks
{
    static class PickByWeight
    {
        public static List<TerritoryIDType> Go(BotMain bot, HashSet<TerritoryIDType> allAvailable, int maxPicks)
        {
            var expansionWeights = allAvailable.ToDictionary(o => o, o => GetExpansionWeight(bot, o));

            var ordered = expansionWeights.OrderByDescending(o => o.Value).ToList();

            AILog.Log("PickTerritories", "Came up with " + expansionWeights.Count + " expansion weights: ");
            foreach (var e in ordered.Take(30))
                AILog.Log("PickTerritories", " - " + bot.TerrString(e.Key) + ": " + e.Value);

            if (!bot.UseRandomness)
                return ordered.Select(o => o.Key).Take(maxPicks).ToList();

            //Normalize weights
            var top = ordered.Take(maxPicks * 2);
            var sub = top.Min(o => o.Value) - 1;
            var normalized = top.ToDictionary(o => o.Key, o => o.Value - sub);

            var picks = new List<TerritoryIDType>();
            while (picks.Count < maxPicks && normalized.Count > 0)
            {
                var pick = RandomUtility.WeightedRandom(normalized.Keys, o => normalized[o]);
                picks.Add(pick);
                normalized.Remove(pick);
            }
            return picks;

        }

        private static float GetExpansionWeight(BotMain bot, TerritoryIDType terrID)
        {
            var td = bot.Map.Territories[terrID];

            var bonusPaths = td.PartOfBonuses
                .Where(o => bot.BonusValue(o) > 0)
                .Select(o => BonusPath.TryCreate(bot, o, ts => ts.ID == terrID))
                .Where(o => o != null)
                .ToDictionary(o => o.BonusID, o => o);

            var turnsToTake = bonusPaths.Keys.ToDictionary(o => o, o => TurnsToTake(bot, td.ID, o, bonusPaths[o]));
            foreach (var cannotTake in turnsToTake.Where(o => o.Value == null).ToList())
            {
                turnsToTake.Remove(cannotTake.Key);
                bonusPaths.Remove(cannotTake.Key);
            }

            var bonusWeights = bonusPaths.Keys.ToDictionary(o => o, o => ExpansionHelper.WeighBonus(bot, o, ts => ts.ID == terrID, turnsToTake[o].NumTurns));

            var weight = 0.0f;

            weight += ExpansionHelper.WeighMultipleBonuses(td.PartOfBonuses.Where(o => bonusWeights.ContainsKey(o)).ToDictionary(o => o, o => bonusWeights[o]));

            AILog.Log("PickTerritories", "Expansion weight for terr " + bot.TerrString(terrID) + " is " + weight + ". " + td.PartOfBonuses.Select(b => "Bonus " + bot.BonusString(b) + " Weight=" + (bonusWeights.ContainsKey(b) ? bonusWeights[b] : 0) + " TurnsToTake=" + (turnsToTake.ContainsKey(b) ? turnsToTake[b].ToString() : "") + " Path=" + (bonusPaths.ContainsKey(b) ? bonusPaths[b].ToString() : "")).JoinStrings(", "));

            return weight;
        }

        /// <summary>
        /// Estimate how many turns it would take us to complete the bonus, assuming we start in the passed territory and can use full deployment every turn
        /// </summary>
        /// <param name="bonusID"></param>
        /// <param name="bonusPath"></param>
        /// <returns></returns>
        private static CaptureTerritories TurnsToTake(BotMain bot, TerritoryIDType terrID, BonusIDType bonusID, BonusPath path)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            var terrsToTake = bonus.Territories.ExceptOne(terrID).ToHashSet(true);

            return CaptureTerritories.TryFindTurnsToTake(bot, path, bot.Settings.InitialPlayerArmiesPerTerritory, bot.Settings.MinimumArmyBonus, terrsToTake, o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution ? bot.Settings.InitialNeutralsInDistribution : o.NumArmies.DefensePower);
        }

    }
}
