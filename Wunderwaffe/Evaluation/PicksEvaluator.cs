/*
* This code was auto-converted from a java project.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using WarLight.Shared.AI.Wunderwaffe.Bot;



namespace WarLight.Shared.AI.Wunderwaffe.Evaluation
{
    public class PicksEvaluator
    {
        public BotMain BotState;
        public PicksEvaluator(BotMain state)
        {
            this.BotState = state;
        }

        public List<TerritoryIDType> GetPicks()
        {
            if (BotState.Map.IsScenarioDistribution(BotState.Settings.DistributionModeID))
            {
                var us = BotState.Map.GetTerritoriesForScenario(BotState.Settings.DistributionModeID, BotState.Me.ScenarioID);
                us.RandomizeOrder();
                return us;
            }


            int maxPicks = BotState.Settings.LimitDistributionTerritories == 0 ? BotState.Map.Territories.Count : (BotState.Settings.LimitDistributionTerritories * BotState.Players.Count(o => o.Value.State == GamePlayerState.Playing));

            var pickableTerritories = BotState.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToList();

            
            var weights = pickableTerritories.ToDictionary(o => o, terrID =>
            {
                var map = BotMap.FromStanding(BotState, BotState.DistributionStanding);

                map.Territories[terrID].OwnerPlayerID = BotState.Me.ID;

                var r = map.MyExpansionValue().PlayerExpansionValue;
                AILog.Log("PicksEvaluator", "PlayerExpansionValue for " + terrID + " " + map.Territories[terrID].Details.Name + " is " + r);
                return r;
            });

            //TODO: Take the top numPicks * 2, then normalize their values and do a weighted random.
            var ret = weights.OrderByDescending(o => o.Value).Take(maxPicks).Select(o => o.Key).Distinct().ToList();

            AILog.Log("PicksEvaluator", "Final picks: " + ret.Select(o => o.ToString()).JoinStrings(","));
            return ret;
        }

        /*
        
        /// <summary>The maximum depth for searching the solution tree.</summary>
        //private int MaxDepth = -1;

        //private const int ACCEPTABLE_TREE_STEPS = 4000;

            SetMaxDepth(stillPickableTerritories.Count);
            var minMaxNode = MinMax(0, new List<int>(), new List<int>());
            Utility.Log("--> minMaxNode.nodeDecision: " + minMaxNode.NodeDecision);
            Utility.Log("--> minMaxNode.minMaxValue: " + minMaxNode.MinMaxValue);
            Utility.Log("--> maxDepth: " + MaxDepth);
            return minMaxNode.NodeDecision;

        private MinMaxNode MinMax(int currentDepth, List<int> opponentPickedTerritories, List<int> pickedTerritories)
        {
            var node = new PicksEvaluator.MinMaxNode(this);
            node.OpponentPickedTerritories.AddRange(opponentPickedTerritories);
            node.PickedTerritories.AddRange(pickedTerritories);

            // If we are at a natural leaf or don't want to search the tree any deeper then evaluate.
            if (currentDepth == MaxDepth)
            {
                var mapCopy = startingTerritoryMap.GetMapCopy();
                FillMovesIntoMap(mapCopy, pickedTerritories, opponentPickedTerritories);
                node.MinMaxValue = (int)(mapCopy.MyExpansionValue.PlayerExpansionValue - mapCopy.OpponentExpansionValue.PlayerExpansionValue);
                return node;
            }

            // Span the tree
            var children = new List<MinMaxNode>();
            var stillPickable = GetStillPickableTerritories(pickedTerritories, opponentPickedTerritories);
            foreach (int territoryId in stillPickable)
            {
                var childPickedTerritories = pickedTerritories.ToList();
                var childOpponentPickedTerritories = opponentPickedTerritories.ToList();

                if (currentPlayer == 1)
                    childPickedTerritories.Add(territoryId);
                else
                    childOpponentPickedTerritories.Add(territoryId);

                children.Add(MinMax(currentDepth + 1, childOpponentPickedTerritories, childPickedTerritories));
            }

            if (children.Count == 0)
                throw new Exception("No children.");

            // Minimize or Maximize
            var bestChild = children[0];
            foreach (var child_1 in children)
            {
                if (currentPlayer == 1 && child_1.MinMaxValue > bestChild.MinMaxValue)
                    bestChild = child_1;
                else
                    if (currentPlayer == -1 && child_1.MinMaxValue < bestChild.MinMaxValue)
                        bestChild = child_1;
            }
            node.MinMaxValue = bestChild.MinMaxValue;
            node.NodeDecision = GetPickedTerritory(node.PickedTerritories, node.OpponentPickedTerritories, bestChild.PickedTerritories, bestChild.OpponentPickedTerritories);
            return node;
        }

        private int GetPickedTerritory(List<int> parentPickedTerritories, List<int> opponentParentPickedTerritories, List<int> childPickedTerritories, List<int> opponentChildPickedTerritories)
        {
            var parentTerritories = parentPickedTerritories.Concat(opponentParentPickedTerritories).ToHashSet(false);

            var pickedTerritory = -1;
            foreach (int territoryId in childPickedTerritories.Concat(opponentChildPickedTerritories))
                if (!parentTerritories.Contains(territoryId))
                    pickedTerritory = territoryId;

            return pickedTerritory;
        }

        /// <summary>Calculates which territories are still unpicked.</summary>
        private List<int> GetStillPickableTerritories(List<int> pickedTerritories, List<int> opponentPickedTerritories)
        {
            var allPickableTerritories = BotState.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToList();
            var stillPickableTerritories = new List<int>();
            foreach (int territoryId in allPickableTerritories)
            {
                if (!pickedTerritories.Contains(territoryId) && !opponentPickedTerritories.Contains(territoryId))
                    stillPickableTerritories.Add(territoryId);
            }
            return stillPickableTerritories;
        }

        /// <summary>Updates the map according to the made moves.</summary>
        private void FillMovesIntoMap(BotMap mapToFill, List<int> ourMoves, List<int> opponentMoves)
        {
            for (var i = 0; i < ourMoves.Count; i++)
                mapToFill.Territories[ourMoves[i]].OwnerPlayerID = BotState.Us.ID;

            for (var i_1 = 0; i_1 < opponentMoves.Count; i_1++)
                mapToFill.Territories[opponentMoves[i_1]].OwnerPlayerID = BotState.OpponentPlayerID;
        }
        
        /// <summary>Sets the maxDepth value.</summary>
        /// <param name="territoriesToDistribute"></param>
        private void SetMaxDepth(int territoriesToDistribute)
        {
            var treeSteps = 1;
            do
            {
                MaxDepth++;
                treeSteps = 1;
                for (var i = territoriesToDistribute; i > territoriesToDistribute - MaxDepth; i--)
                    treeSteps *= i;
            }
            while (MaxDepth <= 10 && treeSteps <= ACCEPTABLE_TREE_STEPS);
            MaxDepth--;
        }

        private class MinMaxNode
        {
            /// <summary>The minMax value.</summary>
            internal int MinMaxValue = 0;

            /// <summary>The territory picked in this node according to the minMax value.</summary>
            internal int NodeDecision = 0;

            /// <summary>The picked territories along this branch including the nodeDecision if it's our turn.</summary>
            internal List<int> PickedTerritories = new List<int>();

            /// <summary>The opponent picked territories along this branch including the nodeDecision if it's the opponent turn.</summary>
            internal List<int> OpponentPickedTerritories = new List<int>();

            internal MinMaxNode(PicksEvaluator _enclosing)
            {
                this._enclosing = _enclosing;
            }

            private readonly PicksEvaluator _enclosing;
        }
        */
    }
}
