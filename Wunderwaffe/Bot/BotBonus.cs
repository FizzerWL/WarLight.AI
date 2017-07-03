using System;
using System.Collections.Generic;
using System.Linq;

namespace WarLight.Shared.AI.Wunderwaffe.Bot
{
    public class BotBonus
    {
        public BotMap Parent;

        public BonusDetails Details
        {
            get { return BotState.Map.Bonuses[ID]; }
        }

        public int Amount
        {
            get
            {
                if (BotState.Settings.OverriddenBonuses.ContainsKey(this.ID))
                    return BotState.Settings.OverriddenBonuses[ID];
                else
                    return Details.Amount;
            }
        }

        public BonusIDType ID;
        public int AttackValue = 0;
        public int TakeOverValue = 0;
        public int PreventTakeOverValue = 0;
        public PlayerIDType? PreventTakeOverOpponent;
        public int ExpansionValueCategory = 0;
        public int DefenseValue = 0;
        public double ExpansionValue = 0;

        public BotBonus(BotMap parent, BonusIDType id)
        {
            this.Parent = parent;
            this.ID = id;
        }

        public BotMain BotState { get { return Parent.BotState; } }

        public int GetExpansionValue()
        {
            // TODO hack
            if (ExpansionValue == 0)
            {
                SetMyExpansionValueHeuristic();
            }
            return (int)ExpansionValue;
        }


        public void SetMyExpansionValueHeuristic()
        {
            this.ExpansionValue = BotState.BonusExpansionValueCalculator.GetExpansionValue(this, true);

        }


        /// <returns>A list with the Territories that are part of this Bonus</returns>
        public List<BotTerritory> Territories
        {
            get
            {
                return Details.Territories.Select(o => Parent.Territories[o]).Distinct().ToList();
            }
        }

        public List<BotTerritory> NeutralTerritories
        {
            get
            {
                List<BotTerritory> outvar = new List<BotTerritory>();
                foreach (var territory in this.Territories)
                {
                    if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID || territory.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                        outvar.Add(territory);
                }
                return outvar;
            }
        }

        public Armies NeutralArmies
        {
            get
            {
                var outvar = new Armies(0);
                foreach (var terr in this.NeutralTerritories)
                    outvar = outvar.Add(terr.Armies);
                return outvar;
            }
        }

        public List<BotBonus> GetNeighborBonuses()
        {
            //var x = this.Territories.SelectMany(o => o.Neighbors);
            return this.Territories.SelectMany(o => o.Neighbors).SelectMany(o => o.Bonuses).Where(o => o.ID != this.ID).Distinct().ToList();
        }

        public List<BotTerritory> GetOpponentTerritories()
        {
            var outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                if (BotState.IsOpponent(territory.OwnerPlayerID))
                    outvar.Add(territory);
            }
            return outvar;
        }

