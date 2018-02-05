

namespace WarLight.Shared.AI
{
    /// <summary>This Move is used in the first part of each round.</summary>
    /// <remarks>
    /// This Move is used in the first part of each round. It represents what Territory
    /// is increased with how many armies.
    /// </remarks>
    public class GameOrderDeploy : GameOrder
    {
        public TerritoryIDType DeployOn;
        public int NumArmies; //int instead of Armies, since special units can never be deployed.

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Deploys; }
        }

        public static GameOrderDeploy Create(PlayerIDType playerID, int numArmies, TerritoryIDType deployOn, bool free)
        {
            var o = new GameOrderDeploy();
            o.NumArmies = numArmies;
            o.PlayerID = playerID;
            o.DeployOn = deployOn;
            return o;
        }

        public override string ToString()
        {
            return "Player " + PlayerID + " deploys " + NumArmies + " armies on " + DeployOn;
        }
    }
}
