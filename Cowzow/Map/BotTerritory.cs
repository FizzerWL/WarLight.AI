/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;
using WarLight.Shared.AI.Cowzow.Fulkerson2;

namespace WarLight.Shared.AI.Cowzow.Map
{
    public class BotTerritory
    {
        public CowzowBot Bot;
        public readonly TerritoryIDType ID;
        public int Armies;
        public PlayerIDType OwnerPlayerID;

        public static readonly TerritoryIDType DummyID = (TerritoryIDType)(-69);
        
        public BotTerritory(CowzowBot bot, TerritoryIDType id)
        {
            this.Bot = bot;
            this.ID = id;
            this.OwnerPlayerID = TerritoryStanding.FogPlayerID;
            this.Armies = 0;
        }

        public TerritoryDetails Details
        {
            get { return Bot.Map.Territories[ID]; }
        }

        public List<BotBonus> Bonuses
        {
            get
            {
                return Details.PartOfBonuses.Select(o => Bot.BotMap.Bonuses[o]).ToList();
            }
        }

        public List<BotTerritory> Neighbors
        {
            get
            {
                return Details.ConnectedTo.Keys.Select(o => Bot.BotMap.Territories[o]).ToList();
            }
        }

        public HashSet<BonusIDType> NeighboringBonuses
        {
            get
            {
                var ret = new HashSet<BonusIDType>();
                ret.AddRange(Details.PartOfBonuses);
                foreach (var neighbor in Neighbors)
                    ret.AddRange(neighbor.Details.PartOfBonuses);
                return ret;
            }
        }

        public bool IsVisible
        {
            get { return OwnerPlayerID != TerritoryStanding.FogPlayerID; }
        }
        

        /// <param name="territory">a Territory object</param>
        /// <returns>True if this Territory is a neighbor of given Territory, false otherwise</returns>
        public bool IsNeighbor(BotTerritory territory)
        {
            if (Neighbors.Contains(territory))
                return true;
            return false;
        }
        

        public int GetStrongestNearestAlly()
        {
            var strength = 0;
            foreach (var n in Neighbors)
                if (n.IsVisible && n.OwnerPlayerID == Bot.Me.ID && n.Armies > strength)
                    strength = n.Armies;
            return strength;
        }

        public int AdjacentEnemyCount()
        {
            var sum = 0;
            foreach (var r in Neighbors)
                if (Bot.IsOpponent(r.OwnerPlayerID))
                    sum++;
            return sum;
        }

        public int GetStrongestNearestEnemy()
        {
            var strength = 0;
            foreach (var n in Neighbors)
                if (Bot.IsOpponent(n.OwnerPlayerID) && n.Armies > strength)
                    strength = n.Armies;
            return strength;
        }

        public bool IsLandlocked()
        {
            foreach (var n in Neighbors)
                if (n.OwnerPlayerID != Bot.Me.ID)
                    return false;
            return true;
        }

        public bool IsFoothold()
        {
            if (OwnerPlayerID != Bot.Me.ID)
                return false;

            return Bonuses.Any(bonus =>
            {
                foreach (var r in bonus.Territories)
                {
                    if (r.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                        return false;
                    if (r.ID != this.ID && r.OwnerPlayerID == Bot.Me.ID)
                        return false;
                }

                return true;
            });
            
        }

        public BotBonus GetBestAdjacentBonus()
        {
            BotBonus best = null;
            var minArmiesNotOwnedByUs = int.MaxValue;
            foreach (var bonusID in NeighboringBonuses)
            {
                var bonus = Bot.BotMap.Bonuses[bonusID];

                var ArmiesNotOwnedByUs = bonus.GuessedArmiesNotOwnedByUs;
                if (best == null || ArmiesNotOwnedByUs < minArmiesNotOwnedByUs)
                {
                    minArmiesNotOwnedByUs = ArmiesNotOwnedByUs;
                    best = bonus;
                }
                else if (minArmiesNotOwnedByUs == ArmiesNotOwnedByUs && bonus.ArmiesReward > best.ArmiesReward)
                    best = bonus;
            }
            return best;
        }


        public override string ToString()
        {
            
            return "Territory " + (ID == DummyID ? "dummy" : Details.Name) + " id=" + ID + ", armies=" + Armies + " ownedBy=" + OwnerPlayerID;
        }

        /// <summary>Get all the edges from this territory to its neighbors</summary>
        public IEnumerable<Edge> GetFromPaths()
        {
            var result = new List<Edge>();
            foreach (var n in Neighbors)
                if (n.OwnerPlayerID != Bot.Me.ID)
                    result.Add(new Edge(this, n, 0));
            return result;
        }

        public IEnumerable<Edge> GetAttackPaths()
        {
            var paths = new List<Edge>();
            foreach (var r in Neighbors)
                if (r.OwnerPlayerID == Bot.Me.ID)
                    paths.Add(new Edge(r, this, r.Armies - Bot.Settings.OneArmyMustStandGuardOneOrZero));
            return paths;
        }

        public int GuessedArmiesNotOwnedByUs
        {
            get
            {
                if (Bot.IsTeammateOrUs(OwnerPlayerID))
                    return 0;

                if (IsVisible)
                    return Armies;
                else if (Bot.DistributionStanding != null)
                    return Bot.DistributionStanding.Territories[ID].NumArmies.DefensePower;
                else
                    return Bot.Settings.InitialNonDistributionArmies;
            }
        }
        
        //public double GetPropScore()
        //{
        //    var score = (double)Bonus.ArmiesReward * Armies / Bonus.ArmiesNotOwnedByUs();
        //    return score;
        //}
    }
}
