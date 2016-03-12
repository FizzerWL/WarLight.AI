using System.Collections.Generic;
using WarLight.Shared.AI;

namespace WarLight.AI.Wunderwaffe.Bot
{
    public class DeploymentHistory
    {
        public BotMain BotState;
        public DeploymentHistory(BotMain state)
        {
            BotState = state;
        }


        private Dictionary<PlayerIDType, int> OpponentDeployments = new Dictionary<PlayerIDType, int>();

        public virtual int GetOpponentDeployment(PlayerIDType opponentID)
        {
            if (BotState.NumberOfTurns < 1)
                return 0;

            return OpponentDeployments[opponentID];
        }
        
        public virtual void Update(PlayerIDType opponentID, int opponentDeployment)
        {
            OpponentDeployments.Add(opponentID, opponentDeployment);
            AILog.Log("DeploymentHistory", "VisibleDeployment for " + opponentID + ": " + opponentDeployment);
        }
        
    }
}
