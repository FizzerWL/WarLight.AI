using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public class PossibleExpandTarget
    {
        public readonly BotMain Bot;
        public readonly TerritoryIDType ID;
        public readonly float WeightFromCriticalPath;
        public readonly Dictionary<BonusIDType, PossibleExpandTargetBonus> Bonuses;

        public PossibleExpandTarget(BotMain bot, TerritoryIDType id, Dictionary<BonusIDType, PossibleExpandTargetBonus> bonuses)
        {
            this.Bot = bot;
            this.ID = id;
            this.Bonuses = bonuses;

            //Add a tie breaker 1 point if we're on the critical path to completing this bonus
            WeightFromCriticalPath = bonuses.Values.Count(o => o.Path.TerritoriesOnCriticalPath.Contains(ID));
        }

        public float WeightFromBonuses
        {
            get { return ExpansionHelper.WeighMultipleBonuses(Bonuses.ToDictionary(o => o.Key, o => o.Value.Weight)); }
        }
        public float Weight
        {
            get { return WeightFromBonuses + WeightFromCriticalPath; }
        }

        public override string ToString()
        {
            return Bot.TerrString(ID) + " Weight=" + Weight + ", CriticalPath=" + WeightFromCriticalPath + ", Bonuses: " + Bonuses.Select(o => Bot.BonusString(o.Key) + " " + o.Value).JoinStrings(", ");
        }
    }

    public class PossibleExpandTargetBonus
    {
        public readonly float BaseWeight;
        public readonly BonusPath Path;
        public readonly CaptureTerritories ToTakeOpt;
        public int DeployedTowardsCapturing;

        public PossibleExpandTargetBonus(float weight, BonusPath path, CaptureTerritories toTake)
        {
            this.BaseWeight = weight;
            this.Path = path;
            this.ToTakeOpt = toTake;
        }

        public float Weight
        {
            get
            {
                //If we've deployed armies towards capturing this bonus, add a small tie-breaker so the AI prioritizes this bonus over other equally weighted ones.
                return BaseWeight + (DeployedTowardsCapturing > 0 ? 0.1f : 0f);
            }
        }

        public override string ToString()
        {
            return "weight=" + Weight + (ToTakeOpt == null ? " No reasonable capture solution found" : " turnsToTake=" + ToTakeOpt.NumTurns + " armiesNeededToDeploy=" + ToTakeOpt.ArmiesNeededToDeploy) + " deployed=" + DeployedTowardsCapturing;
        }
    }
}
