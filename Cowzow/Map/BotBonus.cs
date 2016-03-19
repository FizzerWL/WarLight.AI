/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;

namespace WarLight.Shared.AI.Cowzow.Map
{
    public class BotBonus
    {
        public readonly int ArmiesReward;
        public readonly BonusIDType ID;

        public CowzowBot Bot;

        public BotBonus(CowzowBot bot, BonusIDType id)
        {
            this.Bot = bot;
            this.ID = id;
            this.ArmiesReward = bot.Settings.OverriddenBonuses.ContainsKey(id) ? bot.Settings.OverriddenBonuses[id] : Details.Amount;
        }

        public BonusDetails Details
        {
            get { return Bot.Map.Bonuses[ID]; }
        }

        public List<BotTerritory> Territories
        {
            get { return Details.Territories.Select(o => Bot.BotMap.Territories[o]).ToList(); }
        }

        //Looks like this isn't used anywhere?
        //public int CompareTo(BotBonus o)
        //{
        //    if (GuessedArmiesNotOwnedByUs < o.GuessedArmiesNotOwnedByUs)
        //        return -1;
        //    if (GuessedArmiesNotOwnedByUs > o.GuessedArmiesNotOwnedByUs)
        //        return 1;
        //    return 0;
        //}
        

        public bool MightBeOwnedByOpponent()
        {
            var terrs = this.Territories.Where(o => o.IsVisible).ToList();

            if (terrs.Count == 0)
                return true;

            var oppID = terrs[0].OwnerPlayerID;
            if (!Bot.IsOpponent(oppID))
                return false;

            foreach (var r in terrs.Skip(1))
                if (r.OwnerPlayerID != oppID)
                    return false;

            return true;
        }
        
        public bool IsUnvisible()
        {
            foreach (var r in Territories)
                if (r.IsVisible)
                    return false;
            return true;
        }
        

        public int EnemyArmiesNotOwnedByUs()
        {
            var troopCount = 0;
            foreach (var r in Territories)
                if (r.OwnerPlayerID == Bot.Me.ID || r.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                    troopCount += r.Armies;
                else
                {
                    //if (Bot.Wastelands.Contains(r))
                    //    troopCount += 10;
                }
            return troopCount;
        }


        public Dictionary<TerritoryIDType, BotTerritory> GetVisibleEnemyTerritories()
        {
            var enemies = new Dictionary<TerritoryIDType, BotTerritory>();
            foreach (var r in Territories)
                if (r.OwnerPlayerID != Bot.Me.ID && r.IsVisible)
                    enemies.Add(r.ID, r);
            return enemies;
        }

        public Dictionary<TerritoryIDType, BotTerritory> GetMyTerritories()
        {
            var myTerritories = new Dictionary<TerritoryIDType, BotTerritory>();
            foreach (var r in Territories)
                if (r.OwnerPlayerID == Bot.Me.ID)
                    myTerritories.Add(r.ID, r);
            return myTerritories;
        }

        public int GuessedArmiesNotOwnedByUs
        {
            get
            {
                return this.Territories.Sum(o => o.GuessedArmiesNotOwnedByUs);
            }
        }
        
        public double ArmiesToNeutralsRatio
        {
            get
            {
                var neutrals = GuessedArmiesNotOwnedByUs;

                if (neutrals == 0)
                    neutrals = 1; //avoid divide by zero

                return (double)ArmiesReward / (double)neutrals;
            }
        }

        public override string ToString()
        {
            return "Bonus " + Details.Name + " [id=" + ID + ", armiesReward=" + ArmiesReward + ", GuessedArmiesNotOwnedByUs=" + GuessedArmiesNotOwnedByUs + ", Ratio= " + ArmiesToNeutralsRatio + "]";
        }

        public BotTerritory GetWeakestTerritory()
        {
            BotTerritory result = null;
            foreach (var r in Territories)
                if (r.IsVisible && (result == null || r.Armies < result.Armies))
                    result = r;
            return result;
        }

        public bool IsSafe()
        {
            var terrsAndNeighbors = new HashSet<TerritoryIDType>();
            foreach (var r in Territories)
            {
                terrsAndNeighbors.Add(r.ID);
                foreach (var n in r.Neighbors)
                    terrsAndNeighbors.Add(n.ID);
            }

            foreach (var terrID in terrsAndNeighbors)
            {
                var terr = Bot.BotMap.Territories[terrID];
                if (Bot.IsOpponent(terr.OwnerPlayerID))
                    return false;
                //if (!terr.IsVisible && State.OpponentStartingTerritories.Contains(terrID))
                //    return false;
            }

            return true;
        }
    }
}
