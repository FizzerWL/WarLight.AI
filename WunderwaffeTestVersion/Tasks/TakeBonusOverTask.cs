using WarLight.AI.WunderwaffeTestVersion.Bot;
using WarLight.AI.WunderwaffeTestVersion.Move;

namespace WarLight.AI.WunderwaffeTestVersion.Tasks
{
    public class TakeBonusOverTask
    {
        /// <summary>
        /// Calculates the needed moves to push the opponent out of a Bonus that has no neutrals in it.
        /// </summary>
        /// <param name="maxDeployment"></param>
        /// <param name="Bonus"></param>
        /// <returns></returns>
        public static Moves CalculateTakeBonusOverTask(BotMain state, int maxDeployment, BotBonus bonus, BotTerritory.DeploymentType conservativeLevel)
        {
            var opponentTerritories = bonus.GetVisibleOpponentTerritories();
            return state.TakeTerritoriesTaskCalculator.CalculateTakeTerritoriesTask(maxDeployment, opponentTerritories, conservativeLevel, "TakeBonusOverTask");
        }
    }
}
