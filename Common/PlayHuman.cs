using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class PlayHuman
    {
        public static readonly PlayerIDType MeID = (PlayerIDType)633947;


        public static void Create(string botName, string opponent, string gameName)
        {
            AILog.Log("PlayHuman", "Creating game...");
            var gameID = HumanGameAPI.CreateGame(new[] {
                            PlayerInvite.Create("me", (TeamIDType)0, null),
                            PlayerInvite.Create("AI@warlight.net", (TeamIDType)0, null),
                            PlayerInvite.Create(opponent, (TeamIDType)1, null)
                        }, gameName, null, settings =>
                        {
                            settings["Fog"] = "NoFog"; //turn off fog so we can see what the AI is doing
                            settings["MaxCardsHold"] = 999; //so AI doesn't have to discard
                            //settings["OrderPriorityCard"] = "none";
                            //settings["OrderDelayCard"] = "none";
                            //settings["BlockadeCard"] = "none";
                            //settings["NumberOfCardsToReceiveEachTurn"] = 1;
                        });
            AILog.Log("PlayHuman", "Created game " + gameID);
            PlayLoop(botName, gameID, MeID);
        }

        public static void PlayLoop(string botName, GameIDType gameID, PlayerIDType playerID)
        {

            var settings = HumanGameAPI.GetGameSettings(gameID);

            int turnNumber = int.MinValue;
            bool checkedAccept = false;

            while (true)
            {
                var status = HumanGameAPI.GetGameStatus(gameID);
                if (status.Item2 == GameState.WaitingForPlayers)
                {

                    if (!checkedAccept)
                    {
                        var game = HumanGameAPI.GetGameInfo(gameID, null);
                        if (game.Players[playerID].State == GamePlayerState.Invited)
                        {
                            AILog.Log("PlayHuman", "Accepting invite...");
                            HumanGameAPI.AcceptGame(gameID);
                            AILog.Log("PlayHuman", "Accepted invite");
                        }

                        checkedAccept = true;
                    }
                }
                else if (status.Item2 == GameState.Finished)
                {
                    AILog.Log("PlayHuman", "Game finished");
                    break;
                }
                else if (status.Item1 > turnNumber)
                {
                    var game = HumanGameAPI.GetGameInfo(gameID, null);

                    if (!EntryPoint.PlayGame(botName, game, MeID, settings.Item1, settings.Item2, picks =>
                    {
                        HumanGameAPI.SendPicks(game.ID, picks);
                        AILog.Log("PlayHuman", "Sent picks");
                    }, orders =>
                    {
                        HumanGameAPI.SendOrders(game.ID, orders, game.NumberOfTurns + 1);
                        AILog.Log("PlayHuman", "Sent orders");
                    }))
                    {
                        AILog.Log("PlayHuman", "We're no longer alive");
                        break;
                    }
                    turnNumber = status.Item1;
                }

                Thread.Sleep(10000);
            }

        }


        public static void PlayForTurn(string botName, GameIDType gameID, int playForTurn)
        {
            AILog.Log("PlayHuman", "Generating orders for game " + gameID + " turn " + playForTurn);
            var settings = HumanGameAPI.GetGameSettings(gameID);
            var game = HumanGameAPI.GetGameInfo(gameID, playForTurn);

            EntryPoint.PlayGame(botName, game, MeID, settings.Item1, settings.Item2, picks => { }, orders => { });
        }

    }
}
