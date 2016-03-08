/*
* This code was auto-converted from a java project.
*/

using System;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;


namespace WarLight.Shared.AI.Wunderwaffe.Heuristics
{
    /// <summary>PlayerExpansionValueHeuristic represents the expansion value of a player looking at the whole board.
    /// </summary>
    public class PlayerExpansionValueHeuristic
    {
        public double PlayerExpansionValue = 0.0;
        public BotMain BotState;

        public PlayerExpansionValueHeuristic(BotMain state, BotMap map, PlayerIDType playerID)
        {
            this.BotState = state;

            //var weightsStr = map.Bonuses.Values.Select(o => o.Details.Name + ": " + BonusExpansionValue(o, playerID)).JoinStrings("\n");

            this.PlayerExpansionValue = map.Bonuses.Values.Sum(o => BonusExpansionValue(o, playerID));
        }

        private double BonusExpansionValue(BotBonus bonus, PlayerIDType playerID)
        {
            BonusExpansionValueHeuristic BonusHeuristic = null;
            if (playerID == BotState.Me.ID)
            {
                if (bonus.MyExpansionValueHeuristic == null)
                    bonus.SetMyExpansionValueHeuristic();

                BonusHeuristic = bonus.MyExpansionValueHeuristic;
            }
            else
            {
                if (bonus.OpponentExpansionValueHeuristics.ContainsKey(playerID) == false)
                    bonus.SetOpponentExpansionValueHeuristic(playerID);

                BonusHeuristic = bonus.OpponentExpansionValueHeuristics[playerID];
            }

            return BonusHeuristic.ExpansionValue;
        }

        public override string ToString()
        {
            return "PlayerExpansionValue: " + PlayerExpansionValue;
        }
    }
}
