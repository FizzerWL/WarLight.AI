using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public struct Armies
    {
        private readonly int _numArmies;
        public readonly bool Fogged;
        public readonly SpecialUnit[] SpecialUnits;

        public Armies(int i = 0, SpecialUnit[] specialUnits = null, bool fogged = false)
        {
            this.Fogged = fogged;
            this._numArmies = i;

            if (specialUnits == null)
                SpecialUnits = new SpecialUnit[0];
            else
                SpecialUnits = specialUnits;
        }

        
        public int NumArmies
        {
            get
            {
                if (Fogged)
                    throw new Exception("NumArmies called on fog");
                else
                    return _numArmies;
            }
        }

        /// <summary>
        /// Returns the number of armies if non-fogged, or 0 if fogged.
        /// </summary>
        /// <returns></returns>
        public int ArmiesOrZero
        {
            get
            {
                if (Fogged)
                    return 0;
                else
                    return NumArmies;
            }
        }
        

        public bool IsEmpty
        {
            get { return NumArmies == 0 && SpecialUnits.Length == 0; }
        }


        public override string ToString()
        {
            return "(NumArmies=" + _numArmies + ", Fogged=" + Fogged + ", Specials=" + SpecialUnits.Length + ")";
        }


        /// <summary>
        /// Will remove the armies only if both this object and the passed Armies are not fogged
        /// Will never allow armies to be negative.
        /// </summary>
        /// <param name="armies"></param>
        public Armies Subtract(Armies armies)
        {
            if (this.Fogged || armies.Fogged)
                return this;

            SpecialUnit[] specials;

            if (SpecialUnits.Length == 0)
                specials = SpecialUnits; //we have none, so there's nothing to subtract.  This is by far the most common case, so check for it first.
            else if (armies.SpecialUnits.Length == 0)
                specials = SpecialUnits; //not subtracting any special units, carry ours forward
            else
            {
                //It's OK to do a N^2 algorithm here since our array length is usually 1, and never higher than 3. It's likely slower to allocate a dictionary.
                var list = new List<SpecialUnit>(SpecialUnits.Length);

                foreach (var spec in SpecialUnits)
                    if (armies.SpecialUnits.None(o => o.ID == spec.ID))
                        list.Add(spec);
                specials = list.ToArray();
            }

            return new Armies(Math.Max(0, this._numArmies - armies._numArmies), specials);
        }

        public Armies Add(Armies armies)
        {
            if (this.Fogged || armies.Fogged)
                return this;

            SpecialUnit[] specials;

            if (armies.SpecialUnits.Length == 0)
                specials = SpecialUnits; //we're adding none, so just keep what we have
            else
            {
                //It's OK to do a N^2 algorithm here since our array length is usually 1, and never higher than 3. It's likely slower to allocate a dictionary.
                var ourSpecialCount = SpecialUnits.Length;
                specials = new SpecialUnit[ourSpecialCount + armies.SpecialUnits.Length];

                for (int i = 0; i < SpecialUnits.Length; i++)
                    specials[i] = SpecialUnits[i].Clone();

                for (int i = 0; i < armies.SpecialUnits.Length; i++)
                    specials[i + ourSpecialCount] = armies.SpecialUnits[i].Clone();
            }


            return new Armies(this._numArmies + armies._numArmies, specials);
        }


        public int DefensePower
        {
            get
            {
                int a = this.NumArmies;
                foreach (var spec in this.SpecialUnits)
                    a = spec.ModifyDefensePower(a);
                return a;
            }
        }

        public int AttackPower
        {
            get
            {
                int a = this.NumArmies;
                foreach (var spec in this.SpecialUnits)
                    a = spec.ModifyAttackPower(a);
                return a;
            }
        }


        public static Armies DeserializeFromString(string raw)
        {
            var split = raw.Split('|');
            var armies = int.Parse(split[0]);

            if (split.Length == 1)
                return new Armies(armies);

            var specials = new SpecialUnit[split.Length - 1];
            for (int i = 0; i < split.Length - 1; i++)
                specials[i] = SpecialUnit.DeserializeFromString(split[i + 1], false);
            return new Armies(armies, specials);
        }

        public string SerializeToString()
        {
            if (SpecialUnits.Length == 0)
                return NumArmies.ToString();

            var sb = new StringBuilder();
            sb.Append(NumArmies.ToString());

            foreach (var special in SpecialUnits)
            {
                sb.Append("|");
                sb.Append(special.SerializeToString());
            }

            return sb.ToString();
        }
    }
}
