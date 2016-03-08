using System;
using System.Collections.Concurrent;
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
    public static class PlayBots
    {
        public static void Go(string[] args)
        {
            //play one with full log to ensure all bots are functional, then suppress log and play games as fast as possible.
            PlayGame(args);

            AILog.DoLog = l => false;

            var threads = Enumerable.Range(0, 3).Select(o => new Thread(() =>
            {
                try
                {
                    while (true)
                        PlayGame(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Thread failed: " + ex);
                }

            })).ToList();

            threads.ForEach(o => o.Start());
            Thread.Sleep(int.MaxValue);
        }

        static ConcurrentDictionary<string, int> _totals = new ConcurrentDictionary<string, int>();

        private static void PlayGame(string[] args)
        {
            var bots = args;

            AILog.Log("PlayBots", "Creating game...");
            var gameID = BotGameAPI.CreateGame(Enumerable.Range(10, bots.Length).Select(o => PlayerInvite.Create((PlayerIDType)o, PlayerInvite.NoTeam, null)), "PlayBots", null, gameSettings =>
            {
                gameSettings["MaxCardsHold"] = 999;
                gameSettings["ReinforcementCard"] = "none";
                gameSettings["Fog"] = "NoFog";
            });

            AILog.Log("PlayBots", "Created game " + gameID);

            var settings = BotGameAPI.GetGameSettings(gameID);
            var game = BotGameAPI.GetGameInfo(gameID, null);

            var botsDict = game.Players.Values.Zip(bots, (gp, bot) => new { Player = gp, Bot = bot }).ToDictionary(o => o.Player.ID, o => o.Bot);

            try
            {

                while (true)
                {
                    game = BotGameAPI.GetGameInfo(gameID, null);
                    if (game.State == GameState.Finished)
                    {
                        var winnerStr = game.Players.Values.Where(o => o.State == GamePlayerState.Won).Select(o => botsDict[o.ID]).JoinStrings(",");
                        _totals.AddOrUpdate(winnerStr, 1, (_, i) => i + 1);
                        Console.WriteLine("Game " + gameID + " finished.  Winner=" + winnerStr + ", totals: " + _totals.OrderByDescending(o => o.Value).Select(o => o.Key + "=" + o.Value).JoinStrings(", "));

                        
                        break;
                    }

                    var players = game.Players.Values.Where(o => o.State == GamePlayerState.Playing).ToList();

                    Action<GamePlayer> play = player =>
                    {
                        var pg = BotGameAPI.GetGameInfo(gameID, player.ID);

                        EntryPoint.PlayGame(botsDict[player.ID], pg, player.ID, settings.Item1, settings.Item2, picks => BotGameAPI.SendPicks(pg.ID, player.ID, picks), orders => BotGameAPI.SendOrders(pg.ID, player.ID, orders, pg.NumberOfTurns + 1));
                    };

                    if (args.Any(o => o.ToLower() == "parallel")) //note: Parallel won't work when teammates, cards, and limited holding cards are involved.
                        players.AsParallel().ForAll(play);
                    else
                        players.ForEach(play);

                    Thread.Sleep(100);
                }
            }
            finally
            {
                GameFinished(gameID);
            }
        }

        private static void GameFinished(GameIDType gameID)
        {
            ExportGame(gameID);
            BotGameAPI.DeleteGame(gameID);
        }

        /// <summary>
        /// Save it off in case we want to look at it later.  To look at it, go to https://www.warlight.net/Play, press Ctrl+Shift+E then click Import
        /// </summary>
        private static void ExportGame(GameIDType gameID)
        {
            var export = BotGameAPI.ExportGame(gameID);
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PlayBots");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, gameID + ".txt"), export);

        }
    }
}
