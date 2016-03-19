/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;
using WarLight.Shared.AI.Cowzow.Comparators;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Fulkerson2
{
    public class TerritoryNetwork
    {
        public Dictionary<TerritoryIDType, Dictionary<string, Edge>> AdjacencyList;
        public CowzowBot Bot;

        public TerritoryNetwork(CowzowBot bot)
        {
            this.Bot = bot;
            AdjacencyList = new Dictionary<TerritoryIDType, Dictionary<string, Edge>>();
        }

        public Edge GetEdge(BotTerritory a, BotTerritory b)
        {
            foreach (var e in AdjacencyList[a.ID].Values)
                if (e.End.ID == b.ID)
                    return e;

            return null;
            //	throw new Exception("Tried to lookup edge for which this is no vertex" + "Territory A = " + a.ID + " Territory B = " + b.ID);
        }
        
        public void AddEdge(BotTerritory a, BotTerritory b, int capacity, bool isStrict)
        {
            var edge = new Edge(a, b, capacity, isStrict);
            AdjacencyList[a.ID].Add(edge.ID, edge);
            //throw new Exception("Tried to add edge for vertex that does not exist. " +a + " to " + b);
        }

        public void AddEdge(BotTerritory a, BotTerritory b, int capacity)
        {
            var edge = new Edge(a, b, capacity);
            AdjacencyList[a.ID].Add(edge.ID, edge);
            //throw new Exception("Tried to add edge for vertex that does not exist. " + a + " to " + b);
        }

        public void AddEdge(BotTerritory a, BotTerritory b)
        {
            var edge = new Edge(a, b, int.MaxValue);
            AdjacencyList[a.ID].Add(edge.ID, edge);
            //throw new Exception("Tried to add edge for vertex that does not exist. " +a + " to " + b);
        }

        public void AddVertex(BotTerritory r)
        {
            if (!AdjacencyList.ContainsKey(r.ID))
                AdjacencyList[r.ID] = new Dictionary<string, Edge>();
        }

        //public void RemoveVertex(BotTerritory r)
        //{
        //    if (AdjacencyList.ContainsKey(r))
        //    {
        //        AdjacencyList[r].Clear();
        //        ICollection<EdgeHashSet> edgeLists = AdjacencyList.Values;
        //        foreach (var edgeList in edgeLists)
        //            foreach (var e in edgeList)
        //                if (e.Start.TerritoryEquals(r) || e.End.TerritoryEquals(r))
        //                    edgeList.Remove(e);
        //    }
        //}

        //public EdgeHashSet GetAdjacencies(Territory r)
        //{
        //	return adjacencyList[r];
        //}

        public List<Edge> FindPath(BotTerritory a, BotTerritory b, Dictionary<TerritoryIDType, EdgeHashSet> adjacencyMap)
        {
            var discoveredMap = new Dictionary<TerritoryIDType, Edge>();
            var queue = new List<BotTerritory>();
            queue.Add(a);
            while (queue.Count > 0)
            {
                var curr = queue[0];
                queue.RemoveAt(0);

                if (curr.ID == b.ID)
                    break;
                var adjacencies = adjacencyMap[curr.ID].Edges.ToList();
                var tpc = new TerritoryPriorityComparator(Bot);
                adjacencies.Sort((f, s) => tpc.Compare(f, s));
                foreach (var e in adjacencies)
                {
                    var dest = e.End;
                    if (e.RemainingFlow() > 0 && !discoveredMap.ContainsKey(dest.ID))
                    {
                        discoveredMap[dest.ID] = e;
                        queue.Add(dest);
                    }
                }
            }
            var path = new List<Edge>();
            var curr_1 = discoveredMap[b.ID];
            while (curr_1 != null)
            {
                path.Insert(0, curr_1);
                curr_1 = discoveredMap[curr_1.Start.ID];
            }
            return path;
        }
    }
}
