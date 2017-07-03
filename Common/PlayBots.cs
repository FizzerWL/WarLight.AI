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
using Newtonsoft.Json.Linq;

namespace WarLight.Shared.AI
{
    public static class PlayBots
    {
        public static void Go(string[] args)
        {
            var bots = args.Where(o => o.Contains('=') == false).ToList();

            Func<string, string, string> getArg = (argName, def) => args.None(o => o.ToLower().StartsWith(argName.ToLower() + "=")) ? def : args.Single(o => o.ToLower().StartsWith(argName.ToLower() + "=")).ToLower().RemoveFromStartOfString(argName + "=");

            //Pass Parallel=true as an argument to make each individual game execute all of the bots in paralell (may cause issues with team games and cards)
            bool parallel = bool.Parse(getArg("parallel", "false"));

            //Pass NumThreads=## as an argument to use multiple threads
            int numThreads = int.Parse(getArg("threads", "1"));

            if (numThreads == 1)
            {
                while (true)
                    PlayGame(bots, parallel);
            }
            else
            {
                AILog.DoLog = l => false;

                var threads = Enumerable.Range(0, numThreads).Select(o => new Thread(() =>
                {
                    Thread.Sleep(100 * o); //stagger them
                    try
                    {
                        while (true)
                            PlayGame(bots, parallel);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Thread failed: " + ex);
                    }

                })).ToList();

                threads.ForEach(o => o.Start());
                Thread.Sleep(int.MaxValue);
            }
        }

        static ConcurrentDictionary<string, int> _totals = new ConcurrentDictionary<string, int>();

        private static void PlayGame(List<string> bots, bool parallel)
        {
            AILog.Log("PlayBots", "Creating game...");
            var templateID = 1;
            var gameID = BotGameAPI.CreateGame(Enumerable.Range(10, bots.Count).Select(o => PlayerInvite.Create((PlayerIDType)o, PlayerInvite.NoTeam, null)), "PlayBots", templateID, gameSettings =>
            {
                gameSettings["MaxCardsHold"] = 999;
                gameSettings["ReinforcementCard"] = "none";
                //gameSettings["Fog"] = GameFogLevel.NoFog.ToString();
                //gameSettings["Fog"] = GameFogLevel.ModerateFog.ToString();
                //gameSettings["Fog"] = GameFogLevel.ExtremeFog.ToString();
                //gameSettings["OneArmyStandsGuard"] = false;
                //ZeroAllBonuses(gameSettings);
                //gameSettings["Map"] = 16114; //Rise of Rome -- use to test how bots respond to super bonuses
                //gameSettings["Map"] = 24591; //big USA, 3066 territories
                //gameSettings["MultiAttack"] = true; 
                //gameSettings["AllowPercentageAttacks"] = false;
                //gameSettings["AllowAttackOnly"] = false;
                //gameSettings["AllowTransferOnly"] = false;

                //var wastelands = new JObject();
                //wastelands["NumberOfWastelands"] = 0;
                //wastelands["WastelandSize"] = 10;
                //gameSettings["Wastelands"] = wastelands;
                //gameSettings["BombCard"] = new JObject(new JProperty("InitialPieces", 0), new JProperty("MinimumPiecesPerTurn", 1), new JProperty("NumPieces", 4), new JProperty("Weight", 1));
                //gameSettings["SanctionsCard"] = new JObject(new JProperty("InitialPieces", 0), new JProperty("MinimumPiecesPerTurn", 1), new JProperty("NumPieces", 4), new JProperty("Weight", 1), new JProperty("Duration", 1), new JProperty("Percentage", 0.5));
                //gameSettings["BlockadeCard"] = new JObject(new JProperty("InitialPieces", 50), new JProperty("MinimumPiecesPerTurn", 1), new JProperty("NumPieces", 1), new JProperty("Weight", 1), new JProperty("MultiplyAmount", 10));
                //gameSettings["DiplomacyCard"] = new JObject(new JProperty("InitialPieces", 0), new JProperty("MinimumPiecesPerTurn", 1), new JProperty("NumPieces", 1), new JProperty("Weight", 1), new JProperty("Duration", 1));
                //gameSettings["NumberOfCardsToReceiveEachTurn"] = 4;
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

                    if (parallel) //note: Parallel won't work when teammates, cards, and limited holding cards are involved.
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

        /// <summary>
        /// Sets all bonuses to 0.  Used to verify that bots can still expand even when there are no bonuses
        /// </summary>
        /// <param name="gameSettings"></param>
        private static void ZeroAllBonuses(JObject gameSettings)
        {
            gameSettings["OverriddenBonuses"] = new JArray(Enumerable.Range(1, 23).Select(o => new JObject(new JProperty("bonusID", o), new JProperty("value", 0)))); //Assumes MME map
            gameSettings["DistributionMode"] = 2; //warlords dist.  We can't use random warlords since that gives one territory per bonus, and there are no bonuses
            gameSettings["BonusArmyPer"] = 1; //extra armies, otherwise games tend to stalemate
        }
    }
}
