using System;
using System.Linq;
using System.Collections.Generic;


namespace WarLight.Shared.AI
{
    public class TerritoryDetails
    {
        public TerritoryIDType ID;
        public string Name;
        public Dictionary<TerritoryIDType, object> ConnectedTo; //In WarLight's real code, the object contains details about how the connections wrap around the map.  That isn't important for AIs, so it's left out here.
        public HashSet<BonusIDType> PartOfBonuses;

        public MapDetails Parent;
        
        public TerritoryDetails(MapDetails parent, TerritoryIDType id)
        {
            this.Parent = parent;
            this.ID = id;
            this.PartOfBonuses = new HashSet<BonusIDType>();
            this.ConnectedTo = new Dictionary<TerritoryIDType, object>();
        }
        
    }
}
