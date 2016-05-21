using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public abstract class SpecialUnit
    {
        public Guid ID;
        public PlayerIDType OwnerID;



        public SpecialUnit Clone()
        {
            var r = CloneSpecific();
            r.ID = this.ID;
            r.OwnerID = this.OwnerID;
            return r;
        }



        protected abstract SpecialUnit CloneSpecific();
        public abstract int ModifyAttackPower(int attackPower);
        public abstract int ModifyDefensePower(int defensePower);
        public abstract bool IsBoss();

        public static SpecialUnit DeserializeFromString(string raw, bool ignoreIDAndOwner)
        {
            var split = raw.Split(',');
            Assert.Fatal(split[0] == "cmdr");
            var ret = new Commander();

            if (!ignoreIDAndOwner)
            {
                ret.ID = Guid.Parse(split[1]);
                ret.OwnerID = (PlayerIDType)int.Parse(split[2]);
            }
            return ret;
        }

        public string SerializeToString()
        {
            //We assume it's a commander for now
            return "cmdr," + this.ID + "," + this.OwnerID;
        }

    }

    public class Commander : SpecialUnit
    {
        protected override SpecialUnit CloneSpecific()
        {
            return new Commander();
        }
        public override int ModifyAttackPower(int attackPower)
        {
            return attackPower + 7;
        }
        public override int ModifyDefensePower(int defensePower)
        {
            return defensePower + 7;
        }
        public override bool IsBoss()
        {
            return false;
        }
    }

    public class Boss1 : SpecialUnit
    {
        protected override SpecialUnit CloneSpecific()
        {
            return new Boss1();
        }
        public override int ModifyAttackPower(int attackPower)
        {
            return attackPower + 200;
        }
        public override int ModifyDefensePower(int defensePower)
        {
            return defensePower + 200;
        }
        public override bool IsBoss()
        {
            return true;
        }
    }
}
