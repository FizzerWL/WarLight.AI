using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod
{
    public class BotMain : IWarLightAI
    {
        public BotMain(bool useRandomness)
        {
            this.UseRandomness = useRandomness;
        }

        public string Name()
        {
            return "Prod 2.0" + (UseRandomness ? " with randomness" : "");
        }

        public string Description()
        {
            return "Version 2.0 of Warzone's production AI." + (UseRandomness ? " This bot allows randomness to influence its actions to keep it from being predictable.  This is the same AI that powers AIs in multi-player games, as well as custom single-player levels." : "");
        }

        public bool SupportsSettings(GameSettings settings, out string whyNot)
        {
            whyNot = null;
            return true; //Prod supports all settings
        }
        public bool RecommendsSettings(GameSettings settings, out string whyNot)
        {
            var sb = new StringBuilder();

            if (settings.NoSplit)
                sb.AppendLine("This bot does not understand no-split mode and will issue attacks as if no-split mode was disabled.");
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

        public bool UseRandomness;

        public GameStanding DistributionStandingOpt;
        public GameStanding Standing;
        public PlayerIDType PlayerID;
        public Dictionary<PlayerIDType, GamePlayer> Players;

        public MapDetails Map;
        public GameSettings Settings;
        public Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        public List<CardInstance> Cards;
        public int CardsMustPlay;
        public Dictionary<PlayerIDType, PlayerIncome> Incomes;

        public PlayerIncome BaseIncome;
        public PlayerIncome EffectiveIncome;

        public List<GamePlayer> Opponents;
        public bool IsFFA; //if false, we're in a 1v1, 2v2, 3v3, etc.  If false, there are more than two entities still alive in the game.  A game can change from FFA to non-FFA as players are eliminated.
        public Dictionary<PlayerIDType, Neighbor> Neighbors;
        public Dictionary<PlayerIDType, int> WeightedNeighbors;
        public HashSet<TerritoryIDType> AvoidTerritories = new HashSet<TerritoryIDType>(); //we're conducting some sort of operation here, such as a a blockade, so avoid attacking or deploying more here.
        private Stopwatch Timer;
        public List<string> Directives;

        public bool PastTime(double seconds)
        {
            var ret = Timer.Elapsed.TotalSeconds >= seconds;

            if (ret)
                AILog.Log("BotMain", "PastTime " + seconds + " seconds, at " + Timer.Elapsed.TotalSeconds + " seconds");

            return ret;
        }

        //not available during picking:
        public MakeOrders.MakeOrdersMain MakeOrders; 
        public MakeOrders.OrdersManager Orders { get { return MakeOrders.Orders; } }

        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay, Stopwatch timer, List<string> directives)
        {
            this.DistributionStandingOpt = distributionStanding;
            this.Standing = latestTurnStanding;
            this.PlayerID = myPlayerID;
            this.Players = players;
            this.Map = map;
            this.Settings = gameSettings;
            this.TeammatesOrders = teammatesOrders;
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
            this.Incomes = incomes;
            this.BaseIncome = Incomes[PlayerID];
            this.EffectiveIncome = this.BaseIncome.Clone();
            this.Neighbors = players.Keys.ExceptOne(PlayerID).ConcatOne(TerritoryStanding.NeutralPlayerID).ToDictionary(o => o, o => new Neighbor(this, o));
            this.Opponents = players.Values.Where(o => o.State == GamePlayerState.Playing && !IsTeammateOrUs(o.ID)).ToList();
            this.IsFFA = Opponents.Count > 1 && (Opponents.Any(o => o.Team == PlayerInvite.NoTeam) || Opponents.GroupBy(o => o.Team).Count() > 1);
            this.WeightedNeighbors = WeightNeighbors();
            this.Timer = timer;
            this.Directives = directives;
            AILog.Log("BotMain", "Prod initialized.  Starting at " + timer.Elapsed.TotalSeconds + " seconds");
        }

        public int ArmiesToTakeMultiAttack(IEnumerable<Armies> defenseArmiesOnManyTerritories)
        {
            var list = defenseArmiesOnManyTerritories.ToList();
            if (list.Count == 0)
                return 0;
            list.Reverse();

            var ret = ArmiesToTake(list[0]);

            foreach(var def in list.Skip(1))
            {
                var toTake = ArmiesToTake(def);
                var mustOccupyTerritory = ret + Settings.OneArmyMustStandGuardOneOrZero;
                var willLoseInFight = SharedUtility.Round(def.DefensePower * Settings.DefenseKillRate);
                ret = Math.Max(toTake, mustOccupyTerritory + willLoseInFight);
            }

            return ret;
        }

        public int ArmiesToTake(Armies defenseArmies)
        {
            Assert.Fatal(!defenseArmies.Fogged, "ArmiesToTake called on fog");

            var ret = SharedUtility.Round((defenseArmies.DefensePower / Settings.OffenseKillRate) - 0.5);

            if (ret == SharedUtility.Round(defenseArmies.DefensePower * Settings.DefenseKillRate))
                ret++;

            if (Settings.RoundingMode == RoundingModeEnum.WeightedRandom && (!UseRandomness || RandomUtility.RandomNumber(3) != 0))
                ret++;

            if (Settings.LuckModifier > 0)
            {
                //Add up some armies to account for luck
                var factor = UseRandomness ? RandomUtility.BellRandom(2.5, 17.5) : 10.0;
                ret += SharedUtility.Round(Settings.LuckModifier / factor * ret); 
            }

            return ret;
        }

        public List<GameOrder> GetOrders()
        {
            MakeOrders = new MakeOrders.MakeOrdersMain(this);
            return MakeOrders.Go();
        }

        public List<TerritoryIDType> GetPicks()
        {
            return MakePicks.PickTerritories.MakePicks(this);
        }

        public string TerrString(TerritoryIDType terrID)
        {
            return Map.Territories[terrID].Name + " (" + terrID + ")";
        }
        public string BonusString(BonusIDType bonusID)
        {
            return BonusString(Map.Bonuses[bonusID]);
        }
        public string BonusString(BonusDetails bonus)
        {
            return bonus.Name + " (id=" + bonus.ID + " val=" + BonusValue(bonus.ID) + ")";
        }
        public GamePlayer GamePlayerReference
        {
            get { return Players[PlayerID]; }
        }
        public bool IsOpponent(PlayerIDType playerID)
        {
            return Players.ContainsKey(playerID) && !IsTeammateOrUs(playerID);
        }
        public bool IsTeammate(PlayerIDType playerID)
        {
            return Players[PlayerID].Team != PlayerInvite.NoTeam && Players.ContainsKey(playerID) && Players[playerID].Team == Players[PlayerID].Team;
        }
        public bool IsTeammateOrUs(PlayerIDType playerID)
        {
            return PlayerID == playerID || IsTeammate(playerID);
        }
        public int BonusValue(BonusIDType bonusID)
        {
            if (Settings.OverriddenBonuses.ContainsKey(bonusID))
                return Settings.OverriddenBonuses[bonusID];
            else
                return Map.Bonuses[bonusID].Amount;
        }

        public IEnumerable<GameOrder> TeammatesSubmittedOrders
        {
            get
            {
                if (TeammatesOrders == null)
                    return new GameOrder[0];
                else
                    return TeammatesOrders.Values.Where(o => o.Orders != null).SelectMany(o => o.Orders);
            }
        }
        

        public bool IsBorderTerritory(GameStanding standing, TerritoryIDType terrID)
        {
            var ts = standing.Territories[terrID];
            if (ts.OwnerPlayerID != PlayerID)
                return false;
            return this.Map.Territories[terrID].ConnectedTo.Keys.Any(c => standing.Territories[c].OwnerPlayerID != this.PlayerID);
        }

        /// <summary>
        /// Territories of ours that aren't entirely enclosed by our own
        /// </summary>
        public IEnumerable<TerritoryStanding> BorderTerritories
        {
            get { return Standing.Territories.Values.Where(o => IsBorderTerritory(Standing, o.ID)); }
        }


        /// <summary>
        /// Returns 0 if it is an ememy, and a positive number otherwise indicating how many turns away from an enemy it is
        /// </summary>
        /// <param name="terrID"></param>
        /// <returns></returns>
        public int DistanceFromEnemy(TerritoryIDType terrID)
        {
            if (IsTeammateOrUs(Standing.Territories[terrID].OwnerPlayerID) == false)
                return 0;

            var terrIDs = new HashSet<TerritoryIDType>();
            terrIDs.Add(terrID);

            var distance = 1;

            while (true)
            {
                var toAdd = terrIDs.SelectMany(o => Map.Territories[o].ConnectedTo.Keys).Except(terrIDs).ToList();

                if (toAdd.Count == 0)
                    return int.MaxValue; //no enemies found on the entire map

                if (toAdd.Any(o => Standing.Territories[o].IsNeutral == false && IsTeammateOrUs(Standing.Territories[o].OwnerPlayerID) == false))
                    break; //found an enemy

                terrIDs.AddRange(toAdd);
                distance++;
            }

            return distance;
        }


        public TerritoryIDType OurNearestSpotTo(TerritoryIDType terr)
        {
            if (Standing.Territories[terr].OwnerPlayerID == PlayerID)
                return terr;

            var visited = new HashSet<TerritoryIDType>();
            visited.Add(terr);

            bool addedOne;

            do
            {
                addedOne = false;

                foreach (var front in visited.ToList())
                {
                    var connections = Map.Territories[front].ConnectedTo.Keys.Where(o => !visited.Contains(o)).ToList();

                    if (UseRandomness)
                        connections.RandomizeOrder();

                    foreach (var conn in connections)
                    {
                        if (Standing.Territories[conn].OwnerPlayerID == PlayerID)
                            return conn;

                        visited.Add(conn);
                        addedOne = true;
                    }
                }
            }
            while (addedOne);

            throw new Exception("Could not find any territories of ours");
        }


        public bool PlayerControlsBonus(BonusDetails b)
        {
            var c = b.ControlsBonus(Standing);
            return c.HasValue && c.Value == PlayerID;
        }

        public TerritoryIDType? MoveTowardsNearestBorderNonNeutralThenNeutral(TerritoryIDType terrID)
        {
            var move = this.MoveTowardsNearestBorder(terrID, false);
            if (!move.HasValue)
                move = this.MoveTowardsNearestBorder(terrID, true);

            return move;
        }

        public TerritoryIDType? MoveTowardsNearestBorder(TerritoryIDType id, bool neutralOk)
        {
            var neighborDistances = new KeyValueList<TerritoryIDType, int>();

            foreach (var immediateNeighbor in Map.Territories[id].ConnectedTo.Keys)
            {
                var nearestBorder = FindNearestBorder(immediateNeighbor, id, neutralOk);
                if (nearestBorder != null)
                    neighborDistances.Add(immediateNeighbor, nearestBorder.Depth);
            }

            if (neighborDistances.Count == 0)
                return null;

            var ret = neighborDistances.GetKey(0);
            int minValue = neighborDistances.GetValue(0);

            for (int i = 1; i < neighborDistances.Count; i++)
            {
                if (neighborDistances.GetValue(i) < minValue)
                {
                    ret = neighborDistances.GetKey(i);
                    minValue = neighborDistances.GetValue(i);
                }
            }

            return ret;
        }

        public class FindNearestBorderResult
        {
            public TerritoryIDType NearestBorder;
            public int Depth;
        }

        public FindNearestBorderResult FindNearestBorder(TerritoryIDType id, TerritoryIDType? exclude, bool neutralOk)
        {
            var queue = new Queue<TerritoryIDType>();
            queue.Enqueue(id);
            var visited = new HashSet<TerritoryIDType>();
            if (exclude.HasValue)
                visited.Add(exclude.Value);

            int depth = 0;

            while (true)
            {
                var r = FindNearestBorderRecurse(queue, visited, neutralOk);
                if (r.HasValue)
                {
                    FindNearestBorderResult ret = new FindNearestBorderResult();
                    ret.NearestBorder = r.Value;
                    ret.Depth = depth;
                    return ret;
                }

                depth++;

                if (queue.Count == 0)
                    return null; //No border

            }

#if CS2HX || CSSCALA
            throw new Exception("Never");
#endif
        }

        private TerritoryIDType? FindNearestBorderRecurse(Queue<TerritoryIDType> queue, HashSet<TerritoryIDType> visited, bool neutralOk)
        {
            var id = queue.Dequeue();

            if (Map.Territories[id].ConnectedTo.Keys.Any(o =>
                {
                    if (neutralOk)
                        return !this.IsTeammateOrUs(this.Standing.Territories[o].OwnerPlayerID);
                    else
                        return this.IsOpponent(this.Standing.Territories[o].OwnerPlayerID);
                }))
                return id; //We're a border

            foreach (var notVisited in Map.Territories[id].ConnectedTo.Keys.Where(o => !visited.Contains(o)))
            {
                queue.Enqueue(notVisited);
                visited.Add(notVisited);
            }

            return null;
        }



        public bool OpponentMightControlBonus(BonusDetails b)
        {
            PlayerIDType? oppID = null;
            foreach (var territoryID in b.Territories)
            {
                var ts = Standing.Territories[territoryID];
                if (ts.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                    continue;

                if (ts.OwnerPlayerID == TerritoryStanding.AvailableForDistribution || ts.OwnerPlayerID == TerritoryStanding.NeutralPlayerID || IsTeammateOrUs(ts.OwnerPlayerID))
                    return false;
                if (!oppID.HasValue)
                    oppID = ts.OwnerPlayerID;
                else if (oppID.Value != ts.OwnerPlayerID)
                    return false; //nobody has it
            }

            return true;
        }

        private Dictionary<BonusIDType, float> _bonusFuzz;

        public float BonusFuzz(BonusIDType bonusID)
        {
            if (!UseRandomness)
                return 0;

            if (_bonusFuzz == null)
                _bonusFuzz = new Dictionary<BonusIDType, float>();

            if (!_bonusFuzz.ContainsKey(bonusID))
                _bonusFuzz.Add(bonusID, (float)RandomUtility.BellRandom(-2, 2));

            return _bonusFuzz[bonusID];
        }



        private Dictionary<PlayerIDType, int> WeightNeighbors()
        {
            var ret = Neighbors.Values
                .Where(o => !IsTeammateOrUs(o.ID)) //Exclude teammates
                .Where(o => o.ID != TerritoryStanding.NeutralPlayerID) //Exclude neutral
                .Where(o => o.ID != TerritoryStanding.FogPlayerID) //only where we can see
                .ToDictionary(o => o.ID, neighbor => neighbor.NeighboringTerritories.Where(o => o.NumArmies.Fogged == false).Sum(n => n.NumArmies.AttackPower));  //Sum each army they have on our borders as the initial weight

            foreach (var borderTerr in BorderTerritories)
            {
                //Subtract one weight for each defending army we have next to that player

                Map.Territories[borderTerr.ID].ConnectedTo.Keys
                    .Select(o => Standing.Territories[o])
                    .Where(o => ret.ContainsKey(o.OwnerPlayerID))
                    .Select(o => o.OwnerPlayerID)
                    .Distinct()
                    .ForEach(o => ret[o] = ret[o] - Standing.Territories[borderTerr.ID].NumArmies.DefensePower);
            }

            return ret;
        }


        /// <summary>
        /// Finds territories that are "safe" -- not near an opponent
        /// </summary>
        /// <param name="acceptableRangeFromOpponent">If 0, territories directly adjanent to an opponent will be returned.  If 1, terriories must have a least one spot away, etc.</param>
        /// <returns></returns>
        public HashSet<TerritoryIDType> TerritoriesNotNearOpponent(int acceptableRangeFromOpponent)
        {
            var ret = this.Standing.Territories.Values.Where(o => o.OwnerPlayerID == PlayerID).Select(o => o.ID).ToHashSet(true);

            //Start with all opponent territories
            var terrs = ret.Where(o => IsOpponent(Standing.Territories[o].OwnerPlayerID)).ToHashSet(true);

            //traverse out, removing found territories from the return value
            for (int i = 0; i < acceptableRangeFromOpponent; i++)
            {
                var traverse = terrs.SelectMany(o => Map.Territories[o].ConnectedTo.Keys);
                terrs.AddRange(traverse);
                ret.RemoveAll(traverse);
            }

            return ret;
        }


    }
}
