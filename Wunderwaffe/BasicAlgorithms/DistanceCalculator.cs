﻿using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Evaluation;

namespace WarLight.Shared.AI.Wunderwaffe.BasicAlgorithms
{
    public static class DistanceCalculator
    {
        public static List<BotTerritory> GetShortestPathToTerritories(BotMap mapToUse, BotTerritory fromTerritory, List<BotTerritory> toTerritories, List<BotTerritory> blockedTerritories)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var annotatedTerritories = CalculateDistances(mapToUse, toTerritories, blockedTerritories);
            outvar.Add(fromTerritory);
            var currentTerritory = fromTerritory;
            var currentDistance = annotatedTerritories[fromTerritory.ID];
            while (currentDistance != 0)
            {
                var closestNeighbor = GetClosestNeighborToTargetTerritories(currentTerritory, annotatedTerritories, blockedTerritories);
                outvar.Add(closestNeighbor);
                currentTerritory = closestNeighbor;
                currentDistance = annotatedTerritories[closestNeighbor.ID];
            }
            return outvar;
        }

        public static void CalculateDistanceToOwnBonuses(BotMap mapToUse)
        {
            var ownBonusTerritories = new List<BotTerritory>();
            foreach (BotBonus bonus in mapToUse.Bonuses.Values)
            {
                if (bonus.IsOwnedByMyself())
                    ownBonusTerritories.AddRange(bonus.Territories);
            }
            if (ownBonusTerritories.Count > 0)
            {
                var annotadedTerritories = CalculateDistances(mapToUse, ownBonusTerritories, null);
                foreach (var territory in annotadedTerritories.Keys)
                {
                    var territoryDistance = annotadedTerritories[territory];
                    mapToUse.Territories[territory].DistanceToOwnBonus = territoryDistance;
                }
            }
        }

        public static void CalculateDistanceToOpponentBonuses(BotMap mapToUse)
        {
            var opponentBonusTerritories = new List<BotTerritory>();
            foreach (BotBonus bonus in mapToUse.Bonuses.Values)
            {
                if (bonus.IsOwnedByAnyOpponent())
                    opponentBonusTerritories.AddRange(bonus.Territories);
            }
            if (opponentBonusTerritories.Count > 0)
            {
                var annotadedTerritories = CalculateDistances(mapToUse, opponentBonusTerritories, null);
                foreach (var territory in annotadedTerritories.Keys)
                    mapToUse.Territories[territory].DistanceToOpponentBonus = annotadedTerritories[territory];
            }
        }

        // TODO
        public static KeyValuePair<BotTerritory, int> GetClosestTerritoryToOpponentBonus(BotMain state, BotMap mapToUse, BotBonus opponentBonus)
        {
            List<BotTerritory> Territories = opponentBonus.Territories;
            var annotadedTerritories = CalculateDistances(mapToUse, Territories, null);
            var minDistance = 1000;
            BotTerritory minDistanceTerritory = null;
            foreach (var territoryID in annotadedTerritories.Keys)
            {
                var territory = mapToUse.Territories[territoryID];
                var territoryDistance = annotadedTerritories[territoryID];
                if (territory.OwnerPlayerID == state.Me.ID && territoryDistance < minDistance)
                {
                    minDistance = annotadedTerritories[territoryID];
                    minDistanceTerritory = territory;
                }
            }
            return new KeyValuePair<BotTerritory, int>(minDistanceTerritory, minDistance);
        }

