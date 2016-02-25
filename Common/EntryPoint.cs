using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarLight.AI
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            if (Debugger.IsAttached)
                Go(args);
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
    WarLight.AI PlayAI <bot name>
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
            bot.Init(playerID, game.Players, map, game.LatestInfo.DistributionStanding, settings, game.NumberOfTurns, game.LatestInfo.Income, game.LatestInfo.LatestTurn == null ? null : game.LatestInfo.LatestTurn.Orders, game.LatestInfo.LatestStanding, game.LatestInfo.PreviousTurnStanding, game.LatestInfo.TeammatesOrders, game.LatestInfo.Cards, game.LatestInfo.CardsMustUse);

            AILog.Log("PlayGame. State=" + game.State + ", numTurns=" + game.NumberOfTurns + ", cardsMustUse=" + game.LatestInfo.CardsMustUse);

            if (game.State == GameState.DistributingTerritories)
                sendPicks(bot.GetPicks());
            else if (game.State == GameState.Playing)
                sendOrders(bot.GetOrders());

            return true;
        }
    }
}
