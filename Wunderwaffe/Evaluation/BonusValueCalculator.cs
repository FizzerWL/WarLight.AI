using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public enum BonusPlan
    {
        None, Break, Defend, TakeOver, PreventTakeOver
    }

    /// <summary>Calculates the Bonus values for break, defend, take over, prevent take over and expand
    /// </summary>
    public class BonusValueCalculator
    {
        public BotMain BotState;
        public BonusValueCalculator(BotMain state)
        {
            this.BotState = state;
        }

        public BonusPlan GetPlanForBonus(BotBonus bonus)
        {
            var highestValue = -1;
            var plan = BonusPlan.None;
            if (bonus.AttackValue > highestValue)
            {
                highestValue = bonus.AttackValue;
                plan = BonusPlan.Break;
            }
            if (bonus.DefenseValue > highestValue)
            {
                highestValue = bonus.DefenseValue;
                plan = BonusPlan.Defend;
            }
            if (bonus.TakeOverValue > highestValue)
            {
                highestValue = bonus.TakeOverValue;
                plan = BonusPlan.TakeOver;
            }
            if (bonus.PreventTakeOverValue > highestValue)
            {
                highestValue = bonus.PreventTakeOverValue;
                plan = BonusPlan.PreventTakeOver;
            }
            return plan;
        }

        /// <param name="mapToUse">visible map</param>
        /// <returns>Only returns the Bonuses with an adjusted factor of &gt; 0.</returns>
        public List<BotBonus> GetSortedBonusesAdjustedFactor(BotMap mapToUse)
        {
            var outvar = new List<BotBonus>();
            var copy = new List<BotBonus>();
            foreach (BotBonus bonus in mapToUse.Bonuses.Values)
            {
                if (GetAdjustedFactor(bonus) > 0)
                    copy.Add(bonus);
            }
            while (copy.Count != 0)
            {
                var bestBonus = copy[0];
                foreach (BotBonus bonus_1 in copy)
                {
                    if (GetAdjustedFactor(bonus_1) > GetAdjustedFactor(bestBonus))
                        bestBonus = bonus_1;
                }
                copy.Remove(bestBonus);
                outvar.Add(bestBonus);
            }
            return outvar;
        }

        private int GetAdjustedFactor(BotBonus bonus)
        {
            var adjustedFactor = Math.Max(bonus.PreventTakeOverValue, Math.Max(bonus.TakeOverValue, Math.Max(bonus.AttackValue, bonus.DefenseValue)));
            return adjustedFactor;
        }

        private const int BonusAttackFactor = 15;
        private const int BonusDefenseFactor = 10;
        private const int BonusTakeOverFactor = 4;
        private const int BonusPreventTakeOverFactor = 15;

        public void CalculateBonusValues(BotMap mapToUse, BotMap mapToWriteIn)
        {
            var ownBonusesUnderAttack = new List<BotBonus>();
            var opponentBonusesUnderAttack = new List<BotBonus>();
            var bonusesWeCanTakeOver = new List<BotBonus>();
            var bonusesOpponentCanTakeOver = new List<BotBonus>();
            // Classify the Bonuses
            foreach (var bonus in mapToUse.Bonuses.Values)
            {
                if (bonus.Amount > 0)
                {
                    if (bonus.IsOwnedByMyself() && bonus.GetOpponentNeighbors().Count > 0)
                        ownBonusesUnderAttack.Add(bonus);
                    if (bonus.IsOwnedByAnyOpponent() && bonus.GetOwnedNeighborTerritories().Count > 0)
                        opponentBonusesUnderAttack.Add(bonus);
                    var vmBonus = BotState.VisibleMap.Bonuses[bonus.ID];
                    if (vmBonus.CanTakeOver())
                        bonusesWeCanTakeOver.Add(bonus);
                    if (vmBonus.CanOpponentTakeOver())
                        bonusesOpponentCanTakeOver.Add(bonus);
                }
            }
            // Calculate the values
            foreach (var bonus_1 in ownBonusesUnderAttack)
                CalculateDefenseValue(bonus_1, mapToWriteIn);
            foreach (var bonus_2 in opponentBonusesUnderAttack)
                CalculateAttackValue(bonus_2, mapToWriteIn);
            foreach (var bonus_3 in bonusesWeCanTakeOver)
                CalculateTakeOverValue(bonus_3, mapToWriteIn);
            foreach (var bonus_4 in bonusesOpponentCanTakeOver)
                CalculatePreventValue(bonus_4, mapToWriteIn);
            // Set the factors to -1 where not possible or makes no sense (0 income)
            foreach (BotBonus bonus in mapToUse.Bonuses.Values)
            {
                var bonusToWriteIn = mapToWriteIn.Bonuses[bonus.ID];
                if (!ownBonusesUnderAttack.Contains(bonus))
                    bonusToWriteIn.DefenseValue = -1;
                if (!opponentBonusesUnderAttack.Contains(bonus))
                    bonusToWriteIn.AttackValue = -1;
                if (!bonusesWeCanTakeOver.Contains(bonus))
                    bonusToWriteIn.TakeOverValue = -1;
                if (!bonusesOpponentCanTakeOver.Contains(bonus))
                    bonusToWriteIn.PreventTakeOverValue = -1;
            }
        }

        private void CalculateDefenseValue(BotBonus bonus, BotMap mapToWriteIn)
        {
            var armiesReward = bonus.Amount;
            var opponentNeighborTerritories = bonus.GetOpponentNeighbors();
            var opponentNeighbors = opponentNeighborTerritories.Count;
            var territoriesUnderThreat = bonus.GetOwnedTerritoriesBorderingNeighborsOwnedByOpponent();
            var amountOfTerritoriesUnderThreat = territoriesUnderThreat.Count;
            var opponentArmies = 0;
            foreach (var opponentNeighbor in opponentNeighborTerritories)
                opponentArmies += opponentNeighbor.Armies.AttackPower;

            var ownArmies = 0;
            foreach (var territoryUnderThread in territoriesUnderThreat)
                ownArmies += territoryUnderThread.GetArmiesAfterDeploymentAndIncomingMoves().DefensePower;

            var ownedNeighborTerritories = bonus.GetOwnedNeighborTerritories().Count;
            var amountTerritories = bonus.Territories.Count;
            var defenseValue = 0;
            defenseValue += armiesReward * 10000;
            defenseValue += opponentArmies * -10;
            defenseValue += ownArmies * 10;
            defenseValue += ownedNeighborTerritories * 1;
            defenseValue += opponentNeighbors * -100;
            defenseValue += amountOfTerritoriesUnderThreat * -1000;
            defenseValue += amountTerritories * -1;
            defenseValue *= BonusDefenseFactor;
            var bonusToWriteIn = mapToWriteIn.Bonuses[bonus.ID];
            bonusToWriteIn.DefenseValue = defenseValue;
            // TODO hack so Bonuses we are taking this turn don't already get a defense value of > 0
            if (!BotState.VisibleMap.Bonuses[bonus.ID].IsOwnedByMyself())
                bonusToWriteIn.DefenseValue = -1;
        }

        private void CalculateAttackValue(BotBonus bonus, BotMap mapToWriteIn)
        {
            var armiesReward = bonus.Amount;
            var ownedNeighbors = bonus.GetOwnedNeighborTerritories();
            var amountOwnedNeighbors = ownedNeighbors.Count;
            var territoriesUnderAttack = bonus.GetVisibleOpponentTerritories();
            var amountTerritoriesUnderAttack = territoriesUnderAttack.Count;
            var opponentArmies = 0;
            foreach (var opponentTerritory in territoriesUnderAttack)
                opponentArmies += opponentTerritory.Armies.DefensePower;
            var ownArmies = 0;
            foreach (var ownedNeighbor in ownedNeighbors)
                ownArmies += ownedNeighbor.GetIdleArmies().AttackPower;
            var opponentNeighbors = bonus.GetOpponentNeighbors().Count;
            var attackValue = 0;
            attackValue += armiesReward * 10000;
            attackValue += opponentArmies * -1;
            attackValue += ownArmies * 3;
            attackValue += amountOwnedNeighbors * 100;
            attackValue += opponentNeighbors * 1;
            attackValue += amountTerritoriesUnderAttack * 1000;
            attackValue *= BonusAttackFactor;
            var bonusToWriteIn = mapToWriteIn.Bonuses[bonus.ID];
            bonusToWriteIn.AttackValue = attackValue;
        }

        private void CalculateTakeOverValue(BotBonus bonus, BotMap mapToWriteIn)
        {
            var armiesReward = bonus.Amount;
            var opponentTerritories = bonus.GetOpponentTerritories();
            var amountOpponentTerritories = opponentTerritories.Count;
            var opponentArmies = 0;
            foreach (var opponentTerritory in opponentTerritories)
                opponentArmies += opponentTerritory.Armies.DefensePower;

            var possibleAttackTerritories = new HashSet<BotTerritory>();
            foreach (var opponentTerritory_1 in opponentTerritories)
                possibleAttackTerritories.AddRange(opponentTerritory_1.GetOwnedNeighbors());

            var ownArmies = 0;
            foreach (var possibleAttackTerritory in possibleAttackTerritories)
                ownArmies += possibleAttackTerritory.GetIdleArmies().AttackPower;

            var amountTerritories = bonus.Territories.Count;
            var opponentNeighbors = bonus.GetOpponentNeighbors().Count;
            var takeOverValue = 0;
            takeOverValue += armiesReward * 10000;
            takeOverValue += opponentArmies * -10;
            takeOverValue += ownArmies * 10;
            takeOverValue += opponentNeighbors * -1;
            takeOverValue += amountOpponentTerritories * -1000;
            takeOverValue += amountTerritories * -1;
            takeOverValue *= BonusTakeOverFactor;
            mapToWriteIn.Bonuses[bonus.ID].TakeOverValue = takeOverValue;
        }

        private void CalculatePreventValue(BotBonus bonus, BotMap mapToWriteIn)
        {
            var ownedTerritories = bonus.GetOwnedTerritories();
            var ownArmies = 0;
            foreach (var ownedTerritory in ownedTerritories)
                ownArmies += ownedTerritory.Armies.DefensePower;

            GamePlayer maxOpponent = null;
            int maxPreventValue = int.MinValue;

            foreach (var opponent in BotState.Opponents)
            {
                var val = CalculatePreventValue(bonus, ownedTerritories, opponent, ownArmies);
                if (val > maxPreventValue)
                {
                    maxPreventValue = val;
                    maxOpponent = opponent;
                }
            }

            if (maxOpponent != null)
            {
                var b = mapToWriteIn.Bonuses[bonus.ID];
                b.PreventTakeOverValue = maxPreventValue;
                b.PreventTakeOverOpponent = maxOpponent.ID;
            }
        }

        private int CalculatePreventValue(BotBonus bonus, List<BotTerritory> ownedTerritories, GamePlayer opponent, int ownArmies)
        {
            var attackingTerritories = new HashSet<BotTerritory>();

            foreach (var ownedTerritory in ownedTerritories)
                attackingTerritories.AddRange(ownedTerritory.Neighbors.Where(o => o.OwnerPlayerID == opponent.ID));

            var opponentArmies = 0;
            foreach (var territory in attackingTerritories)
                opponentArmies += territory.Armies.AttackPower;

            var amountTerritories = bonus.Territories.Count;
            var preventValue = 0;
            preventValue += bonus.Amount * 10000;
            preventValue += opponentArmies * -10;
            preventValue += ownArmies * 10;
            preventValue += ownedTerritories.Count * 1000;
            preventValue += amountTerritories * -1;
            preventValue *= BonusPreventTakeOverFactor;

            return preventValue;
        }
    }
}
