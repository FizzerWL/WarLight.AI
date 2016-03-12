using WarLight.AI.WunderwaffeTestVersion.Bot;
using WarLight.AI.WunderwaffeTestVersion.Move;

namespace WarLight.AI.WunderwaffeTestVersion.Tasks
{
    /// <summary>BonusRunTaks is responsible for moving in the direction of an opponent bonus.
    /// </summary>
    /// <remarks>BonusRunTaks is responsible for moving in the direction of an opponent bonus.
    /// </remarks>
    public class BonusRunTask
    {
        public static Moves CalculateBonusRunMoves(BotBonus opponentBonus, int maxDeployment
        , BotMap visibleMap, BotMap workingMap)
        {
            var outvar = new Moves();
            return outvar;
        }

        private static bool AreWeAlreadyMovingInDirection(BotBonus opponentBonus, BotMap visibleMap, BotMap workingMap)
        {
            var alreadyMovingInDirection = false;
            return alreadyMovingInDirection;
        }
    }
}
