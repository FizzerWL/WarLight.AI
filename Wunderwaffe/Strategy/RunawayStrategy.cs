// /*
// * This code was auto-converted from a java project.
// */

//using System.Collections.Generic;
//using WarLight.AI.Wunderwaffe.Bot;

//using WarLight.AI.Wunderwaffe.Move;


//namespace WarLight.AI.Wunderwaffe.Strategy
//{
//    /// <summary>This class is responsible for calculating the runaway moves when the game is completely lost.
//    /// </summary>
//    public class RunawayStrategy
//    {
//        public BotState BotState;
//        public RunawayStrategy(BotState state)
//        {
//            this.BotState = state;
//        }

//        public void CalculateRunawayMoves(Moves moves)
//        {
//            var bestRunawayTerritory = GetBestDeploymentTerritory();
//            var pam = new BotOrderDeploy(BotState.Me.ID, bestRunawayTerritory, BotState.MyIncome);
//            MovesCommitter.CommittPlaceArmiesMove(pam);
//            moves.PlaceArmiesMoves.Add(pam);
//            var bestTerritoryToAttack = GetBestTerritoryToAttack(bestRunawayTerritory);
//            var atm = new BotOrderAttackTransfer(BotState.Me.ID, bestRunawayTerritory, bestTerritoryToAttack, bestRunawayTerritory.GetIdleArmies());
//            MovesCommitter.CommittAttackTransferMove(BotState, atm);
//            moves.AttackTransferMoves.Add(atm);
//        }

//        private BotTerritory GetBestDeploymentTerritory()
//        {
//            var ownedTerritories = BotState.VisibleMap.GetOwnedTerritories();
//            var bestTerritory = ownedTerritories[0];
//            foreach (var territory in ownedTerritories)
//            {
//                if (territory.Armies.AttackPower > bestTerritory.Armies.AttackPower)
//                    bestTerritory = territory;
//            }
//            return bestTerritory;
//        }

//        private BotTerritory GetBestTerritoryToAttack(BotTerritory ourTerritory)
//        {
//            var nonCanAttackTerritories = GetNoCanAttackTerritories(ourTerritory);
//            var stayAwayTerritories = GetTerritoriesToStayAwayFrom(ourTerritory);
//            var goodTerritoryToAttack = GetGoodAttackTerritory(ourTerritory, nonCanAttackTerritories, stayAwayTerritories);

//            if (goodTerritoryToAttack != null)
//                return goodTerritoryToAttack;
//            foreach (var neighbor in ourTerritory.Neighbors)
//            {
//                if (neighbor.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
//                    return neighbor;
//            }
//            if (ourTerritory.GetOpponentNeighbors().Count == 0)
//                return ourTerritory;

//            var bestOpponentNeighbor = ourTerritory.GetOpponentNeighbors()[0];
//            foreach (var opponentNeighbor in ourTerritory.GetOpponentNeighbors())
//            {
//                if (opponentNeighbor.Armies.DefensePower < bestOpponentNeighbor.Armies.DefensePower)
//                    bestOpponentNeighbor = opponentNeighbor;
//            }
//            return bestOpponentNeighbor;
//        }

//        private BotTerritory GetGoodAttackTerritory(BotTerritory ourTerritory, List<BotTerritory> nonCanAttackTerritories, List<BotTerritory> stayAwayTerritories)
//        {
//            var possibleAttackTerritories = new List<BotTerritory>();
//            foreach (var neighbor in ourTerritory.Neighbors)
//            {
//                if (!nonCanAttackTerritories.Contains(neighbor) && !stayAwayTerritories.Contains(neighbor))
//                    possibleAttackTerritories.Add(neighbor);
//            }
//            if (possibleAttackTerritories.Count == 0)
//                return null;
//            BotState.VisibleMap.SetOpponentExpansionValue();
//            var bestNeighbor = possibleAttackTerritories[0];
//            foreach (var neighbor_1 in possibleAttackTerritories)
//                foreach(var bonus in neighbor_1.Bonuses)
//                {
//                    if (!bonus.IsOwnedByOpponent())
//                        bestNeighbor = neighbor_1;
//                }
//            return bestNeighbor;
//        }

//        private List<BotTerritory> GetTerritoriesToStayAwayFrom(BotTerritory ourTerritory)
//        {
//            List<BotTerritory> stayAwayTerritories = new List<BotTerritory>();
//            foreach (var ownedNeighbor in ourTerritory.GetOwnedNeighbors())
//            {
//                if (ownedNeighbor.GetOpponentNeighbors().Count > 0)
//                {
//                    stayAwayTerritories.Add(ownedNeighbor);
//                    foreach (var neighborNeighbor in ownedNeighbor.Neighbors)
//                        stayAwayTerritories.Add(neighborNeighbor);
//                }
//            }

//            BotTerritory biggestOpponentStackNeighbor = null;
//            var biggestOpponentStack = 0;
//            foreach (var opponentNeighbor in ourTerritory.GetOpponentNeighbors())
//            {
//                if (opponentNeighbor.Armies.AttackPower > biggestOpponentStack)
//                {
//                    biggestOpponentStack = opponentNeighbor.Armies.AttackPower;
//                    biggestOpponentStackNeighbor = opponentNeighbor;
//                }
//            }
//            if (biggestOpponentStackNeighbor != null)
//            {
//                stayAwayTerritories.Add(biggestOpponentStackNeighbor);
//            }
//            return stayAwayTerritories;
//        }

//        private List<BotTerritory> GetNoCanAttackTerritories(BotTerritory ourTerritory)
//        {
//            var outvar = new List<BotTerritory>();
//            foreach (var neighbor in ourTerritory.GetNonOwnedNeighbors())
//            {
//                var ourAttackingArmies = ourTerritory.GetIdleArmies().NumArmies + BotState.MyIncome;
//                if (neighbor.Armies.DefensePower > ourAttackingArmies * BotState.Settings.OffensiveKillRate)
//                    outvar.Add(neighbor);
//            }
//            return outvar;
//        }
//    }
//}
