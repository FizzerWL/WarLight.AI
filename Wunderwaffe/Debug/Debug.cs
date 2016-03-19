using System.Collections.Generic;
using System.Linq;
using System.Text;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Debug
{
    public class Debug
    {


        public static void PrintDebugOutputBeginTurn(BotMain state)
        {
            int roundNumber = state.NumberOfTurns + 1;
            AILog.Log("Debug", "========================= NumTurns=" + roundNumber + " ==========================");
        }

        public static void PrintDebugOutput(BotMain state)
        {
            foreach (var opp in state.Opponents)
                PrintOpponentBonuses(opp.ID, state);
        }


        private static void PrintDistances(BotMain state)
        {
            //AILog.Log("Debug", "Territory distances:");
            //foreach (var territory in state.VisibleMap.GetOwnedTerritories())
            //{
            //    var message = territory.ID + " --> " + territory.DirectDistanceToOpponentBorder + " | " + territory.DistanceToUnimportantSpot + " | " + territory.DistanceToImportantSpot + " | " + territory.DistanceToHighlyImportantSpot + " | " + territory.DistanceToOpponentBorder + " | " + territory.DistanceToImportantOpponentBorder + " || " + TransferMovesChooser.GetAdjustedDistance(territory);

            //    AILog.Log("Debug", message);
            //}
        }



        public static void PrintTerritories(BotMap map, BotMain BotState)
        {
            List<BotTerritory> territories = map.Territories.Values.ToList();
            //List<BotTerritory> opponentTerritories = territories.Where(o => o.OwnerPlayerID == BotState.Opponents.First().ID).ToList();
            AILog.Log("Debug", "Territories:");
            foreach (BotTerritory territory in territories)
            {
                string player = "fog";
                if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                {
                    player = "neutral";
                }
                else if (territory.OwnerPlayerID == BotState.Me.ID)
                {
                    player = "Me";
                }
                else if (territory.OwnerPlayerID == BotState.Opponents.First().ID)
                {
                    player = "opponent";
                }

                AILog.Log("Debug", territory.Details.Name + ": (" + player + " | " + territory.IsOwnershipHeuristic + ")  --> " + territory.Armies.AttackPower);
            }
        }

        public static void PrintGuessedDeployment(BotMap map, BotMain BotState)
        {
            //AILog.Log("Debug", "Guessed deployment:");
            //foreach (BotTerritory territory in map.Territories.Values)
            //{
            //    if (territory.IsVisible && BotState.IsOpponent(territory.OwnerPlayerID))
            //    {

            //        AILog.Log("Debug", territory.Details.Name + ": " + territory.GetTotalDeployment(BotTerritory.DeploymentType.Normal) + "  |  " + territory.GetTotalDeployment(BotTerritory.DeploymentType.Conservative));
            //    }
            //}
        }


        public static void printExpandBonusValues(BotMap map, BotMain BotState)
        {
            //AILog.Log("Debug", "Bonus expansion values:");
            //foreach (BotBonus bonus in map.Bonuses.Values)
            //{
            //    if (bonus.GetOwnedTerritoriesAndNeighbors().Count > 0 && !bonus.IsOwnedByMyself())
            //    {
            //        AILog.Log("Debug", bonus.Details.Name + ": " + bonus.GetExpansionValue());
            //    }
            //}
        }

        public static void PrintTerritoryValues(BotMap map, BotMain BotState)
        {
            //AILog.Log("Debug", "Territory attack values:");
            //foreach (BotTerritory territory in map.Territories.Values)
            //{
            //    if (territory.IsVisible && BotState.IsOpponent(territory.OwnerPlayerID))
            //    {
            //        AILog.Log("Debug", territory.Details.Name + ": " + territory.AttackTerritoryValue);
            //    }
            //}

            //AILog.Log("Debug", "Territory expansion values:");
            //foreach (BotTerritory territory in map.Territories.Values)
            //{
            //    if (territory.IsVisible && territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
            //    {
            //        AILog.Log("Debug", territory.Details.Name + ": " + territory.ExpansionTerritoryValue);
            //    }
            //}

            //AILog.Log("Debug", "Territory defend values:");
            //foreach (BotTerritory territory in map.Territories.Values)
            //{
            //    if (territory.OwnerPlayerID == BotState.Me.ID && territory.GetOpponentNeighbors().Count > 0)
            //    {
            //        AILog.Log("Debug", territory.Details.Name + ": " + territory.DefenceTerritoryValue);
            //    }
            //}

        }


        public static void PrintMoves(BotMain state, Moves moves)
        {

            //for (int i = 0; i < moves.Orders.Count; i++)
            //{
            //    var order = moves.Orders[i];
            //    if (order is BotOrderAttackTransfer)
            //    {
            //        BotOrderAttackTransfer atm = (BotOrderAttackTransfer)order;
            //        AILog.Log("Debug", atm.ToString());
            //    }
            //}
        }

        public static void PrintAllTerritories(BotMain state, BotMap map)
        {
            AILog.Log("Debug", "Territories:");
            foreach (var territory in map.Territories.Values)
            {
                var id = territory.ID;
                var player = territory.OwnerPlayerID;
                var armies = territory.Armies;
                var ownershipHeuristic = territory.IsOwnershipHeuristic;
                var deployment = territory.GetTotalDeployment(BotTerritory.DeploymentType.Normal);
                AILog.Log("Debug", " - Territory " + id + " (" + player + " | " + armies + " | " + ownershipHeuristic + " | " + deployment + ")");
            }
        }

        private static void PrintOpponentBonuses(PlayerIDType opponentID, BotMain state)
        {
            var message = new StringBuilder();
            message.Append("Opponent owns Bonuses: ");
            foreach (var bonus in state.VisibleMap.Bonuses.Values)
            {
                if (bonus.IsOwnedByOpponent(opponentID))
                    message.Append(bonus.Details.Name + ", ");
            }
            AILog.Log("Debug", message.ToString());
        }
    }
}
