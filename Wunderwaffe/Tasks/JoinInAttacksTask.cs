using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>
    /// JoinInAttacksTask is responsible that territories with idle armies join in
    /// attacks to opponent spots, happening from other owned territories.
    /// </summary>
    public static class JoinInAttacksTask
    {
        public static Moves CalculateJoinInAttacksTask(BotMain state)
        {
            var outvar = new Moves();
            foreach (var ourTerritory in state.VisibleMap.GetOpponentBorderingTerritories())
            {
                var bestSeriouslyAttackedNeighbor = GetBestSeriouslyAttackedNeighbor(ourTerritory);
                if (ourTerritory.GetIdleArmies().AttackPower > 1 && bestSeriouslyAttackedNeighbor != null)
                {
                    outvar.AddOrder(new BotOrderAttackTransfer(state.Me.ID, ourTerritory, bestSeriouslyAttackedNeighbor, ourTerritory.GetIdleArmies(), "JoinInAttacksTask"));
                }
            }
            return outvar;
        }

        private static BotTerritory GetBestSeriouslyAttackedNeighbor(BotTerritory ourTerritory)
        {
            BotTerritory outvar = null;
            var bestNeighborAttackValue = -1;
            foreach (var opponentNeighbor in ourTerritory.GetOpponentNeighbors())
            {
                if (IsTerritorySeriouslyAttacked(opponentNeighbor) && opponentNeighbor.AttackTerritoryValue > bestNeighborAttackValue)
                {
                    bestNeighborAttackValue = opponentNeighbor.AttackTerritoryValue;
                    outvar = opponentNeighbor;
                }
            }
            return outvar;
        }

        private static bool IsTerritorySeriouslyAttacked(BotTerritory opponentTerritory)
        {
            foreach (var atm in opponentTerritory.IncomingMoves)
            {
                if (atm.Armies.AttackPower > 1 && atm.Message != AttackMessage.TryoutAttack)
                    return true;
            }
            return false;
        }
    }
}
