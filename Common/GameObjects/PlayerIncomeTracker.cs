using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class PlayerIncomeProgress
    {
        public BonusIDType Restriction;
        public int Deployed;
        public int TotalToDeploy;

        internal PlayerIncomeProgress(BonusIDType bonusID, int deployed, int totalToDeploy)
        {
            this.Restriction = bonusID;
            this.Deployed = deployed;
            this.TotalToDeploy = totalToDeploy;
        }
    }

    public class PlayerIncomeTracker
    {
        private Dictionary<TerritoryIDType, int> _freeArmiesUsedOn;
        private Dictionary<BonusIDType, int> _armiesUsedOnBonuses;
        private PlayerIncome _income;
        private MapDetails _map;

        public override string ToString()
        {
            return "_freeArmiesUsedOn=" + _freeArmiesUsedOn.Select(o => "(" + o.Key + "/" + o.Value + ")").JoinStrings(",") + "; _armiesUsedOnBonuses=" + _armiesUsedOnBonuses.Select(o => "(" + o.Key + "/" + o.Value + ")").JoinStrings(",");
        }
        

        /// <param name="income">We can pass a null PlayerIncome class, but none of the methods on this class should be called until one is supplied via SupplyIncome</param>
        public PlayerIncomeTracker(PlayerIncome income, MapDetails map)
        {
            _income = income;
            _map = map;
            _armiesUsedOnBonuses = new Dictionary<BonusIDType, int>();
            _freeArmiesUsedOn = new Dictionary<TerritoryIDType, int>();
        }

        public void SupplyIncome(PlayerIncome income)
        {
            Assert.Fatal(income != null, "Cannot un-load income");
            this._income = income;
        }


        internal void Reset()
        {
            if (_income == null)
                return; //we never even loaded, so there's nothing to reset

            _armiesUsedOnBonuses.Clear();
            _freeArmiesUsedOn.Clear();
        }

        public bool FullyDeployed
        {
            get
            {
                Assert.Fatal(_income != null, "_income is null");
                if (_freeArmiesUsedOn.Values.Sum() != _income.FreeArmies)
                    return false;

                foreach (var restriction in _income.BonusRestrictions)
                    if (_armiesUsedOnBonuses.ValueOrZero(restriction.Key) != restriction.Value)
                        return false;

                return true;
            }
        }

        public int RemainingUndeployed
        {
            get
            {
                Assert.Fatal(_income != null, "_income is null");
                Assert.Fatal(_freeArmiesUsedOn != null, "_freeArmiesUsedOn is null");

                var ret = _income.FreeArmies - _freeArmiesUsedOn.Values.Sum();

                foreach (var restriction in _income.BonusRestrictions)
                    ret += restriction.Value - _armiesUsedOnBonuses.ValueOrZero(restriction.Key);

                return ret;
            }
        }

        public int FreeArmiesUndeployed
        {
            get
            {
                Assert.Fatal(_income != null, "_income is null");
                return _income.FreeArmies - _freeArmiesUsedOn.Values.Sum();
            }
        }
        
        public KeyValuePair<int, int> FreeProgress
        {
            get
            {
                Assert.Fatal(_income != null, "_income is null");
                return new KeyValuePair<int, int>(this._freeArmiesUsedOn.Values.Sum(), _income.FreeArmies);
            }
        }

        /// <param name="numArmies">Can be negative to indicate we're freeing armies we previously used.</param>
        /// <returns>
        /// True if the change was successful. False if we couldn't do it.
        /// </returns>
        public bool TryRecordUsedArmies(TerritoryIDType territoryID, int numArmies)
        {
            Assert.Fatal(_income != null, "_income is null");
            if (numArmies == 0)
                return true; //no-op
            else if (numArmies < 0)
                return TryRemoveUsedArmies(territoryID, -numArmies);


            if (_income.BonusRestrictions.Count > 0)
            {
                var td = _map.Territories[territoryID];

                foreach (var bonusID in td.PartOfBonuses)
                {
                    if (_income.BonusRestrictions.ContainsKey(bonusID))
                    {
                        var used = _armiesUsedOnBonuses.ValueOrZero(bonusID);
                        var avail = _income.BonusRestrictions[bonusID];
                        if (avail > used)
                        {
                            var toUse = Math.Min(numArmies, avail - used); //can't name a variable "use" in actionscript

                            if (_income.FreeArmies - _freeArmiesUsedOn.Values.Sum() < numArmies - toUse)
                                return false; //We have some armies we could award from this bonus, but we wouldn't have enough free armies to award the rest of what's requested.  Therefore, abort now before recording bonus armies, so that the function doesn't change something and also return false.

                            _armiesUsedOnBonuses.AddTo(bonusID, toUse);
                            _freeArmiesUsedOn.AddTo(territoryID, numArmies - toUse);

                            return true;
                        }
                    }
                }
            }

            //Take free armies.
            int freeAvail = _income.FreeArmies - _freeArmiesUsedOn.Values.Sum();

            if (freeAvail < numArmies)
                return false;

            _freeArmiesUsedOn.AddTo(territoryID, numArmies);
            return true;
        }

        public bool TryRemoveUsedArmies(TerritoryIDType territoryID, int armiesToRemove)
        {
            Assert.Fatal(_income != null, "_income is null");

            //First try to remove free income since those are the most important
            var freeUsedHere = _freeArmiesUsedOn.ValueOrZero(territoryID);

            if (freeUsedHere >= armiesToRemove)
            {
                _freeArmiesUsedOn.AddTo(territoryID, -armiesToRemove);
                return true;
            }

            //Try to free bonus income if we can't satisfy it with free income
            if (_income.BonusRestrictions.Count > 0)
            {
                var needToRemoveFromBonus = armiesToRemove - freeUsedHere;

                var td = _map.Territories[territoryID];

                foreach (var bonusID in td.PartOfBonuses)
                    if (_income.BonusRestrictions.ContainsKey(bonusID))
                    {
                        var used = _armiesUsedOnBonuses.ValueOrZero(bonusID);

                        if (used < needToRemoveFromBonus)
                            return false; //can't remove enough from the bonus and free armies combined

                        _armiesUsedOnBonuses.AddTo(bonusID, -needToRemoveFromBonus);
                        _freeArmiesUsedOn.Remove(territoryID);

                        //Anytime we take bonus armies away, we could be in a situation where we could move free armies into bonus armies.  Check for this
                        CheckFreeMovingToBonus(bonusID);
                        return true;
                    }
            }

            return false; //couldn't remove from bonuses and free armies combined
        }

        private void CheckFreeMovingToBonus(BonusIDType bonusID)
        {
            var availInBonus = _income.BonusRestrictions[bonusID] - _armiesUsedOnBonuses.ValueOrZero(bonusID);

            Assert.Fatal(availInBonus > 0, "CheckFreeMovingToBonus called with 0 armies available to move");

            foreach (var terrID in _map.Bonuses[bonusID].Territories)
            {
                var free = _freeArmiesUsedOn.ValueOrZero(terrID);

                if (free > 0)
                {
                    var toMove = Math.Min(free, availInBonus);

                    _freeArmiesUsedOn.AddTo(terrID, -toMove);
                    _armiesUsedOnBonuses.AddTo(bonusID, toMove);
                    availInBonus -= toMove;

                    if (availInBonus == 0)
                        return;
                }
            }
        }

        /// <summary>
        /// Returns the maximum number of armies that can be deployed on this territory beyond what's already deployed.  For example, if your income is 5, and you've already deployed 3 elsewhere, this will return 2.
        /// </summary>
        /// <param name="terrID"></param>
        /// <returns></returns>
        public int MaxAdditionalCanDeployOn(TerritoryIDType territoryID)
        {
            Assert.Fatal(_income != null, "_income is null");
            int freeAvail = this.FreeArmiesUndeployed;

            if (_income.BonusRestrictions.Count == 0)
                return freeAvail;

            var td = _map.Territories[territoryID];

            foreach (var bonusID in td.PartOfBonuses)
            {
                if (_income.BonusRestrictions.ContainsKey(bonusID))
                    return freeAvail + _income.BonusRestrictions[bonusID] - _armiesUsedOnBonuses.ValueOrZero(bonusID);
            }

            //No bonuses were restricted, just free can go here
            return freeAvail;
        }
        public List<PlayerIncomeProgress> RestrictedBonusProgress
        {
            get
            {
                Assert.Fatal(_income != null, "_income is null");
                var ret = new List<PlayerIncomeProgress>();

                foreach (var kv in _income.BonusRestrictions)
                    ret.Add(new PlayerIncomeProgress(kv.Key, _armiesUsedOnBonuses.ValueOrZero(kv.Key), kv.Value));

                return ret;
            }
        }

        public int ArmiesDeployedToBonus(BonusIDType bonusID)
        {
            return _armiesUsedOnBonuses.ValueOrZero(bonusID);
        }

        public int TotalArmiesDeployed
        {
            get
            {
                return _freeArmiesUsedOn.Values.Sum() + _armiesUsedOnBonuses.Values.Sum();
            }
        }
    }
}
