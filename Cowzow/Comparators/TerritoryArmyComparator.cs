/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Comparators
{
    public class TerritoryArmyComparator : IComparer<BotTerritory>
    {
        public virtual int Compare(BotTerritory e1, BotTerritory e2)
        {
            if (e1.Armies < e2.Armies)
                return -1;
            if (e1.Armies > e2.Armies)
                return 1;
            return 0;
        }
    }
}
