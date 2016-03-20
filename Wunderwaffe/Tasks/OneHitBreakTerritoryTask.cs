using System;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>
    /// OneHitBreakTerritoryTask is responsible for calculating an attack plan to break a single territory with a single attack
    /// without pulling other territories in.
    /// </summary>
    public class OneHitBreakTerritoryTask
    {
        public static Moves CalculateBreakTerritoryTask(BotMain state, BotTerritory opponentTerritory, int maxDeployment, BotTerritory.DeploymentType conservativeLevel)
        {
            var outvar = new Moves();
            var opponentArmies = opponentTerritory.GetArmiesAfterDeploymentAndIncomingAttacks(conservativeLevel);
            var neededAttackArmies = opponentTerritory.getNeededBreakArmies(opponentArmies.DefensePower);
            var ownedNeighbors = opponentTerritory.GetOwnedNeighbors();
            var presortedOwnedNeighbors = state.TerritoryValueCalculator.SortDefenseValue(ownedNeighbors);
            var sortedOwnedNeighbors = BotMap.GetOrderedListOfTerritoriesByIdleArmies(presortedOwnedNeighbors);
            var territoryToUse = sortedOwnedNeighbors[0];
            var idleArmies = territoryToUse.GetIdleArmies();
            var neededDeployment = Math.Max(0, neededAttackArmies - idleArmies.AttackPower);
            if (neededDeployment > maxDeployment)
            {
                return null;
            }

            if (neededDeployment > 0)
            {
                outvar.AddOrder(new BotOrderDeploy(state.Me.ID, territoryToUse, neededDeployment));
            }

            Armies attackArmies = new Armies(neededAttackArmies);

            //var atm = new BotOrderAttackTransfer(state.Me.ID, territoryToUse, opponentTerritory, idleArmies.Add(new Armies(neededDeployment)), "OneHitBreakTerritoryTask");
            var atm = new BotOrderAttackTransfer(state.Me.ID, territoryToUse, opponentTerritory, attackArmies, "OneHitBreakTerritoryTask");
            outvar.AddOrder(atm);
            return outvar;
        }
    }
}
