/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;

namespace WarLight.Shared.AI.Cowzow.Map
{
    public class BotMap
    {
        public Dictionary<TerritoryIDType, BotTerritory> Territories;
        public Dictionary<BonusIDType, BotBonus> Bonuses;

        public CowzowBot Bot;

        public BotMap(CowzowBot bot, MapDetails map, GameStanding latest)
        {
            this.Bot = bot;
            Territories = new Dictionary<TerritoryIDType, BotTerritory>();
            Bonuses = new Dictionary<BonusIDType, BotBonus>();

            foreach (var terr in map.Territories.Values)
            {
                var t = new BotTerritory(bot, terr.ID);
                this.Territories.Add(terr.ID, t);

                var ts = latest.Territories[terr.ID];
                t.OwnerPlayerID = ts.OwnerPlayerID;
                if (!ts.NumArmies.Fogged)
                    t.Armies = ts.NumArmies.NumArmies;
            }

            foreach (var bonus in map.Bonuses.Values)
                this.Bonuses.Add(bonus.ID, new BotBonus(bot, bonus.ID));
        }
        

        /// <summary>add a Territory to the map</summary>
        /// <param name="territory">: Territory to be added</param>
        public void Add(BotTerritory territory)
        {
            if (Territories.ContainsKey(territory.ID))
                throw new Exception("Territory cannot be added: id " + territory.ID + " already exists.");
            Territories[territory.ID] = territory;
        }

        /// <summary>add a Bonus to the map</summary>
        /// <param name="bonus">: Bonus to be added</param>
        public void Add(BotBonus bonus)
        {
            if (Bonuses.ContainsKey(bonus.ID))
                throw new Exception("Bonus cannot be added. Id: " + bonus.ID + " already exists.");
            Bonuses[bonus.ID] = bonus;
        }
        

        public IEnumerable<BotTerritory> GetUnfriendlyTerritories()
        {
            return VisibleTerritories.Where(o => Bot.IsTeammateOrUs(o.OwnerPlayerID) == false);
        }

        public IEnumerable<BotTerritory> VisibleTerritories
        {
            get
            {
                return this.Territories.Values.Where(o => o.IsVisible);
            }
        }
        
        /// <param name="id">: a Territory id number</param>
        /// <returns>: the matching Territory object</returns>
        public BotTerritory GetTerritory(TerritoryIDType id)
        {
            var result = Territories[id];
            if (result == null)
                throw new Exception("Could not find territory with id: " + id);
            return result;
        }

        /// <param name="id">: a Bonus id number</param>
        /// <returns>: the matching Bonus object</returns>
        public BotBonus GetBonus(BonusIDType id)
        {
            return Bonuses[id];
        }
        
    }
}
