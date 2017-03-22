using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public enum MultiAttackPlanType
    {
        MainStack, OneTerritoryOffshoot
    }
    class MultiAttackPlan
    {
        public BotMain Bot;
        public TerritoryIDType To;
        public MultiAttackPlanType Type;
        public MultiAttackPlan(BotMain bot, TerritoryIDType to, MultiAttackPlanType type)
        {
            this.Bot = bot;
            this.To = to;
            this.Type = type;
        }

        public override string ToString()
        {
            if (Type == MultiAttackPlanType.MainStack)
                return Bot.TerrString(To);
            else
                return Bot.TerrString(To) + " " + Type;
        }

        public static List<MultiAttackPlan> TryCreate(BotMain bot, MultiAttackPathToBonus pathToBonus, GameStanding standing, TerritoryIDType terr)
        {
            var ret = new List<MultiAttackPlan>();

            //First get us to the bonus via our pre-defined path.  Skip the last one, as we don't want to actually enter the bonus yet
            for (int i = 0; i < pathToBonus.PathToGetThere.Count - 1; i++)
                ret.Add(new MultiAttackPlan(bot, pathToBonus.PathToGetThere[i], MultiAttackPlanType.MainStack));

            var stackOn = ret.Count > 0 ? ret.Last().To : terr;

            var bonus = bot.Map.Bonuses[pathToBonus.BonusID];
            var allUnownedTerrsInBonus = bonus.Territories.Where(o => standing.Territories[o].OwnerPlayerID != bot.PlayerID).ToHashSet(true);

            var visited = new HashSet<TerritoryIDType>();
            var stack = new Stack<MultiAttackPlan>();
            if (!TryTraverseBonus(bot, standing, allUnownedTerrsInBonus.ToHashSet(true), stackOn, visited, stack, 0))
                return null;

            return ret.Concat(Enumerable.Reverse(stack).Skip(1)).ToList(); //skip the first one, as that's the one we're already on.  Our plan just contains the movements we want to make

        }

        private static bool TryTraverseBonus(BotMain bot, GameStanding standing, HashSet<TerritoryIDType> allUnownedTerrsInBonus, TerritoryIDType terrID, HashSet<TerritoryIDType> visited, Stack<MultiAttackPlan> stack, int depth)
        {
            if (depth > 20)
                return false; //prevent stack overflows. If the bonus is too deep, we'll just skip it

            if (bot.PastTime(10))
                return false; //don't look for too long. This algorithm can take forever on large maps, so abort if we're slow.

            visited.Add(terrID);
            stack.Push(new MultiAttackPlan(bot, terrID, MultiAttackPlanType.MainStack));

            if (allUnownedTerrsInBonus.All(o => visited.Contains(o)))
                return true; //we did it, we traversed the bonus

            var nextSteps = bot.Map.Territories[terrID].ConnectedTo.Keys.Where(o => !visited.Contains(o) && allUnownedTerrsInBonus.Contains(o)).ToList();

            //Disable offshoots.  Their plan calculates correctly, however the code that calculates the number of armies we need doesn't take into account offshoots, so it ends up using remainders when we don't really get those remainders.  This makes them not able to complete the bonus.
            //if (nextSteps.Count == 0)
            //{
            //    //We're an offshoot.
            //    stack.Peek().Type = MultiAttackPlanType.OneTerritoryOffshoot;
            //    return false;
            //}

            //Traverse from big to small.  We want to take bigger territories first so that more remainders are left for taking smaller ones
            foreach (var next in nextSteps.OrderByDescending(o => ExpansionHelper.GuessNumberOfArmies(bot, o, standing).DefensePower))
            {
                if (TryTraverseBonus(bot, standing, allUnownedTerrsInBonus, next, visited, stack, depth + 1))
                    return true;
            }

            //Could not find a solution through here. Back up and keep searching.
            visited.Remove(terrID);
            stack.Pop();
            return false;
        }
    }
}
