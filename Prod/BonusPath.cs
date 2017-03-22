using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod
{
    /// <summary>
    /// Calculates the path to take the bonus in the fewest number of turns, ignoring armies.
    /// </summary>
    public class BonusPath
    {
        public BonusIDType BonusID;
        public HashSet<TerritoryIDType> TerritoriesOnCriticalPath;
        public int TurnsToTakeByDistance; //how long to take the bonus, assuming we have infinite armies

        public BonusPath(BonusIDType bonusID, int turnsToTakeByDistance, HashSet<TerritoryIDType> terrsOnCriticalPath)
        {
            this.BonusID = bonusID;
            this.TurnsToTakeByDistance = turnsToTakeByDistance;
            this.TerritoriesOnCriticalPath = terrsOnCriticalPath;
        }

        public override string ToString()
        {
            return "TurnsToTakeByDistance=" + TurnsToTakeByDistance;
        }

        public static BonusPath TryCreate(BotMain bot, BonusIDType bonusID, Func<TerritoryStanding, bool> weOwn)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            var allUnownedTerrsInBonus = bonus.Territories.Where(o => !weOwn(bot.Standing.Territories[o])).ToHashSet(true);

            if (allUnownedTerrsInBonus.Count == 0)
                return new BonusPath(bonusID, 0, new HashSet<TerritoryIDType>()); //Already own the bonus. We'll only get here with one-territory bonuses during distribution

            var terrsToTake = allUnownedTerrsInBonus.ToHashSet(true);

            var ownedTerritoriesTraverse = bot.Standing.Territories.Values.Where(o => weOwn(o)).Select(o => o.ID).ToHashSet(true);
            HashSet<TerritoryIDType> finalTerritoriesCaptured = null;

            var turns = 1;

            while (true)
            {
                var takeThisTurn = terrsToTake.Where(o => bot.Map.Territories[o].ConnectedTo.Keys.Any(z => ownedTerritoriesTraverse.Contains(z))).ToHashSet(true);

                if (takeThisTurn.Count == 0)
                {
                    //We can't take it without leaving the bonus.
                    AILog.Log("BonusPath", "  Could not find a way to take bonus " + bot.BonusString(bonus) + " without leaving it");
                    return null;
                }

                if (takeThisTurn.Count == terrsToTake.Count)
                {
                    //We captured the bonus
                    finalTerritoriesCaptured = takeThisTurn;
                    break;
                }

                //Keep expanding!
                turns++;
                ownedTerritoriesTraverse.AddRange(takeThisTurn);
                terrsToTake.RemoveAll(takeThisTurn);
            }

            var terrsWeOwnInOrAroundBonus = bonus.Territories.Concat(bonus.Territories.SelectMany(o => bot.Map.Territories[o].ConnectedTo.Keys)).Where(o => weOwn(bot.Standing.Territories[o])).ToHashSet(false);
            var traverse = allUnownedTerrsInBonus.Concat(terrsWeOwnInOrAroundBonus).ToHashSet(false);

            var criticalPath = new HashSet<TerritoryIDType>();

            foreach(var final in finalTerritoriesCaptured)
            {
                var path = FindPath.TryFindShortestPathReversed(bot, o => weOwn(bot.Standing.Territories[o]), final, o => traverse.Contains(o));

                if (path != null)
                {
                    //AILog.Log("BonusPath", "  Critical path to " + bot.TerrString(final) + " goes " + path.Select(o => bot.TerrString(o)).JoinStrings(" -> "));
                    criticalPath.AddRange(path);
                }
                else
                {
                    AILog.Log("BonusPath", "  Could not find a path to " + bot.TerrString(final));
                }
            }

            //AILog.Log("BonusPath", "With infinite armies, we can take bonus " + bot.BonusString(bonus) + " in " + TurnsToTake + " turns. " + /*" Final territories=" + finalTerritoriesCaptured.Select(o => bot.TerrString(o)).JoinStrings(", ") +*/ "  Critical path=" + TerritoriesOnCriticalPath.Select(o => bot.TerrString(o)).JoinStrings(", "));

            return new BonusPath(bonusID, turns, criticalPath);
        }

    }
}
