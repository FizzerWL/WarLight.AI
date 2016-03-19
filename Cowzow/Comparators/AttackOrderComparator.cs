/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Bot;
using WarLight.Shared.AI.Cowzow.Map;
using WarLight.Shared.AI.Cowzow.Move;

namespace WarLight.Shared.AI.Cowzow.Comparators
{
    public class AttackOrderComparator : IComparer<BotOrderAttackTransfer>
    {
        internal BonusAnalyzer Analyzer;
        internal HashSet<TerritoryIDType> Placements;
        public CowzowBot Bot;

        public AttackOrderComparator(CowzowBot bot)
        {
            this.Bot = bot;
            Placements = bot.MyDeployments.ToHashSet(true);
            Analyzer = bot.Analyzer;
        }

        public int Compare(BotOrderAttackTransfer a, BotOrderAttackTransfer b)
        {
            var rank1 = GetRank(a);
            var rank2 = GetRank(b);
            if (rank1 > rank2)
                return -1;
            if (rank1 < rank2)
                return 1;
            if (a.Armies > b.Armies)
                return -1;
            if (a.Armies < b.Armies)
                return 1;
            return 0;
        }

        public int GetRank(BotOrderAttackTransfer e)
        {
            if (Bot.IsOpponent(e.ToTerritory.OwnerPlayerID))
            {
                var allyCount = 0;
                var count = 0;
                foreach (var n in e.ToTerritory.Neighbors)
                {
                    if (n.OwnerPlayerID == Bot.Me.ID && n.Bonuses.Any(o => o.ArmiesReward > 1))
                        allyCount++;
                    if (n.OwnerPlayerID == Bot.Me.ID && n.Bonuses.All(o => o.GuessedArmiesNotOwnedByUs == 0 && o.ArmiesReward > 1))
                        count++;
                }
                if (count > 1 || (e.ToTerritory.Bonuses.Any(o => o.GuessedArmiesNotOwnedByUs == e.ToTerritory.Armies) && allyCount > 1))
                    return 6;
            }
            if (e.FromTerritory.GetStrongestNearestEnemy() >= e.FromTerritory.Armies)
                return 5;
            if (Placements.Contains(e.FromTerritory.ID))
            {
                if (e.FromTerritory.Bonuses.Any(o => o.GuessedArmiesNotOwnedByUs == 0))
                    return -3;
                if (e.ToTerritory.Bonuses.Any(Analyzer.SoonBeOwned))
                    return -1;
                if (Bot.IsOpponent(e.ToTerritory.OwnerPlayerID))
                    return -2;
                if (e.ToTerritory.OwnerPlayerID == Bot.Me.ID)
                    return 0;
                return 1;
            }
            if (Bot.IsOpponent(e.ToTerritory.OwnerPlayerID))
                return 4;
            if (e.ToTerritory.OwnerPlayerID == Bot.Me.ID)
                return 2;
            return 3;
        }
    }
}
