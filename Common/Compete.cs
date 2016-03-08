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
    /// <summary>
    /// Play the passed bot against all other bots, and keep track of how we do against them.
    /// </summary>
    public static class Compete
    {
        const string AIBotIdentifier = "AI";
        static ConcurrentDictionary<string, Stats> _botStats = new ConcurrentDictionary<string, Stats>();

        public static void Go(string botName, int numThreads)
        {
            var opponents = BotFactory.Names.Where(o => o.Equals(botName, StringComparison.OrdinalIgnoreCase) == false).ToList();
            opponents.Add(AIBotIdentifier);
            Console.WriteLine(botName + " opponents: " + opponents.JoinStrings(", "));


            if (numThreads == 1)
            {
                int gameNum = 0;
                while (true)
                    foreach (var opp in opponents.OrderByRandom())
                        PlayGame(botName, opp, 0, gameNum++);
            }
            else
            {

                AILog.DoLog = l => l == "Speeds";

                var threads = Enumerable.Range(0, numThreads).Select(threadNum => new Thread(() =>
                {
                    Thread.Sleep(100 * threadNum); //stagger them
                    int gameNum = 0;
                    while (true)
                        try
                        {

                            foreach (var opp in opponents.OrderByRandom())
                                PlayGame(botName, opp, threadNum, gameNum++);
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

        class Stats
        {
            public int Wins;
            public int Losses;
            public int TotalGames { get { return Wins + Losses; } }
            public double Percent { get { return (double)Wins / (double)TotalGames; } }

            public override string ToString()
            {
                return Wins + "/" + TotalGames + " " + Percent.ToString("#0.0%");
            }

            public void Record(bool win)
            {
                if (win)
                    Interlocked.Increment(ref Wins);
                else
                    Interlocked.Increment(ref Losses);
            }
        }

        static void PlayGame(string botName, string opponent, int threadNum, int gameNum)
        {
            var usID = (PlayerIDType)10;
            var oppID = (PlayerIDType)11;
            
            var invite = new List<PlayerInvite>();
            invite.Add(PlayerInvite.Create(usID, PlayerInvite.NoTeam, null));

            if (opponent == AIBotIdentifier)
                invite.Add(PlayerInvite.Create("AI@warlight.net", PlayerInvite.NoTeam, null));
            else
                invite.Add(PlayerInvite.Create(oppID, PlayerInvite.NoTeam, null));

            AILog.Log("Compete", "Creating game...");
            var gameID = BotGameAPI.CreateGame(invite, "Compete", null, gameSettings =>
            {
                gameSettings["MaxCardsHold"] = 999;
                gameSettings["ReinforcementCard"] = "none";
            });

            AILog.Log("Compete", "Created game " + gameID);

            var settings = BotGameAPI.GetGameSettings(gameID);
            var game = BotGameAPI.GetGameInfo(gameID, null);
            bool? won = null;

            try
            {

                while (true)
                {
                    game = BotGameAPI.GetGameInfo(gameID, null);

                    if (game.State == GameState.Finished)
                    {
                        won = game.Players.Values.Single(o => o.State == GamePlayerState.Won).ID == usID;
                        _botStats.GetOrAdd(opponent, _ => new Stats()).Record(won.Value);
                        Console.WriteLine("T" + threadNum.ToString("00") + " G" + gameNum.ToString("00") + ": " + (won.Value ? "Won " : "Lost") + " game vs " + opponent + " " + gameID + " finished. Totals: " + _botStats.OrderBy(o => o.Key).Select(o => o.Key + "=" + o.Value).JoinStrings(", "));
                        break;
                    }

                    //Play ourselves
                    var pg = BotGameAPI.GetGameInfo(gameID, usID);
                    EntryPoint.PlayGame(botName, pg, usID, settings.Item1, settings.Item2, picks => BotGameAPI.SendPicks(pg.ID, usID, picks), orders => BotGameAPI.SendOrders(pg.ID, usID, orders, pg.NumberOfTurns + 1));


                    //Play opponent
                    if (opponent != AIBotIdentifier)
                    {
                        pg = BotGameAPI.GetGameInfo(gameID, oppID);
                        EntryPoint.PlayGame(opponent, pg, oppID, settings.Item1, settings.Item2, picks => BotGameAPI.SendPicks(pg.ID, oppID, picks), orders => BotGameAPI.SendOrders(pg.ID, oppID, orders, pg.NumberOfTurns + 1));
                    }

                    Thread.Sleep(100);
                }
            }
            finally
            {
                ExportGame(gameID, opponent, won);
                BotGameAPI.DeleteGame(gameID);
            }

        }


        /// <summary>
        /// Save it off in case we want to look at it later.  To look at it, go to https://www.warlight.net/Play, press Ctrl+Shift+E then click Import
        /// </summary>
        private static void ExportGame(GameIDType gameID, string oppBot, bool? won)
        {
            var export = BotGameAPI.ExportGame(gameID);
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Compete");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, gameID + "_" + oppBot + (!won.HasValue ? "" : won.Value ? "_win" : "_loss") + ".txt"), export);

        }
    }
}
