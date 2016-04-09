using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class StressTest
    {
        static readonly PlayerIDType MeID = (PlayerIDType)10;
        
        public static void Go(string[] args)
        {
            var botName = args[0];
            var numThreads = args.Length > 1 ? int.Parse(args[1]) : 1;
            AILog.DoLog = log => log == "Speeds";

            if (numThreads == 1)
            {
                while (true)
                {
                    PlayGame(botName);
                    EntryPoint.LogSpeeds();
                }
            }
            else
            {

                var threads = Enumerable.Range(0, numThreads).Select(o => new Thread(() =>
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
                    EntryPoint.LogSpeeds();
                }
            }
        }
        
        public static void PlayGame(string botName)
        {
            var players = new[] {
                PlayerInvite.Create(MeID, PlayerInvite.NoTeam, null),
                PlayerInvite.Create("AI@warlight.net", PlayerInvite.NoTeam, null)
            };

            AILog.Log("StressTest", "Creating game...");
            var gameID = BotGameAPI.CreateGame(players, "AI Competition", null, gameSettings =>
            {
                gameSettings["MaxCardsHold"] = 999;

                gameSettings["Map"] = 52545; //53822; //hex earth, 3200 territories
                //gameSettings["Map"] = 24591; //big USA, 3066 territories
                //gameSettings["Map"] = 42717; //thirty years war, 2264 territories
                //gameSettings["Map"] = 34083; //Africa big, 1544 territories
                gameSettings["DistributionMode"] = 0; //full dist
                gameSettings["TerritoryLimit"] = 0; //terr limit
                gameSettings["MultiAttack"] = true; gameSettings["AllowPercentageAttacks"] = true;
                //gameSettings["AutomaticTerritoryDistribution"] = "Automatic"; //skip picking, if you're only looking to optimize orders
            });


            var settings = BotGameAPI.GetGameSettings(gameID);

            try
            {
                while (true)
                {
                    var game = BotGameAPI.GetGameInfo(gameID, MeID);
                    if (game.State == GameState.Finished || game.NumberOfTurns >= 2)
                        break;

                    if (!EntryPoint.PlayGame(botName, game, MeID, settings.Item1, settings.Item2, picks => BotGameAPI.SendPicks(game.ID, MeID, picks), orders => BotGameAPI.SendOrders(game.ID, MeID, orders, game.NumberOfTurns + 1)))
                        break;

                    Thread.Sleep(100);
                }
            }
            finally
            {
                ExportGame(gameID);
                BotGameAPI.DeleteGame(gameID);
            }
        }

        /// <summary>
        /// Save it off in case we want to look at it later.  To look at it, go to https://www.warlight.net/Play, press Ctrl+Shift+E then click Import
        /// </summary>
        private static void ExportGame(GameIDType gameID)
        {
            var export = BotGameAPI.ExportGame(gameID);
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "StressTest");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, gameID + ".txt"), export);

        }
    }
}
