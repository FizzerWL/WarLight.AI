using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    // This task is there for moving towards distant opponent bonuses
    public class BonusRunTask
    {
        public BotMain BotState;

        public BonusRunTask(BotMain state)
        {
            BotState = state;
        }

        public Moves CalculateBonusRunMoves(int maxDeployment)
        {
            Moves bonusRunMoves = new Moves();
            AILog.Log("BonusRunTask", "bonus run task begin");
            if (!IsOpponentBonusPresent())
            {
                return null;
            }
            AILog.Log("BonusRunTask", "opponent bonus present");
            BotTerritory closestDistanceTerritory = getClosestDistanceTerritory();
            if (areWeAlreadyMovingInRightDirection(closestDistanceTerritory.DistanceToOpponentBonus))
            {
                return null;
            }
            AILog.Log("BonusRunTask", "not already moving in direction and closest distance territory is: " + closestDistanceTerritory.Details.Name + " with " + closestDistanceTerritory.DistanceToOpponentBonus);
            List<BotTerritory> toTerritories = getPossibleToTerritories(closestDistanceTerritory);
            AILog.Log("BonusRunTask", "ToTerritories.Count: " + toTerritories.Count);
            if (toTerritories.Count == 0)
            {
                return null;
            }
            AILog.Log("BonusRunTask", "to territories.count > 0 for, " + closestDistanceTerritory.Details.Name);
            if (!IsMovingTowardsOpponentSmart(toTerritories.First()))
            {
                return null;
            }
            AILog.Log("BonusRunTask", "found solution");
            List<BotTerritory> toTerritoryAsList = new List<BotTerritory>();
            toTerritoryAsList.Add(toTerritories[0]);
            return BotState.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(maxDeployment, toTerritoryAsList, BotTerritory.DeploymentType.Normal, "BonusRunTask");
        }

        private bool IsMovingTowardsOpponentSmart(BotTerritory ourTerritory)
        {
            // TODO
            return true;
        }

        private List<BotTerritory> getPossibleToTerritories(BotTerritory ourTerritory)
        {
            List<BotTerritory> neutralNeighbors = ourTerritory.Neighbors.Where(o => o.OwnerPlayerID == TerritoryStanding.NeutralPlayerID).ToList();
            List<BotTerritory> closerNeighbors = neutralNeighbors.Where(o => o.DistanceToOpponentBonus < ourTerritory.DistanceToOpponentBonus).ToList();
            closerNeighbors = closerNeighbors.OrderBy(o => o.Armies.AttackPower).ToList();
            return closerNeighbors;
        }

        private bool IsOpponentBonusPresent()
        {
            List<BotBonus> allBonuses = BotState.VisibleMap.Bonuses.Values.Where(o => o.Details.Amount > 0).ToList();
            List<BotBonus> opponentBonuses = allBonuses.Where(o => o.IsOwnedByAnyOpponent()).ToList();
            return opponentBonuses.Count > 0;
            //   BotState.VisibleMap.Territories.First().Value.DistanceToOpponentBonus > -1;
        }

        private bool areWeAlreadyMovingInRightDirection(int closestDistance)
        {
            List<BotTerritory> ownedTerritories = BotState.WorkingMap.GetOwnedTerritories();
            ownedTerritories = ownedTerritories.OrderBy(o => o.DistanceToOpponentBonus).ToList();

            BotTerritory visibleOwnedTerritory = BotState.VisibleMap.Territories.Values.Where(o => o.ID.Equals(ownedTerritories.First().ID)).First();
            return visibleOwnedTerritory.DistanceToOpponentBonus < closestDistance;
        }

        private BotTerritory getClosestDistanceTerritory()
        {
            List<BotTerritory> ownedTerritories = BotState.VisibleMap.GetOwnedTerritories();
            ownedTerritories = ownedTerritories.OrderBy(o => o.DistanceToOpponentBonus).ToList();
            return ownedTerritories.First();
        }

    }
}