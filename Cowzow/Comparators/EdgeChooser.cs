/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Fulkerson2;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Comparators
{
    public class EdgeChooser
    {
        internal List<Edge> Edges;
        internal EdgePriorityComparator Eval;
        internal Dictionary<TerritoryIDType, int> ResourceMap;

        public EdgeChooser(IEnumerable<Edge> edges, Dictionary<TerritoryIDType, int> resourceMap, EdgePriorityComparator eval)
        {
            this.Edges = edges.ToList();
            this.ResourceMap = resourceMap;
            this.Eval = eval;
        }

        public bool HasNext()
        {
            return Edges.Count > 0;
        }

        public Edge Next()
        {
            var best = Edges[0];
            foreach (var e in Edges)
                if (Eval.Compare(e, best) < 0)
                    best = e;
                else
                {
                    if (Eval.Compare(e, best) == 0)
                    {
                        var bestResource = ResourceMap[best.Start.ID];
                        var currResource = ResourceMap[e.Start.ID];
                        if (currResource > bestResource)
                            best = e;
                    }
                }

            for (var i = 0; i < Edges.Count; i++)
            {
                var curr = Edges[i];
                if (curr.ID == best.ID)
                {
                    Edges.RemoveAt(i);
                    break;
                }
            }

            return best;
        }
    }
}
