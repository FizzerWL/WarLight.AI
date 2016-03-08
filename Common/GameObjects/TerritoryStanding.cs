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

        public TerritoryStanding(TerritoryIDType terrID, PlayerIDType playerID, Armies armies)
        {
            this.ID = terrID;
            this.OwnerPlayerID = playerID;
            this.NumArmies = armies;
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
            return new TerritoryStanding(this.ID, this.OwnerPlayerID, this.NumArmies);
        }


    }
}
