 /*
 * This code was auto-converted from a java project.
 */

using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public class LastVisibleMapUpdater
    {
        public BotMain BotState;
        public LastVisibleMapUpdater(BotMain state)
        {
            this.BotState = state;
        }
        
        public void StoreOpponentDeployment()
        {
            var lastVisibleMap = BotState.LastVisibleMap;
            foreach (var opponentTerritory in lastVisibleMap.AllOpponentTerritories)
            {
                var armiesDeployed = BotState.HistoryTracker.GetOpponentDeployment(opponentTerritory.OwnerPlayerID);
                if (armiesDeployed > 0)
                    MovesCommitter.CommittPlaceArmiesMove(new BotOrderDeploy(opponentTerritory.OwnerPlayerID, opponentTerritory, armiesDeployed));
            }
        }
    }
}
