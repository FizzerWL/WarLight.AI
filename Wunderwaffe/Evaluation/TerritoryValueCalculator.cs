using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    /// <summary>This class is responsible for calculating territory values for each territory.
    /// </summary>
    /// <remarks>
    /// This class is responsible for calculating territory values for each territory. With this information valid decisions can be
    /// made where to expand, which territories to defend and which opponent territories to attack.
    /// </remarks>
    public class TerritoryValueCalculator
    {
        public BotMain BotState;
        public TerritoryValueCalculator(BotMain state)
        {
            this.BotState = state;
        }
        public const int LOWEST_MEDIUM_PRIORITY_VALUE = 1000;

        public const int LOWEST_HIGH_PRIORITY_VALUE = 1000000;

        private const int ATTACK_ADJUSTMENT_FACTOR = 4;

        private const int DEFENSE_ADJUSTMENT_FACTOR = 4;

        // TODO: was 3
        /// <summary>Sorts the territories from high priority to low priority.</summary>
        /// <remarks>
        /// Sorts the territories from high priority to low priority. If it's an opponent territory then the AttackValue is used and
        /// if it's an owned territory the defense value is used.
        /// </remarks>
        /// <param name="unsortedTerritories">opponent bordering territories and opponent territories</param>
        /// <returns>sorted territories</returns>
        public List<BotTerritory> SortTerritoriesAttackDefense(List<BotTerritory> unsortedTerritories
        )
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var copy = new List<BotTerritory>();
            copy.AddRange(unsortedTerritories);
            while (copy.Count != 0)
            {
                var mostImportantTerritory = copy[0];
                var mostImportantTerritoryValue = Math.Max(mostImportantTerritory.AttackTerritoryValue, mostImportantTerritory.DefenceTerritoryValue);
                foreach (var territory in copy)
                {
                    var territoryValue = Math.Max(territory.AttackTerritoryValue, territory.DefenceTerritoryValue);
                    if (territoryValue > mostImportantTerritoryValue)
                    {
                        mostImportantTerritory = territory;
                        mostImportantTerritoryValue = territoryValue;
                    }
                }
                copy.Remove(mostImportantTerritory);
                outvar.Add(mostImportantTerritory);
            }
            return outvar;
        }

        /// <returns>sorted visible neutral territories with a flanking value of &gt; 0</returns>
        public List<BotTerritory> GetSortedFlankingValueTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var visibleNeutrals = BotState.VisibleMap.GetVisibleNeutralTerritories();
            List<BotTerritory> visibleFlankingTerritories = new List<BotTerritory>();
            foreach (var visibleNeutral in visibleNeutrals)
            {
                if (visibleNeutral.FlankingTerritoryValue > 0)
                {
                    visibleFlankingTerritories.Add(visibleNeutral);
                }
            }
            while (visibleFlankingTerritories.Count != 0)
            {
                var bestTerritory = visibleFlankingTerritories[0];
                foreach (var territory in visibleFlankingTerritories)
                {
                    if (territory.FlankingTerritoryValue > bestTerritory.FlankingTerritoryValue)
                        bestTerritory = territory;
                }
                outvar.Add(bestTerritory);
                visibleFlankingTerritories.Remove(bestTerritory);
            }
            return outvar;
        }

        public List<BotTerritory> GetSortedAttackValueTerritories()
        {
            var outvar = new List<BotTerritory>();
            var opponentTerritories = BotState.VisibleMap.Territories.Values.Where(o => BotState.IsOpponent(o.OwnerPlayerID)).ToList();
            var copy = new List<BotTerritory>();
            copy.AddRange(opponentTerritories);
            while (copy.Count != 0)
            {
                var maxAttackValue = 0;
                var maxAttackValueTerritory = copy[0];
                foreach (var territory in copy)
                {
                    if (territory.AttackTerritoryValue > maxAttackValue)
                    {
                        maxAttackValue = territory.AttackTerritoryValue;
                        maxAttackValueTerritory = territory;
                    }
                }
                copy.Remove(maxAttackValueTerritory);
                outvar.Add(maxAttackValueTerritory);
            }
            return outvar;
        }

        public List<BotTerritory> GetSortedAttackTerritories(BotTerritory fromTerritory)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var sortedOpponentTerritories = GetSortedAttackValueTerritories();
            foreach (var opponentTerritory in sortedOpponentTerritories)
            {
                if (fromTerritory.Neighbors.Contains(opponentTerritory))
                {
                    outvar.Add(opponentTerritory);
                }
            }
            return outvar;
        }

        /// <summary>Only returns the territories next to the opponent.</summary>
        /// <remarks>Only returns the territories next to the opponent.</remarks>
        /// <returns></returns>
        public List<BotTerritory> GetSortedDefenceValueTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var opponentBorderingTerritories = BotState.VisibleMap.GetOpponentBorderingTerritories();
            var copy = new List<BotTerritory>();
            copy.AddRange(opponentBorderingTerritories);
            while (copy.Count != 0)
            {
                var maxDefenceValue = 0;
                var maxDefenceValueTerritory = copy[0];
                foreach (var territory in copy)
                {
                    if (territory.DefenceTerritoryValue > maxDefenceValue)
                    {
                        maxDefenceValue = territory.DefenceTerritoryValue;
                        maxDefenceValueTerritory = territory;
                    }
                }
                copy.Remove(maxDefenceValueTerritory);
                outvar.Add(maxDefenceValueTerritory);
            }
            return outvar;
        }

        public List<BotTerritory> SortAttackValue(List<BotTerritory> inTerritories)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var sortedAttackTerritories = GetSortedAttackValueTerritories();
            foreach (var territory in sortedAttackTerritories)
            {
                if (inTerritories.Contains(territory))
                {
                    outvar.Add(territory);
                }
            }
            return outvar;
        }

        /// <summary>If an inTerritory isn't next to an opponent it isn't returned</summary>
        /// <param name="inTerritories"></param>
        /// <returns></returns>
        public List<BotTerritory> SortDefenseValue(List<BotTerritory> inTerritories)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var sortedDefenceTerritories = GetSortedDefenceValueTerritories();
            foreach (var territory in sortedDefenceTerritories)
            {
                if (inTerritories.Contains(territory))
                {
                    outvar.Add(territory);
                }
            }
            return outvar;
        }

        public List<BotTerritory> SortDefenseValueFullReturn(List<BotTerritory> inTerritories)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var copy = new List<BotTerritory>();
            copy.AddRange(inTerritories);
            while (copy.Count > 0)
            {
                var highestPrioTerritory = copy[0];
                foreach (var territory in copy)
                {
                    if (territory.DefenceTerritoryValue > highestPrioTerritory.DefenceTerritoryValue)
                    {
                        highestPrioTerritory = territory;
                    }
                }
                copy.Remove(highestPrioTerritory);
                outvar.Add(highestPrioTerritory);
            }
            return outvar;
        }

        public List<BotTerritory> SortExpansionValue(List<BotTerritory> inTerritories)
        {
            return inTerritories.OrderByDescending(o => o.ExpansionTerritoryValue).ToList();
        }
        
        /// <remarks>Calculates the territory values.</remarks>
        /// <param name="mapToWriteIn">the map in which the territory values are to be inserted</param>
        /// <param name="mapToUse">the map to use for calculating the values</param>
        public void CalculateTerritoryValues(BotMap mapToWriteIn, BotMap mapToUse)
        {
            foreach (BotBonus bonus in mapToUse.Bonuses.Values)
            {
                bonus.SetMyExpansionValueHeuristic();
            }
            foreach (var territory in mapToUse.Territories.Values)
            {
                if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                {
                    CalculateExpansionTerritoryValue(territory, mapToWriteIn);
                    CalculateFlankingValue(territory, mapToWriteIn, mapToUse);
                }
                else if (IsOwnedBorderTerritory(territory.ID))
                    CalculateDefenseTerritoryValue(territory, mapToWriteIn);

                if (IsOpponentTerritory(territory.ID) || IsNeutralTerritory(territory.ID))
                {
                    CalculateAttackTerritoryValue(territory, mapToWriteIn);
                }
            }
        }

        private bool IsNeutralTerritory(TerritoryIDType territoryId)
        {
            var territory = BotState.VisibleMap.Territories[territoryId];
            return territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID;
        }

        private bool IsOpponentTerritory(TerritoryIDType territoryId)
        {
            var territory = BotState.VisibleMap.Territories[territoryId];
            return BotState.IsOpponent(territory.OwnerPlayerID);
        }

        private bool IsOwnedBorderTerritory(TerritoryIDType territoryId)
        {
            var territory = BotState.VisibleMap.Territories[territoryId];
            return territory.OwnerPlayerID == BotState.Me.ID && territory.GetOpponentNeighbors().Count > 0;
        }

        /// <remarks>Calculates the attack territory values.</remarks>
        /// <param name="territory">allowed are neutral and opponent territories</param>
        public void CalculateAttackTerritoryValue(BotTerritory territory, BotMap mapToWriteIn
        )
        {
            var currentValue = 0;
            // Add 100.000 * armies reward to the value if it's a spot in an
            // opponent Bonus
            if (territory.Bonuses.Any(o => o.IsOwnedByAnyOpponent()))
            {
                currentValue += LOWEST_HIGH_PRIORITY_VALUE * territory.Bonuses.Sum(o => o.Amount);
            }
            // Add 1000 to the value for each bordering own Bonus
            currentValue += territory.AttackTerritoryValue + LOWEST_MEDIUM_PRIORITY_VALUE * territory.GetAmountOfBordersToOwnBonus();
            // Add 1000 to the value for each bordering opponent Bonus
            currentValue += LOWEST_MEDIUM_PRIORITY_VALUE * territory.GetAmountOfBordersToOpponentBonus();

            foreach (var bonus in territory.Bonuses)
            {
                // Add 1000 * armies reward for the opponent having all but one neutral spot in the Bonus
                var neutralArmiesInBonus = bonus.NeutralArmies.DefensePower;
                var amountOfOwnedTerritories = bonus.GetOwnedTerritories().Count;
                if (amountOfOwnedTerritories == 0 && neutralArmiesInBonus <= 2 && neutralArmiesInBonus > 0)
                {
                    currentValue += bonus.Amount * LOWEST_MEDIUM_PRIORITY_VALUE;
                }
            }

            // Add up to 30 to the armies reward for being close to an opponent
            // Bonus
            if (territory.DistanceToOpponentBonus == 1)
                currentValue += 30;
            else if (territory.DistanceToOpponentBonus == 2)
                currentValue += 20;
            else if (territory.DistanceToOpponentBonus == 3)
                currentValue += 10;

            // Add up to 30 to the armies reward for being close to an own bonus
            if (territory.DistanceToOwnBonus == 1)
                currentValue += 30;
            else if (territory.DistanceToOwnBonus == 2)
                currentValue += 20;
            else if (territory.DistanceToOwnBonus == 3)
                currentValue += 10;

            // Add 1 to the value for each opponent bordering territory
            currentValue += 1 * territory.GetOpponentNeighbors().Count;

            foreach (var vmBonus in territory.Bonuses)
            {
                // Add 10 - the amount of neutrals in the Bonus
                currentValue += Math.Max(0, 10 - vmBonus.NeutralArmies.DefensePower);


                // Add stuff if the opponent seems to currently expand in that Bonus
                if (BotState.NumberOfTurns > 0 && vmBonus.GetOwnedTerritories().Count == 0 && vmBonus.Amount > 0 && !vmBonus.IsOwnedByAnyOpponent())
                {
                    var opponentIsExpanding = false;
                    foreach (var opponentTerritory in vmBonus.GetVisibleOpponentTerritories())
                    {
                        var lwmTerritory = BotState.LastVisibleMapX.Territories[opponentTerritory.ID];
                        if (lwmTerritory.IsVisible&& lwmTerritory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                            opponentIsExpanding = true;
                    }
                    if (opponentIsExpanding)
                    {
                        if (vmBonus.NeutralArmies.DefensePower <= 2)
                            currentValue += vmBonus.Amount * 30;
                        else if (vmBonus.NeutralArmies.DefensePower <= 4)
                            currentValue += vmBonus.Amount * 10;
                        else
                            currentValue += vmBonus.Amount * 5;
                    }
                }
            }

            currentValue *= ATTACK_ADJUSTMENT_FACTOR;
            mapToWriteIn.Territories[territory.ID].AttackTerritoryValue = currentValue;
        }

        private void CalculateDefenseTerritoryValue(BotTerritory territory, BotMap mapToWriteIn)
        {
            var currentValue = 0;

            // Add 1000 to the value for each bordering own Bonus
            currentValue += LOWEST_MEDIUM_PRIORITY_VALUE * territory.GetAmountOfBordersToOwnBonus
                ();
            // Add 1000 to the value for each bordering opponent Bonus
            currentValue += LOWEST_MEDIUM_PRIORITY_VALUE * territory.GetAmountOfBordersToOpponentBonus
                ();
            // Add up to 30 to the armies reward for being close to an opponent Bonus
            if (territory.DistanceToOpponentBonus == 1)
                currentValue += 30;
            else if (territory.DistanceToOpponentBonus == 2)
                currentValue += 20;
            else if (territory.DistanceToOpponentBonus == 3)
                currentValue += 10;

            // Add up to 30 to the armies reward for being close to an own Bonus
            if (territory.DistanceToOwnBonus == 1)
                currentValue += 30;
            else if (territory.DistanceToOwnBonus == 2)
                currentValue += 20;
            else if (territory.DistanceToOwnBonus == 3)
                currentValue += 10;


            foreach (var vmBonus in territory.Bonuses)
            {
                // Add 100.000 * armies reward to the value if it's a spot in an owned Bonus
                if (vmBonus.IsOwnedByMyself())
                {
                    currentValue += LOWEST_HIGH_PRIORITY_VALUE * vmBonus.Amount;
                }
                // Add 100.000 * armies reward to the value if it's the only spot in an opponent Bonus
                if (vmBonus.IsOwnedByAnyOpponent())
                {
                    currentValue += LOWEST_HIGH_PRIORITY_VALUE * vmBonus.Amount;
                }

                // Add 10 - the amount of neutrals in the Bonus
                currentValue += Math.Max(0, 10 - vmBonus.NeutralArmies.DefensePower);
                // Add stuff if it's the most important Bonus
                var isMostImportantBonus = true;

                // vmBonus.setMyExpansionValueHeuristic();
                // vmBonus.setMyExpansionValueHeuristic();
                var bonusExpansionValue = vmBonus.ExpansionValue;
                foreach (var bonus in BotState.VisibleMap.Bonuses.Values)
                {
                    if (bonus.ExpansionValue > bonusExpansionValue)
                        isMostImportantBonus = false;
                }
                if (isMostImportantBonus && vmBonus.Amount > 0 && !vmBonus
                    .IsOwnedByMyself() && !vmBonus.ContainsOpponentPresence() && vmBonus.NeutralArmies.DefensePower < 8)
                    currentValue += 1;

            }
            currentValue *= DEFENSE_ADJUSTMENT_FACTOR;
            var territoryToWriteIn = mapToWriteIn.Territories[territory.ID];
            territoryToWriteIn.DefenceTerritoryValue = currentValue;
        }

        /// <summary>Calculates the expansion territory value for a territory.</summary>
        /// <remarks>Calculates the expansion territory value for a territory.</remarks>
        /// <param name="territory">the territory which can be part of an arbitrary map</param>
        /// <param name="mapToWriteIn">the map in which the calculated territory value is to be inserted
        /// </param>
        private void CalculateExpansionTerritoryValue(BotTerritory territory, BotMap mapToWriteIn)
        {
            var currentValue = 0;
            var neighborsWithinBonus = territory.GetNeighborsWithinSameBonus();
            // Add 1000 for each unknown neighbor within the same Bonus
            foreach (var neighbor in neighborsWithinBonus)
            {
                if (neighbor.OwnerPlayerID == TerritoryStanding.NeutralPlayerID && neighbor.GetOwnedNeighbors().Count == 0)
                    currentValue += 1000;
            }
            // Add 100 for each neighbor within the same Bonus
            foreach (var neighbor_1 in neighborsWithinBonus)
            {
                if (neighbor_1.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                    currentValue += 100;
            }
            // Add 10 for each opponent neighbor
            currentValue += 10 * territory.GetOpponentNeighbors().Count;

            // Add 1 for each neutral neighbor in another Bonus
            foreach (var neighbor_2 in territory.Neighbors)
            {
                if (neighbor_2.OwnerPlayerID == TerritoryStanding.NeutralPlayerID && !neighborsWithinBonus.Contains(neighbor_2))
                    currentValue += 1;
            }

            mapToWriteIn.Territories[territory.ID].ExpansionTerritoryValue = currentValue;
        }

        /// <summary>Calculates the flanking value</summary>
        /// <param name="territory">the territory neutral territory (from the mapToUse) I guess</param>
        /// <param name="mapToWriteIn">visible map</param>
        /// <param name="mapToUse">map with already made move decisions</param>
        private void CalculateFlankingValue(BotTerritory territory, BotMap mapToWriteIn, BotMap mapToUse)
        {
            var neighbors = territory.Neighbors;
            var bonusNeighborTerritories = new List<BotTerritory>();

            foreach (var neighbor in neighbors)
            {
                if (!neighbor.IsVisible && neighbor.Bonuses.Any(b => b.IsOwnedByAnyOpponent() && !IsBonusAlreadyFlanked(b)))
                    bonusNeighborTerritories.Add(neighbor);
            }

            // TODO develop more complex algorithm also with already made decisions
            var flankingValue = 0;
            foreach (var bonusNeighborTerritory in bonusNeighborTerritories)
                flankingValue += bonusNeighborTerritory.Bonuses.Sum(b => b.Amount);

            var territoryToWriteIn = mapToWriteIn.Territories[territory.ID];
            territoryToWriteIn.FlankingTerritoryValue = flankingValue;
        }

        private bool IsBonusAlreadyFlanked(BotBonus opponentBonus)
        {
            var flankedTerritories = new HashSet<BotTerritory>();
            foreach (var ownedNeighbor in opponentBonus.GetOwnedNeighborTerritories())
                foreach (var opponentNeighbor in ownedNeighbor.GetOpponentNeighbors())
                    if (opponentNeighbor.Details.PartOfBonuses.Contains(opponentBonus.ID))
                        flankedTerritories.Add(opponentNeighbor);

            return flankedTerritories.Count >= 2 ? true : false;
        }
    }
}
