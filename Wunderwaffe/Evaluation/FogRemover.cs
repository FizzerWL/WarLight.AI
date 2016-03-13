using System.Collections.Generic;
using System.Linq;
using WarLight.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI;

namespace WarLight.AI.Wunderwaffe.Evaluation
{
    public class FogRemover
    {

        public static List<TerritoryIDType> PickedTerritories = null;

        private BotMain BotState;
        public FogRemover(BotMain state)
        {
            this.BotState = state;
        }

        public void RemoveFog()
        {
            BotMap lvMap = null;
            if (BotState.NumberOfTurns == -1)
            {
                return;
            }
            else if (BotState.NumberOfTurns == 0)
            {
                lvMap = BotMap.FromStanding(BotState, BotState.DistributionStanding);
            }
            else
            {
                lvMap = BotMain.LastVisibleMap;
            }

            BotMap visibleMap = BotState.VisibleMap;

            if (BotState.NumberOfTurns == 0)
            {
                RemoveFogAfterPicks();
                return;
            }
            // Step 1 - Save: Remove all fog from known opponent territories and territories that we lost
            RemoveFogPreviousTurnIntel();

            // Step 2 - Remove fog based on the opponent deployment
            RemoveFogOpponentDeployment();

            // Step 3 - Assume for all remaining fog (which previously was neutral) that it stays neutral
            RemoveRemainingNeutralFog();
        }


        private void RemoveFogOpponentDeployment()
        {

        }

        private void RemoveRemainingNeutralFog()
        {
            BotMap visibleMap = BotState.VisibleMap;
            BotMap lastVisibleMap = BotMain.LastVisibleMap;
            foreach (BotTerritory vmFogTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
            {
                BotTerritory lvmFogTerritory = lastVisibleMap.Territories[vmFogTerritory.ID];
                if (lvmFogTerritory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                {
                    vmFogTerritory.OwnerPlayerID = TerritoryStanding.NeutralPlayerID;
                    vmFogTerritory.Armies = new Armies(lvmFogTerritory.Armies.AttackPower);
                }
            }
        }

        private void RemoveFogPreviousTurnIntel()
        {
            BotMap visibleMap = BotState.VisibleMap;
            BotMap lastVisibleMap = BotMain.LastVisibleMap;
            foreach (BotTerritory vmFogTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
            {
                BotTerritory lvmFogTerritory = lastVisibleMap.Territories[vmFogTerritory.ID];
                if (lvmFogTerritory.OwnerPlayerID == BotState.Me.ID)
                {
                    vmFogTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
                    vmFogTerritory.Armies = new Armies(1);
                }
                else if (lvmFogTerritory.OwnerPlayerID == BotState.Opponents.First().ID)
                {
                    vmFogTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
                    vmFogTerritory.Armies = new Armies(lvmFogTerritory.Armies.AttackPower);
                }
            }
        }


        private void RemoveFogAfterPicks()
        {
            BotMap visibleMap = BotState.VisibleMap;
            List<BotTerritory> ownedTerritories = visibleMap.GetOwnedTerritories();
            int maxPickNumber = 0;
            for (int i = 0; i < PickedTerritories.Count; i++)
            {
                TerritoryIDType pickedTerritoryId = PickedTerritories[i];
                if (visibleMap.Territories[pickedTerritoryId].OwnerPlayerID == BotState.Me.ID)
                {
                    maxPickNumber = i;
                }
            }
            for (int i = 0; i < maxPickNumber; i++)
            {
                BotTerritory pickedTerritory = visibleMap.Territories[PickedTerritories[i]];
                if (pickedTerritory.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                {

                    pickedTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
                    pickedTerritory.Armies = new Armies(1);
                }
            }


            BotMap lvMap = BotMap.FromStanding(BotState, BotState.DistributionStanding);
            // territories in distribution have first 0 neutrals in the lvMap
            List<TerritoryIDType> pickableTerritories = BotState.DistributionStanding.Territories.Values.
                Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).
                Select(o => o.ID).ToList();
            foreach (BotTerritory vmTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
            {
                BotTerritory lvmTerritory = lvMap.Territories[vmTerritory.ID];
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















            PickedTerritories = null;
        }


    }

}
