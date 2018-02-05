using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WarLight.Shared.AI
{
    public static class Communication
    {
        public static string HttpRoot = "http://aiserver.warzone.com/api/";
        //public static string HttpRoot = "http://192.168.1.105:81/AIServer/api/";
        //public static string HttpRoot = "http://192.168.1.105:9000/AIServer/api/";

        public static JToken Call(string api, JToken input)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(HttpRoot + api);
            req.Method = "POST";

            var bytes = Encoding.UTF8.GetBytes(input.ToString());
            using (var stream = req.GetRequestStream())
                stream.Write(bytes, 0, bytes.Length);

            using (var response = req.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var responseRaw = reader.ReadToEnd();

                var json = JToken.Parse(responseRaw);
                if (json["error"] != null)
                    throw new Exception("Server call to " + api + " failed.  Response = " + json + ", sent = " + input);

                return json;
            }
        }

        public static LatestGameInfoAuthenticated ReadLatestInfo(JToken gameInfo)
        {
            var ret = new LatestGameInfoAuthenticated();

            ret.LatestStanding = ReadGameStanding(gameInfo["latestStanding"]);
            ret.PreviousTurnStanding = ReadGameStanding(gameInfo["previousTurnStanding"]);
            ret.DistributionStanding = ReadGameStanding(gameInfo["distributionStanding"]);
            ret.LatestTurn = ReadGameTurn(gameInfo["latestTurn"]);
            ret.Income = gameInfo["incomes"].As<JArray>().ToDictionary(o => (PlayerIDType)(int)o["playerID"], o => ReadPlayerIncome((JObject)o));
            ret.CardsMustUse = gameInfo["cardsMustUse"] == null ? 0 : (int)gameInfo["cardsMustUse"];
            ret.Cards = gameInfo["cards"] == null ? null : gameInfo["cards"].As<JArray>().Select(ReadCardInstance).ToList();

            if (gameInfo["teammatesOrders"] != null)
            {
                ret.TeammatesOrders = new Dictionary<PlayerIDType, TeammateOrders>();

                foreach(var teammatesOrders in ((JArray)gameInfo["teammatesOrders"]))
                {
                    var pid = (PlayerIDType)(int)teammatesOrders["playerID"];
                    var orders = new TeammateOrders();
                    if (teammatesOrders["picks"] != null)
                        orders.Picks = ((JArray)teammatesOrders["picks"]).Select(o => (TerritoryIDType)(int)o).ToArray();
                    if (teammatesOrders["orders"] != null)
                        orders.Orders = ReadOrders(teammatesOrders["orders"]);

                    ret.TeammatesOrders.Add(pid, orders);
                }
            }

            return ret;
        }

        public static GameObject ReadGameObject(JToken response, GameIDType gameID)
        {
            var ret = new GameObject();

            var gameNode = response["game"];

            ret.ID = gameID;
            ret.NumberOfTurns = (int)gameNode["numberOfTurns"];
            ret.State = (GameState)Enum.Parse(typeof(GameState), (string)gameNode["state"]);
            ret.Players = ((JArray)gameNode["players"]).Select(Communication.ReadGamePlayer).ToDictionary(o => o.ID, o => o);

            var gameInfo = response["gameInfo"];
            if (gameInfo != null && gameInfo.Type == JTokenType.Object)
                ret.LatestInfo = Communication.ReadLatestInfo(gameInfo);

            return ret;
        }

        public static MapDetails ReadMap(JToken mapNode)
        {
            var map = new MapDetails();

            map.ID = (MapIDType)(int)mapNode["id"];
            map.Name = (string)mapNode["name"];

            foreach (var terrNode in (JArray)mapNode["territories"])
            {
                var td = new TerritoryDetails(map, (TerritoryIDType)(int)terrNode["id"]);
                td.Name = (string)terrNode["name"];
                td.ConnectedTo = terrNode["connectedTo"].As<JArray>().Select(o => (TerritoryIDType)(int)o).ToDictionary(o => o, o => (object)null);
                map.Territories.Add(td.ID, td);
            }

            foreach (var bonusNode in (JArray)mapNode["bonuses"])
            {
                var bd = new BonusDetails(map, (BonusIDType)(int)bonusNode["id"], (int)bonusNode["value"]);
                bd.Name = (string)bonusNode["name"];
                bd.Territories = bonusNode["territoryIDs"].As<JArray>().Select(o => (TerritoryIDType)(int)o).ToList();

                map.Bonuses.Add(bd.ID, bd);
                foreach (var terrID in bd.Territories)
                    map.Territories[terrID].PartOfBonuses.Add(bd.ID);
            }

            foreach (var distNode in (JArray)mapNode["distributionModes"])
            {
                var d = new DistributionMode();
                d.ID = (DistributionIDType)(int)distNode["id"];
                d.Name = (string)distNode["name"];
                d.Type = (string)distNode["type"];

                if (distNode["territories"] != null)
                    d.Territories = distNode["territories"].Cast<JProperty>().ToDictionary(o => (TerritoryIDType)int.Parse(o.Name), o => (ushort)(int)o.Value);

                map.DistributionModes.Add(d.ID, d);
            }
            return map;
        }

        public static GameSettings ReadSettings(JToken settingsNode)
        {
            var overriddenBonuses = settingsNode["OverriddenBonuses"].As<JArray>().ToDictionary(o => (BonusIDType)(int)o["bonusID"], o => (int)o["value"]);
            var terrLimit = settingsNode["TerritoryLimit"].Type == JTokenType.String ? 0 : (int)settingsNode["TerritoryLimit"];
            var roundingMode = (RoundingModeEnum)Enum.Parse(typeof(RoundingModeEnum), (string)settingsNode["RoundingMode"]);
            var fog = (GameFogLevel)Enum.Parse(typeof(GameFogLevel), (string)settingsNode["Fog"]);

            var cards = new Dictionary<CardIDType, object>();
            Action<CardType, string> addCard = (cardType, nodeName) =>
            {
                var node = settingsNode[nodeName];
                if (node.Type == JTokenType.Object)
                    cards.Add(cardType.CardID, null);
            };
            addCard(CardType.Airlift, "AirliftCard");
            addCard(CardType.Blockade, "BlockadeCard");
            addCard(CardType.Bomb, "BombCard");
            addCard(CardType.Diplomacy, "DiplomacyCard");
            addCard(CardType.EmergencyBlockade, "AbandonCard");
            addCard(CardType.Gift, "GiftCard");
            addCard(CardType.OrderDelay, "OrderDelayCard");
            addCard(CardType.OrderPriority, "OrderPriorityCard");
            addCard(CardType.Reconnaissance, "ReconnaissanceCard");
            addCard(CardType.Reinforcement, "ReinforcementCard");
            addCard(CardType.Sanctions, "SanctionsCard");
            addCard(CardType.Spy, "SpyCard");
            addCard(CardType.Surveillance, "SurveillanceCard");


            return new GameSettings(
                (double)settingsNode["OffensiveKillRate"] / 100.0,
                (double)settingsNode["DefensiveKillRate"] / 100.0,
                (bool)settingsNode["OneArmyStandsGuard"],
                (int)settingsNode["MinimumArmyBonus"],
                (int)settingsNode["InitialNeutralsInDistribution"],
                (int)settingsNode["InitialNonDistributionArmies"],
                terrLimit,
                (DistributionIDType)(int)settingsNode["DistributionMode"],
                overriddenBonuses,
                (bool)settingsNode["Commanders"],
                (bool)settingsNode["AllowAttackOnly"],
                (bool)settingsNode["AllowTransferOnly"],
                (int)settingsNode["InitialPlayerArmiesPerTerritory"],
                roundingMode,
                (double)settingsNode["LuckModifier"],
                (bool)settingsNode["MultiAttack"],
                (bool)settingsNode["AllowPercentageAttacks"],
                cards,
                (bool)settingsNode["LocalDeployments"],
                fog,
                (bool)settingsNode["NoSplit"]
                );

        }

        public static CardInstance ReadCardInstance(JToken jToken)
        {
            CardInstance ret;
            if (jToken["armies"] != null)
            {
                ret = new ReinforcementCardInstance();
                ret.As<ReinforcementCardInstance>().Armies = (int)jToken["armies"];
            }
            else
                ret = new CardInstance();

            ret.CardID = (CardIDType)(int)jToken["cardID"];
            ret.ID = (CardInstanceIDType)Guid.Parse((string)jToken["cardInstanceID"]);
            return ret;
        }

        public static PlayerIncome ReadPlayerIncome(JObject node)
        {
            var ret = new PlayerIncome();
            ret.FreeArmies = (int)node["freeArmies"];
            ret.BonusRestrictions = node["bonusRestrictions"].As<JArray>().ToDictionary(o => (BonusIDType)(int)o["bonusID"], o => (int)o["value"]);
            return ret;
        }

        static CultureInfo EnUS = new CultureInfo("en-US");

        public static GameTurn ReadGameTurn(JToken jToken)
        {
            if (jToken.Type == JTokenType.String && (string)jToken == "null")
                return null;

            var ret = new GameTurn();

            ret.Date = DateTime.Parse((string)jToken["date"], EnUS);
            ret.Orders = ReadOrders(jToken["orders"]);
            return ret;
        }

        public static GameStanding ReadGameStanding(JToken jToken)
        {
            var ret = new GameStanding();

            if (jToken == null || (jToken.Type == JTokenType.String && (string)jToken == "null"))
                return null;

            foreach(var terr in (JArray)jToken)
            {
                var terrID = (TerritoryIDType)(int)terr["terrID"];
                var playerID = ToPlayerID((string)terr["ownedBy"]);
                var armies = ToArmies((string)terr["armies"]);
                ret.Territories.Add(terrID, TerritoryStanding.Create(terrID, playerID, armies));
            }

            return ret;
        }

        public static Armies ToArmies(string str)
        {
            if (str == "Fogged")
                return new Armies(fogged: true);
            else
                return Armies.DeserializeFromString(str);
        }

        public static PlayerIDType ToPlayerID(string str)
        {
            if (str == "AvailableForDistribution")
                return TerritoryStanding.AvailableForDistribution;
            else if (str == "Fogged")
                return TerritoryStanding.FogPlayerID;
            else if (str == "Neutral")
                return TerritoryStanding.NeutralPlayerID;
            else
                return (PlayerIDType)int.Parse(str);
        }

        public static GameOrder[] ReadOrders(JToken jToken)
        {
            var ret = new List<GameOrder>();
            foreach(var jOrder in (JArray)jToken)
                ret.Add(ReadOrder(jOrder));

            return ret.ToArray();
        }

        public static GameOrder ReadOrder(JToken jOrder)
        {
            var type = (string)jOrder["type"];

            var playerID = (PlayerIDType)(int)jOrder["playerID"];
            if (type == "GameOrderDeploy")
            {
                var terrID = (TerritoryIDType)(int)jOrder["deployOn"];
                var armies = (int)jOrder["armies"];
                return GameOrderDeploy.Create(playerID, armies, terrID, false);
            }

            if (type == "GameOrderAttackTransfer")
            {
                var from = (TerritoryIDType)(int)jOrder["from"];
                var to = (TerritoryIDType)(int)jOrder["to"];
                var armies = ToArmies((string)jOrder["numArmies"]);
                var attackTeammates = (bool)jOrder["attackTeammates"];
                var byPercent = (bool)jOrder["byPercent"];
                var attackTransfer = (AttackTransferEnum)Enum.Parse(typeof(AttackTransferEnum), (string)jOrder["attackTransfer"]);
                return GameOrderAttackTransfer.Create(playerID, from, to, attackTransfer, byPercent, armies, attackTeammates);
            }

            if (type.StartsWith("GameOrderPlayCard"))
            {
                var cardInstanceID = (CardInstanceIDType)Guid.Parse((string)jOrder["cardInstanceID"]);
                switch (type)
                {
                    case "GameOrderPlayCardReinforcement":
                        return GameOrderPlayCardReinforcement.Create(cardInstanceID, playerID);
                    case "GameOrderPlayCardAbandon":
                        return GameOrderPlayCardAbandon.Create(cardInstanceID, playerID, (TerritoryIDType)(int)jOrder["targetTerritoryID"]);
                    case "GameOrderPlayCardAirlift":
                        return GameOrderPlayCardAirlift.Create(cardInstanceID, playerID, (TerritoryIDType)(int)jOrder["from"], (TerritoryIDType)(int)jOrder["to"], ToArmies((string)jOrder["armiesToAirlift"]));
                    case "GameOrderPlayCardBlockade":
                        return GameOrderPlayCardBlockade.Create(cardInstanceID, playerID, (TerritoryIDType)(int)jOrder["targetTerritoryID"]);
                    case "GameOrderPlayCardBomb":
                        return GameOrderPlayCardBomb.Create(cardInstanceID, playerID, (TerritoryIDType)(int)jOrder["targetTerritoryID"]);
                    case "GameOrderPlayCardDiplomacy":
                        return GameOrderPlayCardDiplomacy.Create(cardInstanceID, playerID, (PlayerIDType)(int)jOrder["playerOne"], (PlayerIDType)(int)jOrder["playerTwo"]);
                    case "GameOrderPlayCardGift":
                        return GameOrderPlayCardGift.Create(cardInstanceID, playerID, (TerritoryIDType)(int)jOrder["territoryID"], (PlayerIDType)(int)jOrder["giftTo"]);
                    case "GameOrderPlayCardOrderDelay":
                        return GameOrderPlayCardOrderDelay.Create(cardInstanceID, playerID);
                    case "GameOrderPlayCardOrderPriority":
                        return GameOrderPlayCardOrderPriority.Create(cardInstanceID, playerID);
                    case "GameOrderPlayCardReconnaissance":
                        return GameOrderPlayCardReconnaissance.Create(cardInstanceID, playerID, (TerritoryIDType)(int)jOrder["targetTerritory"]);
                    case "GameOrderPlayCardSanctions":
                        return GameOrderPlayCardSanctions.Create(cardInstanceID, playerID, (PlayerIDType)(int)jOrder["sanctionedPlayerID"]);
                    case "GameOrderPlayCardSpy":
                        return GameOrderPlayCardSpy.Create(cardInstanceID, playerID, (PlayerIDType)(int)jOrder["targetPlayerID"]);
                    case "GameOrderPlayCardSurveillance":
                        return GameOrderPlayCardSurveillance.Create(cardInstanceID, playerID, (BonusIDType)(int)jOrder["targetBonus"]);
                    case "GameOrderPlayCardFogged":
                        return GameOrderPlayCardFogged.Create(playerID, cardInstanceID);

                }
            }


            switch (type)
            {
                case "GameOrderReceiveCard":

                    var ret = new GameOrderReceiveCard();
                    ret.PlayerID = (PlayerIDType)(int)jOrder["playerID"];
                    ret.InstancesCreated = new List<CardInstance>();
                    foreach(var card in (JArray)jOrder["cardsMadeWhole"])
                    {
                        var inst = new CardInstance();
                        inst.ID = (CardInstanceIDType)Guid.Parse((string)card["cardInstanceID"]);
                        inst.CardID = (CardIDType)(int)card["cardID"];
                        ret.InstancesCreated.Add(inst);
                    }
                    return ret;
                case "GameOrderStateTransition":
                    var stateTrans = new GameOrderStateTransition();
                    stateTrans.PlayerID = (PlayerIDType)(int)jOrder["playerID"];
                    stateTrans.NewState = (GamePlayerState)Enum.Parse(typeof(GamePlayerState), (string)jOrder["newState"]);
                    return stateTrans;
                case "ActiveCardWoreOff":
                    var active = new ActiveCardWoreOff();
                    active.PlayerID = (PlayerIDType)(int)jOrder["playerID"];
                    active.CardInstanceID = (CardInstanceIDType)Guid.Parse((string)jOrder["cardInstanceID"]);
                    return active;

                case "GameOrderDiscard":
                    return GameOrderDiscard.Create((PlayerIDType)(int)jOrder["playerID"], (CardInstanceIDType)Guid.Parse((string)jOrder["cardInstanceID"]));
            }

            throw new Exception("Need handler for order type " + type);
        }

        public static GamePlayer ReadGamePlayer(JToken json)
        {
            var id = (PlayerIDType)(int)json["id"];
            var state = (GamePlayerState)Enum.Parse(typeof(GamePlayerState), (string)json["state"]);
            var team = json["team"] == null ? PlayerInvite.NoTeam : (TeamIDType)(int)json["team"];
            var scenario = json["scenario"] == null ? (ushort)0 : (ushort)(int)json["scenario"];
            return new GamePlayer(id, state, team, scenario, (bool)json["isAI"], (bool)json["humanTurnedIntoAI"], (bool)json["hasCommittedOrders"]);
        }

        public static void WriteOrder(JObject jOrder, GameOrder order)
        {
            jOrder["type"] = order.GetType().Name;
            jOrder["playerID"] = (int)order.PlayerID;

            if (order is GameOrderDeploy)
            {
                var deploy = (GameOrderDeploy)order;

                jOrder["armies"] = deploy.NumArmies;
                jOrder["deployOn"] = (int)deploy.DeployOn;
            }
            else if (order is GameOrderAttackTransfer)
            {
                var attack = (GameOrderAttackTransfer)order;

                jOrder["from"] = (int)attack.From;
                jOrder["to"] = (int)attack.To;
                jOrder["numArmies"] = attack.NumArmies.SerializeToString();
                jOrder["attackTeammates"] = attack.AttackTeammates;
            }
            else if (order is GameOrderDiscard)
            {
                var cardInstanceID = order.As<GameOrderDiscard>().CardInstanceID;
                jOrder["cardInstanceID"] = cardInstanceID.ToString();
            }
            else if (order is GameOrderPlayCard)
            {
                var cardInstanceID = order.As<GameOrderPlayCard>().CardInstanceID;
                jOrder["cardInstanceID"] = cardInstanceID.ToString();

                if (order is GameOrderPlayCardSpy)
                    jOrder["targetPlayerID"] = (int)order.As<GameOrderPlayCardSpy>().TargetPlayerID;
                else if (order is GameOrderPlayCardAbandon)
                    jOrder["targetTerritoryID"] = (int)order.As<GameOrderPlayCardAbandon>().TargetTerritoryID;
                else if (order is GameOrderPlayCardAirlift)
                {
                    var airlift = order.As<GameOrderPlayCardAirlift>();
                    jOrder["from"] = (int)airlift.FromTerritoryID;
                    jOrder["to"] = (int)airlift.ToTerritoryID;
                    jOrder["armiesToAirlift"] = airlift.Armies.SerializeToString();
                }
                else if (order is GameOrderPlayCardGift)
                {
                    var gift = order.As<GameOrderPlayCardGift>();
                    jOrder["territoryID"] = (int)gift.TerritoryID;
                    jOrder["giftTo"] = (int)gift.GiftTo;
                }
                else if (order is GameOrderPlayCardDiplomacy)
                {
                    var dip = order.As<GameOrderPlayCardDiplomacy>();
                    jOrder["playerOne"] = (int)dip.PlayerOne;
                    jOrder["playerTwo"] = (int)dip.PlayerTwo;
                }
                else if (order is GameOrderPlayCardSanctions)
                    jOrder["sanctionedPlayerID"] = (int)order.As<GameOrderPlayCardSanctions>().SanctionedPlayerID;
                else if (order is GameOrderPlayCardReconnaissance)
                    jOrder["targetTerritory"] = (int)order.As<GameOrderPlayCardReconnaissance>().TargetTerritory;
                else if (order is GameOrderPlayCardSurveillance)
                    jOrder["targetBonus"] = (int)order.As<GameOrderPlayCardSurveillance>().TargetBonus;
                else if (order is GameOrderPlayCardBlockade)
                    jOrder["targetTerritoryID"] = (int)order.As<GameOrderPlayCardBlockade>().TargetTerritoryID;
                else if (order is GameOrderPlayCardBomb)
                    jOrder["targetTerritoryID"] = (int)order.As<GameOrderPlayCardBomb>().TargetTerritoryID;



            }
            else
                throw new Exception("Need handler for order type " + order);


        }
    }
}
