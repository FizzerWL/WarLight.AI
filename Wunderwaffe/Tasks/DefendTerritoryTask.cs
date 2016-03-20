using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    public class DefendTerritoryTask
    {
        public BotMain BotState;
        public DefendTerritoryTask(BotMain state)
        {
            this.BotState = state;
        }

        /// <summary>Returns the needed moves to defend the territory.</summary>
        /// <remarks>
        /// Returns the needed moves to defend the territory. If not possible then returns null. If no defense needed returns
        /// empty moves. First tries to fulfill the needed armies with background armies.
        /// </remarks>
        /// <param name="territoryToDefend"></param>
        /// <param name="maxDeployment"></param>
        /// <returns></returns>
        public Moves CalculateDefendTerritoryTask(BotTerritory territoryToDefend, int maxDeployment, bool useBackgroundArmies, BotTerritory.DeploymentType lowerConservativeLevel, BotTerritory.DeploymentType upperConservativeLevel)
        {
            var outvar = new Moves();
            var oppNeighbors = territoryToDefend.GetOpponentNeighbors();

            if (oppNeighbors.Count == 0)
                return null;

            var maxOpponentDeployment = oppNeighbors.Select(o => o.OwnerPlayerID).Distinct().Max(o => BotState.GetGuessedOpponentIncome(o, BotState.VisibleMap));
            for (var i = 0; i < maxOpponentDeployment; i++)
            {
                var defendMoves = CalculateDefendTerritoryMoves(territoryToDefend, maxDeployment, useBackgroundArmies, i, lowerConservativeLevel, upperConservativeLevel);
                if (defendMoves != null)
                    outvar = defendMoves;
                else
                    return outvar;
            }
            return null;
        }

        private Moves CalculateDefendTerritoryMoves(BotTerritory territoryToDefend, int maxDeployment, bool useBackgroundArmies, int step, BotTerritory.DeploymentType lowerConservativeLevel, BotTerritory.DeploymentType upperConservativeLevel)
        {
            var outvar = new Moves();
            var maxAttackingArmies = 0;
            var currentDeployment = 0;
            foreach (var opponentNeighbor in territoryToDefend.GetOpponentNeighbors())
            {
                currentDeployment += opponentNeighbor.GetTotalDeployment(lowerConservativeLevel);
                var opponentArmies = opponentNeighbor.GetArmiesAfterDeployment(lowerConservativeLevel).AttackPower;
                var upperOpponentArmies = opponentNeighbor.GetArmiesAfterDeployment(upperConservativeLevel).AttackPower;
                var deploymentDifference = upperOpponentArmies - opponentArmies;
                for (var i = 0; i < step; i++)
                {
                    if (deploymentDifference > 0)
                    {
                        deploymentDifference--;
                        opponentArmies++;
                        currentDeployment++;
                    }
                }
                var idleArmies = opponentArmies - 1;
                maxAttackingArmies += idleArmies;
            }
            // Adjust stuff so opponent can't deploy eyerything to every territory
            var maxOpponentDeployment = territoryToDefend.GetOpponentNeighbors().Select(o => o.OwnerPlayerID).Distinct().Max(o => BotState.GetGuessedOpponentIncome(o, BotState.VisibleMap));
            var deploymentDifference_1 = maxOpponentDeployment - currentDeployment;
            maxAttackingArmies -= deploymentDifference_1;
            var opponentKills = SharedUtility.Round(maxAttackingArmies * BotState.Settings.OffenseKillRate);
            var ownArmies = territoryToDefend.GetArmiesAfterDeploymentAndIncomingMoves().DefensePower;
            var missingArmies = Math.Max(0, opponentKills - ownArmies + 1);
            // First try to pull in more armies
            if (missingArmies > 0 && useBackgroundArmies)
            {
                var neighborsWithIdleArmies = GetNeighborsWithIdleArmies(territoryToDefend);
                foreach (var neighbor in neighborsWithIdleArmies)
                {
                    var armiesToTransfer = Math.Min(missingArmies, neighbor.GetIdleArmies().NumArmies);
                    if (armiesToTransfer > 0)
                    {
                        outvar.AddOrder(new BotOrderAttackTransfer(BotState.Me.ID, neighbor, territoryToDefend, new Armies(armiesToTransfer), "DefendTerritoryTask"));
                        missingArmies -= armiesToTransfer;
                    }
                }
            }
            // Then try to deploy
            if (missingArmies <= maxDeployment && missingArmies > 0)
                outvar.AddOrder(new BotOrderDeploy(BotState.Me.ID, territoryToDefend, missingArmies));
            else if (missingArmies > maxDeployment)
                return null;

            return outvar;
        }

        private List<BotTerritory> GetNeighborsWithIdleArmies(BotTerritory territoryToDefend)
        {
            var unsortedNeighbors = new List<BotTerritory>();
            foreach (var ownedNeighbor in territoryToDefend.GetOwnedNeighbors())
            {
                if (ownedNeighbor.GetOpponentNeighbors().Count == 0 && ownedNeighbor.GetIdleArmies().NumArmies > 0)
                {
                    unsortedNeighbors.Add(ownedNeighbor);
                }
            }
            // Sort according to the amount of idle armies
            var outvar = new List<BotTerritory>();
            while (unsortedNeighbors.Count != 0)
            {
                var biggestIdleArmyTerritory = unsortedNeighbors[0];
                foreach (var territory in unsortedNeighbors)
                {
                    if (territory.GetIdleArmies().AttackPower > biggestIdleArmyTerritory.GetIdleArmies().AttackPower)
                        biggestIdleArmyTerritory = territory;
                }
                outvar.Add(biggestIdleArmyTerritory);
                unsortedNeighbors.Remove(biggestIdleArmyTerritory);
            }
            return outvar;
        }
    }
}
