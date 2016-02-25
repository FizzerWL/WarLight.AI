 /*
 * This code was auto-converted from a java project.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using WarLight.AI.Wunderwaffe.BasicAlgorithms;
using WarLight.AI.Wunderwaffe.Bot;
using WarLight.AI.Wunderwaffe.Evaluation;

using WarLight.AI.Wunderwaffe.Move;

using WarLight.AI.Wunderwaffe.Tasks;

namespace WarLight.AI.Wunderwaffe.Strategy
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
            BotState.GameState.EvaluateGameState();

            //Never run away -- this isn't fun when playing against humans.
            //if (BotState.GameState.IsGameCompletelyLost)
            //{
            //    Utility.Log("Game completely lost!");
            //    new RunawayStrategy(BotState).CalculateRunawayMoves(movesSoFar);
            //    CalculatedMoves = movesSoFar;
            //    return;
            //}

            PlayCardsTask.PlayCards(BotState, movesSoFar);

            AILog.Log("Starting armies: " + BotState.MyIncome.Total);
            CalculateXBonusMoves(movesSoFar, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            AILog.Log("Armies used after calculateXBonusMoves type 1: " + movesSoFar.GetTotalDeployment());
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateXBonusMoves(movesSoFar, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateXBonusMoves(movesSoFar, BotTerritory.DeploymentType.Conservative, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateXBonusMoves type 2: " + movesSoFar.GetTotalDeployment());
            CalculateSnipeBonusMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateSnipeBonusMoves" + movesSoFar.GetTotalDeployment());
            CalculateXBonusMoves(movesSoFar, 0, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            // calculateXBonusMoves(movesSoFar, 0, 1);
            // Utility.Debug("Armies used after calculateXBonusMoves type 0: " + movesSoFar.GetTotalDeployment(), 1);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            // int movesWithoutExpansion = movesSoFar.attackTransferMoves.size();
            CalculateExpansionMoves(movesSoFar, 100000, -51000);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateExpansionMoves: " + movesSoFar.GetTotalDeployment());
            // int movesWithExpansion = movesSoFar.attackTransferMoves.size();
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, false, true, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateNoPlanBreakDefendMoves1: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, true, false, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateNoPlanBreakDefendMoves: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, false, true, BotTerritory.DeploymentType.Conservative, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, false, true, true, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateFlankBonusMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateFlankBonusMoves: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, true, false, false, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
            CalculateNoPlanBreakDefendMoves(movesSoFar, true, false, false, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Conservative);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.VisibleMap);
            CalculateNoPlanAttackTerritoriesMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            AILog.Log("Armies used after calculateNoPlanAttackTerritoriesMoves2: " + movesSoFar.GetTotalDeployment());
            BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.VisibleMap);
            CalculateMoveIdleArmiesMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateJoinInAttacksMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            var supportTransferMoves = TransferMovesChooser.CalculateJoinStackMoves(BotState);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            MovesCommitter.CommittMoves(BotState, supportTransferMoves);
            movesSoFar.MergeMoves(supportTransferMoves);
            // XX
            CalculateNoPlanCleanupMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateMoveIdleArmiesMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateJoinInAttacksMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            CalculateNoPlanTryoutAttackMoves(movesSoFar);
            BotState.DeleteBadMovesTask.CalculateDeleteBadMovesTask(movesSoFar);
            // end xx
            AILog.Log("Armies used after all moves done: " + movesSoFar.GetTotalDeployment());
            BotState.MapUpdater.UpdateMap(BotState.WorkingMap);
            DistanceCalculator.CalculateDistanceToBorder(BotState, BotState.VisibleMap, BotState.WorkingMap);
            BotState.ExpansionMapUpdater.UpdateExpansionMap();
            DistanceCalculator.CalculateDistanceToUnimportantTerritories(BotState.ExpansionMap, BotState.VisibleMap);
            DistanceCalculator.CalculateDistanceToImportantExpansionTerritories(BotState.ExpansionMap, BotState.VisibleMap);
            DistanceCalculator.CalculateDistanceToOpponentBorderCare3(BotState.ExpansionMap, BotState.VisibleMap);
            foreach (BotBonus emBonus in BotState.ExpansionMap.Bonuses.Values)
                emBonus.InsertMyExpansionValueHeuristic(BotState.VisibleMap.Bonuses[emBonus.ID].MyExpansionValueHeuristic);
            DistanceCalculator.CalculateDistanceToHighlyImportantExpansionTerritories(BotState.ExpansionMap, BotState.VisibleMap);
            DistanceCalculator.CalculateDistanceToOpponentBorderCare4(BotState.ExpansionMap, BotState.VisibleMap);
            var transferMoves = TransferMovesChooser.CalculateTransferMoves2(BotState);
            MovesCommitter.CommittMoves(BotState, transferMoves);
            movesSoFar.MergeMoves(transferMoves);
            CalculateDelayMoves(movesSoFar);
            MovesCleaner.CleanupMoves(BotState, movesSoFar);
            // movesSoFar = MovesScheduler.scheduleMoves(movesSoFar);
            movesSoFar = BotState.MovesScheduler2.ScheduleMoves(movesSoFar);
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
                AILog.Log("FLANK_Bonus_MOVES");
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
            var alreadyHandledBonuses = new List<BotBonus>();
            while (solutionFound)
            {
                solutionFound = false;
                BotState.BonusValueCalculator.CalculateBonusValues(BotState.WorkingMap, BotState.VisibleMap);
                BotState.TerritoryValueCalculator.CalculateTerritoryValues(BotState.VisibleMap, BotState.WorkingMap);
                var bonusesToX = BotState.BonusValueCalculator.GetSortedBonusesAdjustedFactor(BotState.VisibleMap);
                bonusesToX.RemoveAll(alreadyHandledBonuses);
                foreach (var bonus in bonusesToX)
                {
                    alreadyHandledBonuses.Add(bonus);
                    var stillAvailableDeployment = BotState.MyIncome.Total - moves.GetTotalDeployment();
                    var plan = BotState.BonusValueCalculator.GetPlanForBonus(bonus);
                    //Utility.Log("Plan for bonus " + bonus.Details.Name + " is " + plan);
                    if (plan == BonusPlan.Break)
                    {
                        var visibleTerritories = bonus.GetVisibleOpponentTerritories();
                        var breakBonusMoves = BreakTerritoriesTask.CalculateBreakTerritoriesTask(BotState, visibleTerritories, stillAvailableDeployment, lowerBoundConservative, upperBoundConservative);
                        if (breakBonusMoves != null)
                        {
                            AILog.Log("BREAK moves calculated for Bonus " + bonus.Details.Name);
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
                            if (defendBonusMoves.Orders.Count > 0)
                                AILog.Log("DEFEND moves calculated for Bonus " + bonus.Details.Name);

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
                            AILog.Log("TAKE_OVER moves calculated for Bonus " + bonus.Details.Name);
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
                        if (preventTakeOverMoves.Orders.Count > 0)
                        {
                            AILog.Log("PREVENT_TAKE_OVER moves calculated for Bonus " + bonus.Details.Name);
                        }
                        break;
                    }
                    else
                        throw new Exception("Unexpected plan");
                }
            }
        }

        // private void calculateXBonusMoves(Moves moves, BotTerritory.DeploymentType conservativeLevel) {
        // boolean solutionFound = true;
        // List<Bonus> alreadyHandledBonuses = new ArrayList<Bonus>();
        //
        // while (solutionFound) {
        // solutionFound = false;
        // BonusValueCalculator.calculatBonusValues(BotState.WorkingMap,
        // BotState.VisibleMap);
        //
        // TerritoryValueCalculator.calculateTerritoryValues(BotState.VisibleMap,
        // BotState.WorkingMap);
        // List<Bonus> BonusesToX = BonusValueCalculator
        // .getSortedBonusesAdjustedFactor(BotState.VisibleMap);
        // BonusesToX.removeAll(alreadyHandledBonuses);
        //
        // for (Bonus Bonus : BonusesToX) {
        // alreadyHandledBonuses.add(Bonus);
        // int stillAvailableDeployment = BotState.StartingArmies - moves.GetTotalDeployment();
        // String plan = BonusValueCalculator.getPlanForBonus(Bonus);
        // if (plan == "BREAK")) {
        // List<Territory> visibleTerritories = Bonus.GetVisibleOpponentTerritories();
        // Moves breakBonusMoves = BreakTerritoriesTask.calculateBreakTerritoriesTask(visibleTerritories,
        // stillAvailableDeployment, conservativeLevel);
        // if (breakBonusMoves != null) {
        // System.err.println("BREAK moves calculated for Bonus " + Bonus.ID);
        // MovesCommitter.committMoves(breakBonusMoves);
        // moves.mergeMoves(breakBonusMoves);
        // solutionFound = true;
        // break;
        // }
        // } else if (plan == "DEFEND")) {
        // Moves defendBonusMoves = DefendBonusTask.calculateDefendBonusTask(Bonus,
        // stillAvailableDeployment, false, conservativeLevel);
        // if (defendBonusMoves != null) {
        // if (defendBonusMoves.GetTotalDeployment ()> 0
        // || defendBonusMoves.attackTransferMoves.size() > 0) {
        // System.err.println("DEFEND moves calculated for Bonus " + Bonus.ID);
        // }
        // MovesCommitter.committMoves(defendBonusMoves);
        // moves.mergeMoves(defendBonusMoves);
        // solutionFound = true;
        // break;
        // }
        //
        // }
        //
        // else if (plan == "TAKE_OVER") /* && conservativeLevel == 1 */) {
        // Moves takeOverMoves = TakeBonusOverTask.calculateTakeBonusOverTask(
        // stillAvailableDeployment, Bonus, conservativeLevel);
        // if (takeOverMoves != null) {
        // System.err.println("TAKE_OVER moves calculated for Bonus " + Bonus.ID);
        // MovesCommitter.committMoves(takeOverMoves);
        // moves.mergeMoves(takeOverMoves);
        // solutionFound = true;
        // break;
        // }
        // } else if (plan == "PREVENT_TAKE_OVER")) {
        // Moves preventTakeOverMoves = PreventBonusTask.calculatePreventBonusTask(Bonus,
        // stillAvailableDeployment, conservativeLevel);
        // MovesCommitter.committMoves(preventTakeOverMoves);
        // moves.mergeMoves(preventTakeOverMoves);
        // solutionFound = true;
        // if (preventTakeOverMoves.GetTotalDeployment() > 0
        // || preventTakeOverMoves.attackTransferMoves.size() > 0) {
        // System.err.println("PREVENT_TAKE_OVER moves calculated for Bonus " + Bonus.ID);
        // }
        // break;
        // }
        // }
        // }
        // }
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
                AILog.Log("Sniped " + bestSnipableBonus.Details.Name);
            }
        }

        private void CalculateExpansionMoves(Moves moves, int maxValue, int minValue)
        {
            var armiesForExpansion = Math.Min(BotState.Settings.MinimumArmyBonus, BotState.MyIncome.Total - moves.GetTotalDeployment());
            var armiesForTakeOver = BotState.MyIncome.Total - moves.GetTotalDeployment();
            if (BotState.VisibleMap.GetOpponentBorderingTerritories().Count == 0)
                armiesForExpansion = BotState.MyIncome.Total - moves.GetTotalDeployment();

            AddValueToImmediateBonuses(armiesForTakeOver);
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            
            sortedAccessibleBonuses = sortedAccessibleBonuses.Where(bonus => bonus.GetExpansionValue() >= minValue && bonus.GetExpansionValue() <= maxValue).ToList();
            var bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(armiesForTakeOver);
            var takenOverBonuses = new List<BotBonus>();
            var armiesUsedForTakeOver = 0;
            foreach (var bonus in sortedAccessibleBonuses)
            {
                if (bonusesThatCanBeTaken.Contains(bonus))
                {
                    var expansionMoves = BotState.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(armiesForTakeOver, bonus.GetNotOwnedTerritories(), BotTerritory.DeploymentType.Normal, "MovesCalculator.CalculateExpansionMoves");
                    MovesCommitter.CommittMoves(BotState, expansionMoves);
                    moves.MergeMoves(expansionMoves);
                    armiesForTakeOver -= expansionMoves.GetTotalDeployment();
                    bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(armiesForTakeOver);
                    takenOverBonuses.Add(bonus);
                    armiesUsedForTakeOver += expansionMoves.GetTotalDeployment();
                }
                else
                    break;
            }
            var isExpandingAfterTakeOverSmart = true;
            if (BotState.VisibleMap.GetOpponentBorderingTerritories().Count > 0)
                isExpandingAfterTakeOverSmart = false;
            if (BotState.WorkingMap.GetOpponentBorderingTerritories().Count > 0)
                isExpandingAfterTakeOverSmart = false;


            var opponentBorderPresent = BotState.VisibleMap.GetOpponentBorderingTerritories().Count > 0;
            armiesForExpansion = Math.Max(0, armiesForExpansion - armiesUsedForTakeOver);
            if (takenOverBonuses.Count == 0 || isExpandingAfterTakeOverSmart)
            {
                BotBonus bonusToExpand = null;
                foreach (var bonus in sortedAccessibleBonuses)
                {
                    if (!takenOverBonuses.Contains(bonus))
                    {
                        var condition1 = bonus.GetVisibleNeutralTerritories().Count > 0;
                        var condition2 = bonus.Amount > 0;
                        var condition3 = !opponentBorderPresent || bonus.ExpansionValueCategory > 0;
                        if (condition1 && condition2 && condition3)
                        {
                            bonusToExpand = bonus;
                            break;
                        }
                    }
                }
                if (bonusToExpand == null)
                    return;
                var foundMoves = true;
                var firstStep = true;
                var debug = 0;
                while (foundMoves)
                {
                    BotState.BonusValueCalculator.CalculateBonusValues(BotState.WorkingMap, BotState.VisibleMap);
                    debug++;
                    foundMoves = false;
                    if (firstStep == false)
                    {
                        if (bonusToExpand.ExpansionValueCategory == 0)
                            return;
                        if (opponentBorderPresent)
                            armiesForExpansion = 0;
                        if (bonusToExpand.GetOpponentNeighbors().Count > 0)
                            return;
                        if (debug == 1)
                            return;
                    }
                    Moves oneStepMoves = null;
                    if (!opponentBorderPresent)
                        oneStepMoves = BotState.TakeTerritoriesTaskCalculator.CalculateOneStepExpandBonusTask(armiesForExpansion, bonusToExpand, false, BotState.WorkingMap, BotTerritory.DeploymentType.Normal);
                    else
                        oneStepMoves = BotState.TakeTerritoriesTaskCalculator.CalculateOneStepExpandBonusTask(armiesForExpansion, bonusToExpand, false, BotState.WorkingMap, BotTerritory.DeploymentType.Normal);

                    if (oneStepMoves != null)
                    {
                        firstStep = false;
                        armiesForExpansion -= oneStepMoves.GetTotalDeployment();
                        MovesCommitter.CommittMoves(BotState, oneStepMoves);
                        moves.MergeMoves(oneStepMoves);
                        foundMoves = true;
                    }
                }
            }
        }

        private void AddValueToImmediateBonuses(int maxDeployment)
        {
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            foreach (BotBonus bonus in sortedAccessibleBonuses)
            {
                if ((bonus.AreAllTerritoriesVisible()) && (!bonus.ContainsOpponentPresence()) && (bonus.Amount > 0) && !bonus.IsOwnedByMyself())
                {
                    var nonOwnedTerritories = bonus.GetNotOwnedTerritories();
                    var expansionMoves = BotState.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(maxDeployment, nonOwnedTerritories, BotTerritory.DeploymentType.Normal, "MovesCalculator.AddValueToImmediateBonuses");
                    if (expansionMoves != null)
                        BotState.BonusExpansionValueCalculator.AddExtraValueForFirstTurnBonus(bonus);
                }
            }
        }

        private List<BotBonus> GetBonusesThatCanBeTaken(int maxDeployment)
        {
            var outvar = new List<BotBonus>();
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            foreach (var bonus in sortedAccessibleBonuses)
            {
                if (bonus.AreAllTerritoriesVisible() && !bonus.ContainsOpponentPresence() && bonus.Amount > 0)
                {
                    var nonOwnedTerritories = bonus.GetNotOwnedTerritories();
                    var expansionMoves = BotState.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(maxDeployment, nonOwnedTerritories, BotTerritory.DeploymentType.Normal, "MovesCalculator.GetBonusesThatCanBeTaken");
                    if (expansionMoves != null)
                        outvar.Add(bonus);
                }
            }
            return outvar;
        }
    }
}
