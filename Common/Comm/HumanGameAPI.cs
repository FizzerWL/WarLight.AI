using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WarLight.Shared.AI
{
    public static class HumanGameAPI
    {
        public static void AcceptGame(GameIDType gameID)
        {
            var input = new JObject(new JProperty("gameID", (int)gameID));
            var response = Communication.Call("AcceptGame", input);
            Assert.Fatal(response["success"] != null);
        }

        public static GameIDType CreateGame(IEnumerable<PlayerInvite> players, string gameName, int? templateID, Action<JObject> writeSettingsOpt = null)
        {
            var playersNode = new JArray();
            foreach (var player in players)
            {
                var playerNode = new JObject();
                playerNode["token"] = player.InviteString;
                if (player.Team != PlayerInvite.NoTeam)
                    playerNode["team"] = (int)player.Team;
                if (player.Slot.HasValue)
                    playerNode["slot"] = (int)player.Slot.Value;

                playersNode.Add(playerNode);
            }

            var settings = new JObject();

            if (writeSettingsOpt != null)
                writeSettingsOpt(settings);
            
            var json = new JObject();

            json["gameName"] = gameName;
            json["players"] = playersNode;
            json["settings"] = settings;

            if (templateID.HasValue)
                json["templateID"] = templateID.Value;


            return (GameIDType)(int)Communication.Call("CreateGame", json)["gameID"];
        }

        public static Tuple<GameSettings, MapDetails> GetGameSettings(GameIDType gameID)
        {
            var gidNode = new JObject(new JProperty("gameID", (int)gameID));
            var response = Communication.Call("GetGameSettings", gidNode);

            var settings = Communication.ReadSettings(response["settings"]);
            var map = Communication.ReadMap(response["map"]);

            return new Tuple<GameSettings, MapDetails>(settings, map);
        }

        public static Tuple<int, GameState> GetGameStatus(GameIDType gameID)
        {
            var gidNode = new JObject(new JProperty("gameID", (int)gameID));
            var response = Communication.Call("GetGameStatus", gidNode);
            return new Tuple<int, GameState>((int)response["turnNumber"], (GameState)(int)response["gameState"]);
        }

        public static GameObject GetGameInfo(GameIDType gameID, int? turnNumber)
        {
            var input = new JObject();
            input["gameID"] = (int)gameID;

            if (turnNumber.HasValue)
                input["turnNumber"] = turnNumber.Value;

            var response = Communication.Call("GetGameInfo", input);

            return Communication.ReadGameObject(response, gameID);
        }

        public static void SendPicks(GameIDType gameID, IEnumerable<TerritoryIDType> picks)
        {
            var input = new JObject(new JProperty("gameID", (int)gameID));
            input.Add("territoryIDs", new JArray(picks.Select(o => (int)o)));
            var response = Communication.Call("SendPicks", input);
            Assert.Fatal(response["success"] != null);
        }

        public static void SendOrders(GameIDType gameID, IEnumerable<GameOrder> orders, int turnNumber)
        {

            var ordersArray = new JArray();
            foreach (var order in orders)
            {
                var jOrder = new JObject();
                Communication.WriteOrder(jOrder, order);
                ordersArray.Add(jOrder);
            }
            var input = new JObject();
            input["gameID"] = (int)gameID;
            input["orders"] = ordersArray;
            input["turnNumber"] = turnNumber;

            var response = Communication.Call("SendOrders", input);
            Assert.Fatal(response["success"] != null);
        }

    }
}
