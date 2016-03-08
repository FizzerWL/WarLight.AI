/*
 * This code was auto-converted from a java project, originally written by Dan Zou
 */

using System;
using System.Linq;
using WarLight.Shared.AI.Cowzow.Map;

namespace WarLight.Shared.AI.Cowzow.Move
{
    /// <summary>This Move is used in the second part of each round.</summary>
    /// <remarks>
    ///     This Move is used in the second part of each round. It represents the attack or transfer of armies from
    ///     fromTerritory to toTerritory. If toTerritory is owned by the player himself, it's a transfer. If toTerritory is
    ///     owned by the opponent, this Move is an attack.
    /// </remarks>
    public class BotOrderAttackTransfer : BotOrder
    {
        public int Armies;
        public readonly BotTerritory FromTerritory;
        public readonly BotTerritory ToTerritory;

        public BotOrderAttackTransfer(PlayerIDType playerName, BotTerritory fromTerritory, BotTerritory toTerritory, int armies)
        {
            this.PlayerID = playerName;
            this.FromTerritory = fromTerritory;
            this.ToTerritory = toTerritory;
            this.Armies = armies;
        }

        public override string ToString()
        {
            return "Attack/transfer from " + FromTerritory.Details.Name + " (" + FromTerritory.ID + ") to " + ToTerritory.Details.Name + " (" + ToTerritory.ID + ") armies=" + Armies;
        }
    }
}
