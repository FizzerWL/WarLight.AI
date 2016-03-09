/*
* This code was auto-converted from a java project.
*/

using System;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Evaluation;

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

        // int opponentArmies = opponentTerritory.getArmiesAfterDeploymentAndIncomingAttacks(lowerConservativeLevel);
        //
        // int neededAttackArmies = (int) Math.ceil(opponentArmies / 0.6);
        // List<Territory> ownedNeighbors = opponentTerritory.GetOwnedNeighbors();
        // List<Territory> presortedOwnedNeighbors = TerritoryValueCalculator.sortDefenseValue(ownedNeighbors);
        // List<Territory> sortedOwnedNeighbors = Map.getOrderedListOfTerritoriesByIdleArmies(presortedOwnedNeighbors);
        //
        // // First deploy and then pull in more territories if necessary.
        // int attackedWithSoFar = 0;
        // for (int i = 0; i < sortedOwnedNeighbors.size(); i++) {
        // if (i == 0) {
        // int neededDeployment = Math.max(0, neededAttackArmies - sortedOwnedNeighbors.get(0).GetIdleArmies());
        // int totalDeployment = Math.min(neededDeployment, maxDeployment);
        // if (totalDeployment > 0) {
        // PlaceArmiesMove pam = new PlaceArmiesMove(BotState.MyPlayerName,
        // sortedOwnedNeighbors.get(0), totalDeployment);
        // out.placeArmiesMoves.add(pam);
        // }
        // int attackingArmies = Math.min(neededAttackArmies, sortedOwnedNeighbors.get(0).GetIdleArmies()
        // + totalDeployment);
        // out.attackTransferMoves.add(new AttackTransferMove(BotState.MyPlayerName,
        // sortedOwnedNeighbors.get(0), opponentTerritory, attackingArmies));
        // attackedWithSoFar += attackingArmies;
        // } else {
        // // i != 0
        // int stillNeededArmies = neededAttackArmies - attackedWithSoFar;
        // if (stillNeededArmies > 0 && sortedOwnedNeighbors.get(i).GetIdleArmies ()> 1) {
        // int newAttackingArmies = Math.min(stillNeededArmies, sortedOwnedNeighbors.get(i).GetIdleArmies());
        // out.attackTransferMoves.add(new AttackTransferMove(BotState.MyPlayerName,
        // sortedOwnedNeighbors.get(i), opponentTerritory, newAttackingArmies));
        // attackedWithSoFar += newAttackingArmies;
        // }
        // }
        // }
        // if (attackedWithSoFar >= neededAttackArmies) {
        // return out;
        // } else {
        // return null;
        // }
        private Moves CalculateBreakTerritoryMoves(BotTerritory opponentTerritory, int maxDeployment, int opponentDeployment, string source)
        {
            var outvar = new Moves();
            var opponentArmies = opponentTerritory.Armies.DefensePower;
            opponentArmies += opponentDeployment;
            var neededAttackArmies = SharedUtility.Ceiling(opponentArmies / BotState.Settings.OffenseKillRate);
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
