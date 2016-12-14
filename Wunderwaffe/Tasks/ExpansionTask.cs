using System;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    public class ExpansionTask
    {
        public BotMain BotState;
        private List<BotBonus> takeOverBonuses = new List<BotBonus>();
        public List<BotBonus> expandBonuses = new List<BotBonus>();

        public ExpansionTask(BotMain state)
        {
            this.BotState = state;
        }

        public void CalculateExpansionMoves(Moves moves)
        {
            int armiesForExpansion = BotState.MyIncome.Total - moves.GetTotalDeployment();
            AddValueToImmediateBonuses(armiesForExpansion);
            CalculateTakeOverMoves(moves);
            CalculateNonTakeOverMoves(moves);
            CalculateNullTakeOverMoves(moves);
        }

        private void CalculateNonTakeOverMoves(Moves moves)
        {
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            int armiesForExpansion = BotState.MyIncome.Total - moves.GetTotalDeployment();
            bool isExpandingAfterTakeOverSmart = IsExpandingAfterTakeOverSmart();
            bool opponentBorderPresent = BotState.VisibleMap.GetOpponentBorderingTerritories().Count > 0;

            if (takeOverBonuses.Count == 0 || isExpandingAfterTakeOverSmart)
            {
                BotBonus bonusToExpand = null;
                foreach (var bonus in sortedAccessibleBonuses)
                {
                    if (!takeOverBonuses.Contains(bonus))
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
                    {
                        oneStepMoves = BotState.TakeTerritoriesTaskCalculator.CalculateOneStepExpandBonusTask(armiesForExpansion, bonusToExpand, true, BotState.WorkingMap, BotTerritory.DeploymentType.Normal);

                    }
                    else
                    {
                        oneStepMoves = BotState.TakeTerritoriesTaskCalculator.CalculateOneStepExpandBonusTask(armiesForExpansion, bonusToExpand, false, BotState.WorkingMap, BotTerritory.DeploymentType.Normal);
                    }

                    if (oneStepMoves != null)
                    {
                        if (!expandBonuses.Contains(bonusToExpand))
                        {
                            expandBonuses.Add(bonusToExpand);
                        }
                        firstStep = false;
                        armiesForExpansion -= oneStepMoves.GetTotalDeployment();
                        MovesCommitter.CommittMoves(BotState, oneStepMoves);
                        moves.MergeMoves(oneStepMoves);
                        foundMoves = true;
                    }
                }
            }
        }

        private void CalculateNullTakeOverMoves(Moves moves)
        {
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            var bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(0);
            foreach (var bonus in sortedAccessibleBonuses)
            {
                if (bonusesThatCanBeTaken.Contains(bonus) && bonus.GetOpponentNeighbors().Count == 0)
                {
                    Moves expansionMoves = getTakeOverMoves(0, bonus);
                    MovesCommitter.CommittMoves(BotState, expansionMoves);
                    moves.MergeMoves(expansionMoves);
                    bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(0);
                    takeOverBonuses.Add(bonus);
                }
            }
        }

        private void CalculateTakeOverMoves(Moves moves)
        {
            int armiesForExpansion = BotState.MyIncome.Total - moves.GetTotalDeployment();
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            var bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(armiesForExpansion);
            foreach (var bonus in sortedAccessibleBonuses)
            {
                if (bonusesThatCanBeTaken.Contains(bonus))
                {
                    Moves expansionMoves = getTakeOverMoves(armiesForExpansion, bonus);
                    MovesCommitter.CommittMoves(BotState, expansionMoves);
                    moves.MergeMoves(expansionMoves);
                    armiesForExpansion -= expansionMoves.GetTotalDeployment();
                    bonusesThatCanBeTaken = GetBonusesThatCanBeTaken(armiesForExpansion);
                    takeOverBonuses.Add(bonus);
                }
                else
                {
                    break;
                }
            }
        }


        private Moves getTakeOverMoves(int armiesForExpansion, BotBonus bonus)
        {
            Moves expansionMoves = BotState.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(armiesForExpansion, bonus.GetNotOwnedTerritories(), BotTerritory.DeploymentType.Normal, "MovesCalculator.CalculateExpansionMoves");
            return expansionMoves;
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
            outvar.RemoveAll(o => takeOverBonuses.Contains(o));
            outvar.RemoveAll(o => expandBonuses.Contains(o));
            return outvar;
        }



    }
}