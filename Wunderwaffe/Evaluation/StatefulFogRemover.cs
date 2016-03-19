//using System.Collections.Generic;
//using System.Linq;
//using WarLight.AI.Wunderwaffe.Bot;
//using WarLight.Shared.AI;

//namespace WarLight.AI.Wunderwaffe.Evaluation
//{
//    public class StatefulFogRemover
//    {

//        public static List<TerritoryIDType> PickedTerritories = null;

//        private BotMain BotState;
//        public StatefulFogRemover(BotMain state)
//        {
//            this.BotState = state;
//        }

//        public void RemoveFog()
//        {
//            if (BotState.NumberOfTurns == 0)
//            {
//                RemoveFogAfterPicks();
//                return;
//            }
//            // Step 1 - Remove all fog from known opponent territories and territories that we lost
//            RemoveFogPreviousTurnIntel();

//            // Step 2 - Remove fog based on the opponent deployment (heuristic guess)
//            RemoveFogOpponentDeployment();

//            // Step 3 - Assume for all remaining fog (which previously was neutral) that it stays neutral
//            RemoveRemainingNeutralFog();
//        }


//        private void RemoveFogOpponentDeployment()
//        {
//            BotMap lvmMap = BotMain.LastVisibleMap;
//            BotMap visibleMap = BotState.VisibleMap;

//            // Copy the old ownership heuristics
//            foreach (BotTerritory lvmTerritory in lvmMap.Territories.Values)
//            {
//                BotTerritory vmTerrirotry = visibleMap.Territories[lvmTerritory.ID];
//                vmTerrirotry.IsOwnershipHeuristic = lvmTerritory.IsOwnershipHeuristic;
//            }

//            // Set all territories to ownership heuristic = false on which we have direct intel
//            foreach (BotTerritory vmTerritory in visibleMap.Territories.Values)
//            {
//                if (vmTerritory.IsVisible)
//                {
//                    vmTerritory.IsOwnershipHeuristic = false;
//                }
//            }

//            int opponentDeployment = BotState.HistoryTracker.GetOpponentDeployment(BotState.Opponents.First().ID);
//            List<BotBonus> sortedLvmBonuses = SortBonusesNeutralCount();
//            int stillMissingIncome = opponentDeployment - BotState.Settings.MinimumArmyBonus;

//            List<BotBonus> guessedLvmBonuses = new List<BotBonus>();
//            foreach (BotBonus possibleLvmBonus in sortedLvmBonuses)
//            {
//                if (stillMissingIncome > 0)
//                {
//                    guessedLvmBonuses.Add(possibleLvmBonus);
//                    stillMissingIncome -= possibleLvmBonus.Amount;
//                }
//            }
//            // switch to the visible map
//            List<BotBonus> guessedVmBonuses = new List<BotBonus>();
//            foreach (BotBonus guessedLvmBonus in guessedLvmBonuses)
//            {
//                guessedVmBonuses.Add(visibleMap.Bonuses[guessedLvmBonus.ID]);
//            }


//            // Calculate the non possible bonuses
//            List<BotBonus> brokenBonuses = new List<BotBonus>();
//            List<BotBonus> wrongGuesses = new List<BotBonus>();
//            foreach (BotBonus guessedVmBonus in guessedVmBonuses)
//            {
//                if (guessedVmBonus.GetOwnedTerritories().Count > 0)
//                {
//                    brokenBonuses.Add(guessedVmBonus);
//                }
//                else if (!guessedVmBonus.CanBeOwnedByOpponent(BotState.Opponents.First().ID))
//                {
//                    wrongGuesses.Add(guessedVmBonus);
//                }
//            }
//            guessedVmBonuses.RemoveWhere(o => brokenBonuses.Contains(o));
//            guessedVmBonuses.RemoveWhere(o => wrongGuesses.Contains(o));

//            // Add the territories of the guessed bonuses to the opponent territories. If they are not already for sure owned then flag them as unsure.
//            foreach (BotBonus guessedVmBonus in guessedVmBonuses)
//            {
//                // probably uncecessary since mistake was someplace else
//                foreach (BotTerritory guessedVmTerritoryX in guessedVmBonus.Territories)
//                {
//                    BotTerritory guessedVmTerritory = visibleMap.Territories[guessedVmTerritoryX.ID];
//                    if (!guessedVmTerritory.IsVisible && guessedVmTerritory.OwnerPlayerID != BotState.Opponents.First().ID)
//                    {
//                        guessedVmTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
//                        guessedVmTerritory.IsOwnershipHeuristic = true;
//                        BotTerritory guessedLvmTerritory = lvmMap.Territories[guessedVmTerritory.ID];
//                        guessedVmTerritory.Armies = new Armies(guessedLvmTerritory.Armies.AttackPower);
//                    }
//                }
//            }

//            //// Do the same thing for the broken bonuses
//            foreach (BotBonus brokenVmBonus in brokenBonuses)
//            {
//                foreach (BotTerritory guessedVmTerritoryX in brokenVmBonus.Territories)
//                {
//                    BotTerritory guessedVmTerritory = visibleMap.Territories[guessedVmTerritoryX.ID];
//                    if (!guessedVmTerritory.IsVisible && guessedVmTerritory.OwnerPlayerID != BotState.Opponents.First().ID)
//                    {
//                        guessedVmTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
//                        guessedVmTerritory.IsOwnershipHeuristic = true;
//                        BotTerritory guessedLvmTerritory = lvmMap.Territories[guessedVmTerritory.ID];
//                        guessedVmTerritory.Armies = new Armies(guessedLvmTerritory.Armies.AttackPower);
//                    }
//                }
//            }

