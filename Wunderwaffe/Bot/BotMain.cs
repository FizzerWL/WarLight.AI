/*
* This code was auto-converted from a java project.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.BasicAlgorithms;
using WarLight.Shared.AI.Wunderwaffe.Evaluation;
using WarLight.Shared.AI.Wunderwaffe.Move;
using WarLight.Shared.AI.Wunderwaffe.Strategy;
using WarLight.Shared.AI.Wunderwaffe.Tasks;
using System.Text;

namespace WarLight.Shared.AI.Wunderwaffe.Bot
{
    public class BotMain : IWarLightAI
    {
        public string Description()
        {
            return "Bot written for an AI competition by Norman";
        }
        public bool SupportsSettings(GameSettings settings, out string whyNot)
        {
            var sb = new StringBuilder();

            if (settings.LocalDeployments)
                sb.AppendLine("This bot does not support Local Deployments");
            if (settings.OneArmyStandsGuard == false)
                sb.AppendLine("This bot does not support games without One Army Stands Guard");

            whyNot = sb.ToString();
            return whyNot.Length == 0;
        }
        public bool RecommendsSettings(GameSettings settings, out string whyNot)
        {
            var sb = new StringBuilder();

            if (settings.FogLevel != GameFogLevel.NoFog)
                sb.AppendLine("This bot works best in no-fog games.");
            if (settings.Commanders)
                sb.AppendLine("This bot does not understand Commanders and won't move or attack with them.");
            if (settings.MultiAttack)
                sb.AppendLine("This bot does not understand Multi-Attack and will only attack one territory at a time.");
            if (settings.Cards.ContainsKey(CardType.Blockade.CardID))
                sb.AppendLine("This bot does not understand how to play Blockade cards.");
            if (settings.Cards.ContainsKey(CardType.Bomb.CardID))
                sb.AppendLine("This bot does not understand how to play Bomb cards.");
            if (settings.Cards.ContainsKey(CardType.Diplomacy.CardID))
                sb.AppendLine("This bot does not understand how to play Diplomacy cards.");
            if (settings.Cards.ContainsKey(CardType.EmergencyBlockade.CardID))
                sb.AppendLine("This bot does not understand how to play Emergency Blockade cards.");
            if (settings.Cards.ContainsKey(CardType.Sanctions.CardID))
                sb.AppendLine("This bot does not understand how to play Sanctions cards.");
            if (settings.Cards.ContainsKey(CardType.OrderPriority.CardID))
                sb.AppendLine("This bot does not understand how to play Order Priority cards.");
            if (settings.Cards.ContainsKey(CardType.OrderDelay.CardID))
                sb.AppendLine("This bot does not understand how to play Order Delay cards.");
            if (settings.Cards.ContainsKey(CardType.Airlift.CardID))
                sb.AppendLine("This bot does not understand how to play Airlift cards.");
            if (settings.Cards.ContainsKey(CardType.Gift.CardID))
                sb.AppendLine("This bot does not understand how to play Gift cards.");
            if (settings.Cards.ContainsKey(CardType.Reconnaissance.CardID))
                sb.AppendLine("This bot does not understand how to play Reconnaissance cards.");
            if (settings.Cards.ContainsKey(CardType.Spy.CardID))
                sb.AppendLine("This bot does not understand how to play Spy cards.");
            if (settings.Cards.ContainsKey(CardType.Surveillance.CardID))
                sb.AppendLine("This bot does not understand how to play Surveillance cards.");

            whyNot = sb.ToString();
            return whyNot.Length == 0;
        }

        public BotMain()
        {
            this.HistoryTracker = new HistoryTracker(this);
            this.MovesScheduler2 = new MovesScheduler2(this);
            this.MovesCalculator = new MovesCalculator(this);
            this.GameState = new GameStateEvaluator(this);
            this.DeleteBadMovesTask = new DeleteBadMovesTask(this);
            this.TerritoryValueCalculator = new TerritoryValueCalculator(this);
            this.ExpansionMapUpdater = new ExpansionMapUpdater(this);
            this.BreakTerritoryTask = new BreakTerritoryTask(this);
            this.BonusValueCalculator = new BonusValueCalculator(this);
            this.BonusExpansionValueCalculator = new BonusExpansionValueCalculator(this);
            this.DefendTerritoryTask = new DefendTerritoryTask(this);
            this.DefendTerritoriesTask = new DefendTerritoriesTask(this);
            this.MapUpdater = new MapUpdater(this);
            this.PreventOpponentExpandBonusTask = new PreventOpponentExpandBonusTask(this);
            this.TakeTerritoriesTaskCalculator = new TakeTerritoriesTaskCalculator(this);
            this.LastVisibleMapUpdater = new LastVisibleMapUpdater(this);
            this.OpponentDeploymentGuesser = new OpponentDeploymentGuesser(this);
            this.PicksEvaluator = new PicksEvaluator(this);
        }

        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings settings, int numTurns, Dictionary<PlayerIDType, PlayerIncome> playerIncomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay)
        {
            this.Players = players;
            this.Me = players[myPlayerID];
            this.Settings = settings;

            this.Map = map;
            this.FullMap = new BotMap(this);
            foreach(var bonus in this.Map.Bonuses.Values)
                FullMap.Bonuses.Add(bonus.ID, new BotBonus(FullMap, bonus.ID));

            foreach (var terr in this.Map.Territories.Values)
                FullMap.Territories.Add(terr.ID, new BotTerritory(FullMap, terr.ID, TerritoryStanding.FogPlayerID, new Armies(0)));

            this.DistributionStanding = distributionStanding;

            this.NumberOfTurns = numTurns;
            this.PlayerIncomes = playerIncomes;

            VisibleMap = latestTurnStanding == null ? null : BotMap.FromStanding(this, latestTurnStanding);
            LastVisibleMap = previousTurnStanding == null ? null : BotMap.FromStanding(this, previousTurnStanding);

            this.PrevTurn = prevTurn;
            this.TeammatesOrders = teammatesOrders ?? new Dictionary<PlayerIDType, TeammateOrders>();
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
        }

        public PicksEvaluator PicksEvaluator;
        public OpponentDeploymentGuesser OpponentDeploymentGuesser;
        public LastVisibleMapUpdater LastVisibleMapUpdater;
        public TakeTerritoriesTaskCalculator TakeTerritoriesTaskCalculator;
        public PreventOpponentExpandBonusTask PreventOpponentExpandBonusTask;
        public HistoryTracker HistoryTracker;
        public MovesScheduler2 MovesScheduler2;
        public MovesCalculator MovesCalculator;
        public GameSettings Settings;
        public List<CardInstance> Cards;
        public int CardsMustPlay;

        public GameStateEvaluator GameState;
        public DeleteBadMovesTask DeleteBadMovesTask;
        public TerritoryValueCalculator TerritoryValueCalculator;
        public ExpansionMapUpdater ExpansionMapUpdater;
        public BreakTerritoryTask BreakTerritoryTask;
        public BonusValueCalculator BonusValueCalculator;
        public BonusExpansionValueCalculator BonusExpansionValueCalculator;


        public DefendTerritoryTask DefendTerritoryTask;
        public DefendTerritoriesTask DefendTerritoriesTask;
        public MapUpdater MapUpdater;

        public Dictionary<PlayerIDType, GamePlayer> Players;
        public GamePlayer Me;
        public Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;

        public bool IsTeammate(PlayerIDType playerID)
        {
            return playerID != Me.ID && Me.Team != PlayerInvite.NoTeam && Players.ContainsKey(playerID) && Players[playerID].Team == Me.Team;
        }
        public bool IsTeammateOrUs(PlayerIDType playerID)
        {
            return Me.ID == playerID || IsTeammate(playerID);
        }
        public bool IsOpponent(PlayerIDType playerID)
        {
            return Players.ContainsKey(playerID) && !IsTeammateOrUs(playerID);
        }

        public IEnumerable<GamePlayer> Opponents
        {
            get { return Players.Values.Where(o => IsOpponent(o.ID)); }
        }

        public BotMap FullMap;
        public BotMap VisibleMap;
        public BotMap LastVisibleMap; //VisibleMap from the previous turn?

        public MapDetails Map;

        /// <summary>
        /// This map is responsible for storing the current situation according to our already made move decisions during the current turn.
        /// </summary>
        public BotMap WorkingMap;
        public BotMap ExpansionMap;
        public GameStanding DistributionStanding;
        public GameOrder[] PrevTurn;
        public Dictionary<PlayerIDType, PlayerIncome> PlayerIncomes;  //Just based on what we can see.  Guaranteed to be accurate for us and teammates, but usually wrong for opponents
        public int NumberOfTurns = -1;

        public PlayerIncome MyIncome
        {
            get { return PlayerIncomes[Me.ID]; }
        }

        public int GetGuessedOpponentIncome(PlayerIDType opponentID, BotMap mapToUse)
        {
            var outvar = Settings.MinimumArmyBonus;
            foreach (var bonus in mapToUse.Bonuses.Values)
                if (bonus.IsOwnedByOpponent(opponentID))
                    outvar += bonus.Amount;

            return outvar;
        }

        public List<TerritoryIDType> GetPicks()
        {
            return PicksEvaluator.GetPicks();
        }
        public List<GameOrder> GetOrders()
        {
            if (this.NumberOfTurns > 1)
                this.LastVisibleMapUpdater.StoreOpponentDeployment();

            Debug.Debug.PrintDebugOutputBeginTurn(this);
            this.HistoryTracker.ReadOpponentDeployment();
            this.WorkingMap = this.VisibleMap.GetMapCopy();
            DistanceCalculator.CalculateDistanceToBorder(this, this.VisibleMap, this.WorkingMap);
            DistanceCalculator.CalculateDirectDistanceToOpponentTerritories(this.VisibleMap, this.VisibleMap);
            DistanceCalculator.CalculateDistanceToOpponentBonuses(this.VisibleMap);
            DistanceCalculator.CalculateDistanceToOwnBonuses(this.VisibleMap);
            this.BonusExpansionValueCalculator.ClassifyBonuses(this.VisibleMap, this.VisibleMap);
            this.TerritoryValueCalculator.CalculateTerritoryValues(this.VisibleMap, this.WorkingMap);

            foreach(var opp in this.Opponents)
                this.OpponentDeploymentGuesser.GuessOpponentDeployment(opp.ID);
            this.MovesCalculator.CalculateMoves();
            Debug.Debug.PrintDebugOutput(this);
            
            // TODO debug
            //Debug.Debug.PrintBonusExpansionValues(this.VisibleMap);



            this.MovesCalculator.CalculatedMoves.DumpToLog();

            return this.MovesCalculator.CalculatedMoves.Convert();
        }

    }
}
