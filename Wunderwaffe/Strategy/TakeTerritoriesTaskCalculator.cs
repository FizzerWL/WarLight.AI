using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Strategy
{
    public class TakeTerritoriesTaskCalculator
    {
        public BotMain BotState;
        public TakeTerritoriesTaskCalculator(BotMain state)
        {
            this.BotState = state;
        }

        public Moves CalculateOneStepExpandBonusTask(int maxDeploymentArg, BotBonus bonus, bool acceptStackOnly, BotMap workingMap, BotTerritory.DeploymentType conservativeLevel)
        {
            var outvar = new Moves();
            var maxDeployment = maxDeploymentArg == -1 ? 1000 : maxDeploymentArg;

            List<BotTerritory> visibleNeutralTerritories = bonus.GetVisibleNeutralTerritories();
            List<BotTerritory> territoriesToRemove = new List<BotTerritory>();
            foreach (var territory in visibleNeutralTerritories)
            {
                if (workingMap.Territories[territory.ID].OwnerPlayerID == BotState.Me.ID)
                    territoriesToRemove.Add(territory);
            }


            //  visibleNeutralTerritories.RemoveAll(territoriesToRemove);
            visibleNeutralTerritories.RemoveAll(i => territoriesToRemove.Contains(i));

            if (visibleNeutralTerritories.Count == 0)
                return null;

            var sortedNeutralTerritories = BotState.TerritoryValueCalculator.SortExpansionValue(visibleNeutralTerritories);
            var territoryToTake = new List<BotTerritory>();
            territoryToTake.Add(sortedNeutralTerritories[0]);
            var takeTerritoryMoves = CalculateTakeTerritoriesTask(-1, territoryToTake, conservativeLevel, "CalculateOneStepExpandBonusTask");
            if (takeTerritoryMoves.GetTotalDeployment() > maxDeployment)
            {
                if (acceptStackOnly)
                {
                    if (maxDeployment > 0)
                    {
                        var territoryToDeploy = takeTerritoryMoves.Orders.OfType<BotOrderDeploy>().First().Territory;
                        var pam = new BotOrderDeploy(BotState.Me.ID, territoryToDeploy, maxDeployment);
                        outvar.AddOrder(pam);
                        return outvar;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                outvar = takeTerritoryMoves;
                return outvar;
            }
        }

        /// <summary>Calculates the necessary moves to take the specified territories to take.</summary>
        /// <param name="maxDeployment">the maximum allowed deployment. If no deployment constraint then put -1.
        /// </param>
        /// <param name="territoriesToTake">the territories that should be taken this turn.</param>
        /// <returns>the necessary moves to take the territories or null if no solution was found.
        /// </returns>
        public Moves CalculateTakeTerritoriesTask(int maxDeploymentArg, List<BotTerritory> territoriesToTake, BotTerritory.DeploymentType conservativeLevel, string attackSource)
        {
            var outvar = new Moves();
            var maxDeployment = maxDeploymentArg == -1 ? int.MaxValue : maxDeploymentArg;

            var stillAvailableDeployment = maxDeployment;
            foreach (var missingTerritory in territoriesToTake)
            {
                var bestNeighborTerritory = GetBestNeighborTerritory(missingTerritory, outvar, territoriesToTake);
                //var missingTerritoryArmies = missingTerritory.GetArmiesAfterDeploymentAndIncomingAttacks(conservativeLevel);
                var neededAttackArmies = missingTerritory.getNeededBreakArmies(missingTerritory.Armies.DefensePower);
                //var neededAttackArmies = (int)Math.Round(missingTerritoryArmies.DefensePower / BotState.Settings.OffensiveKillRate);
                var missingArmies = GetMissingArmies(bestNeighborTerritory, missingTerritory, outvar, conservativeLevel);
                if (missingArmies > stillAvailableDeployment)
                    return null;

                if (missingArmies > 0)
                {
                    var pam = new BotOrderDeploy(BotState.Me.ID, bestNeighborTerritory, missingArmies);
                    outvar.AddOrder(pam);
                    stillAvailableDeployment -= missingArmies;
                }
                outvar.AddOrder(new BotOrderAttackTransfer(BotState.Me.ID, bestNeighborTerritory, missingTerritory, new Armies(neededAttackArmies), attackSource));
            }
            return outvar;
        }

        private int GetMissingArmies(BotTerritory expandingTerritory, BotTerritory toBeTakenTerritory, Moves madeExpansionDecisions, BotTerritory.DeploymentType conservativeLevel)
        {
            var idleArmies = GetOverflowIdleArmies(expandingTerritory, madeExpansionDecisions);
            //var toBeTakenTerritoryArmies = toBeTakenTerritory.GetArmiesAfterDeploymentAndIncomingAttacks(conservativeLevel);
            var neededArmies = toBeTakenTerritory.getNeededBreakArmies(toBeTakenTerritory.Armies.DefensePower);
            if (idleArmies.AttackPower >= neededArmies)
                return 0;
            else
                return neededArmies - idleArmies.AttackPower;
        }

        private BotTerritory GetBestNeighborTerritory(BotTerritory missingTerritory, Moves madeExpansionDecisions, List<BotTerritory> territoriesToTake)
        {
            var ownedNeighbors = missingTerritory.GetOwnedNeighbors();
            var presortedOwnedNeighbors = BotState.TerritoryValueCalculator.SortDefenseValueFullReturn
                (ownedNeighbors);
            var maximumIdleArmies = 0;
            // First calculate the maximum amount of armies of an owned neighbor.
            foreach (var ownedNeighbbor in presortedOwnedNeighbors)
            {
                var idleArmies = GetOverflowIdleArmies(ownedNeighbbor, madeExpansionDecisions);
                if (idleArmies.NumArmies > maximumIdleArmies)
                    maximumIdleArmies = idleArmies.NumArmies;
            }
            // Second calculate the owned neighbor having the maximum amount of idle armies while having a minimum amount of sill missing neighbors.
            var minimumMissingNeighbors = 1000;
            BotTerritory outvar = null;
            foreach (var ownedNeighbor in presortedOwnedNeighbors)
            {
                var missingNeighborTerritories = GetStillMissingNeighborTerritories(ownedNeighbor, madeExpansionDecisions, territoriesToTake).Count;
                if (GetOverflowIdleArmies(ownedNeighbor, madeExpansionDecisions).NumArmies == maximumIdleArmies && missingNeighborTerritories < minimumMissingNeighbors)
                {
                    outvar = ownedNeighbor;
                    minimumMissingNeighbors = missingNeighborTerritories;
                }
            }
            if (outvar == null)
            {
                outvar = presortedOwnedNeighbors[0];
            }
            return outvar;
        }

        /// <remarks>Calculates which territories are still missing after the made expansion decisions.
        /// </remarks>
        /// <param name="madeExpansionDecisions"></param>
        /// <param name="territoriesToTake"></param>
        /// <returns></returns>
        private List<BotTerritory> GetStillMissingTerritories(Moves madeExpansionDecisions,
            List<BotTerritory> territoriesToTake)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            List<BotTerritory> territoriesThatWeTook = new List<BotTerritory>();
            foreach (var atm in madeExpansionDecisions.Orders.OfType<BotOrderAttackTransfer>())
                territoriesThatWeTook.Add(atm.To);

            foreach (var territory in territoriesToTake)
            {
                if (!territoriesThatWeTook.Contains(territory))
                    outvar.Add(territory);
            }
            return outvar;
        }

        /// <remarks>Calculates the territories next to our territory which are still missing after the already made expansion decisions.
        /// </remarks>
        /// <param name="territory"></param>
        /// <param name="madeExpansionDecisions"></param>
        /// <param name="territoriesToTake"></param>
        /// <returns></returns>
        private List<BotTerritory> GetStillMissingNeighborTerritories(BotTerritory territory, Moves
        madeExpansionDecisions, List<BotTerritory> territoriesToTake)
        {
            var stillMissingTerritories = GetStillMissingTerritories(madeExpansionDecisions
                , territoriesToTake);
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var neighbor in territory.Neighbors)
            {
                if (stillMissingTerritories.Contains(neighbor))
                {
                    outvar.Add(neighbor);
                }
            }
            return outvar;
        }

        /// <summary>Calculates the amount of idle armies on the territory after the already made expansion decisions.
        /// </summary>
        /// <param name="territory"></param>
        /// <param name="expansionDecisions"></param>
        /// <returns></returns>
        private Armies GetOverflowIdleArmies(BotTerritory territory, Moves expansionDecisions)
        {
            var outvar = territory.GetIdleArmies();
            foreach (var placeArmiesMove in expansionDecisions.Orders.OfType<BotOrderDeploy>())
            {
                if (placeArmiesMove.Territory.ID == territory.ID)
                    outvar = outvar.Add(new Armies(placeArmiesMove.Armies));
            }
            foreach (var expansionMove in expansionDecisions.Orders.OfType<BotOrderAttackTransfer>())
            {
                if (expansionMove.From.ID == territory.ID)
                    outvar = outvar.Subtract(expansionMove.Armies);
            }
            return outvar;
        }
    }
}
