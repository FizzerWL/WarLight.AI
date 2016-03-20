/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;
using WarLight.Shared.AI.Cowzow.Comparators;
using WarLight.Shared.AI.Cowzow.Fulkerson2;

namespace WarLight.Shared.AI.Cowzow.Map
{
    public class MapUtils
    {
        public static Edge FindBest(List<Edge> edgeList, EdgePriorityComparator eval)
        {
            var best = edgeList[0];
            foreach (var e in edgeList)
                if (eval.Compare(e, best) < 0)
                    best = e;
            edgeList.Remove(best);
            return best;
        }

        public static List<BotTerritory> FilterTerritoriesByPlayerList(ICollection<BotTerritory> territories, PlayerIDType playerName)
        {
            var result = new List<BotTerritory>();
            foreach (var r in territories)
                if (r.OwnerPlayerID == playerName)
                    result.Add(r);
            return result;
        }

        public static int[,] ConstructAdjacencyGraph(CowzowBot Bot, IList<BotTerritory> myAttackers, IList<BotTerritory> myTargets)
        {
            var graph = new int[myAttackers.Count, myTargets.Count];
            for (var i = 0; i < myAttackers.Count; i++)
            {
                var myAttacker = myAttackers[i];
                var myNeighbors = myAttacker.Neighbors;
                for (var j = i; j < myTargets.Count; j++)
                {
                    var myTarget = myTargets[j];
                    if (myNeighbors.Contains(myTarget))
                    {
                        if (myTarget.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                            graph[i, j] = (int)(myTarget.Armies * 1.5 + 1);
                        else
                            graph[i, j] = (int)(myTarget.Armies * 1.5 + 5);
                    }
                }
            }

            return graph;
        }
    }
}
