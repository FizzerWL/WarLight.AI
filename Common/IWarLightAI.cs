using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="gameID">ID of the game, for debugging</param>
        /// <param name="timer">A timer that represents how long we've been working on our AI.  In single-player games, this will include time that previous AIs spent.  AIs can optionally use this to play faster if the player has been waiting a long time.  Generally, we don't want to keep the player waiting more than 15 seconds.</param>
        /// <param name="directives">Some single player levels need to pass custom logic to the AI, which will appear in this parameter.</param>
        void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay, Stopwatch timer, List<string> directives);

        List<TerritoryIDType> GetPicks();

        List<GameOrder> GetOrders();


        /// <summary>
        /// Return the name of your bot.  It should be short (no more than 20 characters, approximately)
        /// </summary>
        /// <returns></returns>
        string Name();

        /// <summary>
        /// Return a general description of this bot, with attribution to the owner if desired.
        /// </summary>
        /// <returns></returns>
        string Description();

        /// <summary>
        /// Allows an AI to specify if it supports the passed settings or not.  Return true if your bot can successfully submit valid orders on these settings, and false if it will crash or otherwise fail to submit valid orders.
        /// If you return false, you should give a reason in the "whyNot" parameter describing why your AI does not support these settings.  For example, if your bot does not support Local Deployments, you can return false and assign whyNot to be "This bot does not support games with the Local Deployments setting enabled."
        /// If there are multiple reasons your bot does not work on these settings, append them to whyNot separated by newlines (\n)
        /// If you return true, the whyNot parameter will be ignored.
        /// </summary>
        /// <returns></returns>
        bool SupportsSettings(GameSettings settings, out string whyNot);

        /// <summary>
        /// Allows an AI to specify if it recommends playing on the passed settings or not.  Return true if your bot will produce good orders on these settings, or false if you don't recommend using your bot on the passed settings.
        /// This will never be called on a bot that returned false to SupportsSettings on the same settings.
        /// If you return false, you should give a reason in the "whyNot" parameter describing why your AI is not recommended on these settings.  For example, if your bot does not take advantage of multi-attack, you can return false and assign whyNot to be "This bot does not take advantage of multi-attack.  It will simply attack one territory at a time."
        /// If there are multiple reasons you don't recommend using your bot on these settings, append them to whyNot separated by newlines (\n)
        /// If you return true, the whyNot parameter will be ignored.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="whyNot"></param>
        /// <returns></returns>
        bool RecommendsSettings(GameSettings settings, out string whyNot);
    }
}
