/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Fulkerson2;
using WarLight.Shared.AI.Cowzow.Map;
using WarLight.Shared.AI.Cowzow.Move;

namespace WarLight.Shared.AI.Cowzow.Bot
{
    public class OrderManager
    {
        public Dictionary<TerritoryIDType, EdgeHashSet> Orders;
        public CowzowBot Bot;

        public OrderManager(CowzowBot bot, HashSet<TerritoryIDType> myTerritories)
        {
            this.Bot = bot;
            Orders = new Dictionary<TerritoryIDType, EdgeHashSet>();
            foreach (var r in myTerritories)
                Orders[r] = new EdgeHashSet();
        }

        public EdgeHashSet GetOrders(TerritoryIDType r)
        {
            if (Orders.ContainsKey(r))
                return Orders[r];
            return new EdgeHashSet();
        }

        public List<BotOrderAttackTransfer> GetFormalOrders()
        {
            var edges = new List<Edge>();
            foreach (var entry in Orders)
                edges.AddRange(entry.Value.Edges);

            var list = new List<BotOrderAttackTransfer>();
            foreach (var e in edges)
                if (e.Start.ID != e.End.ID)
                    list.Add(new BotOrderAttackTransfer(Bot.Me.ID, e.Start, e.End, e.Flow));
            return list;
        }

        public Dictionary<TerritoryIDType, EdgeHashSet> GetOrders()
        {
            return Orders;
        }

        public Dictionary<TerritoryIDType, int> GetRemainingMoves()
        {
            var remainingOrders = new Dictionary<TerritoryIDType, int>();
            foreach (var entry in Orders)
            {
                var attacker = entry.Key;
                var moves = entry.Value;
                var sum = 0;
                foreach (var e in moves.Edges)
                    sum += e.Flow;

                var remaining = Bot.BotMap.Territories[attacker].Armies - 1 - sum;
                if (remaining > 0)
                    remainingOrders[attacker] = remaining;
            }
            return remainingOrders;
        }

        public void AddOrder(BotTerritory attacker, BotTerritory target, int troops)
        {
            if (!Orders.ContainsKey(attacker.ID))
                throw new Exception("Tried to add order for territory not in the set " + attacker);

            var edges = Orders[attacker.ID];
            var candidate = new Edge(attacker, target, troops);
            candidate.Flow = troops;
            if (!edges.Contains(candidate))
                edges.Add(candidate);
            else
            {
                foreach (var e in edges.Edges)
                    if (e.End.ID == target.ID)
                    {
                        e.Flow += troops;
                        break;
                    }
            }
        }
    }
}