//            List<BotBonus> handledBonuses = new List<BotBonus>();
//            handledBonuses.AddRange(guessedVmBonuses);
//            handledBonuses.AddRange(brokenBonuses);
//            List<BotTerritory> handledTerritories = new List<BotTerritory>();
//            foreach (BotBonus handledBonus in handledBonuses)
//            {
//                handledTerritories.AddRange(handledBonus.Territories);
//            }

//            foreach (BotBonus nonPossibleBonus in visibleMap.Bonuses.Values.Where(o => !(o.CanBeOwnedByOpponent()) && !brokenBonuses.Contains(o)))
//            {
//                foreach (BotTerritory territoryX in nonPossibleBonus.Territories)
//                {
//                    if (handledTerritories.Contains(territoryX))
//                    {
//                        continue;
//                    }
//                    BotTerritory territory = visibleMap.Territories[territoryX.ID];
//                    if (territory.OwnerPlayerID == BotState.Opponents.First().ID && territory.IsOwnershipHeuristic)
//                    {
//                        territory.IsOwnershipHeuristic = false;
//                        territory.OwnerPlayerID = TerritoryStanding.NeutralPlayerID;
//                        BotTerritory lvmTerritory = lvmMap.Territories[territory.ID];
//                        territory.Armies = new Armies(lvmTerritory.Armies.AttackPower);
//                    }
//                }
//            }


//        }

//        private List<BotBonus> SortBonusesNeutralCount()
//        {
//            BotMap lvMap = BotMain.LastVisibleMap;
//            List<BotBonus> possibleBonuses = lvMap.Bonuses.Values.Where(o => o.Amount > 0 && o.CanBeOwnedByOpponent(BotState.Opponents.First().ID)).ToList();
//            possibleBonuses = possibleBonuses.OrderBy(o => o.NeutralArmies.AttackPower).ToList();
//            return possibleBonuses;
//        }


//        private void RemoveRemainingNeutralFog()
//        {
//            BotMap visibleMap = BotState.VisibleMap;
//            BotMap lastVisibleMap = BotMain.LastVisibleMap;
//            foreach (BotTerritory vmFogTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
//            {
//                BotTerritory lvmFogTerritory = lastVisibleMap.Territories[vmFogTerritory.ID];
//                if (lvmFogTerritory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
//                {
//                    vmFogTerritory.OwnerPlayerID = TerritoryStanding.NeutralPlayerID;
//                    vmFogTerritory.Armies = new Armies(lvmFogTerritory.Armies.AttackPower);
//                }
//            }
//        }

//        private void RemoveFogPreviousTurnIntel()
//        {
//            BotMap visibleMap = BotState.VisibleMap;
//            BotMap lastVisibleMap = BotMain.LastVisibleMap;
//            foreach (BotTerritory vmFogTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
//            {
//                BotTerritory lvmFogTerritory = lastVisibleMap.Territories[vmFogTerritory.ID];
//                if (lvmFogTerritory.OwnerPlayerID == BotState.Me.ID)
//                {
//                    vmFogTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
//                    vmFogTerritory.Armies = new Armies(1);
//                }
//                else if (lvmFogTerritory.OwnerPlayerID == BotState.Opponents.First().ID)
//                {
//                    vmFogTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
//                    vmFogTerritory.Armies = new Armies(lvmFogTerritory.Armies.AttackPower);
//                }
//            }
//        }


//        private void RemoveFogAfterPicks()
//        {
//            BotMap visibleMap = BotState.VisibleMap;
//            List<BotTerritory> ownedTerritories = visibleMap.GetOwnedTerritories();
//            int maxPickNumber = 0;
//            for (int i = 0; i < PickedTerritories.Count; i++)
//            {
//                TerritoryIDType pickedTerritoryId = PickedTerritories[i];
//                if (visibleMap.Territories[pickedTerritoryId].OwnerPlayerID == BotState.Me.ID)
//                {
//                    maxPickNumber = i;
//                }
//            }
//            for (int i = 0; i < maxPickNumber; i++)
//            {
//                BotTerritory pickedTerritory = visibleMap.Territories[PickedTerritories[i]];
//                if (pickedTerritory.OwnerPlayerID == TerritoryStanding.FogPlayerID)
//                {

//                    pickedTerritory.OwnerPlayerID = BotState.Opponents.First().ID;
//                    pickedTerritory.Armies = new Armies(1);
//                }
//            }


//            BotMap lvMap = BotMap.FromStanding(BotState, BotState.DistributionStanding);
//            // territories in distribution have first 0 neutrals in the lvMap
//            List<TerritoryIDType> pickableTerritories = BotState.DistributionStanding.Territories.Values.
//                Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).
//                Select(o => o.ID).ToList();
//            foreach (BotTerritory vmTerritory in visibleMap.Territories.Values.Where(territory => territory.OwnerPlayerID == TerritoryStanding.FogPlayerID))
//            {
//                BotTerritory lvmTerritory = lvMap.Territories[vmTerritory.ID];
//                vmTerritory.OwnerPlayerID = TerritoryStanding.NeutralPlayerID;
//                if (pickableTerritories.Contains(vmTerritory.ID))
//                {
//                    vmTerritory.Armies = new Armies(BotState.Settings.InitialNeutralsInDistribution);
//                }
//                else
//                {
//                    vmTerritory.Armies = new Armies(lvmTerritory.Armies.NumArmies);
//                }
//            }
//            PickedTerritories = null;
//        }


//    }

//}
