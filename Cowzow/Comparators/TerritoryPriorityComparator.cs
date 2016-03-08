/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;
using WarLight.Shared.AI.Cowzow.Fulkerson2;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Comparators
{
    public class TerritoryPriorityComparator : IComparer<Edge>
    {
        public CowzowBot Bot;
        public TerritoryPriorityComparator(CowzowBot bot)
        {
            this.Bot = bot;
        }

        public virtual int Compare(Edge o1, Edge o2)
        {
            var rank1 = GetRank(o1.End);
            var rank2 = GetRank(o2.End);
            if (rank1 < rank2)
                return -1;
            if (rank1 > rank2)
                return 1;
            var srScore1 = o1.End.Bonuses.Sum(b => b.GuessedArmiesNotOwnedByUs);
            var srScore2 = o2.End.Bonuses.Sum(b => b.GuessedArmiesNotOwnedByUs);
            if (srScore1 < srScore2)
                return -1;
            if (srScore1 > srScore2)
                return 1;
            return 0;
        }

        private int GetRank(BotTerritory r)
        {
            if (r.OwnerPlayerID == Bot.Me.ID)
                return 2;
            if (r.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                return 1;
            return 0;
        }
    }
}
