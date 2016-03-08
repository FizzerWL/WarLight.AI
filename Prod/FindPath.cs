using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod
{
    class FindPath
    {
        public static List<TerritoryIDType> TryFindShortestPath(BotMain bot, Func<TerritoryIDType, bool> start, TerritoryIDType finish, Func<TerritoryIDType, bool> visitOpt = null)
        {
            var ret = TryFindShortestPath(bot, finish, start, visitOpt);
            if (ret == null)
                return null;
            ret.RemoveAt(ret.Count - 1);
            ret.Reverse();
            ret.Add(finish);
            return ret;
        }

        public static List<TerritoryIDType> TryFindShortestPath(BotMain bot, TerritoryIDType start, Func<TerritoryIDType, bool> finish, Func<TerritoryIDType, bool> visitOpt = null)
        {
            var previous = new Dictionary<TerritoryIDType, TerritoryIDType>();
            var distances = new Dictionary<TerritoryIDType, int>();
            var nodes = new List<TerritoryIDType>();

            foreach (var vertex in bot.Map.Territories.Keys.Where(o => visitOpt == null || visitOpt(o)))
            {
                if (vertex == start)
                    distances[vertex] = 0;
                else
                    distances[vertex] = int.MaxValue;

                nodes.Add(vertex);
            }

            while (true)
            {
                if (nodes.Count == 0)
                    return null;

                nodes.Sort((x, y) => distances[x] - distances[y]);

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (finish(smallest))
                {
                    var ret = new List<TerritoryIDType>();
                    while (previous.ContainsKey(smallest))
                    {
                        ret.Add(smallest);
                        smallest = previous[smallest];
                    }

                    ret.Reverse();
                    return ret;
                }

                if (distances[smallest] == int.MaxValue)
                    return null;

                foreach (var neighbor in bot.Map.Territories[smallest].ConnectedTo.Where(o => visitOpt == null || visitOpt(o)))
                {
                    var alt = distances[smallest] + 1;
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = smallest;
                    }
                }
            }
        }

        //BotMain Bot;
        //HashSet<TerritoryIDType> Terrs;
        //HashSet<TerritoryIDType> Visited = new HashSet<TerritoryIDType>();

        //public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, List<TerritoryIDType>>> Results;

        //public FindPath(BotMain bot, IEnumerable<TerritoryIDType> traverse)
        //{
        //    this.Bot = bot;
        //    this.Terrs = traverse.ToHashSet(false);
        //    this.Results = Terrs.ToDictionary(o => o, o => Bot.Map.Territories[o].ConnectedTo.Where(z => Terrs.Contains(z)).ToDictionary(z => z, z => new List<TerritoryIDType>()));

        //    Traverse(Terrs.First());
        //}

        //void Traverse(TerritoryIDType node)
        //{
        //    if (Visited.Contains(node))
        //        return;
        //    Visited.Add(node);

        //    foreach (var neighbor in Bot.Map.Territories[node].ConnectedTo.Where(o => Terrs.Contains(o)))
        //    {
        //        Traverse(neighbor);
        //        foreach (var key in Results[neighbor].Keys.Where(key => key != node))
        //            if (!Results[node].ContainsKey(key) || Results[neighbor][key].Count < Results[node][key].Count)
        //                Results[node][key] = new[] { neighbor }.Concat(Results[neighbor][key]).ToList();
        //    }
        //}

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();
        //    foreach (var result in Results)
        //    {
        //        sb.AppendLine(Bot.TerrString(result.Key) + " connects to: ");
        //        foreach (var conn in result.Value)
        //            sb.AppendLine(" - " + Bot.TerrString(conn.Key) + " through " + conn.Value.Select(o => Bot.TerrString(o)).JoinStrings(" -> "));
        //    }
        //    return sb.ToString();
        //}

    }
}
