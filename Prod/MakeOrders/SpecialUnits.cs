using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    static class SpecialUnits
    {
        public static void Go(BotMain bot)
        {
            foreach(var terr in bot.Standing.Territories.Values)
            {
                foreach(var su in terr.NumArmies.SpecialUnits)
                    if (su.OwnerID == bot.PlayerID) 
                    {
                        if (su is Commander)
                            DoCommander(bot, terr, (Commander)su);
                        else if (su.IsBoss())
                            DoBoss(bot, terr, su);
                    }
            }
        }


        private static TerritoryIDType? CommanderDirective(BotMain bot, TerritoryIDType commanderOn)
        {
            var directive = bot.Directives.SingleOrDefault(o => o.StartsWith("CommanderRunTo "));
            if (directive == null)
                return null;

            var runTo = (TerritoryIDType)int.Parse(directive.RemoveFromStartOfString("CommanderRunTo "));
            return FindPath.TryFindShortestPath(bot, commanderOn, t => t == runTo)[0];
        }

        /// <summary>
        /// Runs the commander away from opponents
        /// </summary>
        private static void DoCommander(BotMain bot, TerritoryStanding cmdrTerritory, Commander cmdr)
        {
            var directive = CommanderDirective(bot, cmdrTerritory.ID);
            if (directive.HasValue)
            {
                AILog.Log("SpecialUnits", "Directive directs us to move the commander from " + bot.TerrString(cmdrTerritory.ID) + " to " + bot.TerrString(directive.Value));

                if (directive.Value != cmdrTerritory.ID)
                    bot.Orders.AddAttack(cmdrTerritory.ID, directive.Value, AttackTransferEnum.AttackTransfer, cmdrTerritory.NumArmies.NumArmies, false, commanders: true);
                bot.AvoidTerritories.Add(cmdrTerritory.ID); //add this so we don't deploy there, we want the commander to stay alone
                return;
            }

            var powerDiff = bot.Map.Territories[cmdrTerritory.ID].ConnectedTo.Keys
                .Select(o => bot.Standing.Territories[o])
                .Where(o => bot.IsOpponent(o.OwnerPlayerID) && o.NumArmies.Fogged == false)
                .Sum(o => o.NumArmies.AttackPower)
                - cmdrTerritory.NumArmies.DefensePower;
            var toDeploy = Math.Max(0, powerDiff);
            if (powerDiff > 0)
            {
                if (bot.UseRandomness)
                    toDeploy = SharedUtility.Round(toDeploy * RandomUtility.BellRandom(0.5, 1.5));
                if (toDeploy > bot.MakeOrders.IncomeTracker.RemainingUndeployed)
                    toDeploy = bot.MakeOrders.IncomeTracker.RemainingUndeployed;

                if (toDeploy > 0 && bot.Orders.TryDeploy(cmdrTerritory.ID, toDeploy))
                    AILog.Log("SpecialUnits", "Deployed " + toDeploy + " to defend commander");
            }

            //Consider this territory and all adjacent territories.  Which is the furthest from any enemy?
            var terrDistances = bot.Map.Territories[cmdrTerritory.ID].ConnectedTo.Keys.ConcatOne(cmdrTerritory.ID)
                .Where(o => bot.Standing.Territories[o].OwnerPlayerID == bot.PlayerID || bot.Standing.Territories[o].NumArmies.DefensePower <= 4) //don't go somewhere that's defended heavily
                .ToDictionary(o => o, o => bot.DistanceFromEnemy(o));

            AILog.Log("SpecialUnits", "Commander run options: " + terrDistances.Select(o => bot.TerrString(o.Key) + " dist=" + o.Value).JoinStrings(", "));

            var sorted = terrDistances.OrderByDescending(o => o.Value).ToList();
            sorted.RemoveWhere(o => o.Value < sorted[0].Value);

            var runTo = bot.UseRandomness ? sorted.Random().Key : sorted[0].Key;

            if (runTo == cmdrTerritory.ID)
                return; //already there

            AILog.Log("SpecialUnits", "Moving commander from " + bot.TerrString(cmdrTerritory.ID) + " to " + bot.TerrString(runTo));
            bot.Orders.AddAttack(cmdrTerritory.ID, runTo, AttackTransferEnum.AttackTransfer, cmdrTerritory.NumArmies.NumArmies + toDeploy, false, commanders: true);
        }

        public class BossRoute
        {
            public BotMain Bot;
            public List<TerritoryIDType> Route;
            public string Name;
            public double Chance; //0 to 1
            public double Weight;

            public BossRoute(string rawCmd, BotMain bot)
            {
                this.Bot = bot;
                this.Route = new List<TerritoryIDType>();
                foreach(var split in rawCmd.RemoveFromStartOfString("BossRoute ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (split.StartsWith("Chance="))
                        this.Chance = SharedUtility.ParseOrZero(split.RemoveFromStartOfString("Chance=")) / 100.0;
                    else if (split.StartsWith("Weight="))
                        this.Weight = SharedUtility.ParseOrZero(split.RemoveFromStartOfString("Weight=")) / 100.0;
                    else if (split.StartsWith("Name="))
                        this.Name = split.RemoveFromStartOfString("Name=");
                    else
                        this.Route.Add((TerritoryIDType)int.Parse(split));
                }
            }

            public override string ToString()
            {
                return "Route Name=" + Name + " Chance=" + Chance + " Weight=" + Weight + " path=" + Route.Select(o => o.ToString()).JoinStrings(",");
            }

            public TerritoryIDType? NextTerr(TerritoryIDType from)
            {
                for (int i = 0; i < Route.Count; i++)
                {
                    if (Route[i] == from)
                    {
                        var next = i + 1 == Route.Count ? Route[0] : Route[i + 1]; //wrap around
                        if (!Bot.Map.Territories[from].ConnectedTo.ContainsKey(next))
                            return null;
                        return next;
                    }
                }

                return null;
            }
        }
        

        private static void DoBoss(BotMain bot, TerritoryStanding terr, SpecialUnit su)
        {
            AILog.Log("SpecialUnits", "Considering boss " + su.ID + " on " + bot.TerrString(terr.ID));

            var routes = bot.Directives.Where(o => o.StartsWith("BossRoute ")).Select(o => new BossRoute(o, bot)).ToList();

            var routeNexts = routes.Select(o => new { Route = o, Terr = o.NextTerr(terr.ID) }).Where(o => o.Terr.HasValue).ToList();

            if (routeNexts.Count > 0)
            {
                var routeNext = routeNexts.WeightedRandom(o => o.Route.Weight);
                AILog.Log("SpecialUnits", routeNexts.Count + " matching routes: " + routeNexts.Select(o => o.Route.Name).JoinStrings(", ") + ", selected " + routeNext.Route);

                if (RandomUtility.RandomPercentage() > routeNext.Route.Chance)
                    AILog.Log("SpecialUnits", "Skipping boss route to " + routeNext.Terr.Value + " due to failed random chance. ");
                else
                {
                    AILog.Log("SpecialUnits", "Moving boss along route to " + bot.TerrString(routeNext.Terr.Value) + ". ");
                    bot.Orders.AddAttack(terr.ID, routeNext.Terr.Value, AttackTransferEnum.AttackTransfer, 0, true, bosses: true);
                    bot.AvoidTerritories.Add(routeNext.Terr.Value);
                    return;
                }
            }
            else if (routes.Count > 0)
            {
                //Move towards the nearest route territory. If there's a tie, take the one that's furthest along in that route
                var terrRoutes = routes.SelectMany(r => r.Route.Select((t, i) => new { Route = r, Terr = t, Index = i }))
                    .GroupBy(o => o.Terr)
                    .Select(o => o.MaxSelectorOrDefault(r => r.Index))
                    .ToDictionary(o => o.Terr, o => o);

                var visited = new HashSet<TerritoryIDType>();
                visited.Add(terr.ID);

                while (true)
                {
                    var visit = visited.SelectMany(o => bot.Map.Territories[o].ConnectedTo.Keys).ToHashSet(false);
                    if (visit.Count == 0)
                        throw new Exception("Never found route territory");

                    var visitOnRoute = visit.Where(o => terrRoutes.ContainsKey(o)).ToList();
                    if (visitOnRoute.Count > 0)
                    {
                        var final = visitOnRoute.Select(o => terrRoutes[o]).MaxSelectorOrDefault(o => o.Index);
                        if (RandomUtility.RandomPercentage() > final.Route.Chance)
                        {
                            AILog.Log("SpecialUnits", "Skipping moving boss to route due to failed random check: " + final.Route);
                            break;
                        }
                        else
                        {
                            var move = FindPath.TryFindShortestPath(bot, terr.ID, t => t == final.Terr);
                            AILog.Log("SpecialUnits", "Moving boss to get back to route. Moving to " + bot.TerrString(move[0]) + " to get to " + bot.TerrString(final.Terr) + " index=" + final.Index + " " + final.Route);
                            bot.Orders.AddAttack(terr.ID, move[0], AttackTransferEnum.AttackTransfer, 0, true, bosses: true);
                            bot.AvoidTerritories.Add(final.Terr);
                            return;
                        }
                    }

                    visited.AddRange(visit);
                }

                
            }

            var attackCandidates = bot.Map.Territories[terr.ID].ConnectedTo.Keys.Select(o => bot.Standing.Territories[o])
                .Where(o => !bot.IsTeammateOrUs(o.OwnerPlayerID) && o.NumArmies.DefensePower < 300 && !bot.AvoidTerritories.Contains(o.ID))
                .ToList();

            if (attackCandidates.Count > 0)
            {
                var ranks = attackCandidates.ToDictionary(o => o.ID, ts =>
                {
                    if (bot.IsOpponent(ts.OwnerPlayerID))
                        return bot.Players[ts.OwnerPlayerID].IsAI ? 3 : 2; //prefer human player
                    else if (ts.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                        return 1;
                    else
                        throw new Exception("Unexpected owner " + ts.OwnerPlayerID);

                });

                var max = ranks.Values.Max();
                var to = ranks.Where(o => o.Value == max).Random().Key;
                AILog.Log("SpecialUnits", "Normal boss move to " + bot.TerrString(to));
                bot.Orders.AddAttack(terr.ID, to, AttackTransferEnum.AttackTransfer, 0, false, bosses: true);
                bot.AvoidTerritories.Add(to);
            }
            else
            {
                //Surrounded by ourself or teammates. Move towards enemy
                var move = bot.MoveTowardsNearestBorderNonNeutralThenNeutral(terr.ID);
                if (move.HasValue)
                {
                    AILog.Log("SpecialUnits", "Landlocked boss move to " + bot.TerrString(move.Value));
                    bot.Orders.AddAttack(terr.ID, move.Value, AttackTransferEnum.AttackTransfer, 0, false, bosses: true);
                }
                
            }
        }

    }
}
