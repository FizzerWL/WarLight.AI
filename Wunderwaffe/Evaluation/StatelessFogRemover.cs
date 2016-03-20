using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public class StatelessFogRemover
    {

        private BotMain BotState;
        public StatelessFogRemover(BotMain state)
        {
            this.BotState = state;
        }

        public void RemoveFog()
        {
            // Step 1: Assume for all fogged territories that they are neutral with the same armies as during the picking stage
            RemoveFogAccordingToPickingState();

            // And that's it for now

        }

        private void RemoveFogAccordingToPickingState()
        {
            if (BotState.DistributionStanding == null)
                return; //auto-dist game, skip

            BotMap pickingStageMap = BotMap.FromStanding(BotState, BotState.DistributionStanding);
            BotMap visibleMap = BotState.VisibleMap;

            // territories in distribution have first 0 neutrals in the lvMap
            List<TerritoryIDType> pickableTerritories = BotState.DistributionStanding.Territories.Values.
                Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).
                Select(o => o.ID).ToList();

            foreach (BotTerritory vmTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
            {
                BotTerritory lvmTerritory = pickingStageMap.Territories[vmTerritory.ID];
                vmTerritory.OwnerPlayerID = TerritoryStanding.NeutralPlayerID;
                if (pickableTerritories.Contains(vmTerritory.ID))
                {
                    vmTerritory.Armies = new Armies(BotState.Settings.InitialNeutralsInDistribution);
                }
                else
                {
                    vmTerritory.Armies = new Armies(lvmTerritory.Armies.NumArmies);
                }
            }

        }


    }
}
