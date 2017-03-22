using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class GameOrderPlayCardAirlift : GameOrderPlayCard
    {
        public TerritoryIDType FromTerritoryID;
        public TerritoryIDType ToTerritoryID;
        private Armies NumArmies;
        public static GameOrderPlayCardAirlift Create(CardInstanceIDType cardInstanceID, PlayerIDType playerID, TerritoryIDType fromTerritoryID, TerritoryIDType toTerritoryID, Armies numArmies)
        {
            var o = new GameOrderPlayCardAirlift();
            o.CardInstanceID = cardInstanceID;
            o.PlayerID = playerID;
            o.FromTerritoryID = fromTerritoryID;
            o.ToTerritoryID = toTerritoryID;
            o.NumArmies = numArmies;
            return o;
        }

        public override TurnPhase? OccursInPhase
        {
            get { return TurnPhase.Airlift; }
        }

        public Armies Armies //must use this function instead of NumArmies directly for historical reasons :(
        {
            get
            {
                return this.NumArmies;
            }
            set
            {
                this.NumArmies = value;
            }
        }
    }
}