        /// <summary>Care0Spots</summary>
        /// <param name="mapToUse"></param>
        public static void CalculateDistanceToUnimportantTerritories(BotMap mapToUse, BotMap mapToWriteIn)
        {
            List<BotTerritory> unimportantTerritories = new List<BotTerritory>();
            foreach (var neutralTerritory in mapToUse.GetNeutralTerritories())
                if (neutralTerritory.Bonuses.All(o => o.ExpansionValueCategory == 0))
                    unimportantTerritories.Add(neutralTerritory);

            var blockedTerritories = mapToUse.AllOpponentTerritories;
            var annotadedTerritories = CalculateDistances(mapToUse, unimportantTerritories, blockedTerritories);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                var territoryToWriteIn = mapToWriteIn.Territories[territory];
                territoryToWriteIn.DistanceToUnimportantSpot = territoryDistance;
            }
        }

        /// <summary>Care1Spots</summary>
        /// <param name="mapToUse"></param>
        public static void CalculateDistanceToImportantExpansionTerritories(BotMap mapToUse, BotMap mapToWriteIn)
        {
            var importantTerritories = new List<BotTerritory>();
            foreach (var neutralTerritory in mapToUse.GetNeutralTerritories())
                if (neutralTerritory.Bonuses.Any(o => o.ExpansionValueCategory == 1))
                    importantTerritories.Add(neutralTerritory);

            var blockedTerritories = mapToUse.AllOpponentTerritories;
            var annotadedTerritories = CalculateDistances(mapToUse, importantTerritories, blockedTerritories);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                var territoryToWriteIn = mapToWriteIn.Territories[territory];
                territoryToWriteIn.DistanceToImportantSpot = territoryDistance;
            }
        }

        /// <summary>Care2Spots</summary>
        /// <param name="mapToUse"></param>
        public static void CalculateDistanceToHighlyImportantExpansionTerritories(BotMap mapToUse, BotMap mapToWriteIn)
        {
            var highlyImportantTerritories = new List<BotTerritory>();
            foreach (var neutralTerritory in mapToUse.GetNeutralTerritories())
            {
                if (neutralTerritory.Bonuses.Any(o => o.ExpansionValueCategory == 1 && o.GetExpansionValue() >= 100))
                    highlyImportantTerritories.Add(neutralTerritory);
            }
            var blockedTerritories = mapToUse.AllOpponentTerritories;
            var annotadedTerritories = CalculateDistances(mapToUse, highlyImportantTerritories, blockedTerritories);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                var territoryToWriteIn = mapToWriteIn.Territories[territory];
                territoryToWriteIn.DistanceToHighlyImportantSpot = territoryDistance;
            }
        }

        /// <summary>Care3Spots</summary>
        /// <param name="mapToUse"></param>
        public static void CalculateDistanceToOpponentBorderCare3(BotMap mapToUse, BotMap mapToWriteIn)
        {
            var opponentTerritories = mapToUse.AllOpponentTerritories;
            var blockedTerritories = mapToUse.GetNeutralTerritories();
            var annotadedTerritories = CalculateDistances(mapToUse, opponentTerritories, blockedTerritories);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                var territoryToWriteIn = mapToWriteIn.Territories[territory];
                territoryToWriteIn.DistanceToOpponentBorder = territoryDistance;
            }
        }

        /// <summary>Care4Spots</summary>
        /// <param name="mapToUse"></param>
        public static void CalculateDistanceToOpponentBorderCare4(BotMap mapToUse, BotMap mapToWriteIn)
        {
            var importantOpponentTerritories = new List<BotTerritory>();
            foreach (var opponentTerritory in mapToUse.AllOpponentTerritories)
            {
                if (opponentTerritory.AttackTerritoryValue >= TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE)
                {
                    importantOpponentTerritories.Add(opponentTerritory);
                }
            }
            var blockedTerritories = mapToUse.GetNeutralTerritories();
            var annotadedTerritories = CalculateDistances(mapToUse, importantOpponentTerritories, blockedTerritories);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                var territoryToWriteIn = mapToWriteIn.Territories[territory];
                territoryToWriteIn.DistanceToImportantOpponentBorder = territoryDistance;
            }
        }

        public static void CalculateDirectDistanceToOpponentTerritories(BotMap mapToUse, BotMap mapToWriteIn)
        {
            var opponentTerritories = mapToUse.AllOpponentTerritories;
            var annotadedTerritories = CalculateDistances(mapToUse, opponentTerritories, null);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                var territoryToWriteIn = mapToWriteIn.Territories[territory];
                territoryToWriteIn.DirectDistanceToOpponentBorder = territoryDistance;
            }
        }

        public static void CalculateDistanceToBorder(BotMain state, BotMap mapToWriteIn, BotMap mapToUse)
        {
            var nonOwnedTerritories = new List<BotTerritory>();
            foreach (var vmTerritory in mapToUse.Territories.Values)
            {
                var wmTerritory = mapToUse.Territories[vmTerritory.ID];
                if (wmTerritory.OwnerPlayerID != state.Me.ID)
                {
                    nonOwnedTerritories.Add(vmTerritory);
                }
            }
            var annotadedTerritories = CalculateDistances(mapToWriteIn, nonOwnedTerritories, null);
            foreach (var territory in annotadedTerritories.Keys)
            {
                var territoryDistance = annotadedTerritories[territory];
                mapToUse.Territories[territory].DistanceToBorder = territoryDistance;
            }
        }

        /// <param name="state"></param>
        /// <param name="toTerritories"></param>
        /// <param name="blockedTerritories">blocked territories. Insert null here if not needed.</param>
        /// <returns></returns>
        public static Dictionary<TerritoryIDType, int> CalculateDistances(BotMap mapToUse, List<BotTerritory> toTerritories, List<BotTerritory> blockedTerritories)
        {
            var outvar = new Dictionary<TerritoryIDType, int>();
            foreach (var territory in mapToUse.Territories.Values)
            {
                if (toTerritories.Contains(territory))
                    outvar.Add(territory.ID, 0);
                else
                    outvar.Add(territory.ID, int.MaxValue);
            }
            // Now do the real stuff
            var hasSomethingChanged = true;
            while (hasSomethingChanged)
            {
                hasSomethingChanged = false;
                foreach (var territory_1 in mapToUse.Territories.Values)
                {
                    var closestNeighbor = GetClosestNeighborToTargetTerritories(territory_1, outvar, blockedTerritories);
                    if (outvar[closestNeighbor.ID] < outvar[territory_1.ID] && outvar[territory_1.ID] != outvar[closestNeighbor.ID] + 1)
                    {
                        outvar[territory_1.ID] = outvar[closestNeighbor.ID] + 1;
                        hasSomethingChanged = true;
                    }
                }
            }
            return outvar;
        }

        private static BotTerritory GetClosestNeighborToTargetTerritories(BotTerritory inTerritory, Dictionary<TerritoryIDType, int> annotatedTerritories, List<BotTerritory> blockedTerritories)
        {
            var nonBlockedNeighbors = new List<BotTerritory>();
            foreach (var neighbor in inTerritory.Neighbors)
            {
                if (blockedTerritories == null || !blockedTerritories.Contains(neighbor))
                {
                    nonBlockedNeighbors.Add(neighbor);
                }
            }
            var closestNeighbor = inTerritory;
            foreach (var neighbor_1 in nonBlockedNeighbors)
            {
                var neighborDistance = annotatedTerritories[neighbor_1.ID];
                if (neighborDistance < annotatedTerritories[closestNeighbor.ID])
                {
                    closestNeighbor = neighbor_1;
                }
            }
            return closestNeighbor;
        }
    }
}
