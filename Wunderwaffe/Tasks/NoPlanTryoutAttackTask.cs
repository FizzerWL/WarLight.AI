using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Evaluation;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>
    /// NoPlanTryoutAttackTask is responsible for attacking stacks of 1 with 2 armies and stacks of 2 with 3 armies since we
    /// might get lucky without the opponent deploying there.
    /// </summary>
    /// <remarks>
    /// NoPlanTryoutAttackTask is responsible for attacking stacks of 1 with 2 armies and stacks of 2 with 3 armies since we
    /// might get lucky without the opponent deploying there.
    /// </remarks>
    public class NoPlanTryoutAttackTask
    {
        /// <summary>We only try to perform 1 attack to a high ranked territory where an attack is possible.
        /// </summary>
        /// <remarks>
        /// We only try to perform 1 attack to a high ranked territory where an attack is possible. For multiple attacks this
        /// function has to be called multiple times.
        /// </remarks>
        /// <param name="attackLowImportantTerritories"></param>
        /// <param name="attackMediumImportantTerritories"></param>
        /// <param name="attackHighImportantTerritories"></param>
        /// <returns></returns>
        public static Moves CalculateNoPlanTryoutAttackTask(BotMain state, bool attackLowImportantTerritories, bool attackMediumImportantTerritories, bool attackHighImportantTerritories)
        {
            var possibleAttackTerritories = GetPossibleTerritoriesToAttack(state, attackLowImportantTerritories, attackMediumImportantTerritories, attackHighImportantTerritories);
            var sortedPossibleAttackTerritories = state.TerritoryValueCalculator.SortAttackValue
                (possibleAttackTerritories);
            foreach (var territoryToAttack in sortedPossibleAttackTerritories)
            {
                var neededNewArmies = 0;
                var ownedNeighbors = territoryToAttack.GetOwnedNeighbors();
                var sortedOwnedNeighbors = state.TerritoryValueCalculator.SortDefenseValue(ownedNeighbors);
                BotTerritory bestNeighbor = null;
                for (var i = sortedOwnedNeighbors.Count - 1; i >= 0; i--)
                {
                    var smallAttackPresent = IsTerritoryAttackingOpponentTerritorySmall(ownedNeighbors[i], territoryToAttack);
                    if (smallAttackPresent)
                        neededNewArmies = territoryToAttack.Armies.DefensePower;
                    else
                        neededNewArmies = territoryToAttack.Armies.DefensePower + 1;

                    if (ownedNeighbors[i].GetIdleArmies().NumArmies >= neededNewArmies)
                    {
                        bestNeighbor = ownedNeighbors[i];
                        break;
                    }
                }
                if (bestNeighbor != null)
                {
                    var outvar = new Moves();
                    if (bestNeighbor.GetIdleArmies().NumArmies > 2)
                    {
                        var atm = new BotOrderAttackTransfer(state.Me.ID, bestNeighbor, territoryToAttack, new Armies(3), "NoPlanTryoutAttackTask");
                        atm.Message = AttackMessage.TryoutAttack;
                        outvar.AddOrder(atm);
                        return outvar;
                    }
                    else
                    {
                        var atm = new BotOrderAttackTransfer(state.Me.ID, bestNeighbor, territoryToAttack, new Armies(neededNewArmies), "NoPlanTryoutAttackTask");
                        atm.Message = AttackMessage.TryoutAttack;
                        outvar.AddOrder(atm);
                        return outvar;
                    }
                }
            }
            return null;
        }

        private static bool IsTerritoryAttackingOpponentTerritorySmall(BotTerritory ourTerritory, BotTerritory opponentTerritory)
        {
            foreach (var atm in opponentTerritory.IncomingMoves)
            {
                if (atm.Armies.AttackPower <= 1 && atm.From.ID == ourTerritory.ID)
                    return true;
            }
            return false;
        }

        private static List<BotTerritory> GetPossibleTerritoriesToAttack(BotMain state, bool attackLowImportantTerritories, bool attackMediumImportantTerritories, bool attackHighImportantTerritories)
        {
            var possibleCandidates = new List<BotTerritory>();
            foreach (var territory in state.VisibleMap.AllOpponentTerritories)
            {
                if (territory.IsVisible)
                {
                    if (territory.AttackTerritoryValue >= TerritoryValueCalculator.LOWEST_HIGH_PRIORITY_VALUE && attackHighImportantTerritories)
                        possibleCandidates.Add(territory);
                    else if (territory.AttackTerritoryValue >= TerritoryValueCalculator.LOWEST_MEDIUM_PRIORITY_VALUE && attackMediumImportantTerritories)
                        possibleCandidates.Add(territory);
                    else if (attackLowImportantTerritories)
                        possibleCandidates.Add(territory);
                }
            }
            var outvar = new List<BotTerritory>();
            foreach (var territory_1 in possibleCandidates)
            {
                if (territory_1.Armies.DefensePower <= 2 && territory_1.IncomingMoves.None(o => o.Armies.AttackPower > 1))
                    outvar.Add(territory_1);
            }
            return outvar;
        }
    }
}
