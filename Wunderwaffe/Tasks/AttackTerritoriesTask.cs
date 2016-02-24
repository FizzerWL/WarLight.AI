/*
* This code was auto-converted from a java project.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using WarLight.AI.Wunderwaffe.Bot;
using WarLight.AI.Wunderwaffe.Evaluation;

using WarLight.AI.Wunderwaffe.Move;

namespace WarLight.AI.Wunderwaffe.Tasks
{
    /// <summary>AttackTerritoryTask is responsible for attacking a single territory.</summary>
    /// <remarks>
    /// AttackTerritoryTask is responsible for attacking a single territory. This isn't for calculating a minimal attack plan but
    /// if a good attack plan is possible then a full force attack plan is calculated.
    /// </remarks>
    public class AttackTerritoryTask
    {
        /// <param name="opponentTerritory"></param>
        /// <param name="maxDeployment"></param>
        /// <returns></returns>
        public static Moves CalculateAttackTerritoryTask(BotMain state, BotTerritory opponentTerritory, int maxDeployment
        )
        {
            var outvar = new Moves();
            var ownedNeighbors = opponentTerritory.GetOwnedNeighbors();
            var presortedOwnedNeighbors = state.TerritoryValueCalculator.SortDefenseValue(ownedNeighbors
                );
            var sortedOwnedNeighbors = BotMap.GetOrderedListOfTerritoriesByIdleArmies(
                presortedOwnedNeighbors);
            // Calculate the attacks
            for (var i = 0; i < sortedOwnedNeighbors.Count; i++)
            {
                var attackingTerritory = sortedOwnedNeighbors[i];
                if (i == 0 && maxDeployment > 0)
                {
                    var pam = new BotOrderDeploy(state.Me.ID, attackingTerritory, maxDeployment);
                    outvar.AddOrder(pam);
                    if (attackingTerritory.GetIdleArmies().AttackPower + maxDeployment > 1)
                    {
                        var atm = new BotOrderAttackTransfer(state.Me.ID, attackingTerritory, opponentTerritory, attackingTerritory.GetIdleArmies().Add(new Armies(maxDeployment)), "AttackTerritoriesTask1");
                        outvar.AddOrder(atm);
                    }
                }
                else
                {
                    if (attackingTerritory.GetIdleArmies().AttackPower > 1)
                    {
                        var atm = new BotOrderAttackTransfer(state.Me.ID, attackingTerritory, opponentTerritory, attackingTerritory.GetIdleArmies(), "AttackTerritoriesTask2");
                        outvar.AddOrder(atm);
                    }
                }
            }
            // Check if we are killing more or equal armies than the opponent
            // double currentOpponentArmies = opponentTerritory.ArmiesAfterDeployment;
            var currentOpponentArmies = (double)opponentTerritory.GetArmiesAfterDeploymentAndIncomingAttacks(BotTerritory.DeploymentType.Normal).DefensePower;
            double opponentKills = 0;
            double ownKills = 0;
            foreach (var atm_1 in outvar.Orders.OfType<BotOrderAttackTransfer>())
            {
                var ourKills = Math.Min(currentOpponentArmies, atm_1.Armies.AttackPower * state.Settings.OffensiveKillRate);
                var opponentKillsAttack = Math.Min(atm_1.Armies.AttackPower, currentOpponentArmies * state.Settings.DefensiveKillRate);
                ownKills += ourKills;
                opponentKills += opponentKillsAttack;
                currentOpponentArmies = Math.Max(0, currentOpponentArmies - ourKills);
            }
            if (ownKills >= opponentKills && outvar.Orders.OfType<BotOrderAttackTransfer>().Any())
                return outvar;
            else
                return null;
        }
    }
}
