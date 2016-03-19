using System;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <remarks>
    /// This class is responsible for preventing the opponent from expanding in a Bonus in which we have no foothold  yet. Preventing the opponent from expanding can happen either by attacking a neutral territory there or by directly attacking the opponent there.
    /// </remarks>
    public class PreventOpponentExpandBonusTask
    {
        public BotMain BotState;
        public PreventOpponentExpandBonusTask(BotMain state)
        {
            this.BotState = state;
        }

        public BotBonus GetBestBonusToPrevent(BotMap visibleMap, BotMap movesMap)
        {
            var possiblePreventableBonuses = new List<BotBonus>();
            foreach (var bonus in visibleMap.Bonuses.Values)
            {
                if (IsPreventingUseful(visibleMap, movesMap, bonus))
                    possiblePreventableBonuses.Add(bonus);
            }
            var ourInterestingBonuses = GetOurInterestingBonuses();
            var ourMaxReward = 0;
            foreach (var ourBonus in ourInterestingBonuses)
            {
                if (ourBonus.Amount > ourMaxReward)
                    ourMaxReward = ourBonus.Amount;
            }
            var opponentMaxReward = 0;
            BotBonus bestOpponentBonus = null;
            foreach (var opponentBonus in possiblePreventableBonuses)
            {
                if (opponentBonus.Amount > opponentMaxReward)
                {
                    bestOpponentBonus = opponentBonus;
                    opponentMaxReward = opponentBonus.Amount;
                }
                else if (opponentBonus.Amount == opponentMaxReward && opponentBonus.NeutralArmies.DefensePower < bestOpponentBonus.NeutralArmies.DefensePower)
                    bestOpponentBonus = opponentBonus;
            }
            foreach (var bonus in possiblePreventableBonuses)
            {
                if (bonus != bestOpponentBonus && bonus.Amount == bestOpponentBonus.Amount && bonus.NeutralArmies.DefensePower == bestOpponentBonus.NeutralArmies.DefensePower)
                {
                    return null;
                }
            }
            if (opponentMaxReward > ourMaxReward)
                return bestOpponentBonus;
            else
                return null;
        }

        public Moves CalculatePreventOpponentExpandBonusTaskk(BotBonus bonusToPrevent, int maxDeployment, BotMap visibleMap, BotMap mapSoFar)
        {
            Moves outvar = null;
            if (!IsPreventingUseful(visibleMap, mapSoFar, bonusToPrevent))
                return null;

            // Step 1: Try to hit the opponent directly
            var possibleBreakTerritories = new List<BotTerritory>();
            possibleBreakTerritories.AddRange(bonusToPrevent.GetVisibleOpponentTerritories());
            outvar = BreakTerritoriesTask.CalculateBreakTerritoriesTask(BotState, possibleBreakTerritories, maxDeployment, BotTerritory.DeploymentType.Normal, BotTerritory.DeploymentType.Normal);
            if (outvar != null)
            {
                return outvar;
            }
            // Step 2: Try to hit a neutral territory there
            var attackableNeutrals = new List<BotTerritory>();
            attackableNeutrals.AddRange(bonusToPrevent.GetVisibleNeutralTerritories());
            var sortedAttackableNeutrals = new List<BotTerritory>();
            while (attackableNeutrals.Count != 0)
            {
                var bestNeutral = attackableNeutrals[0];
                foreach (var neutral in attackableNeutrals)
                {
                    if (neutral.AttackTerritoryValue > bestNeutral.AttackTerritoryValue)
                        bestNeutral = neutral;
                }
                sortedAttackableNeutrals.Add(bestNeutral);
                attackableNeutrals.Remove(bestNeutral);
            }
            foreach (var attackableNeutral in sortedAttackableNeutrals)
            {
                var attackTerritoryMoves = CalculateAttackNeutralMoves(attackableNeutral, maxDeployment);
                if (attackTerritoryMoves != null)
                {
                    return attackTerritoryMoves;
                }
            }
            return null;
        }

        private List<BotBonus> GetOurInterestingBonuses()
        {
            var sortedAccessibleBonuses = BotState.BonusExpansionValueCalculator.SortAccessibleBonuses(BotState.VisibleMap);
            var outvar = new List<BotBonus>();
            foreach (var bonus in sortedAccessibleBonuses)
            {
                if ((bonus.AreAllTerritoriesVisible()) && (!bonus.ContainsOpponentPresence()) && (bonus.Amount > 0) && !bonus.IsOwnedByMyself() && bonus.NeutralArmies.DefensePower <= 4)
                    outvar.Add(bonus);
            }
            return outvar;
        }

        private Moves CalculateAttackNeutralMoves(BotTerritory neutralTerritory, int maxDeployment)
        {
            Moves outvar = null;
            var neededAttackArmies = neutralTerritory.getNeededBreakArmies(neutralTerritory.Armies.DefensePower);
            var ownedNeighbors = neutralTerritory.GetOwnedNeighbors();
            var bestNeighbor = ownedNeighbors[0];
            foreach (var ownedNeighbor in ownedNeighbors)
            {
                if (ownedNeighbor.GetIdleArmies().AttackPower > bestNeighbor.GetIdleArmies().ArmiesOrZero)
                    bestNeighbor = ownedNeighbor;
            }
            var neededDeployment = Math.Max(0, neededAttackArmies - bestNeighbor.GetIdleArmies().ArmiesOrZero);
            if (neededDeployment <= maxDeployment)
            {
                outvar = new Moves();
                if (neededDeployment > 0)
                    outvar.AddOrder(new BotOrderDeploy(BotState.Me.ID, bestNeighbor, neededDeployment));

                var atm = new BotOrderAttackTransfer(BotState.Me.ID, bestNeighbor, neutralTerritory, new Armies(neededAttackArmies), "PreventOpponentExpandBonusTask");
                // TODO bad?
                if (bestNeighbor.GetOpponentNeighbors().Count == 0)
                    atm.Message = AttackMessage.Snipe;

                outvar.AddOrder(atm);
            }
            return outvar;
        }

        private bool IsPreventingUseful(BotMap visibleMap, BotMap movesMap, BotBonus bonus)
        {
            if (bonus.GetOwnedTerritoriesAndNeighbors().Count == 0)
                return false;

            var mmBonus = movesMap.Bonuses[bonus.ID];
            if (mmBonus.GetOwnedTerritories().Count > 0)
                return false;
            if (bonus.IsOwnedByAnyOpponent())
                return false;
            if (bonus.GetOwnedNeighborTerritories().Count == 0)
                return false;
            if (bonus.Amount < 2)
                return false;
            if (bonus.NeutralArmies.DefensePower > 4)
                return false;
            foreach (var neutralTerritory in bonus.NeutralTerritories)
                if (neutralTerritory.GetOpponentNeighbors().Count == 0)
                    return false;

            return true;
        }
    }
}
