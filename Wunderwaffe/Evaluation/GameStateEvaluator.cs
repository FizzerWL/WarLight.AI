using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;


namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    /// <remarks>GameState is responsible for evaluating whether we are winning or losing.
    /// </remarks>
    public class GameStateEvaluator
    {
        public BotMain BotState;
        public GameStateEvaluator(BotMain state)
        {
            this.BotState = state;
        }

        public bool IsGameCompletelyLost = false;

        public void EvaluateGameState()
        {
            // Don't switch from a lost game to an open game
            if (IsGameCompletelyLost)
                return;

            var ownArmies = new Armies(0);
            foreach (var territory in BotState.VisibleMap.GetOwnedTerritories())
                ownArmies = ownArmies.Add(territory.Armies);

            var opponents = BotState.Opponents.Where(o => o.State == GamePlayerState.Playing).Select(o => new OpponentInfo(BotState, o)).ToList();
            // int opponentIncome =
            // BotState.getGuessedOpponentIncome(BotState.VisibleMap);

            if (opponents.Count == 1 && BotState.MyIncome.Total == BotState.Settings.MinimumArmyBonus && opponents[0].Income >= BotState.MyIncome.Total * 2.4 && opponents[0].TotalArmies.NumArmies * 3 > ownArmies.NumArmies)
                IsGameCompletelyLost = true;
        }

        class OpponentInfo
        {
            public List<BotTerritory> Territories;
            public Armies TotalArmies;
            public int Income;

            public OpponentInfo(BotMain state, GamePlayer opponent)
            {
                this.Territories = state.VisibleMap.OpponentTerritories(opponent.ID);
                var opponentArmies = new Armies(0);
                foreach (var territory in Territories)
                    opponentArmies = opponentArmies.Add(territory.Armies);
                TotalArmies = opponentArmies;

                this.Income = state.HistoryTracker.OpponentDeployment(opponent.ID);

            }
        }

    }
}
