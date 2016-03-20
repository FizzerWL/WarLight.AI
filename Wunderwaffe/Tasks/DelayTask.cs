using System;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <remarks>
    /// MoveIdleArmiesTask is responsible for helping us getting last order. This happens by moving around armies in a semi reasonable way or by small attacks with 1.
    /// </remarks>
    public static class DelayTask
    {
        /// <param name="maxMovesBeforeRiskyAttack">the maximum amount of moves that should happen before the first risky attack.
        /// </param>
        /// <returns></returns>
        public static Moves CalculateDelayTask(BotMain state, Moves movesSoFar, int maxMovesBeforeRiskyAttack, int minMovesBeforeRiskyAttack)
        {
            var outvar = new Moves();
            if (!IsRiskyAttackPresent(state, movesSoFar))
                return outvar;
            var amountOfSafeMoves = CalculateAmountOfSafeMoves(state, movesSoFar);
            // Step 1: Try to move armies next to neutral territories.
            // Step 1: Try to move armies next to neutral territories.
            var maximumNewDelays = Math.Max(0, maxMovesBeforeRiskyAttack - amountOfSafeMoves);
            foreach (var territory in state.VisibleMap.GetNonOpponentBorderingBorderTerritories())
            {
                var ownedNeighbors = territory.GetOwnedNeighbors();
                var sortedDistanceNeighbors = state.VisibleMap.SortTerritoriesDistanceToBorder
                    (ownedNeighbors);
                var maxPossibleDelays = Math.Min(sortedDistanceNeighbors.Count, territory.GetIdleArmies().NumArmies);
                var delaysToAdd = Math.Min(maximumNewDelays, maxPossibleDelays);
                for (var i = 0; i < delaysToAdd; i++)
                {
                    var territoryToTransferTo = sortedDistanceNeighbors[i];
                    var atm = new BotOrderAttackTransfer(state.Me.ID, territory, territoryToTransferTo, new Armies(1), "DelayTask");
                    outvar.AddOrder(atm);
                    maximumNewDelays--;
                }
            }

            // Step 2: If the minMovesBeforeRiskyAttack constraint isn't fulfilled
            // then also add delay moves next to the opponent
            var stillNeededDelays = Math.Max(0, minMovesBeforeRiskyAttack - (amountOfSafeMoves
                 + outvar.Orders.OfType<BotOrderAttackTransfer>().Count()));
            foreach (var territory_1 in state.VisibleMap.GetOpponentBorderingTerritories())
            {
                var ownedNeighbors = territory_1.GetOwnedNeighbors();
                var sortedDistanceNeighbors = state.VisibleMap.SortTerritoriesDistanceToBorder
                    (ownedNeighbors);
                var maxPossibleDelays = Math.Min(sortedDistanceNeighbors.Count, territory_1.GetIdleArmies().NumArmies);
                var delaysToAdd = Math.Min(stillNeededDelays, maxPossibleDelays);
                for (var i = 0; i < delaysToAdd; i++)
                {
                    var territoryToTransferTo = sortedDistanceNeighbors[i];
                    var atm = new BotOrderAttackTransfer(state.Me.ID, territory_1, territoryToTransferTo, new Armies(1), "DelayTask2");
                    outvar.AddOrder(atm);
                    stillNeededDelays--;
                }
            }
            return outvar;
        }

        private static bool IsRiskyAttackPresent(BotMain state, Moves movesSoFar)
        {
            foreach (var atm in movesSoFar.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (state.IsOpponent(atm.To.OwnerPlayerID))
                {
                    var attackingArmies = atm.Armies;
                    var maxOpponentArmies = atm.To.Armies.Add(new Armies(state.GetGuessedOpponentIncome(atm.To.OwnerPlayerID, state.VisibleMap)));
                    if (attackingArmies.AttackPower > 1 && attackingArmies.AttackPower * state.Settings.OffenseKillRate <= maxOpponentArmies.DefensePower * state.Settings.DefenseKillRate)
                        return true;
                }
            }
            return false;
        }

        private static int CalculateAmountOfSafeMoves(BotMain state, Moves movesSoFar)
        {
            var outvar = 0;
            foreach (var atm in movesSoFar.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (atm.Armies.AttackPower <= 1 || state.IsOpponent(atm.To.OwnerPlayerID))
                    outvar++;
                else
                {
                    var maxOpponentArmies = atm.To.Armies.Add(new Armies(state.GetGuessedOpponentIncome(atm.To.OwnerPlayerID, state.VisibleMap)));
                    var attackingArmies = atm.Armies;
                    if (attackingArmies.AttackPower * state.Settings.OffenseKillRate > maxOpponentArmies.DefensePower * state.Settings.DefenseKillRate)
                        outvar++;
                }
            }
            return outvar;
        }
    }
}
