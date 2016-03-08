/*
* This code was auto-converted from a java project.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using WarLight.AI.Wunderwaffe.Bot;



namespace WarLight.AI.Wunderwaffe.Evaluation
{
    /// <summary>This class is responsible for finding out which Bonuses to expand into.
    /// </summary>
    /// <remarks>
    /// This class is responsible for finding out which Bonuses to expand into. This happens by giving all Bonuses
    /// values. Furthermore this class is used during picking stage.
    /// </remarks>
    public class BonusExpansionValueCalculator
    {
        public BotMain BotState;
        public BonusExpansionValueCalculator(BotMain state)
        {
            this.BotState = state;
        }
        public List<BotBonus> SortBonuses(BotMap mapToUse, PlayerIDType playerID)
        {
            var allBonuses = mapToUse.Bonuses.Values.ToList();
            var sortedBonuses = new List<BotBonus>();
            //mapToUse.SetOpponentExpansionValue();
            while (!allBonuses.IsEmpty())
            {
                var bestBonus = allBonuses[0];
                double bestValue = 0;
                if (playerID == BotState.Me.ID)
                    bestValue = bestBonus.MyExpansionValueHeuristic.ExpansionValue;
                else
                {
                    bestValue = bestBonus.OpponentExpansionValueHeuristics[playerID].ExpansionValue;
                }
                foreach (BotBonus bonus in allBonuses)
                {
                    double value = 0;
                    if (playerID == BotState.Me.ID)
                        value = bonus.MyExpansionValueHeuristic.ExpansionValue;
                    else
                        value = bonus.OpponentExpansionValueHeuristics[playerID].ExpansionValue;

                    if (value > bestValue)
                    {
                        bestBonus = bonus;
                        bestValue = value;
                    }
                }
                allBonuses.Remove(bestBonus);
                sortedBonuses.Add(bestBonus);
            }
            return sortedBonuses;
        }

        public HashSet<BotBonus> SortAccessibleBonuses(BotMap mapToUse)
        {
            var copy = new List<BotBonus>();
            copy.AddRange(mapToUse.Bonuses.Values);
            var outvar = new HashSet<BotBonus>();
            while (!copy.IsEmpty())
            {
                var highestPrioBonus = copy[0];
                foreach (BotBonus bonus in copy)
                {
                    if (bonus.GetExpansionValue() > highestPrioBonus.GetExpansionValue())
                        highestPrioBonus = bonus;
                }
                copy.Remove(highestPrioBonus);
                outvar.Add(highestPrioBonus);
            }
            // Remove the non accessible Bonuses
            List<BotBonus> nonAccessibleBonuses = new List<BotBonus>();
            foreach (BotBonus bonus_1 in mapToUse.Bonuses.Values)
            {
                if (bonus_1.GetOwnedTerritoriesAndNeighbors().Count == 0)
                    nonAccessibleBonuses.Add(bonus_1);
            }
            outvar.RemoveAll(nonAccessibleBonuses);
            return outvar;
        }

        public void AddExtraValueForFirstTurnBonus(BotBonus bonus)
        {
            bonus.MyExpansionValueHeuristic.AddExtraValueForFirstTurnBonus(bonus);
        }

        /// <summary>Classifies the Bonus according to the intel from the temporaryMap.
        /// </summary>
        /// <remarks>
        /// Classifies the Bonus according to the intel from the temporaryMap. However the results of the classification aren't written to the temporary map but to the visible map.
        /// </remarks>
        /// <param name="temporaryMap"></param>
        public void ClassifyBonuses(BotMap temporaryMap, BotMap mapToWriteIn)
        {
            foreach (var bonus in temporaryMap.Bonuses.Values)
            {
                bonus.SetMyExpansionValueHeuristic();
                foreach(var opponent in BotState.Opponents)
                    bonus.SetOpponentExpansionValueHeuristic(opponent.ID);

                // Categorize the expansion values. Possible values are 0 = rubbish and 1 = good
                var toMuchNeutrals = false;
                var neutralArmies = bonus.NeutralArmies.DefensePower;
                if (neutralArmies > 14)
                    toMuchNeutrals = true;
                else if (neutralArmies >= 10 && bonus.Amount <= 3)
                    toMuchNeutrals = true;
                else if (neutralArmies >= 8 && bonus.Amount <= 2)
                    toMuchNeutrals = true;
                else if (neutralArmies >= 6 && bonus.Amount <= 1)
                    toMuchNeutrals = true;

                if (bonus.IsOwnedByMyself() || bonus.Amount == 0 || bonus.ContainsOpponentPresence() || toMuchNeutrals)
                    mapToWriteIn.Bonuses[bonus.ID].ExpansionValueCategory = 0;
                else
                    mapToWriteIn.Bonuses[bonus.ID].ExpansionValueCategory = 1;
            }
        }
    }
}
