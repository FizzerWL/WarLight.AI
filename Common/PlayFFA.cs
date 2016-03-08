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
    public static class PlayFFA
    {
        public static void Go(string[] args)
        {
            //while(true)
                PlayGame(args);
        }

        private static void PlayGame(string[] args)
        {
            var botName = args[0];

            AILog.Log("PlayFFA", "Creating game...");
            var gameID = BotGameAPI.CreateGame(Enumerable.Range(10, 6).Select(o => PlayerInvite.Create((PlayerIDType)o, PlayerInvite.NoTeam, null)), "PlayFFA", null, gameSettings =>
            //var gameID = BotGameAPI.CreateGame(Enumerable.Range(10, 6).Select(o => PlayerInvite.Create((PlayerIDType)o, (TeamIDType)(o == 0 ? 0 : 1), (SlotType)o)), "PlayFFA", 17, gameSettings =>
            {
                //gameSettings["DistributionMode"] = 0; //full distribution, so we have plenty of starting territories
                //gameSettings["InitialNeutralsInDistribution"] = 2; //since we're full distribution, most territories an in-distribution one so we want them to start with 2s not 4s
                gameSettings["MaxCardsHold"] = 999;
                //gameSettings["AutomaticTerritoryDistribution"] = "Automatic";
            });

            AILog.Log("PlayFFA", "Created game " + gameID);

            var settings = BotGameAPI.GetGameSettings(gameID);

            try
            {

                while (true)
                {
                    var game = BotGameAPI.GetGameInfo(gameID, null);
                    if (game.State == GameState.Finished)
                    {
                        AILog.Log("PlayFFA", "Game finished: " + gameID);
                        break;
                    }

                    var players = game.Players.Values.Where(o => o.State == GamePlayerState.Playing).ToList();

                    Action<GamePlayer> play = player =>
                    {
                        var pg = BotGameAPI.GetGameInfo(gameID, player.ID);

                        EntryPoint.PlayGame(botName, pg, player.ID, settings.Item1, settings.Item2, picks => BotGameAPI.SendPicks(pg.ID, player.ID, picks), orders => BotGameAPI.SendOrders(pg.ID, player.ID, orders, pg.NumberOfTurns + 1));
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
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PlayFFA");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, gameID + ".txt"), export);

        }
    }
}
