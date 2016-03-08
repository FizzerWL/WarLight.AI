using System;
using System.Linq;
using System.Collections.Generic;


namespace WarLight.Shared.AI
{
    public class TerritoryDetails
    {
        public TerritoryIDType ID;
        public string Name;
        public HashSet<TerritoryIDType> ConnectedTo;
        public HashSet<BonusIDType> PartOfBonuses;

        public MapDetails Parent;
        
        public TerritoryDetails(MapDetails parent, TerritoryIDType id)
        {
            this.Parent = parent;
            this.ID = id;
            this.PartOfBonuses = new HashSet<BonusIDType>();
            this.ConnectedTo = new HashSet<TerritoryIDType>();
        }
        
    }
}
