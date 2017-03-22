using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod
{

    public class Neighbor
    {
        public PlayerIDType ID;
        private BotMain Bot;

        public Neighbor(BotMain bot, PlayerIDType id)
        {
            ID = id;
            Bot = bot;
        }

        public IEnumerable<TerritoryStanding> Territories
        {
            get
            {

                return Bot.Standing.Territories.Values.Where(o => o.OwnerPlayerID == ID);
            }
        }

        public IEnumerable<TerritoryStanding> NeighboringTerritories
        {
            get
            {
                return Territories.Where(o => Bot.Map.Territories[o.ID].ConnectedTo.Keys.Any(c => Bot.Standing.Territories[c].OwnerPlayerID == Bot.PlayerID));
            }
        }
    }

}
