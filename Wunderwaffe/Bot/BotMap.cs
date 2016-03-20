using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Wunderwaffe.Bot
{
    public class BotMap
    {
        public Dictionary<TerritoryIDType, BotTerritory> Territories;
        public Dictionary<BonusIDType, BotBonus> Bonuses;

        public BotMain BotState;

        public BotMap(BotMain state)
        {
            this.Territories = new Dictionary<TerritoryIDType, BotTerritory>();
            this.Bonuses = new Dictionary<BonusIDType, BotBonus>();
            this.BotState = state;
        }
        
        /// <returns>: a new Map object exactly the same as this one</returns>
        public BotMap GetMapCopy()
        {
            var newMap = new BotMap(this.BotState);
            foreach (var bonus in Bonuses.Values)
            {
                var newBonus = new BotBonus(newMap, bonus.ID);
                newBonus.ExpansionValueCategory = bonus.ExpansionValueCategory;
                newMap.Bonuses.Add(newBonus.ID, newBonus);
            }
            foreach (var territory in Territories.Values)
            {
                var newTerritory = new BotTerritory(newMap, territory.ID, territory.OwnerPlayerID, territory.Armies);
                newTerritory.IsOwnershipHeuristic = territory.IsOwnershipHeuristic;
                newTerritory.ExpansionTerritoryValue = territory.ExpansionTerritoryValue;
                newTerritory.AttackTerritoryValue = territory.AttackTerritoryValue;
                newTerritory.DefenceTerritoryValue = territory.DefenceTerritoryValue;
                newTerritory.Armies = territory.Armies;
                newMap.Territories.Add(newTerritory.ID, newTerritory);
            }
            return newMap;
        }

        public virtual List<BotTerritory> GetNeutralTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories.Values)
            {
                if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                {
                    outvar.Add(territory);
                }
            }
            return outvar;
        }

        public List<BotTerritory> VisibleOpponentTerritories(PlayerIDType opponentID)
        {
            return this.OpponentTerritories(opponentID).Where(o => o.IsVisible).ToList();
        }

        public List<BotTerritory> OpponentTerritories(PlayerIDType opponentID)
        {
            return this.Territories.Values.Where(o => o.OwnerPlayerID == opponentID).ToList();
        }
        public List<BotTerritory> AllOpponentTerritories
        {
            get
            {
                return this.Territories.Values.Where(o => BotState.IsOpponent(o.OwnerPlayerID)).ToList();
            }
        }

        public static BotMap FromStanding(BotMain state, GameStanding stand)
        {
            Assert.Fatal(stand != null, "stand is null");

            var map = state.VisibleMap.GetMapCopy();
            foreach (var terr in stand.Territories.Values)
            {
                var territory = map.Territories[terr.ID];
                territory.OwnerPlayerID = terr.OwnerPlayerID;
                territory.Armies = terr.NumArmies;
            }
            return map;
        }

        public virtual string MapString
        {
            get
            {
                var mapString = new StringBuilder();
                foreach (var territory in Territories.Values)
                {
                    mapString.Append(territory.ID + ";" + territory.OwnerPlayerID + ";" + territory.Armies + " ");
                }
                return mapString.ToString();
            }
        }

        public virtual List<BotTerritory> GetOwnedTerritories()
        {
            List<BotTerritory> ownedTerritories = new List<BotTerritory>();
            foreach (var territory in this.Territories.Values)
                if (territory.OwnerPlayerID == BotState.Me.ID)
                    ownedTerritories.Add(territory);

            return ownedTerritories;
        }

        public virtual List<BotTerritory> GetVisibleNeutralTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories.Values)
            {
                // TODO changed bug
                if (territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID && territory.GetOwnedNeighbors().Count > 0)
                {
                    outvar.Add(territory);
                }
            }
            return outvar;
        }

        public virtual List<BotTerritory> GetOpponentBorderingTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var ownedTerritories = GetOwnedTerritories();
            foreach (var ownedTerritory in ownedTerritories)
            {
                if (ownedTerritory.GetOpponentNeighbors().Count > 0)
                {
                    outvar.Add(ownedTerritory);
                }
            }
            return outvar;
        }

        public virtual List<BotTerritory> GetNonOwnedTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var territory in this.Territories.Values)
            {
                if (territory.OwnerPlayerID != BotState.Me.ID)
                    outvar.Add(territory);
            }
            return outvar;
        }

        public virtual List<BotBonus> GetOwnedBonuses()
        {
            List<BotBonus> outvar = new List<BotBonus>();
            foreach (var bonus in this.Bonuses.Values)
                if (bonus.IsOwnedByMyself())
                    outvar.Add(bonus);
            return outvar;
        }

        public virtual List<BotTerritory> GetBorderTerritories()
        {
            var ownedTerritories = GetOwnedTerritories();
            List<BotTerritory> borderTerritories = new List<BotTerritory>();
            foreach (var ownedTerritory in ownedTerritories)
            {
                var isBorderTerritory = false;
                var neighbors = ownedTerritory.Neighbors;
                foreach (var neighbor in neighbors)
                    if (neighbor.OwnerPlayerID != BotState.Me.ID)
                        isBorderTerritory = true;
                if (isBorderTerritory)
                    borderTerritories.Add(ownedTerritory);
            }
            return borderTerritories;
        }

        public virtual List<BotTerritory> SortTerritoriesDistanceToBorder(List<BotTerritory> inTerritories)
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var copy = new List<BotTerritory>();
            copy.AddRange(inTerritories);
            while (copy.Count != 0)
            {
                var lowestDistanceTerritory = copy[0];
                foreach (var territory in copy)
                {
                    if (territory.DistanceToBorder < lowestDistanceTerritory.DistanceToBorder)
                    {
                        lowestDistanceTerritory = territory;
                    }
                }
                outvar.Add(lowestDistanceTerritory);
                copy.Remove(lowestDistanceTerritory);
            }
            return outvar;
        }

        public virtual List<BotTerritory> GetNonOpponentBorderingBorderTerritories()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            var borderTerritories = this.GetBorderTerritories();
            foreach (var borderTerritory in borderTerritories)
                if (borderTerritory.GetOpponentNeighbors().Count == 0)
                    outvar.Add(borderTerritory);
            return outvar;
        }

        public static List<TerritoryIDType> GetTerritoryIDs(List<BotTerritory> territories)
        {
            return territories.Select(o => o.ID).ToList();
        }

        public virtual List<BotTerritory> SortTerritoriesIdleArmies(List<BotTerritory> inTerritories)
        {
            var outvar = new List<BotTerritory>();
            outvar.AddRange(inTerritories);
            var hasSomethingChanged = true;
            while (hasSomethingChanged)
            {
                hasSomethingChanged = false;
                for (var i = 0; i < inTerritories.Count - 1; i++)
                {
                    var territory1 = inTerritories[i];
                    var territory2 = inTerritories[i + 1];
                    if (territory2.GetIdleArmies().AttackPower > territory1.GetIdleArmies().AttackPower)
                    {
                        hasSomethingChanged = true;
                        outvar[i] = territory2;
                        outvar[i + 1] = territory1;
                    }
                }
            }
            return outvar;
        }

        public static List<BotBonus> SortBonusesArmiesReward(List<BotBonus> bonuses)
        {
            var outvar = new List<BotBonus>();
            var copy = new List<BotBonus>();
            copy.AddRange(bonuses);
            while (copy.Count != 0)
            {
                var highestRewardBonus = copy[0];
                foreach (BotBonus bonus in copy)
                {
                    if (bonus.Amount > highestRewardBonus.Amount)
                        highestRewardBonus = bonus;
                }
                copy.Remove(highestRewardBonus);
                outvar.Add(highestRewardBonus);
            }
            return outvar;
        }

        public static List<BotTerritory> GetOrderedListOfTerritoriesByIdleArmies(List<BotTerritory> terrs)
        {
            var outvar = new List<BotTerritory>();
            var copy = new List<BotTerritory>();
            copy.AddRange(terrs);
            while (copy.Count != 0)
            {
                var highestIdleArmiesTerritory = copy[0];
                foreach (var territory in copy)
                {
                    var defenseValueBigger = territory.DefenceTerritoryValue > highestIdleArmiesTerritory.DefenceTerritoryValue;
                    var defenseValueBiggerAndEqualIdles = territory.GetIdleArmies().AttackPower == highestIdleArmiesTerritory.GetIdleArmies().AttackPower && defenseValueBigger;
                    if (territory.GetIdleArmies().AttackPower > highestIdleArmiesTerritory.GetIdleArmies().AttackPower || defenseValueBiggerAndEqualIdles)
                        highestIdleArmiesTerritory = territory;

                }
                copy.Remove(highestIdleArmiesTerritory);
                outvar.Add(highestIdleArmiesTerritory);
            }
            return outvar;
        }

        /// <summary>Returns the according territories from this map to territories from another map.
        /// </summary>
        /// <remarks>Returns the according territories from this map to territories from another map.
        /// </remarks>
        /// <param name="otherMapTerritories"></param>
        /// <returns></returns>
        public virtual List<BotTerritory> CopyTerritories(List<BotTerritory> otherMapTerritories)
        {
            List<BotTerritory> thisMapTerritories = new List<BotTerritory>();
            foreach (var otherMapTerritory in otherMapTerritories)
                thisMapTerritories.Add(this.Territories[otherMapTerritory.ID]);

            return thisMapTerritories;
        }
    }
}
