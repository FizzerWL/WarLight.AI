using System;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>BreakTerritoryTask is responsible for breaking a single territory with a good attack plan.
    /// </summary>
    /// <remarks>BreakTerritoryTask is responsible for breaking a single territory with a good attack plan.
    /// </remarks>
    public class BreakTerritoryTask
    {
        public BotMain BotState;
        public BreakTerritoryTask(BotMain state)
        {
            this.BotState = state;
        }

        /// <remarks>Calculates a good plan to break the territory with minimal deployment. Returns null if no solution was found.
        /// </remarks>
        /// <param name="opponentTerritory"></param>
        /// <param name="maxDeployment"></param>
        /// <returns></returns>
        public Moves CalculateBreakTerritoryTask(BotTerritory opponentTerritory, int maxDeployment, BotTerritory.DeploymentType lowerConservativeLevel, BotTerritory.DeploymentType upperConservativeLevel, string source)
        {
            var outvar = new Moves();
            var lowestBoundDeployment = opponentTerritory.GetArmiesAfterDeployment(lowerConservativeLevel).NumArmies;
            var uppestBoundDeployment = opponentTerritory.GetArmiesAfterDeployment(upperConservativeLevel).NumArmies;
            for (var deployment = lowestBoundDeployment; deployment <= uppestBoundDeployment; deployment++)
            {
                var solution = CalculateBreakTerritoryMoves(opponentTerritory, maxDeployment, deployment, source);
                if (solution == null)
                    return outvar;
                else
                    outvar = solution;
            }
            return outvar;
        }

        private Moves CalculateBreakTerritoryMoves(BotTerritory opponentTerritory, int maxDeployment, int opponentDeployment, string source)
        {
            var outvar = new Moves();
            var opponentArmies = opponentTerritory.Armies.DefensePower;
            opponentArmies += opponentDeployment;
            var neededAttackArmies = opponentTerritory.getNeededBreakArmies(opponentArmies);
            //var neededAttackArmies = SharedUtility.Round(opponentArmies / BotState.Settings.OffensiveKillRate);
            var ownedNeighbors = opponentTerritory.GetOwnedNeighbors();
            var presortedOwnedNeighbors = BotState.TerritoryValueCalculator.SortDefenseValue(ownedNeighbors);
            var sortedOwnedNeighbors = BotMap.GetOrderedListOfTerritoriesByIdleArmies(
                presortedOwnedNeighbors);
            // First deploy and then pull in more territories if necessary.
            // First deploy and then pull in more territories if necessary.
            var attackedWithSoFar = 0;
            for (var i = 0; i < sortedOwnedNeighbors.Count; i++)
            {
                if (i == 0)
                {
                    var neededDeployment = Math.Max(0, neededAttackArmies - sortedOwnedNeighbors[0].GetIdleArmies().NumArmies);
                    var totalDeployment = Math.Min(neededDeployment, maxDeployment);
                    if (totalDeployment > 0)
                    {
                        var pam = new BotOrderDeploy(BotState.Me.ID, sortedOwnedNeighbors[0], totalDeployment);
                        outvar.AddOrder(pam);
                    }
                    var attackingArmies = Math.Min(neededAttackArmies, sortedOwnedNeighbors[0].GetIdleArmies().NumArmies + totalDeployment);

                    outvar.AddOrder(new BotOrderAttackTransfer(BotState.Me.ID, sortedOwnedNeighbors[0], opponentTerritory, new Armies(attackingArmies), source));
                    attackedWithSoFar += attackingArmies;
                }
                else
                {
                    // i != 0
                    var stillNeededArmies = neededAttackArmies - attackedWithSoFar;
                    if (stillNeededArmies > 0 && sortedOwnedNeighbors[i].GetIdleArmies().NumArmies > 1)
                    {
                        var newAttackingArmies = Math.Min(stillNeededArmies, sortedOwnedNeighbors[i].GetIdleArmies().NumArmies);
                        outvar.AddOrder(new BotOrderAttackTransfer(BotState.Me.ID, sortedOwnedNeighbors[i], opponentTerritory, new Armies(newAttackingArmies), "BreakTerritoryTask2"));
                        attackedWithSoFar += newAttackingArmies;
                    }
                }
            }
            if (attackedWithSoFar >= neededAttackArmies)
                return outvar;
            else
                return null;
        }
    }
}
