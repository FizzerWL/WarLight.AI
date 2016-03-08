using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class PlayExported
    {
        public static void Go(string[] args)
        {
            var botName = args[0];
            var folder = args[1];
            var gameID = (GameIDType)int.Parse(args[2]);
            var playerID = (PlayerIDType)int.Parse(args[3]);
            var turnNumber = args[4].ToLower() == "latest" ? (int?)null : int.Parse(args[4]) - 1;

            var details = BotGameAPI.GetBotExportedGame(gameID, ReadExported(folder, gameID), playerID, turnNumber);

            EntryPoint.PlayGame(botName, details.Item3, playerID, details.Item1, details.Item2, picks => { }, orders => { });
        }

        public static string ReadExported(string folder, GameIDType gameID)
        {
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), folder);
            return File.ReadAllText(Path.Combine(dir, Directory.GetFiles(dir, gameID + "*.txt").Single()));
        }
    }
}
