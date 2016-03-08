 /*
* This code was auto-converted from a java project.
*/

using System;
using System.Linq;
using System.Collections.Generic;


namespace WarLight.Shared.AI
{
    public class BonusDetails
    {
        public BonusIDType ID;
        public string Name;
        public int Amount; //Number of armies the bonus gives.  Note that game creators can override the amount, so take care when accessing this value as it might be wrong for the game.  Only use this value if the bonus ID is not present in GameSettings.OverriddenBonuses
        public List<TerritoryIDType> Territories;

        public MapDetails Parent;
        
        public BonusDetails(MapDetails parent, BonusIDType id, int armiesReward)
        {
            this.Parent = parent;
            this.ID = id;
            this.Amount = armiesReward;
            this.Territories = new List<TerritoryIDType>();
        }

        
        public override string ToString()
        {
            return Name + " ID = " + this.ID + ", ArmiesReward = " + this.Amount;
        }


        /// <summary>
        /// Determines who controls this bonus. Will return null if nobody does or at least one territory is fogged
        /// </summary>
        public PlayerIDType? ControlsBonus(GameStanding standing)
        {
            Assert.Fatal(standing != null, "standing is null");

            PlayerIDType playerID = (PlayerIDType)int.MinValue;
            foreach (var territoryID in this.Territories)
            {
                TerritoryStanding cs = standing.Territories[territoryID];
                if (cs.OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                    return null;
                if (cs.IsNeutral)
                    return null;
                else if (cs.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                    return null;
                else if ((int)playerID == int.MinValue)
                    playerID = cs.OwnerPlayerID;
                else if (playerID != cs.OwnerPlayerID)
                    return null;
            }

            Assert.Fatal(playerID != TerritoryStanding.FogPlayerID && playerID != TerritoryStanding.NeutralPlayerID);
            if ((int)playerID == int.MinValue)
                throw new Exception("Bonus " + this.Name + " (" + this.ID + ") has no territories assigned");

            return new Nullable<PlayerIDType>(playerID);
        }

    }
}
