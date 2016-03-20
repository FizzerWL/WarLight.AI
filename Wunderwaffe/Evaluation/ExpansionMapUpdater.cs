using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;

namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    /// <summary>This class is responsible for updating the ExpansionMap.</summary>
    /// <remarks>
    /// This class is responsible for updating the ExpansionMap. The expansion map is
    /// similar to the visible map with the exception that territories in which we
    /// expanded are marked as owned. This allows us to not transfer in the direction
    /// on neutrals that we are taking this turn and it allows us to transfer from
    /// another spot to a territory that we are taking this turn.
    /// </remarks>
    public class ExpansionMapUpdater
    {
        public BotMain BotState;
        public ExpansionMapUpdater(BotMain state)
        {
            this.BotState = state;
        }

        public void UpdateExpansionMap()
        {
            BotState.ExpansionMap = BotState.VisibleMap.GetMapCopy();
            var visibleMap = BotState.VisibleMap;
            var expansionMap = BotState.ExpansionMap;
            var vmNeutralTerritories = visibleMap.GetNeutralTerritories();
            List<BotTerritory> vmNeutralTerritoriesThatWeTake = new List<BotTerritory>();
            // find out which territories we are taking by expansion
            foreach (var vmNeutralTerritory in vmNeutralTerritories)
            {
                if (vmNeutralTerritory.IsVisible)
                {
                    var attackingArmies = vmNeutralTerritory.GetIncomingArmies();
                    if (vmNeutralTerritory.getOwnKills(attackingArmies.AttackPower, vmNeutralTerritory.Armies.DefensePower) >= vmNeutralTerritory.Armies.DefensePower)
                        vmNeutralTerritoriesThatWeTake.Add(vmNeutralTerritory);
                }
            }
            // update the expansionMap according to our expansion
            foreach (var vmTakenTerritory in vmNeutralTerritoriesThatWeTake)
            {
                var emTakenTerritory = expansionMap.Territories[vmTakenTerritory.ID];
                emTakenTerritory.OwnerPlayerID = BotState.Me.ID;
                emTakenTerritory.Armies = vmTakenTerritory.GetIncomingArmies().Subtract(new Armies(SharedUtility.Round(vmTakenTerritory.Armies.DefensePower * BotState.Settings.DefenseKillRate)));
            }
        }
    }
}
