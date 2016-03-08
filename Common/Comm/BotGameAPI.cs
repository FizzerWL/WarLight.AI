using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WarLight.Shared.AI
{
    public static class BotGameAPI
    {
        
        public static GameIDType CreateGame(IEnumerable<PlayerInvite> players, string gameName, int? templateID = null, Action<JObject> writeSettingsOpt = null)
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

            return (GameIDType)(int)Communication.Call("CreateBotGame", json)["gameID"];
        }

        public static Tuple<GameSettings, MapDetails> GetGameSettings(GameIDType gameID)
        {
            var gidNode = new JObject(new JProperty("gameID", (int)gameID));
            var response = Communication.Call("GetBotGameSettings", gidNode);

            var settings = Communication.ReadSettings(response["settings"]);
            var map = Communication.ReadMap(response["map"]);

            return new Tuple<GameSettings, MapDetails>(settings, map);
        }
        
        public static GameObject GetGameInfo(GameIDType gameID, PlayerIDType? playerID)
        {
            var input = new JObject();
            input["gameID"] = (int)gameID;

            if (playerID.HasValue)
                input["playerID"] = (int)playerID.Value;

            var response = Communication.Call("GetBotGameInfo", input);

            return Communication.ReadGameObject(response, gameID);
        }

        public static void SendPicks(GameIDType gameID, PlayerIDType playerID, IEnumerable<TerritoryIDType> picks)
        {
            var input = new JObject();
            input["gameID"] = (int)gameID;
            input["playerID"] = (int)playerID;

            input.Add("territoryIDs", new JArray(picks.Select(o => (int)o)));
            var response = Communication.Call("SendPicksBotGame", input);
            Assert.Fatal(response["success"] != null);
        }

        public static void SendOrders(GameIDType gameID, PlayerIDType playerID, IEnumerable<GameOrder> orders, int turnNumber)
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
            input["playerID"] = (int)playerID;
            input["orders"] = ordersArray;
            input["turnNumber"] = turnNumber;

            var response = Communication.Call("SendOrdersBotGame", input);
            Assert.Fatal(response["success"] != null);
        }

        public static string ExportGame(GameIDType gameID)
        {
            var gidNode = new JObject(new JProperty("gameID", (int)gameID));

            return (string)Communication.Call("ExportBotGame", gidNode)["result"];
        }

        public static void DeleteGame(GameIDType gameID)
        {
            var gidNode = new JObject(new JProperty("gameID", (int)gameID));
            Communication.Call("DeleteBotGame", gidNode);
        }

        public static Tuple<GameSettings, MapDetails, GameObject> GetBotExportedGame(GameIDType gameID, string exportedGame, PlayerIDType playerID, int? turnNumber)
        {
            var input = new JObject();
            input["exportedGame"] = exportedGame;
            input["playerID"] = (int)playerID;

            if (turnNumber.HasValue)
                input["turnNumber"] = turnNumber;

            var response = Communication.Call("GetBotExportedGameInfo", input);

            var game = Communication.ReadGameObject(response, gameID);
            var settings = Communication.ReadSettings(response["settings"]);
            var map = Communication.ReadMap(response["map"]);
            return new Tuple<GameSettings, MapDetails, GameObject>(settings, map, game);
        }
    }
}
