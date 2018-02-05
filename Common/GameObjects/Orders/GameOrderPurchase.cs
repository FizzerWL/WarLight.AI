using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    /// <summary>
    /// Used for purchasing cities in commerce games.  Support for commerce games is not fully implemented in the AI framework.
    /// </summary>
    public class GameOrderPurchase : GameOrder
    {
        public Dictionary<TerritoryIDType, int> BuildCities;

        public static GameOrderPurchase Create(PlayerIDType playerID)
        {
            var r = new GameOrderPurchase();
            r.PlayerID = playerID;
            r.BuildCities = new Dictionary<TerritoryIDType, int>();
            return r;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Purchase; }
        }

        public int Cost(ResourceType resource, GameSettings settings, int armiesDeployedSoFarThisTurn, GameStanding standing)
        {
            return 0; //not implemented
        }

    }
}
