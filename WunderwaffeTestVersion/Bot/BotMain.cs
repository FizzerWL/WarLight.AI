using System.Linq;
using System.Collections.Generic;
using WarLight.AI.WunderwaffeTestVersion.BasicAlgorithms;
using WarLight.AI.WunderwaffeTestVersion.Evaluation;
using WarLight.AI.WunderwaffeTestVersion.Strategy;
using WarLight.AI.WunderwaffeTestVersion.Tasks;
using WarLight.Shared.AI;

namespace WarLight.AI.WunderwaffeTestVersion.Bot
{
    public class BotMain : IWarLightAI
    {
        // Gets called multiple times during the game...
        public BotMain()
        {
            this.FogRemover = new FogRemover(this);
            this.HistoryTracker = new HistoryTracker(this);
            this.MovesScheduler2 = new MovesScheduler(this);
            this.MovesCalculator = new MovesCalculator(this);
            this.DeleteBadMovesTask = new DeleteBadMovesTask(this);
            this.TerritoryValueCalculator = new TerritoryValueCalculator(this);
            this.ExpansionMapUpdater = new ExpansionMapUpdater(this);
            this.BreakTerritoryTask = new BreakTerritoryTask(this);
            this.BonusValueCalculator = new BonusValueCalculator(this);
            this.ExpansionTask = new ExpansionTask(this);
            this.BonusExpansionValueCalculator = new BonusExpansionValueCalculator(this);
            this.DefendTerritoryTask = new DefendTerritoryTask(this);
            this.DefendTerritoriesTask = new DefendTerritoriesTask(this);
            this.MapUpdater = new MapUpdater(this);
            this.PreventOpponentExpandBonusTask = new PreventOpponentExpandBonusTask(this);
            this.TakeTerritoriesTaskCalculator = new TakeTerritoriesTaskCalculator(this);
            this.OpponentDeploymentGuesser = new OpponentDeploymentGuesser(this);
            this.PicksEvaluator = new PicksEvaluator(this);
        }

        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings settings, int numTurns, Dictionary<PlayerIDType, PlayerIncome> playerIncomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay)
        {
            this.Players = players;
            this.Me = players[myPlayerID];
            this.Settings = settings;

            this.Map = map;
            this.VisibleMap = new BotMap(this);
            foreach (var bonus in this.Map.Bonuses.Values)
            {
                VisibleMap.Bonuses.Add(bonus.ID, new BotBonus(VisibleMap, bonus.ID));
            }

            foreach (var terr in this.Map.Territories.Values)
            {
                VisibleMap.Territories.Add(terr.ID, new BotTerritory(VisibleMap, terr.ID, TerritoryStanding.FogPlayerID, new Armies(0)));
            }

            VisibleMap = BotMap.FromStanding(this, latestTurnStanding);

            this.DistributionStanding = distributionStanding;

            this.NumberOfTurns = numTurns;
            this.PlayerIncomes = playerIncomes;

            this.PrevTurn = prevTurn;
            this.TeammatesOrders = teammatesOrders ?? new Dictionary<PlayerIDType, TeammateOrders>();
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
        }


        public FogRemover FogRemover;
        public PicksEvaluator PicksEvaluator;
        public OpponentDeploymentGuesser OpponentDeploymentGuesser;
        public TakeTerritoriesTaskCalculator TakeTerritoriesTaskCalculator;
        public PreventOpponentExpandBonusTask PreventOpponentExpandBonusTask;
        public HistoryTracker HistoryTracker;
        public MovesScheduler MovesScheduler2;
        public MovesCalculator MovesCalculator;
        public GameSettings Settings;
        public List<CardInstance> Cards;
        public int CardsMustPlay;

        public DeleteBadMovesTask DeleteBadMovesTask;
        public TerritoryValueCalculator TerritoryValueCalculator;
        public ExpansionMapUpdater ExpansionMapUpdater;
        public BreakTerritoryTask BreakTerritoryTask;
        public ExpansionTask ExpansionTask;
        public BonusValueCalculator BonusValueCalculator;
        public BonusExpansionValueCalculator BonusExpansionValueCalculator;

        public int MustStandGuardOneOrZero
        {
            get { return Settings.OneArmyMustStandGuardOneOrZero; }
        }

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

        public BotMap VisibleMap;
        public static BotMap LastVisibleMap;

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


        // TODO hier
        public List<GameOrder> GetOrders()
        {
            Debug.Debug.PrintDebugOutputBeginTurn(this);
            FogRemover.RemoveFog();
            this.HistoryTracker.ReadOpponentDeployment();
            this.WorkingMap = this.VisibleMap.GetMapCopy();
            DistanceCalculator.CalculateDistanceToBorder(this, this.VisibleMap, this.WorkingMap);
            DistanceCalculator.CalculateDirectDistanceToOpponentTerritories(this.VisibleMap, this.VisibleMap);
            DistanceCalculator.CalculateDistanceToOpponentBonuses(this.VisibleMap);
            DistanceCalculator.CalculateDistanceToOwnBonuses(this.VisibleMap);
            this.BonusExpansionValueCalculator.ClassifyBonuses(this.VisibleMap, this.VisibleMap);
            this.TerritoryValueCalculator.CalculateTerritoryValues(this.VisibleMap, this.WorkingMap);

            foreach (var opp in this.Opponents)
            {
                this.OpponentDeploymentGuesser.GuessOpponentDeployment(opp.ID);
            }
            this.MovesCalculator.CalculateMoves();
            Debug.Debug.PrintDebugOutput(this);

            Debug.Debug.printExpandBonusValues(VisibleMap, this);
            Debug.Debug.PrintTerritoryValues(VisibleMap, this);
            //this.MovesCalculator.CalculatedMoves.DumpToLog();
            LastVisibleMap = VisibleMap.GetMapCopy();
            return this.MovesCalculator.CalculatedMoves.Convert();
        }

    }
}
