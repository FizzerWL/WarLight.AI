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
    public class EdgePriorityComparator : IComparer<Edge>
    {
        private readonly BonusAnalyzer Analyzer;
        private readonly Dictionary<string, double> Cache; //Key contains edge IDs
        private readonly Dictionary<TerritoryIDType, int> CaptureCosts;
        public Dictionary<string, Edge> DumpSet = new Dictionary<string, Edge>();
        public CowzowBot Bot;

        public EdgePriorityComparator(CowzowBot bot, Dictionary<TerritoryIDType, int> captureCosts, BonusAnalyzer analyzer)
        {
            this.Bot = bot;
            Cache = new Dictionary<string, double>();
            this.Analyzer = analyzer;
            this.CaptureCosts = captureCosts;
            if (analyzer == null)
                throw new ArgumentNullException();
        }

        public int Compare(Edge e1, Edge e2)
        {
            var score1 = GetScore(e1);
            var score2 = GetScore(e2);
            if (score1 > score2)
                return -1;
            if (score1 < score2)
                return 1;
            if (e1.End.Armies < e2.End.Armies)
                return -1;
            if (e1.End.Armies > e2.End.Armies)
                return 1;
            return (int)e1.End.ID - (int)e2.End.ID;
        }
        

        public double GetScore(Edge e)
        {
            if (Cache.ContainsKey(e.ID))
                return Cache[e.ID];

            var score = 0.0;

            foreach (var bonus in e.End.Bonuses)
                score += GetBonusScore(e, bonus);

            score = score / CaptureCosts[e.End.ID];
            Cache[e.ID] = score;
            return score;
        }


        private double GetBonusScore(Edge e, BotBonus bonus)
        {
            var end = e.End;

            var score = 0.0;

            if (end.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
            {
                score += (double)bonus.ArmiesReward * (double)end.Armies / (double)bonus.GuessedArmiesNotOwnedByUs;
                if (bonus.GuessedArmiesNotOwnedByUs >= 7)
                    score *= 0.7 - ((bonus.GuessedArmiesNotOwnedByUs - 7) * 0.05);
                if (bonus.ArmiesToNeutralsRatio < 0.3)  
                    score *= 0.35;
                if (!bonus.IsSafe())
                {
                    score *= 0.65;
                    if (bonus.GuessedArmiesNotOwnedByUs > 4) //TODO
                        score *= 0.25;
                }
                if (bonus.ArmiesReward > 0 /*&& !bonus.HasWasteland()*/ && bonus.IsSafe())
                {
                    // && end.getBonus().getSize() <= 4
                    score *= 1.1;
                    if (bonus.Territories.Count <= 4 && bonus.ArmiesReward >= 2 && bonus.GuessedArmiesNotOwnedByUs < Bot.MyIncome.Total) //TODO
                        score *= 1.3;
                }
                if (Analyzer.SoonBeOwned(bonus) && bonus.ArmiesReward > 1 && end.Armies < 4) //TODO
                {
                    if (Bot.NumberOfTurns == 0)
                        score += 0.10 * bonus.ArmiesReward;
                    else
                        score += 0.20 * bonus.ArmiesReward / bonus.Territories.Count;
                }
                // dumpSet.add(e);
                foreach (var neighbor in end.Neighbors)
                {
                    var nb = neighbor.Bonuses.Where(b => Analyzer.MightBeOwned(b) && b.IsUnvisible() && b.ArmiesReward > 1);
                    if (end.Armies < 4 && nb.Any())
                    {
                        // && !neighbor.getBonus().hasWasteland()
                        var modifier = 0.25 * nb.Sum(o => o.ArmiesReward);
                        //if (neighbor.Bonus.HasWasteland())
                        //    modifier *= 0.7; //TODO: Check the desirability of this bonus and adjust accordingly here
                        score += modifier;
                        DumpSet.Add(e.ID, e);
                        break;
                    }
                }
                // score += 0.05 * neighbor.getBonus().getArmiesReward();
                // Tie Break
                foreach (var neighbor_1 in end.Neighbors)
                {
                    if (neighbor_1.Bonuses.Any(b => b.MightBeOwnedByOpponent()))
                        score += 0.03;
                    else
                    {
                        if (!neighbor_1.IsVisible)
                        {
                            //if (!neighbor_1.Bonus.HasWasteland())
                            //    score += 0.02;
                            //else
                            score += 0.01;
                        }
                        else
                        {
                            if (Bot.NumberOfTurns == 0 && Bot.IsOpponent(neighbor_1.OwnerPlayerID))
                                score += 0.02;
                        }
                    }
                }
            }
            else
            {
                if (Bot.IsOpponent(end.OwnerPlayerID))
                {
                    score += bonus.ArmiesReward * 1.0 / Math.Max(1, bonus.GuessedArmiesNotOwnedByUs);
                    if (Analyzer.SoonBeOwned(bonus) /*&& !bonus.HasWasteland()*/)
                    {
                        if (Analyzer.MightBeOwned(bonus))
                        {
                            score += 0.8 * bonus.ArmiesReward;
                            if (bonus.ArmiesReward > 1)
                                DumpSet.Add(e.ID, e);
                        }
                        else
                            score += 0.1 * bonus.ArmiesReward;
                    }
                    else
                    {
                        if (bonus.MightBeOwnedByOpponent())
                            score += 0.1 * bonus.ArmiesReward;
                    }

                    if (end.GetBestAdjacentBonus().GuessedArmiesNotOwnedByUs <= 0 && e.Start.Bonuses.Any(b => b.ID == end.GetBestAdjacentBonus().ID))
                        score += (double)end.GetBestAdjacentBonus().ArmiesReward / bonus.GuessedArmiesNotOwnedByUs;

                    foreach(var neighborBonus in end.Neighbors.Where(o => o.OwnerPlayerID != Bot.Me.ID).SelectMany(o => o.Bonuses).Where(o => o.ID != bonus.ID && Analyzer.MightBeOwned(o)))
                        score += neighborBonus.ArmiesReward * 0.03;
                }
                else
                {
                    if (end.IsFoothold())
                    {
                        score += 0.5 * bonus.ArmiesReward;
                        foreach (var r in bonus.Territories)
                            if (r.IsVisible && Bot.IsOpponent(r.OwnerPlayerID))
                                score += bonus.ArmiesReward * 0.5 / bonus.Territories.Count;
                    }
                    if (end.GetStrongestNearestEnemy() > 0 && bonus.GuessedArmiesNotOwnedByUs <= 0)
                    {
                        var count = 0;
                        foreach (var territory in bonus.Territories)
                            if (territory.OwnerPlayerID == Bot.Me.ID && territory.GetStrongestNearestEnemy() > 0)
                                count++;
                        score += 1.2 * bonus.ArmiesReward / Math.Max(1, count);
                    }
                    foreach (var fromPath in end.GetFromPaths())
                        if (fromPath.End.OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                            score += 0.1 * GetScore(fromPath);
                        else
                        {
                            if (Bot.IsOpponent(fromPath.End.OwnerPlayerID))
                                score += 0.4 * GetScore(fromPath);
                        }
                    if (end.GetStrongestNearestEnemy() > 0)
                        score += 2 * bonus.ArmiesReward * bonus.ArmiesToNeutralsRatio / bonus.Territories.Count;
                }
            }

            return score;
        }
    }
}
