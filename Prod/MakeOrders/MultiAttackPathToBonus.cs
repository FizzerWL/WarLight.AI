using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public class MultiAttackPathToBonus
    {
        public BotMain Bot;
        public TerritoryIDType StartFrom;
        public BonusIDType BonusID;
        public int EstArmiesNeededToCapture; //estimated total stack size we need to start with to get there and capture it.  It's an estimate since we don't know the actual order we're take the territories -- we created this just by ordering them arbitrarily
        public int JumpsToGetToBonus; //How far we are from the bonus. 0 means we're in or adjacent to the bonus
        public int ArmiesNeedToKillToGetThere;
        public List<TerritoryIDType> PathToGetThere;

        private MultiAttackPathToBonus(BotMain bot, TerritoryIDType startFrom, BonusIDType bonusID, int jumpsToGetToBonus, int armiesNeededToCapture, int armiesNeedToKillToGetThere, List<TerritoryIDType> pathToGetThere)
        {
            this.Bot = bot;
            this.StartFrom = startFrom;
            this.BonusID = bonusID;
            this.JumpsToGetToBonus = jumpsToGetToBonus;
            this.EstArmiesNeededToCapture = armiesNeededToCapture;
            this.ArmiesNeedToKillToGetThere = armiesNeedToKillToGetThere;
            this.PathToGetThere = pathToGetThere;
        }
        

        /// <summary>
        /// Returns null if we can't find a way to take the bonus or if we already own it
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="bonusID"></param>
        /// <returns></returns>
        public static MultiAttackPathToBonus TryCreate(BotMain bot, TerritoryIDType startFrom, BonusIDType bonusID, GameStanding standing, int maxDistance)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            var allUnownedTerrsInBonus = bonus.Territories.Where(o => standing.Territories[o].OwnerPlayerID != bot.PlayerID).ToHashSet(true);

            if (allUnownedTerrsInBonus.Count == 0)
                return null; //already own it

            HashSet<TerritoryIDType> terrsWeEnterBonus;

            int jumpsToGetToBonus = DistanceToTerrs(bot, startFrom, bonus.Territories.ToHashSet(true), standing, maxDistance, out terrsWeEnterBonus);
            if (jumpsToGetToBonus == int.MaxValue)
                return null; //can't take it within a reasonable searching distance

            if (jumpsToGetToBonus == 0)
            {
                //We're already in it
                var armiesNeededToCapture = bot.ArmiesToTakeMultiAttack(allUnownedTerrsInBonus.Select(o => ExpansionHelper.GuessNumberOfArmies(bot, o, standing, MultiAttackExpand.GuessOpponentNumberOfArmiesInFog)));
                return new MultiAttackPathToBonus(bot, startFrom, bonusID, 0, armiesNeededToCapture, 0, new List<TerritoryIDType>());
            }
            else
            {
                var pathToGetThere = FindPath.TryFindShortestPath(bot, startFrom, t => terrsWeEnterBonus.Contains(t), visit => visit == startFrom || bot.IsTeammateOrUs(standing.Territories[visit].OwnerPlayerID) == false);
                if (pathToGetThere == null)
                    return null;

                var getThere = pathToGetThere.ExceptOne(pathToGetThere.Last());
                var armiesNeededToCapture = bot.ArmiesToTakeMultiAttack(getThere.Concat(allUnownedTerrsInBonus).Select(o => ExpansionHelper.GuessNumberOfArmies(bot, o, standing, MultiAttackExpand.GuessOpponentNumberOfArmiesInFog)));

                return new MultiAttackPathToBonus(bot, startFrom, bonusID, jumpsToGetToBonus, armiesNeededToCapture, getThere.Sum(o => ExpansionHelper.GuessNumberOfArmies(bot, o, standing).DefensePower), pathToGetThere);
            }

        }
        
        private static int DistanceToTerrs(BotMain bot, TerritoryIDType startFrom, HashSet<TerritoryIDType> terrs, GameStanding standing, int maxDistance, out HashSet<TerritoryIDType> terrsWeEntered)
        {
            Assert.Fatal(terrs.Count > 0, "No terrs");
            var visited = new HashSet<TerritoryIDType>();
            visited.Add(startFrom);

            var contains = visited.Where(o => terrs.Contains(o)).ToHashSet(true);
            if (contains.Count > 0)
            {
                //We're already there
                terrsWeEntered = new HashSet<TerritoryIDType>();
                return 0;
            }

            int distance = 1;

            while (true)
            {
                var expand = visited.SelectMany(o => bot.Map.Territories[o].ConnectedTo.Keys).Where(o => visited.Contains(o) == false && standing.Territories[o].OwnerPlayerID != bot.PlayerID).ToHashSet(false); 

                if (expand.Count == 0)
                {
                    terrsWeEntered = null;
                    return int.MaxValue;
                }

                contains = expand.Where(o => terrs.Contains(o)).ToHashSet(true);
                if (contains.Count > 0)
                {
                    //Found it
                    terrsWeEntered = contains;
                    return distance;
                }

                distance++;
                if (distance > maxDistance)
                {
                    terrsWeEntered = null;
                    return int.MaxValue;
                }
                visited.AddRange(expand);
            }

#if CS2HX || CSSCALA
            throw new Exception("Never");
#endif


        }
    }
}
