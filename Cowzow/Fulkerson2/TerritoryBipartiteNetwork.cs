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
    public class TerritoryBipartiteNetwork : TerritoryNetwork
    {
        public HashSet<TerritoryIDType> Attackers;
        public BotTerritory End;
        internal EdgePriorityComparator Eval;
        public BotTerritory Start;
        public HashSet<TerritoryIDType> Targets;


        public TerritoryBipartiteNetwork(CowzowBot bot, EdgePriorityComparator eval)
            : base(bot)
        {
            Start = new BotTerritory(bot, BotTerritory.DummyID);
            End = new BotTerritory(bot, BotTerritory.DummyID);
            AddVertex(Start);
            AddVertex(End);
            Attackers = new HashSet<TerritoryIDType>();
            Targets = new HashSet<TerritoryIDType>();
            this.Eval = eval;
        }

        public void AddAttacker(BotTerritory r)
        {
            AddAttacker(r, r.Armies - Bot.Settings.OneArmyMustStandGuardOneOrZero);
        }

        public void AddAttacker(BotTerritory r, int armiesAvailable)
        {
            if (!Attackers.Contains(r.ID))
            {
                Attackers.Add(r.ID);
                AddVertex(r);
                AddEdge(Start, r, armiesAvailable);
            }
        }

        public void AddTarget(BotTerritory r)
        {
            AddTarget(r, int.MaxValue);
        }

        public void AddTarget(BotTerritory r, int armiesNeeded)
        {
            if (!Targets.Contains(r.ID))
            {
                Targets.Add(r.ID);
                AddVertex(r);
                AddEdge(r, End, armiesNeeded);
            }
        }

        //public void ClearIntermediateEdges()
        //{
        //    foreach (var r in Attackers)
        //        AdjacencyList[r].Clear();
        //    foreach (var r_1 in Targets)
        //    {
        //        var edges = AdjacencyList[r_1];
        //        foreach (var e in edges.Values)
        //            if (e.End.ID != End.ID)
        //                edges.Remove(e.ID);
        //    }
        //}

        public EdgeHashSet GenerateStrictFlow()
        {
            var residual = CreateResidual();
            var resourceMap = new Dictionary<TerritoryIDType, Edge>();
            foreach (var e in residual[Start.ID].Edges)
                resourceMap[e.End.ID] = e;
            var path = FindStrictPath(Start, End, residual);
            while (path.Count > 0)
            {
                var bottleneck = path[0].RemainingFlow();
                foreach (var e_1 in path)
                    if (e_1.RemainingFlow() < bottleneck)
                        bottleneck = e_1.RemainingFlow();
                SendFlow(path, residual);
                path = FindStrictPath(Start, End, residual);
            }
            var result = new EdgeHashSet();
            foreach (var attacker in Attackers)
                foreach (var e_1 in residual[attacker].Edges)
                    if (e_1.Flow > 0)
                        result.Add(e_1);
            return result;
        }

        private void SendFlow(List<Edge> path, Dictionary<TerritoryIDType, EdgeHashSet> residual)
        {
            var resource = 0;
            for (var i = 1; i < path.Count; i += 2)
            {
                var sender = path[i];
                var receiver = path[i + 1];
                resource = sender.RemainingFlow();
                SendFlowSingle(sender, resource, residual);
                SendFlowSingle(receiver, resource, residual);
                if (i == 1)
                    SendFlowSingle(path[0], resource, residual);
            }
        }

        private void SendFlowSingle(Edge e, int flow, Dictionary<TerritoryIDType, EdgeHashSet> residual)
        {
            e.Flow += flow;
            if (residual.ContainsKey(e.End.ID))
            {
                foreach (var candidate in residual[e.End.ID].Edges)
                    if (candidate.End.ID == e.Start.ID)
                        candidate.Flow -= flow;
            }
        }

        public List<Edge> FindStrictPathWithStart(Edge startEdge, EdgeHashSet visited, Dictionary<TerritoryIDType, EdgeHashSet> residual)
        {
            var resourceMap = new Dictionary<TerritoryIDType, int>();
            foreach (var entryPoint in residual[Start.ID].Edges)
                resourceMap[entryPoint.End.ID] = entryPoint.RemainingFlow();
            var discoveredMap = new Dictionary<string, Edge>();
            var pushFlow = startEdge.RemainingFlow();
            var bestpath = new List<Edge>();
            var stack = new Stack<Edge>();
            stack.Push(startEdge);
            while (stack.Count > 0)
            {
                var curr = stack.Pop();
                var workingFlow = pushFlow;
                if (!IsPositive(curr) && curr.Start.ID != startEdge.End.ID)
                    workingFlow += resourceMap[curr.Start.ID];
                if (visited.Contains(curr))
                    continue;
                if (curr.RemainingFlow() == 0)
                    continue;
                if (!IsPositive(curr) && curr.RemainingFlow() > workingFlow)
                    continue;
                if (!IsPositive(curr))
                    pushFlow = curr.RemainingFlow();
                if (curr.End.ID == End.ID)
                {
                    var candidate = new List<Edge>();
                    var temp = curr;
                    while (temp != null)
                    {
                        candidate.Insert(0, temp);
                        temp = discoveredMap.ContainsKey(temp.ID) ? discoveredMap[temp.ID] : null;
                    }
                    if (candidate.Count > 2)
                    {
                        if (bestpath.Count == 0 || Eval == null || Eval.Compare(candidate[candidate.Count - 2], bestpath[bestpath.Count - 2]) < 0)
                            bestpath = candidate;
                    }
                }
                else
                {
                    visited.Add(curr);
                    var edges = new List<Edge>(residual[curr.End.ID].Edges);
                    if (IsPositive(curr) && Eval != null)
                        edges.Sort((a,b) => Eval.Compare(a, b));
                    for (var i = edges.Count - 1; i >= 0; i--)
                    {
                        var neighbor = edges[i];
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                            discoveredMap[neighbor.ID] = curr;
                        }
                    }
                }
            }
            return bestpath;
        }

        public List<Edge> FindStrictPath(BotTerritory a, BotTerritory b, Dictionary<TerritoryIDType, EdgeHashSet> residual)
        {
            var bestPath = new List<Edge>();
            var visited = new EdgeHashSet();
            foreach (var path in residual[Start.ID].Edges)
                if (path.RemainingFlow() > 0)
                {
                    var candidate = FindStrictPathWithStart(path, visited, residual);
                    if (bestPath.Count == 0 || Eval == null || candidate.Count > 2 && Eval.Compare(candidate[candidate.Count - 2], bestPath[bestPath.Count - 2]) < 0)
                        bestPath = candidate;
                }
            return bestPath;
        }

        private bool IsPositive(Edge e)
        {
            return e.Start.ID == Start.ID || Targets.Contains(e.Start.ID);
        }


        public Dictionary<TerritoryIDType, EdgeHashSet> CreateResidual()
        {
            var residual = new Dictionary<TerritoryIDType, EdgeHashSet>();
            foreach (var key in AdjacencyList.Keys)
                residual[key] = new EdgeHashSet(AdjacencyList[key].Values);
            foreach (var attacker in Attackers)
            {
                var attackerEdges = residual[attacker];
                foreach (var forward in attackerEdges.Edges)
                {
                    var backward = new Edge(forward.End, forward.Start, forward.Capacity, forward.IsStrict);
                    backward.Flow = backward.Capacity;
                    residual[backward.Start.ID].Add(backward);
                }
            }
            return residual;
        }
    }
}
