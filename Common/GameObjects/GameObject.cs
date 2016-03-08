using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public enum GameState
    {
        WaitingForPlayers = 2,
        Playing = 3,
        Finished = 4,
        DistributingTerritories = 5
    }

    public class GameObject
    {
        public GameIDType ID;
        public int NumberOfTurns;
        public Dictionary<PlayerIDType, GamePlayer> Players;
        public GameState State;

        public LatestGameInfoAuthenticated LatestInfo;
    }

    public class LatestGameInfoAuthenticated
    {
        public GameStanding LatestStanding;
        public GameStanding PreviousTurnStanding;
        public GameStanding DistributionStanding;
        public GameTurn LatestTurn;
        public Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        public Dictionary<PlayerIDType, PlayerIncome> Income;
        public List<CardInstance> Cards;
        public int CardsMustUse;
    }

    public class TeammateOrders
    {
        public GameOrder[] Orders;
        public TerritoryIDType[] Picks;
    }

}
