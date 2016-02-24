 /*
 * This code was auto-converted from a java project.
 */

using System;
using System.Collections.Generic;
using WarLight.AI.Wunderwaffe.Evaluation;



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
            AILog.Log("VisibleDeployment for " + opponentID + ": " + opponentDeployment);
        }
        
    }
}
