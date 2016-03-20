using System;
using System.Collections.Generic;
using System.Linq;


using WarLight.Shared.AI.Wunderwaffe.Tasks;
using WarLight.Shared.AI.Wunderwaffe.BasicAlgorithms;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;
using WarLight.Shared.AI.Wunderwaffe.Evaluation;

namespace WarLight.Shared.AI.Wunderwaffe.Strategy
{
    public class MovesCalculator
    {
        public BotMain BotState;
        public MovesCalculator(BotMain state)
        {
            this.BotState = state;
        }
        public Moves CalculatedMoves = new Moves();


        public void CalculateMoves()
        {
            CalculatedMoves = new Moves();
            var movesSoFar = new Moves();

            PlayCardsTask.PlayCardsBeginTurn(BotState, movesSoFar);

            AILog.Log("MovesCalculator","Starting armies: " + BotState.MyIncome.Total);
            CalculateXBonusMoves(movesSoFar, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            AILog.Log("MovesCalculator", "Armies used after calculateXBonusMoves type 1: " + movesSoFar.GetTotalDeployment());
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateXBonusMoves(movesSoFar, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateXBonusMoves(movesSoFar, BotTerritory.DeploymentType.Conservative, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateXBonusMoves type 2: " + movesSoFar.GetTotalDeployment());
            CalculateSnipeBonusMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateSnipeBonusMoves: " + movesSoFar.GetTotalDeployment());
            CalculateXBonusMoves(movesSoFar, 0, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);

            BotState.ExpansionTask.CalculateExpansionMoves(movesSoFar);


            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateExpansionMoves: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, false, true, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateNoPlanBreakDefendMoves1: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, true, false, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateNoPlanBreakDefendMoves: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, false, true, BotTerritory.DeploymentType.Conservative, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, true, true, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateFlankBonusMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateFlankBonusMoves: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, true, false, false, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, true, false, false, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.VisibleMap);
            CalculateNoPlanAttackTerritoriesMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("MovesCalculator", "Armies used after calculateNoPlanAttackTerritoriesMoves2: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.VisibleMap);
            CalculateMoveIdleArmiesMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateJoinInAttacksMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            var supportTransferMoves = TransferMovesChooser.CalculateJoinStackMoves(BotState);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            MovesCommitter.CommittMoves(BotState, supportTransferMoves);
            movesSoFar.MergeMoves(supportTransferMoves);

            CalculateNoPlanCleanupMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateMoveIdleArmiesMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateJoinInAttacksMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateNoPlanTryoutAttackMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);

            AILog.Log("MovesCalculator", "Armies used after all moves done: " + movesSoFar.GetTotalDeployment());
            BotState.MapUpdater.UpdateMap(BotState.WorkingMap);
            DistanceCalculator.CalculateDistanceToBorder(BotState, BotState.VisibleMap, BotState.WorkingMap);
            BotState.ExpansionMapUpdater.UpdateExpansionMap();
            DistanceCalculator.CalculateDistanceToUnimportantTerritories(BotState.ExpansionMap, BotState.VisibleMap);
            DistanceCalculator.CalculateDistanceToImportantExpansionTerritories(BotState.ExpansionMap, BotState.VisibleMap);
            DistanceCalculator.CalculateDistanceToOpponentBorderCare3(BotState.ExpansionMap, BotState.VisibleMap);
            foreach (BotBonus emBonus in BotState.ExpansionMap.Bonuses.Values)
                emBonus.ExpansionValue = BotState.VisibleMap.Bonuses[emBonus.ID].ExpansionValue;
            DistanceCalculator.CalculateDistanceToHighlyImportantExpansionTerritories(BotState.ExpansionMap, BotState.VisibleMap);
            DistanceCalculator.CalculateDistanceToOpponentBorderCare4(BotState.ExpansionMap, BotState.VisibleMap);
            var transferMoves = TransferMovesChooser.CalculateTransferMoves2(BotState);
            MovesCommitter.CommittMoves(BotState, transferMoves);
            movesSoFar.MergeMoves(transferMoves);
            CalculateDelayMoves(movesSoFar);
            MovesCleaner.CleanupMoves(BotState, movesSoFar);
            movesSoFar = BotState.MovesScheduler2.ScheduleMoves(movesSoFar);

            PlayCardsTask.DiscardCardsEndTurn(BotState, movesSoFar);

            CalculatedMoves = movesSoFar;
        }

        private void CalculateJoinInAttacksMoves(Moves moves)
        {
            var joinInAttackMoves = JoinInAttacksTask.CalculateJoinInAttacksTask(BotState);
            MovesCommitter.CommittMoves(BotState, joinInAttackMoves);
            moves.MergeMoves(joinInAttackMoves);
        }

        private void CalculateMoveIdleArmiesMoves(Moves moves)
        {
            var idleArmiesMoves = MoveIdleArmiesTask.CalculateMoveIdleArmiesTask(BotState);
            MovesCommitter.CommittMoves(BotState, idleArmiesMoves);
            moves.MergeMoves(idleArmiesMoves);
            var idleExpansionArmiesMoves = MoveIdleArmiesTask.CalculateMoveIdleExpansionArmiesTask(BotState);
            MovesCommitter.CommittMoves(BotState, idleExpansionArmiesMoves);
            moves.MergeMoves(idleExpansionArmiesMoves);
        }

        private void CalculateDelayMoves(Moves moves)
        {
            var maxMovesBeforeRiskyAttack = 7;
            var minMovesBeforeRiskyAttack = 1;
            var delayMoves = DelayTask.CalculateDelayTask(BotState, moves, maxMovesBeforeRiskyAttack,
                minMovesBeforeRiskyAttack);
            MovesCommitter.CommittMoves(BotState, delayMoves);
            moves.MergeMoves(delayMoves);
        }

        private void CalculateNoPlanCleanupMoves(Moves moves)
        {
            // The cleanup deployment
            var armiesToDeploy = BotState.MyIncome.Total - moves.GetTotalDeployment();
            var cleanupDeploymentMoves = NoPlanCleanupTask.CalculateNoPlanCleanupDeploymentTask(BotState, armiesToDeploy, moves);
            MovesCommitter.CommittMoves(BotState, cleanupDeploymentMoves);
            moves.MergeMoves(cleanupDeploymentMoves);
            CalculateMoveIdleArmiesMoves(moves);
            // The cleanup expansion moves. We need here already the distance to the opponent border
            BotState.ExpansionMapUpdater.UpdateExpansionMap();
            DistanceCalculator.CalculateDistanceToOpponentBorderCare3(BotState.ExpansionMap, BotState.VisibleMap);
            var cleanupExpansionMoves = NoPlanCleanupTask.CalculateNoPlanCleanupExpansionTask(BotState, moves);
            MovesCommitter.CommittMoves(BotState, cleanupExpansionMoves);
            moves.MergeMoves(cleanupExpansionMoves);
        }

        private void CalculateNoPlanTryoutAttackMoves(Moves moves)
        {
            var foundMove = true;
            while (foundMove)
            {
                foundMove = false;
                var tryoutAttackMoves = NoPlanTryoutAttackTask.CalculateNoPlanTryoutAttackTask(BotState, false, true, true);
                if (tryoutAttackMoves != null)
                {
                    foundMove = true;
                    MovesCommitter.CommittMoves(BotState, tryoutAttackMoves);
                    moves.MergeMoves(tryoutAttackMoves);
                }
            }
        }

        private void CalculateNoPlanBreakDefendMoves(Moves moves, bool lowImportance, bool mediumImportance, bool highImportance, BotTerritory.DeploymentType lowerConservative, BotTerritory.DeploymentType upperConservative)
        {
            List<BotTerritory> territoriesToDefend = new List<BotTerritory>();
            foreach (var territory in BotState.VisibleMap.GetOwnedTerritories())
            {
                var importance = territory.DefenceTerritoryValue;
                var lowImportant = importance < TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE;
                var mediumImportant = importance < TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE && importance >= TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE;
                var highImportant = importance >= TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE;
                if ((lowImportance && lowImportant) || (mediumImportance && mediumImportant) || (highImportance && highImportant))
                    territoriesToDefend.Add(territory);
            }
            var possibleTerritoriesToAttack = new List<BotTerritory>();
            foreach (var opponentTerritory in BotState.VisibleMap.Territories.Values.Where(o => o.IsVisible
             && BotState.IsOpponent(o.OwnerPlayerID)))
            {
                var wmOpponentTerritory = BotState.WorkingMap.Territories[opponentTerritory.ID];
                if (BotState.IsOpponent(wmOpponentTerritory.OwnerPlayerID))
                    possibleTerritoriesToAttack.Add(opponentTerritory);
            }
            var territoriesToAttack = new List<BotTerritory>();
            foreach (var territory_1 in possibleTerritoriesToAttack)
            {
                var importance = territory_1.AttackTerritoryValue;
                var lowImportant = importance < TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE;
                var mediumImportant = importance < TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE && importance >= TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE;
                var highImportant = importance >= TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE;
                if ((lowImportance && lowImportant) || (mediumImportance && mediumImportant) || (highImportance && highImportant))
                    territoriesToAttack.Add(territory_1);
            }
            var combinedTerritories = new List<BotTerritory>();
            combinedTerritories.AddRange(territoriesToDefend);
            combinedTerritories.AddRange(territoriesToAttack);
            var sortedTerritories = BotState.TerritoryValueCalculator.SortTerritoriesAttackDefense(combinedTerritories);
            foreach (var territory_2 in sortedTerritories)
            {
                var maxDeployment = BotState.MyIncome.Total - moves.GetTotalDeployment();
                Moves defendBreakMoves = null;
                if (territory_2.OwnerPlayerID == BotState.Me.ID)
                    defendBreakMoves = BotState.DefendTerritoryTask.CalculateDefendTerritoryTask(territory_2, maxDeployment, true, lowerConservative, upperConservative);
                else
                    defendBreakMoves = BotState.BreakTerritoryTask.CalculateBreakTerritoryTask(territory_2, maxDeployment, lowerConservative, upperConservative, "CalculateNoPlanBreakDefendMoves");

                if (defendBreakMoves != null)
                {
                    MovesCommitter.CommittMoves(BotState, defendBreakMoves);
                    moves.MergeMoves(defendBreakMoves);
                }
            }
        }

        private void CalculateFlankBonusMoves(Moves moves)
        {
            var maxDeployment = BotState.MyIncome.Total - moves.GetTotalDeployment();
            var flankBonusMoves = FlankBonusTask.CalculateFlankBonusTask(BotState, maxDeployment);
            if (flankBonusMoves != null)
            {
                MovesCommitter.CommittMoves(BotState, flankBonusMoves);
                moves.MergeMoves(flankBonusMoves);
            }
        }

        private void CalculateNoPlanAttackTerritoriesMoves(Moves moves)
        {
            var foundAnAttack = true;
            while (foundAnAttack)
            {
                var maxDeployment = BotState.MyIncome.Total - moves.GetTotalDeployment();
                var unplannedAttackMoves = NoPlanAttackBestTerritoryTask.CalculateNoPlanAttackBestTerritoryTask
                    (BotState, maxDeployment);
                if (unplannedAttackMoves == null)
                    foundAnAttack = false;
                else
                {
                    MovesCommitter.CommittMoves(BotState, unplannedAttackMoves);
                    moves.MergeMoves(unplannedAttackMoves);
                }
            }
        }

        private void CalculateXBonusMoves(Moves moves, BotTerritory.DeploymentType lowerBoundConservative, BotTerritory.DeploymentType upperBoundConservative)
        {
            var solutionFound = true;
            List<BotBonus> alreadyHandledBonuses = new List<BotBonus>();
            while (solutionFound)
            {
                solutionFound = false;
                BotState.BonusValueCalculator.CalculateBonusValues(BotState.WorkingMap, BotState.VisibleMap);
                BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
                List<BotBonus> bonusesToX = BotState.BonusValueCalculator.GetSortedBonusesAdjustedFactor(BotState.VisibleMap);
                //bonusesToX.RemoveAll(alreadyHandledBonuses);
                bonusesToX.RemoveAll(i => alreadyHandledBonuses.Contains(i));

                foreach (var bonus in bonusesToX)
                {
                    alreadyHandledBonuses.Add(bonus);
                    var stillAvailableDeployment = BotState.MyIncome.Total - moves.GetTotalDeployment();
                    var plan = BotState.BonusValueCalculator.GetPlanForBonus(bonus);
                    if (plan == BonusPlan.Break)
                    {
                        var visibleTerritories = bonus.GetVisibleOpponentTerritories();
                        var breakBonusMoves = BreakTerritoriesTask.CalculateBreakTerritoriesTask(BotState, visibleTerritories, stillAvailableDeployment, lowerBoundConservative, upperBoundConservative);
                        if (breakBonusMoves != null)
                        {
                            MovesCommitter.CommittMoves(BotState, breakBonusMoves);
                            moves.MergeMoves(breakBonusMoves);
                            solutionFound = true;
                            break;
                        }
                    }
                    else if (plan == BonusPlan.Defend)
                    {
                        var defendBonusMoves = DefendBonusTask.CalculateDefendBonusTask(BotState, bonus, stillAvailableDeployment, false, (BotTerritory.DeploymentType)Math.Max(1, (int)lowerBoundConservative), (BotTerritory.DeploymentType)Math.Max(1, (int)upperBoundConservative));
                        if (defendBonusMoves != null)
                        {
                            MovesCommitter.CommittMoves(BotState, defendBonusMoves);
                            moves.MergeMoves(defendBonusMoves);
                            solutionFound = true;
                            break;
                        }
                    }
                    else if (plan == BonusPlan.TakeOver)
                    {
                        var takeOverMoves = TakeBonusOverTask.CalculateTakeBonusOverTask(BotState, stillAvailableDeployment, bonus, lowerBoundConservative);
                        if (takeOverMoves != null)
                        {
                            MovesCommitter.CommittMoves(BotState, takeOverMoves);
                            moves.MergeMoves(takeOverMoves);
                            solutionFound = true;
                            break;
                        }
                    }
                    else if (plan == BonusPlan.PreventTakeOver && bonus.PreventTakeOverOpponent.HasValue)
                    {
                        var preventTakeOverMoves = PreventBonusTask.CalculatePreventBonusTask(BotState, bonus, bonus.PreventTakeOverOpponent.Value, stillAvailableDeployment, lowerBoundConservative);
                        MovesCommitter.CommittMoves(BotState, preventTakeOverMoves);
                        moves.MergeMoves(preventTakeOverMoves);
                        solutionFound = true;
                        break;
                    }
                    else
                    {
                        throw new Exception("Unexpected plan");
                    }
                }
            }
        }

        private void CalculateSnipeBonusMoves(Moves moves)
        {
            var maxDeployment = BotState.MyIncome.Total - moves.GetTotalDeployment();
            var bestSnipableBonus = BotState.PreventOpponentExpandBonusTask.GetBestBonusToPrevent(BotState.VisibleMap, BotState.WorkingMap);
            if (bestSnipableBonus == null)
                return;

            var snipeBonusMoves = BotState.PreventOpponentExpandBonusTask.CalculatePreventOpponentExpandBonusTaskk(bestSnipableBonus, maxDeployment, BotState.VisibleMap, BotState.WorkingMap);
            if (snipeBonusMoves != null)
            {
                MovesCommitter.CommittMoves(BotState, snipeBonusMoves);
                moves.MergeMoves(snipeBonusMoves);
            }
        }

    }
}
