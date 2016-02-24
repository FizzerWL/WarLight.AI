using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI
{
    public class GameSettings
    {
        public double OffensiveKillRate;
        public double DefensiveKillRate;
        public bool OneArmyMustStandGuard;
        public int MinimumArmyBonus;
        public int InitialNeutralsInDistribution;
        public int InitialNonDistributionArmies;
        public int LimitDistributionTerritories;
        public DistributionIDType DistributionModeID;
        public Dictionary<BonusIDType, int> OverriddenBonuses;
        public bool Commanders;
        public bool AllowAttackOnly;
        public bool AllowTransferOnly;

        public GameSettings(double offensiveKillRate, double defensiveKillRate, bool oneArmyMustStandGuard, int baseIncome, int initialNeutralsInDistribution, int initialNonDistributionArmies, int limitDistributionTerritories, DistributionIDType distributionModeID, Dictionary<BonusIDType, int> overriddenBonuses, bool commanders, bool allowAttackOnly, bool allowTransferOnly)
        {

            this.OffensiveKillRate = offensiveKillRate;
            this.DefensiveKillRate = defensiveKillRate;
            this.OneArmyMustStandGuard = oneArmyMustStandGuard;
            this.MinimumArmyBonus = baseIncome;
            this.InitialNeutralsInDistribution = initialNeutralsInDistribution;
            this.InitialNonDistributionArmies = initialNonDistributionArmies;
            this.LimitDistributionTerritories = limitDistributionTerritories;
            this.DistributionModeID = distributionModeID;
            this.OverriddenBonuses = overriddenBonuses;
            this.Commanders = commanders;
            this.AllowAttackOnly = allowAttackOnly;
            this.AllowTransferOnly = allowTransferOnly;
        }


    }
}
