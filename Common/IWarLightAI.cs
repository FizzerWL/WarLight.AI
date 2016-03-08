using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public interface IWarLightAI
    {

        /// <summary>
        /// Called every turn, before GetPicks or GetOrders.
        /// </summary>
        /// <param name="players">All of the players in the game, and details about them</param>
        /// <param name="myPlayerID">Your player ID</param>
        /// <param name="map">Details of the map, such as what territories exist, how they connect, and bonuses.</param>
        /// <param name="gameSettings">Settings of the game, such as how many armies you start with</param>
        /// <param name="numberOfTurns">Number of turns that have passed in the game.  -1 during territory distribution.</param>
        /// <param name="incomes">How many armies each player in the game is making.  Guaranteed to be correct for you, but will probably be incorrect for opponents due to fog.</param>
        /// <param name="latestTurnStanding">What the board looks like right now.</param>
        /// <param name="previousTurnStanding">What the board looked like on the previous turn.</param>
        /// <param name="distributionStanding">What the board looked like during territory distribution</param>
        /// <param name="prevTurn">All of the orders you could see on the previous turn</param>
        /// <param name="cards">Whole cards that you or your team controls, if any</param>
        /// <param name="cardsMustPlay">If you must play a card this turn, this will be positive</param>
        /// <param name="teammatesOrders">Orders your teammates have committed, if any.</param>
        void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay);

        List<TerritoryIDType> GetPicks();

        List<GameOrder> GetOrders();
    }
}