        public List<BotTerritory> GetOwnedTerritories()
        {
            var outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories)
                if (territory.OwnerPlayerID == BotState.Me.ID)
                    outvar.Add(territory);
            return outvar;
        }

        public int DistanceFrom(Func<BotTerritory, bool> pred)
        {
            if (this.Territories.Any(pred))
                return 0;
            var terrs = this.Territories.Select(o => o.ID).ToHashSet(true);

            int distance = 1;

            while (true)
            {
                var next = terrs.SelectMany(o => BotState.Map.Territories[o].ConnectedTo.Keys).Where(o => terrs.Contains(o) == false).ToList();

                if (next.Count == 0)
                    return int.MaxValue;

                if (next.Any(o => pred(Parent.Territories[o])))
                    return distance;
                terrs.AddRange(next);

                distance++;
            }
#if CSSCALA
            throw new Exception("Never");
#endif
        }

        public List<BotTerritory> GetOwnedTerritoriesAndNeighbors()
        {
            var territoriesToConsider = new HashSet<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                territoriesToConsider.Add(territory);
                territoriesToConsider.AddRange(territory.Neighbors);
            }
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var territoryToConsider in territoriesToConsider)
            {
                if (territoryToConsider.OwnerPlayerID == BotState.Me.ID)
                    outvar.Add(territoryToConsider);
            }
            return outvar;
        }

        public List<BotTerritory> GetOwnedNeighborTerritories()
        {
            var territoriesToConsider = new HashSet<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                var neighbors = territory.GetOwnedNeighbors();
                foreach (var neighbor in neighbors)
                {
                    if (!neighbor.Details.PartOfBonuses.Contains(this.ID))
                        territoriesToConsider.Add(neighbor);
                }
            }
            var outvar = new List<BotTerritory>();
            outvar.AddRange(territoriesToConsider);
            return outvar;
        }

        public bool ContainsOwnPresence()
        {
            return this.GetOwnedTerritories().Count > 0;
        }

        public bool ContainsTeammatePresence()
        {
            return this.Territories.Any(o => BotState.IsTeammate(o.OwnerPlayerID));
        }

        public bool ContainsOpponentPresence()
        {
            return this.Territories.Any(o => BotState.IsOpponent(o.OwnerPlayerID));
        }

        public bool ContainsPresenseBy(PlayerIDType playerID)
        {
            return this.Territories.Any(o => o.OwnerPlayerID == playerID);
        }

        public bool AreAllTerritoriesVisibleToOpponent(PlayerIDType opponentID)
        {
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID != opponentID && territory.GetOpponentNeighbors().Count == 0)
                    return false;
            }
            return true;
        }

        public bool AreAllTerritoriesVisible()
        {
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID != BotState.Me.ID && territory.GetOwnedNeighbors().Count == 0)
                    return false;
            }
            return true;
        }

        public List<BotTerritory> GetVisibleOpponentTerritories()
        {
            var outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                if (BotState.IsOpponent(territory.OwnerPlayerID))
                    outvar.Add(territory);
            }
            return outvar;
        }

        public List<BotTerritory> GetVisibleNeutralTerritories()
        {
            var outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID && territory.GetOwnedNeighbors().Count > 0)
                    outvar.Add(territory);
            }
            return outvar;
        }

        public List<BotTerritory> GetNotOwnedTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID != BotState.Me.ID)
                    outvar.Add(territory);
            }
            return outvar;
        }

        public bool CanOpponentTakeOver()
        {
            if (this.IsOwnedByAnyOpponent())
                return false;
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID || (territory.OwnerPlayerID == BotState.Me.ID && territory.GetOpponentNeighbors().Count == 0))
                    return false;
            }
            return true;
        }

        public bool CanTakeOver()
        {
            if (this.IsOwnedByMyself())
                return false;
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID != BotState.Me.ID && territory.GetOwnedNeighbors().Count == 0)
                    return false;
            }
            return true;
        }

        public bool IsOwnedByAnyOpponent()
        {
            var terrs = this.Territories;
            if (terrs.Count == 0)
                return false;
            foreach (var territory in terrs.Skip(1))
                if (territory.OwnerPlayerID != terrs[0].OwnerPlayerID || BotState.IsOpponent(territory.OwnerPlayerID) == false)
                    return false;
            return true;
        }

        public bool IsOwnedByOpponent(PlayerIDType opponentID)
        {
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID != opponentID)
                    return false;
            }
            return true;
        }

        public bool CanBeOwnedByOpponent(PlayerIDType opponentID)
        {
            foreach (var territory in this.Territories)
            {
                if (territory.IsVisible && territory.OwnerPlayerID != opponentID)
                    return false;
            }
            return true;
        }


        public bool CanBeOwnedByOpponent()
        {
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID == BotState.Me.ID || (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID && territory.GetOwnedNeighbors().Count > 0))
                    return false;
            }
            return true;
        }

        /// <summary>Calculates the opponent owned neighbor territories that aren't part of this Bonus.
        /// </summary>
        /// <remarks>Calculates the opponent owned neighbor territories that aren't part of this Bonus.
        /// </remarks>
        /// <returns></returns>
        public List<BotTerritory> GetOpponentNeighbors()
        {
            var outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories)
            {
                foreach (var opponentNeighbor in territory.GetOpponentNeighbors())
                {
                    if (!opponentNeighbor.Details.PartOfBonuses.Contains(this.ID))
                        outvar.Add(opponentNeighbor);
                }
            }
            return outvar;
        }

        public List<BotTerritory> GetOwnedTerritoriesBorderingNeighborsOwnedByOpponent()
        {
            return this.GetOwnedTerritories().Where(owned => owned.Neighbors.Any(z => BotState.IsOpponent(z.OwnerPlayerID))).ToList();
        }

        public List<BotTerritory> GetOwnedTerritoriesBorderingOpponent()
        {
            return this.GetOwnedTerritories().Where(owned => owned.Neighbors.Any(z => BotState.IsOpponent(z.OwnerPlayerID))).ToList();
        }



        public List<BotTerritory> GetOwnedTerritoriesBorderingNeighborsOwnedBy(PlayerIDType playerID)
        {
            return this.GetOwnedTerritories().Where(owned => owned.Neighbors.Any(z => z.OwnerPlayerID == playerID)).ToList();
        }


        public List<BotTerritory> GetOpponentTerritoriesBorderingOwnedNeighbors()

        {
            var outvar = new List<BotTerritory>();
            foreach (var opponentTerritory in this.GetOpponentTerritories())
            {
                if (opponentTerritory.GetOwnedNeighbors().Count > 0)
                    outvar.Add(opponentTerritory);
            }
            return outvar;
        }


        public bool IsOwnedByMyself()
        {
            var isOwnedByMyself = true;
            foreach (var territory in this.Territories)
            {
                if (territory.OwnerPlayerID != BotState.Me.ID)
                    isOwnedByMyself = false;
            }
            return isOwnedByMyself;
        }

        public bool IsPlayerPresent(PlayerIDType playerID)
        {
            if (playerID == BotState.Me.ID && this.ContainsOwnPresence())
                return true;
            if (BotState.IsOpponent(playerID) && this.ContainsOpponentPresence())
                return true;
            return false;
        }

        public override string ToString()
        {
            return "ID = " + this.ID + " " + Details.Name + ", ArmiesReward = " + this.Amount;
        }
    }
}
