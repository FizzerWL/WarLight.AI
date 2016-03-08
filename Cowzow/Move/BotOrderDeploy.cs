/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Move
{
    /// <remarks>
    ///     This Move is used in the first part of each round. It represents what Territory is increased  with how many armies.
    /// </remarks>
    public class BotOrderDeploy : BotOrder
    {
        public int Armies;
        public readonly BotTerritory Territory;

        public BotOrderDeploy(PlayerIDType playerName, BotTerritory territory, int armies)
        {
            PlayerID = playerName;
            this.Territory = territory;
            this.Armies = armies;
        }
        
        public override string ToString()
        {
            return "Deploy " + Armies + " armies on " + Territory.Details.Name + " (" + Territory.ID + ")";
        }
    }
}
