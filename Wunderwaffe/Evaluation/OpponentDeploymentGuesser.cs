using System;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public class OpponentDeploymentGuesser
    {
        public BotMain BotState;
        public OpponentDeploymentGuesser(BotMain state)
        {
            this.BotState = state;
        }

        public void GuessOpponentDeployment(PlayerIDType opponentID)
        {
            Assert.Fatal(BotState.NumberOfTurns != -1);
            if (BotState.NumberOfTurns == 0)
            {
                foreach (var vmTerritory in BotState.VisibleMap.OpponentTerritories(opponentID))
                {
                    var armies = BotState.Settings.MinimumArmyBonus;
                    MovesCommitter.CommittPlaceArmiesMove(new BotOrderDeploy(opponentID, vmTerritory, armies));
                }
            }
            else
            {
                foreach (var vmTerritory1 in BotState.VisibleMap.OpponentTerritories(opponentID))
                {
                    var lvmTerritory = BotState.LastVisibleMapX.Territories[vmTerritory1.ID];
                    var guessedOpponentDeployment = 0;
                    if (lvmTerritory.IsVisible && lvmTerritory.OwnerPlayerID == opponentID)
                    {
                        var opponentIncome = BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap);
                        guessedOpponentDeployment = Math.Min(lvmTerritory.GetTotalDeployment(BotTerritory.DeploymentType.Normal), opponentIncome);
                        if (HasDeploymentReasonDisapeared(lvmTerritory, vmTerritory1))
                        {
                            var boundDeployment = GetBoundOpponentDeployment(opponentID, vmTerritory1);
                            var maxDeployment = BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap) - boundDeployment;
                            guessedOpponentDeployment = Math.Min(5, maxDeployment);
                        }
                    }
                    else
                    {
                        var boundDeployment = GetBoundOpponentDeployment(opponentID, vmTerritory1);
                        var maxDeployment = BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap) - boundDeployment;
                        guessedOpponentDeployment = Math.Max(1, Math.Min(5, maxDeployment));
                    }
                    var pam = new BotOrderDeploy(opponentID, vmTerritory1, guessedOpponentDeployment);
                    MovesCommitter.CommittPlaceArmiesMove(pam);
                    var conservativePam = new BotOrderDeploy(opponentID, vmTerritory1, BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap));
                    MovesCommitter.CommittPlaceArmiesMove(conservativePam, BotTerritory.DeploymentType.Conservative);
                }
            }
        }

        /// <summary>Calculate the opponent deployment that is bound to other territories.</summary>
        /// <remarks>Calculate the opponent deployment that is bound to other territories.</remarks>
        /// <param name="opponentTerritory"></param>
        /// <returns></returns>
        private int GetBoundOpponentDeployment(PlayerIDType opponentID, BotTerritory opponentTerritory)
        {
            var sortedOpponentTerritories = BotState.TerritoryValueCalculator.GetSortedAttackValueTerritories();
            var moreImportantTerritories = new List<BotTerritory>();
            foreach (var territory in sortedOpponentTerritories)
            {
                if (territory.ID != opponentTerritory.ID)
                    moreImportantTerritories.Add(territory);
                else
                    break;
            }
            var boundDeployment = 0;
            var stillAvailableIncome = BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap);
            foreach (var territory_1 in moreImportantTerritories)
            {
                var armiesNeededThere = GetOpponentNeededDeployment(territory_1);
                if (armiesNeededThere <= stillAvailableIncome)
                {
                    boundDeployment += armiesNeededThere;
                    stillAvailableIncome -= armiesNeededThere;
                }
            }
            return boundDeployment;
        }

        private int GetOpponentNeededDeployment(BotTerritory opponentTerritory)
        {
            var neededDeployment = 0;
            if (opponentTerritory.AttackTerritoryValue < TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE)
                return 0;

            var ourAttackingArmies = 0;
            foreach (var ownedNeighbor in opponentTerritory.GetOwnedNeighbors())
                ourAttackingArmies += ownedNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

            if (opponentTerritory.AttackTerritoryValue > TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE)
                ourAttackingArmies += 5;

            // TODO adapt to no luck
            neededDeployment = Math.Max(0, (int)Math.Round(ourAttackingArmies * BotState.Settings.OffenseKillRate));

            return neededDeployment;
        }

        private bool HasDeploymentReasonDisapeared(BotTerritory lvmTerritory, BotTerritory vmTerritory
        )
        {
            if (lvmTerritory.AttackTerritoryValue >= TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE && vmTerritory.AttackTerritoryValue < TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE)
                return true;
            else if (lvmTerritory.AttackTerritoryValue >= TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE && vmTerritory.AttackTerritoryValue < TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE)
                return true;
            else
                return false;
        }
    }
}
