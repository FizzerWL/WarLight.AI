using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WarLight.AI.Prod
{
    public class FindNearestBorderResult
    {
        public TerritoryIDType NearestBorder;
        public int Depth;
    }

    public class UtilizeSpareArmiesObject
    {
        public GameOrderAttackTransfer Order;
        public int Available;
        public UtilizeSpareArmiesObject(GameOrderAttackTransfer order, int available)
        {
            Order = order;
            Available = available;
        }
    }

    public class PossibleAttack
    {
        public TerritoryIDType From, To;
        public double DefenseImportance = 0;
        public double OffenseImportance = 0;
        private TakePlayingTurnContainer Parent;

        public PossibleAttack(TakePlayingTurnContainer parent, TerritoryIDType from, TerritoryIDType to)
        {
            From = from;
            To = to;
            Parent = parent;
        }
    }

    public class Neighbor
    {
        public PlayerIDType ID;
        private TakePlayingTurnContainer Parent;

        public Neighbor(TakePlayingTurnContainer parent, PlayerIDType id)
        {
            ID = id;
            Parent = parent;
        }

        public IEnumerable<TerritoryStanding> Territories
        {
            get
            {

                return Parent.Standing.Territories.Values.Where(o => o.OwnerPlayerID == ID);
            }
        }

        public IEnumerable<TerritoryStanding> NeighboringTerritories
        {
            get
            {

                return Territories.Where(o => Parent.Map.Territories[o.ID].ConnectedTo.Any(c => Parent.Standing.Territories[c].OwnerPlayerID == Parent.Player));
            }
        }
    }
    public class TakePlayingTurnContainer
    {
        public GameStanding Standing;
        public MapDetails Map;
        public GameSettings Settings;
        public Dictionary<PlayerIDType, GamePlayer> Players;

        public PlayerIDType Player;
        public PlayerIncome Income;
        PlayerIncomeTracker IncomeTracker;
        public List<GameOrder> Orders;
        public Dictionary<PlayerIDType, Neighbor> Neighbors;
        private Stopwatch _aiTimer;
        private Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        public List<CardInstance> Cards;
        public int CardsMustPlay;

        public TakePlayingTurnContainer(MapDetails map, GameStanding noFogStanding, Dictionary<PlayerIDType, GamePlayer> players, GameSettings settings, PlayerIDType player, PlayerIncome income, List<CardInstance> cards, int cardsMustPlay, Dictionary<PlayerIDType, TeammateOrders> activeOrders, Stopwatch aiTimer)
        {
            this.Map = map;
            this.Standing = noFogStanding;
            this.Players = players;
            this.Settings = settings;
            this.Player = player;
            this.Income = income;
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
            this.TeammatesOrders = activeOrders;
            this._aiTimer = aiTimer;

            Orders = new List<GameOrder>();
            Neighbors = players.Keys.ExceptOne(player).ConcatOne(TerritoryStanding.NeutralPlayerID).ToDictionary(o => o, o => new Neighbor(this, o));

            BuildOrders();

        }

        /// <summary>
        /// Inserts order based on phase
        /// </summary>
        /// <param name="order"></param>
        public void AddOrder(GameOrder orderToAdd)
        {
            for (int i = 0; i < Orders.Count; i++)
            {
                if ((int)orderToAdd.OccursInPhase.Value < (int)Orders[i].OccursInPhase.Value)
                {
                    Orders.Insert(i, orderToAdd);
                    return;
                }
            }

            Orders.Add(orderToAdd);
        }


        public GamePlayer GamePlayerReference
        {
            get { return Players[Player]; }
        }
        public IEnumerable<TerritoryStanding> Territories
        {
            get { return Standing.Territories.Values; }
        }

        public IEnumerable<TerritoryStanding> AttackableTerritories
        {
            get
            {

                return Territories.Where(o => Map.Territories[o.ID].ConnectedTo.Any(c => Standing.Territories[c].OwnerPlayerID == Player));
            }
        }

        public IEnumerable<GameOrder> TeammatesSubmittedOrders
        {
            get
            {
                if (this.TeammatesOrders == null)
                    return new GameOrder[0];
                else
                    return this.TeammatesOrders.Values.Where(o => o.Orders != null).SelectMany(o => o.Orders);
            }
        }

        /// <summary>
        /// Territories of ours that aren't entirely enclosed by our own
        /// </summary>
        public IEnumerable<TerritoryStanding> BorderTerritories
        {
            get
            {

                return Territories
                    .Where(o => Standing.Territories[o.ID].OwnerPlayerID == Player)
                    .Where(o => this.Map.Territories[o.ID].ConnectedTo
                        .Any(c => this.Standing.Territories[c].OwnerPlayerID != this.Player));
            }
        }


        public bool IsTeammate(PlayerIDType playerID)
        {
            return Players[Player].Team != PlayerInvite.NoTeam && Players.ContainsKey(playerID) && Players[playerID].Team == Players[Player].Team;
        }
        public bool IsTeammateOrUs(PlayerIDType playerID)
        {
            return Player == playerID || IsTeammate(playerID);
        }
        public int BonusValue(BonusIDType bonusID)
        {
            if (Settings.OverriddenBonuses.ContainsKey(bonusID))
                return Settings.OverriddenBonuses[bonusID];
            else
                return Map.Bonuses[bonusID].Amount;
        }


        /// <summary>
        /// Asserts that the orders built so far deploy all armies the AI is receiving
        /// </summary>
        //private void VerifyIncomeAccurate()
        //{

        //	var actualIncome = GamePlayerReference.IncomeFromStanding(Orders.OfType<GameOrderPlayCardReinforcement>().Select(o => ((ReinforcementCardInstance)Game.LatestTurnStanding_ReadOnly.FindCard(o.CardInstanceID)).Armies).SumInts(), Game.LatestTurnStanding_ReadOnly, false, false);
        //	var ordersDeploy = Orders.OfType<GameOrderDeploy>().Select(o => o.NumArmies).SumInts();

        //	if (actualIncome.Total != ordersDeploy)
        //	{
        //		//Throw some details in the error for debugging
        //		var sb = new StringBuilder();
        //		sb.AppendLine("Order incomes inaccurate. ActualIncome = " + actualIncome + ", OrdersDeploy=" + ordersDeploy + ", NumOrders=" + Orders.Count);
        //		foreach (var order in Orders)
        //			sb.AppendLine("Order: " + order.ToString());
        //		Assert.Fatal(false, sb.ToString());

        //	}
        //}

        private void BuildOrders()
        {
            IncomeTracker = new PlayerIncomeTracker(Income, this.Map);

            var weightedAttacks = WeightAttacks();

            PlayCards();

            //Commanders run away
            CommandersMovement();

            //Ensure teammates coordinate on bonuses
            ResolveTeamBonuses();

            //Expand into good opportunities
            Expand(IncomeTracker.RemainingUndeployed, -500);

            //Now defend/attack
            DefendAttack(IncomeTracker.RemainingUndeployed, weightedAttacks);

            //Now expand into anything remaining
            Expand(IncomeTracker.RemainingUndeployed, int.MinValue);

            //If there's still remaining income, deploy it randomly
            DeployRemaining();

            //Move any unused landlocked armies towards a border
            MoveLandlockedUp();

            //If any attack has spare armies on the source territory, make the attack use them
            UtilizeSpareArmies();

            //Verify we've deployed all armies
            //VerifyIncomeAccurate();
            Assert.Fatal(IncomeTracker.FullyDeployed, "Not fully deployed");
        }

        private void CommandersMovement()
        {
            if (!Settings.Commanders)
                return;

            var cmdrTerritory = Standing.Territories.Values.SingleOrDefault(o => o.NumArmies.SpecialUnits.OfType<Commander>().Any(t => t.OwnerID == Player));

            if (cmdrTerritory == null)
                return;

            //Consider this territory and all adjacent territories.  Which is the furthest from any enemy?
            var terrDistances = Map.Territories[cmdrTerritory.ID].ConnectedTo.ConcatOne(cmdrTerritory.ID)
                .Where(o => Standing.Territories[o].OwnerPlayerID == Player || Standing.Territories[o].NumArmies.DefensePower <= 4) //don't go somewhere that's defended heavily
                .ToDictionary(o => o, o => DistanceFromEnemy(o));

            var sorted = terrDistances.OrderByDescending(o => o.Value).ToList();
            sorted.RemoveWhere(o => o.Value < sorted[0].Value);

            var runTo = sorted.Random().Key;

            if (runTo == cmdrTerritory.ID)
                return; //already there

            AddAttack(cmdrTerritory.ID, runTo, AttackTransferEnum.AttackTransfer, cmdrTerritory.NumArmies.NumArmies, false);
        }

        /// <summary>
        /// Returns 0 if it is an ememy, and a positive number otherwise signaling how many turns away from an enemy it is
        /// </summary>
        /// <param name="terrID"></param>
        /// <returns></returns>
        private int DistanceFromEnemy(TerritoryIDType terrID)
        {
            if (IsTeammateOrUs(Standing.Territories[terrID].OwnerPlayerID) == false)
                return 0;

            var terrIDs = new HashSet<TerritoryIDType>();
            terrIDs.Add(terrID);

            var distance = 1;

            while (true)
            {
                var toAdd = terrIDs.SelectMany(o => Map.Territories[o].ConnectedTo).Except(terrIDs).ToList();

                if (toAdd.Count == 0)
                    return int.MaxValue; //no enemies found on the entire map

                if (toAdd.Any(o => Standing.Territories[o].IsNeutral == false && IsTeammateOrUs(Standing.Territories[o].OwnerPlayerID) == false))
                    break; //found an enemy

                terrIDs.AddRange(toAdd);
                distance++;
            }

            return distance;
        }

        private void DeployRemaining()
        {
            //Must do locked before free, otherwise a free deployment could happen to go into a locked bonus and we'd come up short.
            DeployRemainingLockedArmies();
            DeployRemainingFreeArmies();
        }

        void DeployRemainingLockedArmies()
        {
            foreach (var deploy in IncomeTracker.RestrictedBonusProgress)
            {
                var count = deploy.TotalToDeploy - deploy.Deployed;
                if (count > 0)
                    Deploy(Map.Bonuses[deploy.Restriction].Territories.Random(), count);
            }
        }

        void DeployRemainingFreeArmies()
        {
            var count = IncomeTracker.FreeArmiesUndeployed;
            if (count == 0)
                return;

            var ourTerritories = Standing.Territories.Values.Where(o => o.OwnerPlayerID == Player).ToList();

            if (ourTerritories.Count == 0)
                return;

            if (_aiTimer.Elapsed.TotalSeconds > 10)
            {
                //If we're overtime, just pick one and deploy it
                Deploy(ourTerritories.Random().ID, count);
            }
            else
            {
                var nearestBorder = FindNearestBorder(ourTerritories.Random().ID, new Nullable<TerritoryIDType>());

                if (nearestBorder != null)
                    Deploy(OurNearestSpotTo(nearestBorder.NearestBorder), count);
                else if (BorderTerritories.Any())
                    Deploy(BorderTerritories.Random().ID, count);
                else
                    Deploy(ourTerritories.Random().ID, count);
            }
        }

        private TerritoryIDType OurNearestSpotTo(TerritoryIDType terr)
        {
            if (Standing.Territories[terr].OwnerPlayerID == Player)
                return terr;

            var visited = new HashSet<TerritoryIDType>();
            visited.Add(terr);

            bool addedOne;

            do
            {
                addedOne = false;

                foreach (var front in visited.ToList())
                {
                    var connections = Map.Territories[front].ConnectedTo.Where(o => !visited.Contains(o)).ToList();
                    connections.RandomizeOrder();

                    foreach (var conn in connections)
                    {
                        if (Standing.Territories[conn].OwnerPlayerID == Player)
                            return conn;

                        visited.Add(conn);
                        addedOne = true;
                    }
                }
            }
            while (addedOne);

            throw new Exception("Could not find any territories of ours");
        }

        private void UtilizeSpareArmies()
        {
            IEnumerable<GameOrderAttackTransfer> attacks = Orders.OfType<GameOrderAttackTransfer>();

            foreach (var o in attacks.Select(o => new UtilizeSpareArmiesObject(o, this.GetArmiesAvailable(o.From)))
                .Where(o => o.Available > 0)
                .GroupBy(o => o.Order.From)
                .Select(o => o.Random()))
                o.Order.NumArmies = o.Order.NumArmies.Add(new Armies(o.Available));
        }

        private void PlayCards()
        {


            if (GamePlayerReference.Team != PlayerInvite.NoTeam && Players.Values.Any(o => !o.IsAIOrHumanTurnedIntoAI && o.Team == this.GamePlayerReference.Team && o.State == GamePlayerState.Playing && !o.HasCommittedOrders))
                return; //If there are any humans on our team that have yet to take their turn, do not play cards.

            //For now, just play all reinforcement cards, and discard if we must use any others.

            var cardsPlayedByTeammate =
                TeammatesSubmittedOrders.OfType<GameOrderPlayCard>().Select(o => o.CardInstanceID)
                .Concat(TeammatesSubmittedOrders.OfType<GameOrderDiscard>().Select(o => o.CardInstanceID))
                .ToHashSet(true);

            int numMustPlay = this.CardsMustPlay;

            foreach (var card in Cards)
            {
                if (cardsPlayedByTeammate.Contains(card.ID))
                    continue; //Teammate played it

                if (card is ReinforcementCardInstance)
                {
                    AddOrder(GameOrderPlayCardReinforcement.Create(card.ID, Player));
                    Income.FreeArmies += card.As<ReinforcementCardInstance>().Armies;
                    numMustPlay--;
                }
                else if (numMustPlay > 0) //For now, just discard all non-reinforcement cards if we must use the card
                {
                    AddOrder(GameOrderDiscard.Create(Player, card.ID));
                    numMustPlay--;
                }
            }
        }

        private bool PlayerControlsBonus(BonusDetails b)
        {
            var c = b.ControlsBonus(Standing);
            return c.HasValue && c.Value == Player;
        }

        private void ResolveTeamBonuses()
        {

            HashSet<TerritoryIDType> terrs = Standing.Territories.Values.Where(o => this.IsTeammateOrUs(o.OwnerPlayerID) && o.NumArmies.NumArmies == 1).Select(o => o.ID).ToHashSet(true);

            foreach (BonusDetails bonus in Map.Bonuses.Values)
            {
                if (bonus.Territories.All(o => terrs.Contains(o)) && bonus.ControlsBonus(Standing).HasValue == false)
                {
                    //This bonus is entirely controlled by our team with 1s, but not by a single player. The player with the most territories should take it.
                    var owners = bonus.Territories.GroupBy(o => this.Standing.Territories[o].OwnerPlayerID).ToList();
                    owners.Sort((f, s) => SharedUtility.CompareInts(s.Count(), f.Count()));

                    Assert.Fatal(owners.Count >= 2);

                    var attacks = bonus.Territories
                        .Where(o => this.Standing.Territories[o].OwnerPlayerID != this.Player) //Territories in the bonus by our teammate
                        .Where(o => this.Map.Territories[o].ConnectedTo.Any(c => this.Standing.Territories[c].OwnerPlayerID == this.Player)) //Where we control an adjacent
                        .Select(o => new PossibleAttack(this, this.Map.Territories[o].ConnectedTo.RandomWhere(c => this.Standing.Territories[c].OwnerPlayerID == this.Player), o));

                    if (owners[0].Count() == owners[1].Count())
                    {
                        //The top two players have the same number of terrs.  50% chance we should try taking one.
                        if (attacks.Any() && RandomUtility.RandomNumber(2) == 0)
                        {
                            var doAttack1 = attacks.Random();
                            if (TryDeploy(doAttack1.From, 2))
                                AddAttack(doAttack1.From, doAttack1.To, Settings.AllowAttackOnly ? AttackTransferEnum.Attack : AttackTransferEnum.AttackTransfer, 2, true);
                        }
                    }
                    else if (owners[0].Key == Player)
                    {
                        //We should take the bonus
                        foreach (var doAttack2 in attacks)
                        {
                            if (TryDeploy(doAttack2.From, 2))
                                AddAttack(doAttack2.From, doAttack2.To, Settings.AllowAttackOnly ? AttackTransferEnum.Attack : AttackTransferEnum.AttackTransfer, 2, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Any armies that are surrounded by our own territories (or our teammates) should move towards the nearest enemy.
        /// </summary>
        private void MoveLandlockedUp()
        {
            if (_aiTimer.Elapsed.TotalSeconds > 10)
                return;


            var start = Environment.TickCount;

            foreach (var landlocked in Standing.Territories.Values.Where(o => o.OwnerPlayerID == this.Player
                && this.Map.Territories[o.ID].ConnectedTo.All(c => this.IsTeammateOrUs(this.Standing.Territories[c].OwnerPlayerID))
                && this.GetArmiesAvailable(o.ID) > 1))
            {
                if (Environment.TickCount - start > 1000 || _aiTimer.Elapsed.TotalSeconds > 10)
                    break; //Extreme cases (i.e. where one player controls all of SE asia), this algorithm can take forever.  We don't care about these extreme cases since they've already won.  Stop processing after too long (max 1 second per AI, max 10 seconds for all AIs combined)

                var moveTowards = MoveTowardsNearestBorder(landlocked.ID);

                if (moveTowards.HasValue)
                    AddAttack(landlocked.ID, moveTowards.Value, Settings.AllowTransferOnly ? AttackTransferEnum.Transfer : AttackTransferEnum.AttackTransfer, this.GetArmiesAvailable(landlocked.ID), false);
            }
        }

        private TerritoryIDType? MoveTowardsNearestBorder(TerritoryIDType id)
        {
            var neighborDistances = new List<KeyValuePair<TerritoryIDType, int>>();

            foreach (var immediateNeighbor in Map.Territories[id].ConnectedTo)
            {
                FindNearestBorderResult nearestBorder = FindNearestBorder(immediateNeighbor, new Nullable<TerritoryIDType>(id));
                if (nearestBorder != null)
                    neighborDistances.Add(new KeyValuePair<TerritoryIDType, int>(immediateNeighbor, nearestBorder.Depth));
            }

            if (neighborDistances.Count == 0)
                return new Nullable<TerritoryIDType>();

            var ret = neighborDistances[0].Key;
            int minValue = neighborDistances[0].Value;

            for (int i = 1; i < neighborDistances.Count; i++)
            {
                if (neighborDistances[i].Value < minValue)
                {
                    ret = neighborDistances[i].Key;
                    minValue = neighborDistances[i].Value;
                }
            }

            return new Nullable<TerritoryIDType>(ret);
        }

        private FindNearestBorderResult FindNearestBorder(TerritoryIDType id, TerritoryIDType? exclude)
        {
            var queue = new Queue<TerritoryIDType>();
            queue.Enqueue(id);
            var visited = new HashSet<TerritoryIDType>();
            if (exclude.HasValue)
                visited.Add(exclude.Value);

            int depth = 0;

            while (true)
            {
                TerritoryIDType? r = FindNearestBorderRecurse(queue, visited);
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

        private TerritoryIDType? FindNearestBorderRecurse(Queue<TerritoryIDType> queue, HashSet<TerritoryIDType> visited)
        {

            var id = queue.Dequeue();

            if (Map.Territories[id].ConnectedTo.Any(o => !this.IsTeammateOrUs(this.Standing.Territories[o].OwnerPlayerID)))
                return new Nullable<TerritoryIDType>(id); //We're a border

            foreach (var notVisited in Map.Territories[id].ConnectedTo.Where(o => !visited.Contains(o)))
            {
                queue.Enqueue(notVisited);
                visited.Add(notVisited);
            }

            return new Nullable<TerritoryIDType>();
        }

        private void DefendAttack(int incomeToUse, IEnumerable<PossibleAttack> weightedAttacks)
        {
            if (!weightedAttacks.Any())
                return; //No attacks possible

            //Divide 50/50 between offense and defense.  Defense armies could still be used for offense if we happen to attack there
            int armiesToOffense = (int)(incomeToUse / 2);
            int armiesToDefense = incomeToUse - armiesToOffense;

            //Find defensive opportunities.
            var defendOpportunities = weightedAttacks.OrderByDescending(o => o.DefenseImportance).ToList();

            AILog.Log("Defend with " + armiesToDefense + ", attack with " + armiesToOffense + ".  Defend ops: " + string.Join(",", defendOpportunities.Select(o => o.DefenseImportance.ToString()).ToArray()));



#if IOS
            //Work around the "attacking to JIT compile method" for below WeightedRandom call
            if (RandomUtility.RandomNumber(2) == -1)
                new List<PossibleAttack>().Select(o => o.DefenseImportance).ToList();
#endif

            if (defendOpportunities.Count > 0)
                for (int i = 0; i < armiesToDefense; i++)
                    TryDeploy(defendOpportunities.WeightedRandom(o => o.DefenseImportance).From, 1);

            var orderedAttacks = weightedAttacks.OrderByDescending(o => o.OffenseImportance);

            AILog.Log("Attacks: " + string.Join(",", orderedAttacks.Select(o => o.OffenseImportance.ToString()).ToArray()));
            AILog.Log(defendOpportunities.Count + " defend ops, " + orderedAttacks.Count() + " attack ops");

            foreach (var attack in orderedAttacks)
            {
                int attackWith = Standing.Territories[attack.To].NumArmies.NumArmies * 2;
                int have = GetArmiesAvailable(attack.From) - 1;
                int need = attackWith - have;

                if (need > armiesToOffense)
                {
                    //We can't swing it. Just deploy the rest and quit. Will try again next round.
                    if (armiesToOffense > 0)
                    {
                        TryDeploy(attack.From, armiesToOffense);
                        armiesToOffense = 0;
                    }
                }
                else
                {
                    //We can attack.  First deploy however many we needed
                    if (need > 0)
                    {
                        if (!TryDeploy(attack.From, need))
                            continue;
                        armiesToOffense -= need;
                        Assert.Fatal(armiesToOffense >= 0);
                    }

                    //Now issue the attack
                    AddAttack(attack.From, attack.To, AttackTransferEnum.AttackTransfer, attackWith, false);
                }
            }
        }

        private void AddAttack(TerritoryIDType from, TerritoryIDType to, AttackTransferEnum attackTransfer, int numArmies, bool attackTeammates)
        {
            IEnumerable<GameOrderAttackTransfer> attacks = Orders.OfType<GameOrderAttackTransfer>();
            var existingFrom = attacks.Where(o => o.From == from).ToList();
            var existingFromTo = existingFrom.Where(o => o.To == to).ToList();

            if (existingFromTo.Any())
                existingFromTo.Single().NumArmies = existingFromTo.Single().NumArmies.Add(new Armies(numArmies));
            else
            {
                var specials = Standing.Territories[from].NumArmies.SpecialUnits;
                if (specials.Length > 0)
                {
                    var used = existingFrom.SelectMany(o => o.NumArmies.SpecialUnits).Select(o => o.ID).ToHashSet(false);
                    specials = specials.Where(o => used.Contains(o.ID) == false).ToArray();
                }

                AddOrder(GameOrderAttackTransfer.Create(this.Player, from, to, attackTransfer, false, new Armies(numArmies, false, specials), attackTeammates));
            }
        }

        private IEnumerable<PossibleAttack> WeightAttacks()
        {

            var weightedNeighbors = WeightNeighbors();
            //build all possible attacks
            List<PossibleAttack> ret = Territories
                .Where(o => o.OwnerPlayerID == this.Player)
                .SelectMany(us =>
                    this.Map.Territories[us.ID].ConnectedTo
                    .Select(k => this.Standing.Territories[k])
                    .Where(k => k.OwnerPlayerID != TerritoryStanding.NeutralPlayerID && !this.IsTeammateOrUs(k.OwnerPlayerID))
                    .Select(k => new PossibleAttack(this, us.ID, k.ID))).ToList();


            //foreach (PossibleAttack a in ret)
            //WeightAttack(a, weightedNeighbors);

            //NormalizeWeights(ret);

            return ret;
        }

        //private void WeightAttack(PossibleAttack possibleAttack, Dictionary<PlayerIDType, int> weightedNeighbors)
        //{
        //    
        //    var opponentID = Standing.Territories.GetValue(possibleAttack.To).OwnerPlayerID;

        //    Assert.Fatal(opponentID != TerritoryStanding.NeutralPlayerID);
        //    Assert.Fatal(!Game.IsTeammateOrUs(opponentID));
        //    Assert.Fatal(weightedNeighbors.ContainsKey(opponentID));

        //    //Seed the border weight with a lessened neighbor weight
        //    possibleAttack.DefenseImportance = possibleAttack.OffenseImportance = weightedNeighbors.GetValue(opponentID) / 10.0;

        //    //Are we defending a bonus we control?
        //    foreach (var defendingBonus in Map.Territories.GetValue(possibleAttack.From).PartOfBonuses
        //        .Select(b => this.Map.Bonuses.GetValue(b))
        //        .Where(b => this.PlayerControlsBonus(b)
        //                    && this.Map.Territories.GetValue(possibleAttack.To).PartOfBonuses
        //                    .Select(b2 => this.Map.Bonuses.GetValue(b2))
        //                    .AnyWhere(b2 => !this.PlayerControlsBonus(b2))))
        //    {
        //        //Defend importance is bonus value * 10
        //        possibleAttack.DefenseImportance += Game.Game.BonusValue(defendingBonus.ID) * 10.0;
        //    }

        //    //Would attacking break an opponents bonus?
        //    foreach (var attackingBonus in Map.Territories.GetValue(possibleAttack.To).PartOfBonuses
        //        .Select(b => this.Map.Bonuses.GetValue(b))
        //        .Where(b => { var c = b.ControlsBonus(this.Standing);  return c.HasValue && c.Value != this.Player; }))
        //    {
        //        //Attack importance is bonus value * 4, lower to be nicer
        //        possibleAttack.OffenseImportance += Game.Game.BonusValue(attackingBonus.ID) * 4.0;
        //    }

        //    //How is our current ratio
        //    int ourArmies = Standing.Territories.GetValue(possibleAttack.From).NumArmies.NumArmies;
        //    int theirArmies = Standing.Territories.GetValue(possibleAttack.To).NumArmies.NumArmies;
        //    double ratio = (double)theirArmies / (double)ourArmies;

        //    if (ourArmies + theirArmies < 10)
        //        ratio = 1; //Small numbers change so rapidly anyway that we just consider it equal.

        //    possibleAttack.OffenseImportance *= ratio;
        //    possibleAttack.DefenseImportance *= ratio;

        //    //Fuzz them so equal importance actions are not consistent
        //    //possibleAttack.OffenseImportance = (possibleAttack.OffenseImportance * 100) + RandomUtility.RandomNumber(100);
        //    //possibleAttack.DefenseImportance = (possibleAttack.DefenseImportance * 100) + RandomUtility.RandomNumber(100);

        //    //Console.WriteLine("Returning " + possibleAttack.OffenseImportance + "," + possibleAttack.DefenseImportance);
        //}

        //private void NormalizeWeights(IEnumerable<PossibleAttack> attacks)
        //{
        //    if (!attacks.Any())
        //        return;

        //    var min = attacks.Select(o => Math.Min(o.OffenseImportance, o.DefenseImportance)).Min();
        //    foreach (var a in attacks)
        //    {
        //        a.DefenseImportance += min + 1;
        //        a.OffenseImportance += min + 1;
        //    }
        //}

        private Dictionary<PlayerIDType, int> WeightNeighbors()
        {

            var ret = Neighbors.Values
                .Where(o => !this.IsTeammateOrUs(o.ID)) //Exclude teammates
                .Where(o => o.ID != TerritoryStanding.NeutralPlayerID) //Exclude neutral
                .ToDictionary(o => o.ID, o => o.NeighboringTerritories.Select(n => n.NumArmies.NumArmies).Sum());  //Sum each army they have on our borders as the initial weight

            foreach (var borderTerr in BorderTerritories)
            {
                //Subtract one weight for each defending army we have next to that player

                Map.Territories[borderTerr.ID].ConnectedTo
                    .Select(o => this.Standing.Territories[o])
                    .Where(o => !this.IsTeammateOrUs(o.OwnerPlayerID)) //Ignore our own and teammates
                    .Select(o => o.OwnerPlayerID)
                    .Where(o => o != TerritoryStanding.NeutralPlayerID)
                    .Distinct()
                    .ForEach(o => ret[o] = ret[o] - this.Standing.Territories[borderTerr.ID].NumArmies.NumArmies);
            }

            return ret;
        }

        private void Deploy(TerritoryIDType terr, int armies)
        {
            if (!TryDeploy(terr, armies))
                throw new Exception("Deploy failed.  Territory=" + terr + ", armies=" + armies + ", us=" + Player + ", Income=" + Income.ToString() + ", IncomeTrakcer=" + IncomeTracker.ToString());
        }

        private bool TryDeploy(TerritoryIDType terrID, int armies)
        {
            Assert.Fatal(Standing.Territories[terrID].OwnerPlayerID == Player);
            Assert.Fatal(armies > 0);

            if (!IncomeTracker.TryRecordUsedArmies(terrID, armies))
                return false;

            IEnumerable<GameOrderDeploy> deploys = Orders.OfType<GameOrderDeploy>();
            GameOrderDeploy existing = deploys.FirstOrDefault(o => o.DeployOn == terrID);

            if (existing != null)
                existing.NumArmies += armies;
            else
                AddOrder(GameOrderDeploy.Create(armies, Player, terrID));

            return true;
        }

        private void Expand(int useArmies, int minWeight)
        {
            AILog.Log("Expand called with useArmies=" + useArmies + ", minWeight=" + minWeight);

            if (useArmies == 0)
                return;


            int armiesToExpandWithRemaining = useArmies;

            AILog.Log("Finding attackable neutrals");

            var attackableNeutrals = AttackableTerritories.Where(o => o.IsNeutral).ToDictionary(o => o.ID, o => 0);

            AssignExpansionWeights(attackableNeutrals);

            AILog.Log("Before filter, " + Player + "'s " + useArmies + " armies got " + attackableNeutrals.Count + " attackable spots with weights " + string.Join(" ; ", attackableNeutrals.Select(o => o.ToString()).ToArray()));


            //Sort by weight
            var expandToList = attackableNeutrals
                .Where(o => o.Value > minWeight) //Don't bother with anything less than the min weight
                .OrderByDescending(o => o.Value)
                .Select(o => o.Key)
                .ToList();

            AILog.Log(Player + " Got " + expandToList.Count + " items over weight " + minWeight + ", top finals are " + string.Join(",", expandToList.Take(4).Select(o => o.ToString()).ToArray()));

            foreach (var expandTo in expandToList)
            {
                //If we've already attacked this, quit
                if (Orders.OfType<GameOrderAttackTransfer>().Any(o => o.To == expandTo))
                    continue;

                int attackWith = SharedUtility.MathMax(1, Standing.Territories[expandTo].NumArmies.DefensePower * 2);

                AILog.Log("Figuring out where to attack from");

                var attackFrom = Map.Territories[expandTo].ConnectedTo
                    .Select(o => this.Standing.Territories[o])
                    .Where(o => o.OwnerPlayerID == this.Player)
                    .ToDictionary(o => o.ID, o => this.GetArmiesAvailable(o.ID))
                    .OrderByDescending(o => o.Value).First();

                AILog.Log("Attacking from " + attackFrom);

                int armiesNeeded = attackWith - attackFrom.Value + 1; //Add one since one army must remain

                if (armiesNeeded > armiesToExpandWithRemaining)
                    break; //Can't manage it, stop expanding.  TODO: We should still continue looking at other opportunities, since we may already have enough armies on hand.

                //Deploy if needed
                if (armiesNeeded > 0)
                {
                    armiesToExpandWithRemaining -= armiesNeeded;
                    if (!TryDeploy(attackFrom.Key, armiesNeeded))
                        continue;
                }

                //Attack
                AddAttack(attackFrom.Key, expandTo, AttackTransferEnum.AttackTransfer, attackWith, true); //TODO: Why is it attacking teammates here?  It shouldn't.
            }

        }

        /// <summary>
        /// Tells us how many armies of ours on our territory that we haven't committed to another action
        /// </summary>
        /// <param name="terrID"></param>
        /// <returns></returns>
        private int GetArmiesAvailable(TerritoryIDType terrID)
        {
            Assert.Fatal(Standing.Territories[terrID].OwnerPlayerID == Player);
            int armies = Standing.Territories[terrID].NumArmies.NumArmies;
            IEnumerable<GameOrderDeploy> deploys = Orders.OfType<GameOrderDeploy>();
            IEnumerable<GameOrderAttackTransfer> attacks = Orders.OfType<GameOrderAttackTransfer>();

            //Add in armies we deployed
            armies += deploys.Where(o => o.DeployOn == terrID).Select(o => o.NumArmies).Sum();

            //Subtract armies we've attacked with
            armies -= attacks.Where(o => o.From == terrID).Select(o => o.NumArmies.NumArmies).Sum();

            //TODO: Cards (subtract airlift out's, return 0 if abandoning)

            //TODO: Subtract 1, since one must remain

            return armies;
        }


        private void AssignExpansionWeights(Dictionary<TerritoryIDType, int> attackableNeutrals)
        {
            AILog.Log("AssignExpansionWeights called with " + attackableNeutrals.Count + " neutrals");

            foreach (var attackableNeutral in attackableNeutrals.Keys.ToList())
            {
                int weight = 0;

                foreach (var bonusID in Map.Territories[attackableNeutral].PartOfBonuses)
                {
                    int bonusValue = BonusValue(bonusID);

                    if (bonusValue == 0)
                        continue; //Don't even consider bonuses with no worth
                    if (bonusValue < 0)
                        weight -= 50; //Don't want negative bonuses

                    //Is it part of a bonus? Add ArmiesPerTurn * 3.  TODO: We should weight income higher in big FFA games
                    weight += 3 * bonusValue;

                    //How many territories do we need to take to get it? Subtract one weight for each army standing in our way
                    foreach (var terrInBonus in Map.Bonuses[bonusID].Territories)
                    {
                        var ts = Standing.Territories[terrInBonus];

                        if (ts.OwnerPlayerID == Player)
                            continue; //Already own it
                        else if (ts.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                            weight -= Settings.InitialNonDistributionArmies; //assume neutral on fogged items
                        else if (IsTeammateOrUs(ts.OwnerPlayerID))
                            weight -= ts.NumArmies.NumArmies * 4; //Teammate in it
                        else if (ts.IsNeutral)
                            weight -= ts.NumArmies.NumArmies; //Neutral in it
                        else
                            weight -= ts.NumArmies.NumArmies * 2; //Opponent in it - expansion less likely
                    }
                }


                attackableNeutrals[attackableNeutral] = weight;


            }
        }

    }
}

