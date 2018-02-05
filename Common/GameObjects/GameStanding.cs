using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameStanding
    {
        public GameStanding()
        {
            this.Territories = new Dictionary<TerritoryIDType, TerritoryStanding>();
        }
        public GameStanding(IEnumerable<TerritoryStanding> terrs)
        {
            this.Territories = terrs.ToDictionary(o => o.ID, o => o);
        }

        public Dictionary<TerritoryIDType, TerritoryStanding> Territories;
        public List<ActiveCard> ActiveCards = new List<ActiveCard>();

        public GameStanding Clone()
        {
            var r = new GameStanding();
            r.Territories = this.Territories.ToDictionary(o => o.Key, o => o.Value.Clone());
            r.ActiveCards = this.ActiveCards.Select(o => o.Clone()).ToList();
            return r;
        }

        public int NumResources(PlayerIDType playerID, ResourceType type)
        {
            return 0; //not implemented
        }

    }
}
