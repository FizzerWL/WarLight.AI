using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <remarks>
    /// This class is responsible for preventing the opponent from taking all of some territories. This is needed to prevent him from completely taking over a Bonus.
    /// </remarks>
    public class PreventTerritoriesTask
    {
        public static Moves CalculatePreventTerritoriesTask(BotMain state, List<BotTerritory> territoriesToPrevent, PlayerIDType opponentID, int maxDeployment, BotTerritory.DeploymentType conservativeLevel)
        {
            var outvar = new Moves();

            if (territoriesToPrevent.Count == 0)
                return outvar;

            var opponentAttacks = CalculateGuessedOpponentTakeOverMoves(state, territoriesToPrevent, opponentID, true, conservativeLevel);
            if (opponentAttacks == null)
                return outvar;

            // Just try to prevent the territory with the highest defense territory value
            var highestDefenceTerritoryValue = 0;
            BotTerritory highestDefenceValueTerritory = null;
            foreach (var territory in territoriesToPrevent)
            {
                if (territory.OwnerPlayerID == state.Me.ID && territory.DefenceTerritoryValue >= highestDefenceTerritoryValue)
                {
                    highestDefenceValueTerritory = territory;
                    highestDefenceTerritoryValue = territory.DefenceTerritoryValue;
                }
            }
            var currentArmies = highestDefenceValueTerritory.GetArmiesAfterDeploymentAndIncomingMoves().DefensePower;
            var attackingArmies = CalculateOpponentAttackingArmies(highestDefenceValueTerritory, opponentAttacks);

            var minimumNeededArmies = SharedUtility.Round(attackingArmies.AttackPower * state.Settings.OffenseKillRate);
            //var minimumNeededArmies = SharedUtility.Round(attackingArmies.AttackPower * state.Settings.OffensiveKillRate);
            var maximumNeededArmies = minimumNeededArmies;
            var maximumMissingArmies = Math.Max(0, maximumNeededArmies - currentArmies);
            var minimumMissingArmies = Math.Max(0, minimumNeededArmies - currentArmies);
            if (maximumMissingArmies <= maxDeployment && maximumMissingArmies > 0)
                outvar.AddOrder(new BotOrderDeploy(state.Me.ID, highestDefenceValueTerritory, maximumMissingArmies));
            else if (minimumMissingArmies <= maxDeployment && maxDeployment > 0)
                outvar.AddOrder(new BotOrderDeploy(state.Me.ID, highestDefenceValueTerritory, maxDeployment));


            // If no solution then empty moves instead of null
            return outvar;
        }

        private static Armies CalculateOpponentAttackingArmies(BotTerritory territory, Moves opponentAttacks)
        {
            var attackingArmies = new Armies(0);
            foreach (var atm in opponentAttacks.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (atm.To.ID == territory.ID)
                    attackingArmies = attackingArmies.Add(atm.Armies);
            }
            return attackingArmies;
        }

        public static Moves CalculateGuessedOpponentTakeOverMoves(BotMain state, List<BotTerritory> territories, PlayerIDType opponentID, bool doesOpponentDeploy, BotTerritory.DeploymentType conservativeLevel)
        {
            var opponentIncome = state.Settings.MinimumArmyBonus;
            if (conservativeLevel == BotTerritory.DeploymentType.Conservative)
                opponentIncome = state.GetGuessedOpponentIncome(opponentID, state.VisibleMap);

            var ownedTerritories = territories.Where(o => o.OwnerPlayerID == state.Me.ID).ToList();
            var opponentAttacks = CalculateMinimumOpponentMoves(state, opponentID, ownedTerritories, conservativeLevel);
            if (opponentAttacks.GetTotalDeployment() > opponentIncome)
                return null;

            if (doesOpponentDeploy)
            {
                var remainingOpponentIncome = opponentIncome - opponentAttacks.GetTotalDeployment();
                while (remainingOpponentIncome > 0 && opponentAttacks.Orders.OfType<BotOrderAttackTransfer>().Any())
                {
                    foreach (var atm in opponentAttacks.Orders.OfType<BotOrderAttackTransfer>())
                    {
                        atm.Armies = atm.Armies.Add(new Armies(1));
                        remainingOpponentIncome--;
                        if (remainingOpponentIncome == 0)
                            break;
                    }
                }
            }
            return opponentAttacks;
        }

        /// <summary>Calculates the minimum opponent moves that he needs to make if we don't deploy.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ownedTerritories"></param>
        /// <returns></returns>
        private static Moves CalculateMinimumOpponentMoves(BotMain state, PlayerIDType opponentID, List<BotTerritory> ownedTerritories, BotTerritory.DeploymentType conservativeLevel)
        {
            var outvar = new Moves();
            foreach (var ownedTerritory in ownedTerritories)
            {
                var attackingOpponentTerritory = GetOpponentNeighborMaxIdleArmies(state, opponentID, ownedTerritory, outvar);

                if (attackingOpponentTerritory == null)
                    continue;

                var stilIdleArmies = CalculateStillOpponentIdleArmies(state, attackingOpponentTerritory, outvar);
                var attackingOpponentArmies = SharedUtility.Round(ownedTerritory.GetArmiesAfterDeploymentAndIncomingAttacks(conservativeLevel).DefensePower / state.Settings.OffenseKillRate);
                var opponentDeployment = Math.Max(0, attackingOpponentArmies - stilIdleArmies.DefensePower);
                if (opponentDeployment > 0)
                    outvar.AddOrder(new BotOrderDeploy(opponentID, attackingOpponentTerritory, opponentDeployment));

                outvar.AddOrder(new BotOrderAttackTransfer(opponentID, attackingOpponentTerritory, ownedTerritory, new Armies(attackingOpponentArmies), "PreventTerritoriesTask"));
            }
            // Now let's assume that the opponent doesen't leave armies idle
            var hasSomethingChanged = true;
            while (hasSomethingChanged)
            {
                hasSomethingChanged = false;
                foreach (var attackTransferMove in outvar.Orders.OfType<BotOrderAttackTransfer>())
                {
                    var stillIdleArmies = CalculateStillOpponentIdleArmies(state, attackTransferMove.From, outvar);
                    if (stillIdleArmies.IsEmpty == false)
                    {
                        hasSomethingChanged = true;
                        attackTransferMove.Armies = attackTransferMove.Armies.Add(new Armies(1));
                    }
                }
            }
            return outvar;
        }

        private static BotTerritory GetOpponentNeighborMaxIdleArmies(BotMain state, PlayerIDType opponentID, BotTerritory ownedTerritory, Moves alreadyMadeAttacks)
        {
            var opponentNeighbors = ownedTerritory.Neighbors.Where(o => o.OwnerPlayerID == opponentID).ToList();

            if (opponentNeighbors.Count == 0)
                return null;

            var maxIdleArmiesTerritory = opponentNeighbors[0];
            var maxIdleArmies = CalculateStillOpponentIdleArmies(state, maxIdleArmiesTerritory, alreadyMadeAttacks);
            foreach (var territory in opponentNeighbors)
            {
                var idleArmies = CalculateStillOpponentIdleArmies(state, territory, alreadyMadeAttacks);
                if (idleArmies.AttackPower > maxIdleArmies.AttackPower)
                {
                    maxIdleArmies = idleArmies;
                    maxIdleArmiesTerritory = territory;
                }
            }
            return maxIdleArmiesTerritory;
        }

        private static Armies CalculateStillOpponentIdleArmies(BotMain state, BotTerritory territory, Moves alreadyMadeMoves)
        {
            var idleArmies = territory.Armies.Subtract(new Armies(state.MustStandGuardOneOrZero));
            foreach (var pam in alreadyMadeMoves.Orders.OfType<BotOrderDeploy>())
            {
                if (pam.Territory.ID == territory.ID)
                    idleArmies = idleArmies.Add(new Armies(pam.Armies));
            }
            foreach (var atm in alreadyMadeMoves.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (atm.From.ID == territory.ID)
                    idleArmies = idleArmies.Subtract(atm.Armies);
            }
            return idleArmies;
        }
    }
}
