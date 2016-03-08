using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class PlayerIncome
    {
        public int FreeArmies;
        public Dictionary<BonusIDType, int> BonusRestrictions = new Dictionary<BonusIDType, int>(); //value is the number of armies that must be deployed to this bonus

        public int Total
        {
            get { return FreeArmies + BonusRestrictions.Values.Sum(); }
        }

        public PlayerIncome(int freeArmies = 0)
        {
            this.FreeArmies = freeArmies;
        }
        public PlayerIncome Clone()
        {
            var ret = new PlayerIncome(FreeArmies);
            ret.BonusRestrictions = this.BonusRestrictions.ToDictionary(o => o.Key, o => o.Value);
            return ret;
        }

        public override string ToString()
        {
            return FreeArmies.ToString() + (BonusRestrictions.Count == 0 ? "" : " local deployments=" + BonusRestrictions.Select(o => o.Key + "=" + o.Value).JoinStrings(", "));
        }

    }
}
