using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace WarLight.Shared.AI
{

    /// <summary>
    /// Adapter to use this bot in competitions at theaigames.com
    /// </summary>
    public class AIGamesParser
    {
        static MapDetails Map;
        static GameStanding DistributionStanding;
        static GameOrder[] PrevTurn;
        static GameStanding PreviousTurnStanding;
        static GameStanding LatestTurnStanding;
        static List<TerritoryIDType> PickedTerritories;
        static List<GameOrder> CompletedOrders;
        static PlayerIDType MyPlayerID;
        static PlayerIDType OpponentPlayerID;
        static int StartingPicksAmount;
        static int StartingArmies;
        static int NumberOfTurns = -1;
        static Dictionary<PlayerIDType, GamePlayer> Players;


        public static void Go(string[] args)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(512))); //from http://theaigames.com/languages/cs

            while (true)
            {
                var line = Console.ReadLine();
                if (line == null)
                    break;
                line = line.Trim();

                if (line.Length == 0)
                    continue;

                var parts = line.Split(' ');
                if (parts[0] == "pick_starting_region")
                {
                    if (PickedTerritories == null)
                    {
                        LatestTurnStanding = DistributionStanding; //during picking, LatestStanding and DistributionStanding are the same thing
                        InitBot();
                        PickedTerritories = Bot.GetPicks();
                        AILog.Log("AIGamesParser", "Bot picked " + PickedTerritories.Select(o => o.ToString()).JoinStrings(" "));
                    }

                    var timeout = long.Parse(parts[1]);
                    // pick which territories you want to start with
                    var pickableStartingTerritories = parts.Skip(2).Select(o => (TerritoryIDType)int.Parse(o)).ToHashSet(false);
                    var pick = PickedTerritories.Where(o => pickableStartingTerritories.Contains(o)).ToList();

                    if (pick.Count > 0)
                        Console.Out.WriteLine(pick[0]);
                    else
                    {
                        AILog.Log("AIGamesParser", "None of bot's picks are still available, picking random");
                        Console.Out.WriteLine(pickableStartingTerritories.Random());
                    }
                }
                else if (parts[0] == "go")
                {
                    var timeout = long.Parse(parts[2]);
                    Assert.Fatal(parts.Length == 3);
                    NumberOfTurns++;


                    // we need to do a move
                    var output = new StringBuilder();
                    if (parts[1] == "place_armies")
                    {
                        AILog.Log("AIGamesParser", "================= Beginning turn " + NumberOfTurns + " =================");

                        //Re-create the bot before every turn.  This is done to simulate how the bot will run in production -- it can't be active and maintaining state for the entirety of a multi-day game, since those can take months or years.  Instead, it will be created before each turn, ran once, then thrown away.
                        InitBot();

                        CompletedOrders = Bot.GetOrders();

                        // place armies
                        foreach (var move in CompletedOrders.OfType<GameOrderDeploy>())
                            output.Append(GetDeployString(move) + ",");
                    }
                    else if (parts[1] == "attack/transfer")
                        foreach (var move in CompletedOrders.OfType<GameOrderAttackTransfer>())
                            output.Append(GetAttackString(move) + ",");
                    else
                        throw new Exception("Unexpected " + parts[1]);

                    if (output.Length > 0)
                        Console.Out.WriteLine(output);
                    else
                        Console.Out.WriteLine("No moves");
                }
                else if (parts[0] == "settings" && parts[1] == "starting_regions")
                {
                    foreach (var terrID in parts.Skip(2).Select(o => (TerritoryIDType)int.Parse(o)))
                    {
                        DistributionStanding.Territories[terrID].OwnerPlayerID = TerritoryStanding.AvailableForDistribution;
                    }
                }
                else if (parts.Length == 3 && parts[0] == "settings")
                    UpdateSettings(parts[1], parts[2]); // update settings
                else if (parts[0] == "setup_map")
                    SetupMap(parts); // initial full map is given
                else if (parts[0] == "update_map")
                {
                    PreviousTurnStanding = LatestTurnStanding;
                    LatestTurnStanding = ReadMap(parts);
                }
                else if (parts[0] == "opponent_moves")
                    ReadOpponentMoves(parts); // all visible opponent moves are given
                else
                    throw new Exception("Unable to parse line \"" + line + "\"");
            }
        }




        private static void UpdateSettings(string key, string value)
        {
            if (key == "your_bot")
                MyPlayerID = ToPlayerID(value);
            else if (key == "opponent_bot")
                OpponentPlayerID = ToPlayerID(value);
            else if (key == "starting_pick_amount")
                StartingPicksAmount = int.Parse(value); // next round
            else if (key == "starting_armies")
                StartingArmies = int.Parse(value);
            else if (key == "timebank" || key == "time_per_move" || key == "max_rounds")
            {
                //don't care
            }
            else
                throw new Exception("Unexpected setting " + key);
        }



        /*
    Bot1: setup_map super_regions 1 5 2 4 3 1 4 3 5 4 6 3 7 3 8 2 9 3 10 3 11 6 12 5 13 4
    Bot1: setup_map regions 1 1 2 1 3 1 4 1 5 1 6 2 7 2 8 2 9 2 10 3 11 3 12 3 13 3 14 4 15 4 16 4 17 4 18 4 19 4 20 5 21 5 22 5 23 5 24 6 25 6 26 6 27 6 28 7 29 7 30 7 31 7 32 7 33 8 34 8 35 8 36 8 37 9 38 9 39 9 40 9 41 10 42 10 43 10 44 10 45 10 46 10 47 11 48 11 49 11 50 11 51 11 52 11 53 11 54 12 55 12 56 12 57 12 58 12 59 13 60 13 61 13 62 13 63 13 64 13
    Bot1: setup_map neighbors 1 3,2,6 2 3,5,4,8,6 3 4 4 24,5,25 5 24,20,7,8 6 8,7 7 9,8,20 9 20,21 10 11,12 11 12,13 12 13,29 13 29,28 14 15 15 18,28,16 16 17,18 17 18,33,19 18 33,28,34,29,30 19 33 20 21,22,24,26,23 21 23 22 26,23,38,24,59 24 26,27,25,38 25 27 27 37 28 30,29 29 31,30,34,32 31 48,32,51 32 34 33 35,34 34 36,35 35 36 37 38,39,40 38 40,55,60 39 54,40,49 40 55,54,56 41 44,42,46,62 42 44,43,45 43 45 44 61,62,45 45 59,61 46 62 47 48,50 48 51,50 49 50,56,54 50 56,51,52,53 51 52 52 53 53 56,58 54 56 55 57,56 56 58,57 57 58 59 61,63,64,60 60 63 61 62,64 62 64 63 64
    Bot1: setup_map wastelands 5 13 40 49 62
    */
        private static void SetupMap(string[] mapInput)
        {
            if (Map == null)
                Map = new MapDetails();


            if (mapInput[1] == "super_regions")
            {
                for (var i = 2; i < mapInput.Length; i++)
                {
                    var bonusID = (BonusIDType)int.Parse(mapInput[i]);
                    i++;
                    var reward = int.Parse(mapInput[i]);
                    Map.Bonuses.Add(bonusID, new BonusDetails(Map, bonusID, reward));
                }
            }
            else if (mapInput[1] == "regions")
            {
                for (var i = 2; i < mapInput.Length; i++)
                {
                    var terrID = (TerritoryIDType)int.Parse(mapInput[i]);
                    i++;
                    var bonusID = (BonusIDType)int.Parse(mapInput[i]);
                    Map.Territories.Add(terrID, new TerritoryDetails(Map, terrID));
                    Map.Bonuses[bonusID].Territories.Add(terrID);
                    Map.Territories[terrID].PartOfBonuses.Add(bonusID);
                }
            }
            else if (mapInput[1] == "neighbors")
            {
                for (var i = 2; i < mapInput.Length; i++)
                {
                    var terrID = (TerritoryIDType)int.Parse(mapInput[i++]);

                    var terr = Map.Territories[terrID];
                    mapInput[i].Split(',').Select(o => (TerritoryIDType)int.Parse(o)).ForEach(o => terr.ConnectedTo.Add(o, null));
                    foreach (var conn in terr.ConnectedTo.Keys)
                        Map.Territories[conn].ConnectedTo[terrID] = null;

                }

                //Map is now done being read
                DistributionStanding = new GameStanding();
                foreach (var terr in Map.Territories.Values)
                    DistributionStanding.Territories.Add(terr.ID, TerritoryStanding.Create(terr.ID, TerritoryStanding.NeutralPlayerID, new Armies(2)));
            }
            else if (mapInput[1] == "wastelands")
            {
                foreach (var terrID in mapInput.Skip(2).Select(o => (TerritoryIDType)int.Parse(o)).ToList())
                    DistributionStanding.Territories[terrID].NumArmies = new Armies(6);
            }
            else if (mapInput[1] == "opponent_starting_regions")
            {  /* don't care */ }
            else
                throw new Exception("Unexpected map input: " + mapInput[1]);
        }

        private static GameStanding ReadMap(string[] mapInput)
        {
            var ret = new GameStanding(Map.Territories.Values.Select(o => TerritoryStanding.Create(o.ID, TerritoryStanding.FogPlayerID, new Armies(fogged: true))));

            for (var i = 1; i < mapInput.Length; i++)
            {
                var ts = ret.Territories[(TerritoryIDType)int.Parse(mapInput[i++])];
                ts.OwnerPlayerID = (PlayerIDType)ToPlayerID(mapInput[i++]);
                ts.NumArmies = new Armies(int.Parse(mapInput[i]));
            }
            return ret;
        }

        private static void ReadOpponentMoves(string[] moveInput)
        {
            var orders = new List<GameOrder>();

            for (var i = 1; i < moveInput.Length; i++)
            {
                GameOrder order;
                if (moveInput[i + 1] == "place_armies")
                {
                    var playerID = ToPlayerID(moveInput[i]);
                    var terrID = (TerritoryIDType)int.Parse(moveInput[i + 2]);
                    var armies = int.Parse(moveInput[i + 3]);
                    i += 3;

                    var existing = orders.OfType<GameOrderDeploy>().FirstOrDefault(o => o.DeployOn == terrID);
                    if (existing != null)
                    {
                        //Don't allow dupe deploy orders.  Just add it to the existing one
                        Assert.Fatal(existing.PlayerID == playerID);
                        existing.NumArmies += armies;
                        continue;
                    }

                    order = GameOrderDeploy.Create(playerID, armies, terrID, false);
                }
                else
                {
                    if (moveInput[i + 1] == "attack/transfer")
                    {
                        var fromTerrID = (TerritoryIDType)int.Parse(moveInput[i + 2]);
                        var toTerrID = (TerritoryIDType)int.Parse(moveInput[i + 3]);


                        var playerID = ToPlayerID(moveInput[i]);
                        var armies = int.Parse(moveInput[i + 4]);
                        order = GameOrderAttackTransfer.Create(playerID, fromTerrID, toTerrID, AttackTransferEnum.AttackTransfer, false, new Armies(armies), false);
                        i += 4;
                    }
                    else
                        throw new Exception("Unexpected order type");
                }

                orders.Add(order);

            }
            PrevTurn = orders.ToArray();
        }

        public static PlayerIDType ToPlayerID(string str)
        {
            if (str == "neutral")
                return TerritoryStanding.NeutralPlayerID;
            else
                return (PlayerIDType)(int.Parse(str.RemoveFromStartOfString("player")) + 1000);
        }

        public static string ToPlayerString(PlayerIDType playerID)
        {
            Assert.Fatal(playerID != TerritoryStanding.NeutralPlayerID, "Neutral");
            Assert.Fatal(playerID != TerritoryStanding.FogPlayerID, "Fog");
            Assert.Fatal(playerID != TerritoryStanding.AvailableForDistribution, "Dist");
            return "player" + ((int)playerID - 1000);
        }

        static string GetDeployString(GameOrderDeploy deploy)
        {
            return ToPlayerString(deploy.PlayerID) + " place_armies " + deploy.DeployOn + " " + deploy.NumArmies;

        }

        static string GetAttackString(GameOrderAttackTransfer attack)
        {
            return ToPlayerString(attack.PlayerID) + " attack/transfer " + attack.From + " " + attack.To + " " + attack.NumArmies.NumArmies;
        }



        private static void InitBot()
        {
            Bot = BotFactory.Construct("Prod");

            Players = new Dictionary<PlayerIDType, GamePlayer>();
            Players.Add(MyPlayerID, new GamePlayer(MyPlayerID, GamePlayerState.Playing, PlayerInvite.NoTeam, 0, false, false, false));
            Players.Add(OpponentPlayerID, new GamePlayer(OpponentPlayerID, GamePlayerState.Playing, PlayerInvite.NoTeam, 0, false, false, false));


            var incomes = new Dictionary<PlayerIDType, PlayerIncome>();
            incomes.Add(MyPlayerID, new PlayerIncome(StartingArmies));
            incomes.Add(OpponentPlayerID, new PlayerIncome(5));

            var settings = new GameSettings(0.6, 0.7, true, 5, 2, 2, 0, (DistributionIDType)0, new Dictionary<BonusIDType, int>(), false, false, false, 2, RoundingModeEnum.StraightRound, 0.16, false, false, new Dictionary<CardIDType, object>(), false, GameFogLevel.Foggy, false);
            Bot.Init((GameIDType)0, MyPlayerID, Players, Map, DistributionStanding, settings, NumberOfTurns, incomes, PrevTurn, LatestTurnStanding, PreviousTurnStanding, new Dictionary<PlayerIDType, TeammateOrders>(), new List<CardInstance>(), 0, Stopwatch.StartNew(), new List<string>());
        }

        static IWarLightAI Bot;

    }
}
