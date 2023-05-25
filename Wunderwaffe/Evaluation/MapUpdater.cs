using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public class MapUpdater
    {
        public BotMain BotState;
        public MapUpdater(BotMain state)
        {
            this.BotState = state;
        }

        // TODO
        public void UpdateMap(BotMap mapToUpdate)
        {
            BotState.WorkingMap = BotState.VisibleMap.GetMapCopy();
            foreach (var wmTerritory in BotState.WorkingMap.Territories.Values)
            {
                var vmTerritory = BotState.VisibleMap.Territories[wmTerritory.ID];
                if (vmTerritory.OwnerPlayerID != BotState.Me.ID && vmTerritory.IsVisible)
                {
                    var toBeKilledArmies = vmTerritory.GetArmiesAfterDeployment(BotTerritory.DeploymentType.Normal);
                    var attackingArmies = vmTerritory.GetIncomingArmies();
                    if (vmTerritory.getOwnKills(attackingArmies.AttackPowerOr(10), toBeKilledArmies.DefensePowerOr(10)) >= vmTerritory.Armies.DefensePowerOr(10))
                    //if (Math.Round(attackingArmies.AttackPowerOr(10) * BotState.Settings.OffensiveKillRate) >= toBeKilledArmies.DefensePowerOr(10))
                    {
                        wmTerritory.OwnerPlayerID = BotState.Me.ID;
                        wmTerritory.Armies = vmTerritory.GetIncomingArmies().Subtract(new Armies(SharedUtility.Round(vmTerritory.GetArmiesAfterDeployment(BotTerritory.DeploymentType.Normal).DefensePowerOr(10) * BotState.Settings.DefenseKillRate)));
                    }
                    else
                    {
                        // TODO
                        wmTerritory.Armies = vmTerritory.GetArmiesAfterDeployment(BotTerritory.DeploymentType.Normal).Subtract(new Armies(SharedUtility.Round(BotState.Settings.OffenseKillRate * vmTerritory.GetIncomingArmies().AttackPowerOr(10))));
                    }
                }
            }
        }

        /// <summary>Updates the working map according to the move input</summary>
        /// <param name="attackTransferMove"></param>
        public void UpdateMap(BotOrderAttackTransfer attackTransferMove, BotMap mapToUpdate, BotTerritory.DeploymentType conservativeLevel)
        {
            var toTerritoryID = attackTransferMove.To.ID;
            var wmTerritory = BotState.WorkingMap.Territories[toTerritoryID];
            var vmTerritory = BotState.VisibleMap.Territories[toTerritoryID];
            var toBeKilledArmies = vmTerritory.GetArmiesAfterDeployment(BotTerritory.DeploymentType.Normal);
            var attackingArmies = vmTerritory.GetIncomingArmies();
            if (vmTerritory.getOwnKills(attackingArmies.AttackPowerOr(10), toBeKilledArmies.DefensePowerOr(10)) >= toBeKilledArmies.DefensePowerOr(10))
            {
                wmTerritory.OwnerPlayerID = BotState.Me.ID;
            }
        }
    }
}
