using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    
    public class TerritoryStanding
    {
        public static readonly PlayerIDType FogPlayerID = (PlayerIDType )(-1);
        public static readonly PlayerIDType NeutralPlayerID = (PlayerIDType)0;
        public static readonly PlayerIDType AvailableForDistribution = (PlayerIDType)(-2);

        public TerritoryIDType ID;
        public PlayerIDType OwnerPlayerID;
        public Armies NumArmies;

        
        public static TerritoryStanding Create(TerritoryIDType territoryID, PlayerIDType owner, Armies armies)
        {
            TerritoryStanding cs = new TerritoryStanding();
            cs.ID = territoryID;
            cs.OwnerPlayerID = owner;
            cs.NumArmies = armies;
            return cs;
        }

        public bool IsNeutral
        {
            get { return OwnerPlayerID == NeutralPlayerID; }
        }

        public override string ToString()
        {
            return this.ID + ": " + this.NumArmies + " - Owned by " + this.OwnerPlayerID;
        }

        public TerritoryStanding Clone()
        {
            return TerritoryStanding.Create(this.ID, this.OwnerPlayerID, this.NumArmies);
        }

        internal int NumStructures(StructureType city)
        {
            return 0; //not implemented
        }
    }
}
