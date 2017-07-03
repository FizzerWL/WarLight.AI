using System;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    public class ExpansionTask
    {
        public BotMain BotState;
        public ExpansionTask(BotMain state)
        {
            this.BotState = state;
        }


        // TODO
        public Moves CalculateExpansionTask(int maxDeployment)
        {
            /*Moves expansionMoves = */new Moves();
            return null;
        }



        public void CalculateExpansionMoves(Moves moves)
        {
            var armiesForExpansion = Math.Min(BotState.Settings.MinimumArmyBonus, BotState.MyIncome.Total - moves.GetTotalDeployment());
            var armiesForTakeOver = BotState.MyIncome.Total - moves.GetTotalDeployment();
            if (BotState.VisibleMap.GetOpponentBorderingTerritories().Count == 0)
            {
                armiesForExpansion = BotState.MyIncome.Total - moves.GetTotalDeployment();
            }

            AddValueToImmediateBonuses(armiesForTakeOver);
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            var bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(armiesForTakeOver);
            var takenOverBonuses = new List<BotBonus>();
            var armiesUsedForTakeOver = 0;

            // calculate the moves for the bonuses that can be immediately taken
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
                {
                    break;
                }
            }


            var isExpandingAfterTakeOverSmart = IsExpandingAfterTakeOverSmart();


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
                {
                    return;
                }
                var foundMoves = true;
                var firstStep = true;
                while (foundMoves)
                {
                    BotState.BonusValueCalculator.CalculateBonusValues(BotState.WorkingMap, BotState.VisibleMap);
                    foundMoves = false;
                    if (firstStep == false)
                    {
                        if (bonusToExpand.ExpansionValueCategory == 0)
                            return;
                        if (opponentBorderPresent)
                            armiesForExpansion = 0;
                        if (bonusToExpand.GetOpponentNeighbors().Count > 0)
                            return;
                    }
                    Moves oneStepMoves = null;
                    if (!opponentBorderPresent)
                        oneStepMoves = BotState.TakeTerritoriesTaskCalculator.CalculateOneStepExpandBonusTask(armiesForExpansion, bonusToExpand, true, BotState.WorkingMap, BotTerritory.DeploymentType.Normal);
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

        private Boolean IsExpandingAfterTakeOverSmart()
        {
            Boolean isSmart = true;
            if (BotState.VisibleMap.GetOpponentBorderingTerritories().Count > 0)
            {
                isSmart = false;
            }
            if (BotState.WorkingMap.GetOpponentBorderingTerritories().Count > 0)
            {
                isSmart = false;
            }
            return isSmart;
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