using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Prod
{
    public class BotMain : IWarLightAI
    {
        GameStanding DistributionStanding;
        GameStanding LatestTurnStanding;
        PlayerIDType MyPlayerID;
        Dictionary<PlayerIDType, GamePlayer> Players;
        MapDetails Map;
        GameSettings Settings;
        Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        List<CardInstance> Cards;
        int CardsMustPlay;
        Dictionary<PlayerIDType, PlayerIncome> Incomes;


        public void Init(PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay)
        {
            this.DistributionStanding = distributionStanding;
            this.LatestTurnStanding = latestTurnStanding;
            this.MyPlayerID = myPlayerID;
            this.Players = players;
            this.Map = map;
            this.Settings = gameSettings;
            this.TeammatesOrders = teammatesOrders;
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
            this.Incomes = incomes;
        }

        public List<GameOrder> GetOrders()
        {
            return new TakePlayingTurnContainer(Map, LatestTurnStanding, Players, Settings, MyPlayerID, Incomes[MyPlayerID], Cards, CardsMustPlay, TeammatesOrders, Stopwatch.StartNew()).Orders;
        }

        public List<TerritoryIDType> GetPicks()
        {
            return GameAI.MakePicks(Players, DistributionStanding, Settings, Map, Players[MyPlayerID].ScenarioID).ToList();
        }

    }
}
