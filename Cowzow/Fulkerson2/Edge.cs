/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Fulkerson2
{
    public class Edge
    {
        public readonly BotTerritory Start;
        public readonly BotTerritory End;
        public readonly int Capacity;
        public readonly bool IsStrict;
        public int Flow;

        public Edge(BotTerritory start, BotTerritory end, int capacity, bool isStrict = false)
        {
            Assert.Fatal(start != null, "start is null");
            Assert.Fatal(end != null, "end is null");
            this.Start = start;
            this.End = end;
            this.Capacity = capacity;
            this.Flow = 0;
            this.IsStrict = isStrict;
        }

        public virtual int RemainingFlow()
        {
            return Math.Max(Capacity - Flow, 0);
        }

        public string ID
        {
            get
            {
                return Start.ID + " " + End.ID;
            }
        }
        

        public override string ToString()
        {
            return "< " + Start.Details.Name + " (" + Start.ID + ") flow=" + Flow + " TO " + End.Details.Name + " (" + End.ID + ") strict=" + IsStrict + ">";
        }
    }
}
