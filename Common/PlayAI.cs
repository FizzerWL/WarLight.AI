using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarLight.AI
{
    public static class PlayAI
    {
        static int NumWins;
        static int NumLosses;
        static readonly PlayerIDType MeID = (PlayerIDType)10;
        
        public static void Go(string[] args)
        {
            var botName = args[0];

            //play one full game with the log printing to ensure everything works, then suppress the log and just play games multi-threaded as fast as possible.
            PlayGame(botName); 

            AILog.SuppressLog = true;

            var threads = Enumerable.Range(0, 3).Select(o => new Thread(() =>
            {
                try
                {
                    while (true)
                        PlayGame(botName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Thread failed: " + ex);
                }

            })).ToList();

            threads.ForEach(o => o.Start());

            while (true)
            {
                Thread.Sleep(30000);
                Console.WriteLine(DateTime.Now + ": Wins: " + NumWins + ", Losses=" + NumLosses);
            }
        }

        private static void PlayGame(string botName)
        {
            AILog.Log("Creating game...");
            var gameID = BotGameAPI.CreateGame(new[] {
                    PlayerInvite.Create(MeID, (TeamIDType)1, null),
                    PlayerInvite.Create("AI@warlight.net", (TeamIDType)2, null),
                    PlayerInvite.Create("AI@warlight.net", (TeamIDType)2, null),
                }, "AI Competition");

            AILog.Log("Created game " + gameID);

            var settings = BotGameAPI.GetGameSettings(gameID);

            while (true)
            {
                var game = BotGameAPI.GetGameInfo(gameID, MeID);
                if (game.State == GameState.Finished)
                {
                    GameFinished(game);
                    break;
                }

                if (!EntryPoint.PlayGame(botName, game, MeID, settings.Item1, settings.Item2, picks => BotGameAPI.SendPicks(game.ID, MeID, picks), orders => BotGameAPI.SendOrders(game.ID, MeID, orders, game.NumberOfTurns + 1)))
                {
                    GameFinished(game);
                    break;
                }

                Thread.Sleep(100);
            }
        }

        private static void GameFinished(GameObject game)
        {
            var winners = game.Players.Values.Where(o => o.State == GamePlayerState.Won).ToList();
            var weWon = winners.Count > 0 && winners.Any(o => o.ID == MeID);
            Console.WriteLine(DateTime.Now + ": Game " + game.ID + " finished.  Won=" + weWon);

            ExportGame(game.ID);

            BotGameAPI.DeleteGame(game.ID);

            if (weWon)
                Interlocked.Increment(ref NumWins);
            else
                Interlocked.Increment(ref NumLosses);
        }

        /// <summary>
        /// Save it off in case we want to look at it later.  To look at it, go to https://www.warlight.net/Play, press Ctrl+Shift+E then click Import
        /// </summary>
        private static void ExportGame(GameIDType gameID)
        {
            var export = BotGameAPI.ExportGame(gameID);
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PlayAI");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, gameID + ".txt"), export);

        }
    }
}
