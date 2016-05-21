using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                Go(args);

                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
            }
            else
            {
                try
                {
                    Go(args);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    Environment.ExitCode = 11;
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine(

@"Usage: 

To start a new game against a human:
    WarLight.AI Create <bot name> <opponent email or profile token> <game name>

To play an existing human game: 
    WarLight.AI Play <bot name> <game ID>

To simulate creating orders for an existing human game at an earlier turn:  (turn number = 0 for picks)
    WarLight.AI Simulate <bot name> <game ID> <turn number>

To play a game against enemy AIs:
    WarLight.AI PlayAI <bot name> [number of opponent AIs]
    WarLight.AI PlayFFA <bot name>

To play bots against each other:
    WarLight.AI PlayBots <bot name> <bot name> [<bot name>...]

To simulate a turn from a bot game:
    WarLight.AI PlayExported <bot name> <folder> <game ID> <player ID> <turn number>

Supported bot names: " + BotFactory.Names.JoinStrings(", "));

            Environment.ExitCode = 5;

        }

        private static void Go(string[] args)
        {
            //AIGamesParser.Go(args); return; //uncomment to make this AI compatible with theaigames.com format


            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            switch (args[0].ToLower())
            {
                case "create":
                    PlayHuman.Create(args[1], args[2], args[3]);
                    break;
                case "play":
                    PlayHuman.PlayLoop(args[1], (GameIDType)int.Parse(args[2]), PlayHuman.MeID);
                    break;
                case "simulate":
                    PlayHuman.PlayForTurn(args[1], (GameIDType)int.Parse(args[2]), int.Parse(args[3]) - 1);
                    break;
                case "playai":
                    PlayAI.Go(args.Skip(1).ToArray());
                    break;
                case "playffa":
                    PlayFFA.Go(args.Skip(1).ToArray());
                    break;
                case "playexported":
                    PlayExported.Go(args.Skip(1).ToArray());
                    break;
                case "playbots":
                    PlayBots.Go(args.Skip(1).ToArray());
                    break;
                case "compete":
                    Compete.Go(args[1], int.Parse(args[2]));
                    break;
                case "playcrazy":
                    PlayCrazy.Go(args.Skip(1).ToArray());
                    break;
                case "stresstest":
                    StressTest.Go(args.Skip(1).ToArray());
                    break;
                default:
                    PrintHelp();
                    break;
            }
        }

        public static bool PlayGame(string botName, GameObject game, PlayerIDType playerID, GameSettings settings, MapDetails map, Action<List<TerritoryIDType>> sendPicks, Action<List<GameOrder>> sendOrders)
        {

            if (game.State == GameState.WaitingForPlayers)
                return true;

            if (!game.Players.ContainsKey(playerID))
                return false; //not in game
            if (game.Players[playerID].State == GamePlayerState.Invited)
                throw new NotImplementedException("TODO: Accept the invite");
            if (game.Players[playerID].State != GamePlayerState.Playing)
                return false; //not alive anymore

            var bot = BotFactory.Construct(botName);
            bot.Init(game.ID, playerID, game.Players, map, game.LatestInfo.DistributionStanding, settings, game.NumberOfTurns, game.LatestInfo.Income, game.LatestInfo.LatestTurn == null ? null : game.LatestInfo.LatestTurn.Orders, game.LatestInfo.LatestStanding, game.LatestInfo.PreviousTurnStanding, game.LatestInfo.TeammatesOrders, game.LatestInfo.Cards, game.LatestInfo.CardsMustUse, Stopwatch.StartNew(), new List<string>());

            AILog.Log("PlayGame", "State=" + game.State + ", numTurns=" + game.NumberOfTurns + ", income=" + game.LatestInfo.Income[playerID] + ", cardsMustUse=" + game.LatestInfo.CardsMustUse);

            if (game.State == GameState.DistributingTerritories)
                sendPicks(_speeds.GetOrAdd(botName, _ => new Speeds()).Record(true, () => bot.GetPicks()));
            else if (game.State == GameState.Playing)
                sendOrders(_speeds.GetOrAdd(botName, _ => new Speeds()).Record(false, () => bot.GetOrders()));

            return true;
        }


        #region Speeds

        class Speeds
        {
            public int TotalPicks = 0;
            public int TotalOrders = 0;
            public TimeSpan ElapsedPicking = TimeSpan.Zero;
            public TimeSpan ElapsedOrdering = TimeSpan.Zero;

            public T Record<T>(bool picking, Func<T> go)
            {
                var sw = Stopwatch.StartNew();

                var ret = go();

                var elapsed = sw.Elapsed;
                lock (this)
                {
                    if (picking)
                    {
                        TotalPicks++;
                        ElapsedPicking += elapsed;
                    }
                    else
                    {
                        TotalOrders++;
                        ElapsedOrdering += elapsed;
                    }
                }
                return ret;
            }

            public override string ToString()
            {
                lock (this)
                {
                    var avgPicking = TotalPicks == 0 ? 0 : ElapsedPicking.TotalMilliseconds / TotalPicks;
                    var avgOrdering = TotalOrders == 0 ? 0 : ElapsedOrdering.TotalMilliseconds / TotalOrders;
                    return avgPicking.ToString("0") + "ms picking, " + avgOrdering.ToString("0") + "ms making orders (" + TotalPicks + "/" + TotalOrders + ")";
                }
            }
        }

        static ConcurrentDictionary<string, Speeds> _speeds = new ConcurrentDictionary<string, Speeds>();


        public static void LogSpeeds()
        {
            AILog.Log("Speeds", _speeds.Select(o => o.Key + " " + o.Value).JoinStrings(", "));
        }

        #endregion

    }
}
