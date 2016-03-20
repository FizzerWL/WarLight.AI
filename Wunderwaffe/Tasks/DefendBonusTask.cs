using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>DefendBonusTask is responsible for defending a Bonus under threat.
    /// </summary>
    /// <remarks>DefendBonusTask is responsible for defending a Bonus under threat.
    /// </remarks>
    public static class DefendBonusTask
    {
        public static Moves CalculateDefendBonusTask(BotMain state, BotBonus bonus, int maxDeployment, bool acceptNotAllDefense, BotTerritory.DeploymentType lowerConservativeLevel, BotTerritory.DeploymentType upperConservativeLevel)
        {
            var outvar = new Moves();
            var threateningTerritories = GetThreateningTerritories(bonus);
            // First see if we can remove the threat by hitting the threatening
            // territory
            if (threateningTerritories.Count == 1)
            {
                var threatTerritory = threateningTerritories[0];
                var territoriesUnderThreat = GetAmountOfTerritoriesUnderThreat(threatTerritory, bonus);
                if (territoriesUnderThreat >= 2 && lowerConservativeLevel != 0)
                {
                    var removeThreatMoves = OneHitBreakTerritoryTask.CalculateBreakTerritoryTask(state, threatTerritory, maxDeployment, lowerConservativeLevel);
                    if (removeThreatMoves != null)
                    {
                        removeThreatMoves.Orders.OfType<BotOrderAttackTransfer>().First().Message = AttackMessage.EarlyAttack;
                        return removeThreatMoves;
                    }
                }
            }
            // If this is not possible try the classic defense
            var territoriesUnderThreat_1 = bonus.GetOwnedTerritoriesBorderingNeighborsOwnedByOpponent();
            outvar = state.DefendTerritoriesTask.CalculateDefendTerritoriesTask(territoriesUnderThreat_1, maxDeployment, acceptNotAllDefense, lowerConservativeLevel, upperConservativeLevel);
            return outvar;
        }

        private static int GetAmountOfTerritoriesUnderThreat(BotTerritory threateningTerritory, BotBonus bonus)
        {
            var outvar = 0;
            foreach (var neighbor in threateningTerritory.GetOwnedNeighbors())
            {
                if (neighbor.Details.PartOfBonuses.Contains(bonus.ID))
                    outvar++;
            }
            return outvar;
        }

        private static List<BotTerritory> GetThreateningTerritories(BotBonus ownedBonus)
        {
            var outvar = new HashSet<BotTerritory>();
            foreach (var territory in ownedBonus.Territories)
                outvar.AddRange(territory.GetOpponentNeighbors());
            var returnList = new List<BotTerritory>();
            returnList.AddRange(outvar);
            return returnList;
        }
    }
}
