using System.Collections.Generic;
using System.Text;
using System.Linq;

using System;

namespace WarLight.Shared.AI
{
    public class MapDetails
    {
        public MapIDType ID;
        public string Name;
        public Dictionary<TerritoryIDType, TerritoryDetails> Territories;
        public Dictionary<BonusIDType, BonusDetails> Bonuses;
        public Dictionary<DistributionIDType, DistributionMode> DistributionModes;

        public MapDetails()
        {
            this.Territories = new Dictionary<TerritoryIDType, TerritoryDetails>();
            this.Bonuses = new Dictionary<BonusIDType, BonusDetails>();
            this.DistributionModes = new Dictionary<DistributionIDType, DistributionMode>();
        }

        public bool IsScenarioDistribution(DistributionIDType distID)
        {
            return DistributionModes.ContainsKey(distID) && DistributionModes[distID].Type == "scenario";
        }

        public List<TerritoryIDType> GetTerritoriesForScenario(DistributionIDType distributionModeID, ushort scenarioID)
        {
            return DistributionModes[distributionModeID].Territories.Where(o => o.Value == scenarioID).Select(o => o.Key).ToList();
        }
    }

    public class DistributionMode
    {
        public DistributionIDType ID;
        public string Name;
        public string Type;
        public Dictionary<TerritoryIDType, ushort> Territories;
    }
}
