using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod
{
    /// <summary>
    /// Calculates the path to take the bonus in the fewest number of turns, ignoring armies.
    /// </summary>
    public class BonusPath
    {
        public HashSet<TerritoryIDType> TerritoriesOnCriticalPath;
        public int TurnsToTakeByDistance; //how long to take the bonus, assuming we have infinite armies

        public BonusPath(BotMain bot, BonusIDType bonusID, Func<TerritoryStanding, bool> weOwn)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            var allUnownedTerrsInBonus = bonus.Territories.Where(o => !weOwn(bot.Standing.Territories[o])).ToHashSet(true);

            Assert.Fatal(allUnownedTerrsInBonus.Count > 0, "We already have the bonus");

            var terrsToTake = allUnownedTerrsInBonus.ToHashSet(true);

            var ownedTerritoriesTraverse = bot.Standing.Territories.Values.Where(o => weOwn(o)).Select(o => o.ID).ToHashSet(true);
            HashSet<TerritoryIDType> finalTerritoriesCaptured = null;

            this.TurnsToTakeByDistance = 1;

            while (true)
            {

                var takeThisTurn = terrsToTake.Where(o => bot.Map.Territories[o].ConnectedTo.Any(z => ownedTerritoriesTraverse.Contains(z))).ToHashSet(true);

                if (takeThisTurn.Count == 0)
                {
                    //We can't take it without leaving the bonus.
                    AILog.Log("BonusPath", "  Could not find a way to take bonus " + bot.BonusString(bonus) + " without leaving it");
                    TurnsToTakeByDistance = int.MaxValue;
                    TerritoriesOnCriticalPath = new HashSet<TerritoryIDType>();
                    return;
                }

                if (takeThisTurn.Count == terrsToTake.Count)
                {
                    //We captured the bonus
                    finalTerritoriesCaptured = takeThisTurn;
                    break;
                }

                //Keep expanding!
                this.TurnsToTakeByDistance++;
                ownedTerritoriesTraverse.AddRange(takeThisTurn);
                terrsToTake.RemoveAll(takeThisTurn);
            }

            var terrsWeOwnInOrAroundBonus = bonus.Territories.Concat(bonus.Territories.SelectMany(o => bot.Map.Territories[o].ConnectedTo)).Where(o => weOwn(bot.Standing.Territories[o])).ToHashSet(false);
            var traverse = allUnownedTerrsInBonus.Concat(terrsWeOwnInOrAroundBonus).ToHashSet(false);

            TerritoriesOnCriticalPath = new HashSet<TerritoryIDType>();

            foreach(var final in finalTerritoriesCaptured)
            {
                var path = FindPath.TryFindShortestPath(bot, o => weOwn(bot.Standing.Territories[o]), final, o => traverse.Contains(o));

                if (path != null)
                {
                    //AILog.Log("BonusPath", "  Critical path to " + bot.TerrString(final) + " goes " + path.Select(o => bot.TerrString(o)).JoinStrings(" -> "));
                    TerritoriesOnCriticalPath.AddRange(path);
                }
                else
                {
                    AILog.Log("BonusPath", "  Could not find a path to " + bot.TerrString(final));
                }
            }

            //AILog.Log("BonusPath", "With infinite armies, we can take bonus " + bot.BonusString(bonus) + " in " + TurnsToTake + " turns. " + /*" Final territories=" + finalTerritoriesCaptured.Select(o => bot.TerrString(o)).JoinStrings(", ") +*/ "  Critical path=" + TerritoriesOnCriticalPath.Select(o => bot.TerrString(o)).JoinStrings(", "));
        }

    }
}
