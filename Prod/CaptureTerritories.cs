using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod
{
    public class CaptureTerritories
    {
        public readonly int NumTurns;
        public readonly int ArmiesNeededToDeploy;

        public CaptureTerritories(int numTurns, int armiesNeededToDeploy)
        {
            this.NumTurns = numTurns;
            this.ArmiesNeededToDeploy = armiesNeededToDeploy;
        }

        public override string ToString()
        {
            return "TurnsToTake=" + NumTurns + " ArmiesNeededToDeploy=" + ArmiesNeededToDeploy;
        }

        public static CaptureTerritories TryFindTurnsToTake(BotMain bot, BonusPath path, int armiesWeHaveInOrAroundBonus, int armiesEarnPerTurn, HashSet<TerritoryIDType> terrsToTake, Func<TerritoryStanding, int> terrToTakeDefenders, int timeout = 30)
        {
            Assert.Fatal(armiesEarnPerTurn >= 0, "Negative armiesEarnPerTurn");
            var armiesDefending = terrsToTake.Sum(o => terrToTakeDefenders(bot.Standing.Territories[o]));

            var avgAttackersNeededPerTerritory = bot.ArmiesToTake(new Armies(SharedUtility.Round((double)armiesDefending / (double)terrsToTake.Count)));
            var avgRemaindersPerTerritory = avgAttackersNeededPerTerritory - SharedUtility.Round(avgAttackersNeededPerTerritory * bot.Settings.DefenseKillRate);


            var armiesNeeded = bot.ArmiesToTake(new Armies(armiesDefending));

            for (int turns = path.TurnsToTakeByDistance; ; turns++)
            {
                if (turns >= timeout)
                    return null; //could not find a solution in time

                var totalDeployed = armiesEarnPerTurn * turns;
                var totalArmies = armiesWeHaveInOrAroundBonus + totalDeployed;

                var territoriesGettingRemaindersFrom = Math.Min(terrsToTake.Count - SharedUtility.Round(terrsToTake.Count / (float)turns), terrsToTake.Count - 1);
                var remaindersCanUse = territoriesGettingRemaindersFrom * avgRemaindersPerTerritory;

                var armiesToStandGuard = territoriesGettingRemaindersFrom * bot.Settings.OneArmyMustStandGuardOneOrZero;


                var totalNeed = armiesNeeded + armiesToStandGuard;
                var totalHave = totalArmies + remaindersCanUse;
                if (totalHave >= totalNeed)
                    return new CaptureTerritories(turns, totalDeployed - (totalHave - totalNeed));

                if (armiesEarnPerTurn == 0)
                    return null; //we can't take it with what we have, and we are earning no armies.  Just abort rather than infinite loop
            }

#if CSSCALA
            throw new Exception();
#endif
        }
    }
}
