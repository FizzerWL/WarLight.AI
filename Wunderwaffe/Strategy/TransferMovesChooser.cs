using System;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Evaluation;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Strategy
{
    public class TransferMovesChooser
    {
        public static Moves CalculateJoinStackMoves(BotMain state)
        {
            // Calculate the border territories and the territories bordering border territories
            var outvar = new Moves();
            var possibleFromTerritories = new HashSet<BotTerritory>();
            possibleFromTerritories.AddRange(state.VisibleMap.GetBorderTerritories());
            var temp = new HashSet<BotTerritory>();
            foreach (var territory in possibleFromTerritories)
            {
                temp.AddRange(territory.GetOwnedNeighbors());
            }
            possibleFromTerritories.AddRange(temp);
            // Calculate which territories use for transferring
            List<BotTerritory> goodTransferTerritories = new List<BotTerritory>();
            foreach (var possibleFromTerritory in possibleFromTerritories)
            {
                if (possibleFromTerritory.GetOpponentNeighbors().Count == 0 || possibleFromTerritory.DefenceTerritoryValue < TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE)
                {
                    goodTransferTerritories.Add(possibleFromTerritory);
                }
            }
            // Calculate where to transfer to
            foreach (var territory_1 in goodTransferTerritories)
            {
                if (territory_1.GetOwnedNeighbors().Count > 0 && territory_1.GetIdleArmies().IsEmpty == false)
                {
                    var territoryValue = territory_1.DefenceTerritoryValue;
                    BotTerritory bestNeighbor = null;
                    var bestNeighborValue = -1;
                    foreach (var neighbor in territory_1.GetOwnedNeighbors())
                    {
                        if (neighbor.GetOpponentNeighbors().Count > 0 && neighbor.DefenceTerritoryValue > bestNeighborValue)
                        {
                            bestNeighbor = neighbor;
                            bestNeighborValue = neighbor.DefenceTerritoryValue;
                        }
                    }
                    if (bestNeighbor != null && bestNeighborValue > territoryValue)
                    {
                        var atm = new BotOrderAttackTransfer(state.Me.ID, territory_1, bestNeighbor, territory_1.GetIdleArmies(), "TransferMovesChooser1");
                        outvar.AddOrder(atm);
                    }
                }
            }
            return outvar;
        }

        public static Moves CalculateTransferMoves2(BotMain state)
        {
            var outvar = new Moves();
            foreach (var territory in state.VisibleMap.GetOwnedTerritories())
            {
                if (territory.GetIdleArmies().IsEmpty == false && territory.GetOpponentNeighbors().Count == 0)
                {
                    var ownedNeighbors = GetOwnedNeighborsAfterExpansion(state, territory);
                    if (ownedNeighbors.Count > 0)
                    {
                        var bestNeighbor = territory;
                        foreach (var neighbor in ownedNeighbors)
                        {
                            bestNeighbor = GetCloserTerritory(bestNeighbor, neighbor);
                        }
                        if (bestNeighbor != territory)
                        {
                            var atm = new BotOrderAttackTransfer(state.Me.ID, territory, bestNeighbor, territory.GetIdleArmies(), "TransferMovesChooser2");
                            outvar.AddOrder(atm);
                        }
                    }
                }
            }
            return outvar;
        }

        private static List<BotTerritory> GetOwnedNeighborsAfterExpansion(BotMain state, BotTerritory ourTerritory)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var emOurTerritory = state.ExpansionMap.Territories[ourTerritory.ID];
            var emOwnedNeighbors = emOurTerritory.GetOwnedNeighbors();
            foreach (var emOwnedNeighbor in emOwnedNeighbors)
            {
                outvar.Add(state.VisibleMap.Territories[emOwnedNeighbor.ID]);
            }
            return outvar;
        }



        private static BotTerritory GetCloserTerritory(BotTerritory territory1, BotTerritory territory2)
        {
            if (GetAdjustedDistance(territory1) < GetAdjustedDistance(territory2))
                return territory1;
            else if (GetAdjustedDistance(territory2) < GetAdjustedDistance(territory1))
                return territory2;

            if (territory1.DistanceToImportantOpponentBorder < territory2.DistanceToImportantOpponentBorder)
                return territory1;
            else if (territory2.DistanceToImportantOpponentBorder < territory1.DistanceToImportantOpponentBorder)
                return territory2;

            if (territory1.DistanceToOpponentBorder < territory2.DistanceToOpponentBorder)
                return territory1;
            else if (territory2.DistanceToOpponentBorder < territory1.DistanceToOpponentBorder)
                return territory2;

            if (territory1.DistanceToHighlyImportantSpot < territory2.DistanceToHighlyImportantSpot)
                return territory1;
            else if (territory2.DistanceToHighlyImportantSpot < territory1.DistanceToHighlyImportantSpot)
                return territory2;

            if (territory1.DistanceToImportantSpot < territory2.DistanceToImportantSpot)
                return territory1;
            else if (territory2.DistanceToImportantSpot < territory1.DistanceToImportantSpot)
                return territory2;

            if (territory1.GetArmiesAfterDeploymentAndIncomingMoves().AttackPower > territory2.GetArmiesAfterDeploymentAndIncomingMoves().AttackPower)
                return territory1;
            else if (territory2.GetArmiesAfterDeploymentAndIncomingMoves().AttackPower > territory1.GetArmiesAfterDeploymentAndIncomingMoves().AttackPower)
                return territory2;

            // Prefer territory2 by default since the initial territory is territory1 so we move more
            return territory2;
        }

        // TODO distances are outdated
        public static int GetAdjustedDistance(BotTerritory territory)
        {
            var distanceToUnimportantSpot = territory.DistanceToUnimportantSpot + 6;
            var distanceToImportantExpansionSpot = territory.DistanceToImportantSpot + 3;
            var distanceToHighlyImportantExpansionSpot = territory.DistanceToHighlyImportantSpot + 3;
            var distanceToOpponentSpot = territory.DistanceToOpponentBorder;
            var distanceToImportantOpponentSpot = territory.DistanceToImportantOpponentBorder;
            var minDistance = Math.Min(Math.Min(Math.Min(distanceToUnimportantSpot, distanceToImportantExpansionSpot), Math.Min(distanceToHighlyImportantExpansionSpot, distanceToOpponentSpot)), distanceToImportantOpponentSpot);
            return minDistance;
        }
    }
}
