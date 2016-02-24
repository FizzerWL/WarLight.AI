/*
* This code was auto-converted from a java project.
*/

using System.Collections.Generic;
using WarLight.AI.Wunderwaffe.Bot;


using WarLight.AI.Wunderwaffe.Strategy;
using System;
using System.Text;

namespace WarLight.AI.Wunderwaffe.Debug
{
    public class Debug
    {

        
        public static void PrintDebugOutputBeginTurn(BotMain state)
        {
            AILog.Log("========================= NumTurns=" + state.NumberOfTurns + " ==========================");
        }

        public static void PrintDebugOutput(BotMain state)
        {
            foreach(var opp in state.Opponents)
                PrintOpponentBonuses(opp.ID, state);
        }

        // printDistances();
        // System.err.println();
        // printBonusValues();
        // System.err.println();
        // System.err.println("StartingPicksAmount: " + BotState.StartingPicksAmount);
        // System.err.println("Known opponent spots: ");
        // for (Territory territory : BotState.VisibleMap.OpponentTerritories) {
        // System.err.print(territory.ID + ", ");
        // }
        // System.err.println();
        private static void PrintDistances(BotMain state)
        {
            AILog.Log("Territory distances:");
            foreach (var territory in state.VisibleMap.GetOwnedTerritories())
            {
                var message = territory.ID + " --> " + territory.DirectDistanceToOpponentBorder + " | " + territory.DistanceToUnimportantSpot + " | " + territory.DistanceToImportantSpot  + " | " + territory.DistanceToHighlyImportantSpot + " | " + territory.DistanceToOpponentBorder + " | " + territory.DistanceToImportantOpponentBorder + " || " + TransferMovesChooser.GetAdjustedDistance(territory);

                AILog.Log(message);
            }
        }

        private static void PrintAllTerritories(BotMain state)
        {
            AILog.Log("Territories:");
            foreach (var territory in state.VisibleMap.Territories.Values)
            {
                var id = territory.ID;
                var player = territory.OwnerPlayerID;
                var armies = territory.Armies;
                var ownershipHeuristic = territory.IsOwnershipHeuristic;
                var deployment = territory.GetTotalDeployment(BotTerritory.DeploymentType.Normal);
                AILog.Log(" - Territory " + id + " (" + player + " | " + armies + " | " + ownershipHeuristic + " | " + deployment + ")");
            }
        }

        private static void PrintOpponentBonuses(PlayerIDType opponentID, BotMain state)
        {
            var message = new StringBuilder();
            message.Append("Opponent " + opponentID + " owns Bonuses: ");
            foreach (var bonus in state.VisibleMap.Bonuses.Values)
            {
                if (bonus.IsOwnedByOpponent(opponentID))
                    message.Append(bonus.ID + ", ");
            }
            AILog.Log(message.ToString());
        }
    }
}
