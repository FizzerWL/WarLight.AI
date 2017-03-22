using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Bot
{
    public class BotTerritory
    {
        public BotMap Parent;

        public TerritoryIDType ID;
        public Armies Armies;
        public PlayerIDType OwnerPlayerID;

        public TerritoryDetails Details
        {
            get { return BotState.Map.Territories[ID]; }
        }

        public override string ToString()
        {
            return ID + " " + Details.Name + " Armies=" + Armies + " Owner=" + OwnerPlayerID + " ExpansionValue=" + ExpansionTerritoryValue;
        }

        public List<BotOrderDeploy> Deployment = new List<BotOrderDeploy>();
        public List<BotOrderDeploy> NullDeployment = new List<BotOrderDeploy>();
        public List<BotOrderDeploy> ConservativeDeployment = new List<BotOrderDeploy>();
        public List<BotOrderAttackTransfer> OutgoingMoves = new List<BotOrderAttackTransfer>();
        public List<BotOrderAttackTransfer> IncomingMoves = new List<BotOrderAttackTransfer>();
        public int DistanceToBorder;
        public int DistanceToUnimportantSpot;
        public int DistanceToImportantSpot;
        public int DistanceToHighlyImportantSpot;
        public int DirectDistanceToOpponentBorder;
        public int DistanceToOpponentBorder;
        public int DistanceToImportantOpponentBorder;
        public int DistanceToOpponentBonus = -1;
        public int DistanceToOwnBonus = -1;
        public int ExpansionTerritoryValue;
        public int AttackTerritoryValue;
        public int DefenceTerritoryValue;
        public int FlankingTerritoryValue;
        public bool IsTerritoryBlocked = false;
        public bool IsOwnershipHeuristic = false;

        public BotTerritory(BotMap parent, TerritoryIDType id, PlayerIDType playerID, Armies armies)
        {
            this.Parent = parent;
            this.ID = id;

            this.OwnerPlayerID = playerID;
            this.Armies = armies;
        }

        private BotMain BotState { get { return Parent.BotState; } }

        public bool IsVisible
        {
            get
            {
                return this.OwnerPlayerID == BotState.Me.ID || this.GetOwnedNeighbors().Count > 0;
            }
        }

        public Armies GetIncomingArmies()
        {
            var incomingArmies = new Armies(0);
            foreach (var atm in this.IncomingMoves)
                incomingArmies = incomingArmies.Add(atm.Armies);

            return incomingArmies;
        }

        /// <summary>For opponent territories.</summary>
        public Armies GetArmiesAfterDeploymentAndIncomingAttacks(DeploymentType type)
        {
            var remainingArmies = this.GetArmiesAfterDeployment(type);
            foreach (var atm in IncomingMoves)
                //remainingArmies = remainingArmies.Subtract(new Armies(SharedUtility.Round(atm.Armies.NumArmies * BotState.Settings.OffensiveKillRate)));
                remainingArmies = remainingArmies.Subtract(new Armies(getOwnKills(atm.Armies.NumArmies, remainingArmies.DefensePower)));

            if (!remainingArmies.Fogged && remainingArmies.NumArmies < 1)
                remainingArmies = new Armies(1, remainingArmies.SpecialUnits);
            return remainingArmies;
        }

        public Armies GetArmiesAfterDeploymentAndIncomingMoves()
        {
            var outvar = this.GetArmiesAfterDeployment(DeploymentType.Normal);
            foreach (var atm in this.IncomingMoves)
                outvar = outvar.Add(atm.Armies);
            return outvar;
        }

        public List<BotTerritory> GetNeighborsWithinSameBonus()
        {
            List<BotTerritory> outvar = new List<BotTerritory>();
            foreach (var neighbor in this.Neighbors)
            {
                if (neighbor.Bonuses.Count == 0 || Bonuses.Count == 0)
                {
                    continue;
                }
                if (neighbor.Bonuses[0] == Bonuses[0])
                {
                    outvar.Add(neighbor);
                }
                // TODO gives wrong result
                //if (Details.PartOfBonuses.Any(o => neighbor.Details.PartOfBonuses.Contains(o)))
                //    outvar.Add(neighbor);
            }
            return outvar;
        }


        public List<BotBonus> Bonuses
        {
            get
            {
                return this.Details.PartOfBonuses.Select(o => Parent.Bonuses[o]).ToList();
            }
        }

        public enum DeploymentType
        {
            Null, Normal, Conservative
        }

        /// <summary>type 1 = normal deployment, type 2 = conservative deployment</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<BotOrderDeploy> GetDeployment(DeploymentType type)
        {
            if (type == DeploymentType.Null)
                return NullDeployment;
            else if (type == DeploymentType.Normal)
                return Deployment;
            else if (type == DeploymentType.Conservative)
                return ConservativeDeployment;
            else
                throw new Exception("DeploymentType");
        }

        public int GetTotalDeployment(DeploymentType type)
        {
            var outvar = 0;
            foreach (BotOrderDeploy pam in this.GetDeployment(type))
                outvar += pam.Armies;

            return outvar;
        }



        public List<BotOrderAttackTransfer> GetExpansionMoves()
        {
            var outvar = new List<BotOrderAttackTransfer>();
            foreach (var atm in this.OutgoingMoves)
            {
                if (Parent.Territories[atm.To.ID].OwnerPlayerID == TerritoryStanding.NeutralPlayerID)
                    outvar.Add(atm);
            }
            return outvar;
        }

        public Armies GetArmiesAfterDeployment(DeploymentType type)
        {
            var armies = this.Armies;
            foreach (var pam in this.GetDeployment(type))
            {
                armies = armies.Add(new Armies(pam.Armies));
            }
            return armies;
        }

        public Armies GetIdleArmies()
        {
            if (IsTerritoryBlocked)
                return new Armies(0);

            var outvar = GetArmiesAfterDeployment(DeploymentType.Normal);
            foreach (var atm in this.OutgoingMoves)
                outvar = outvar.Subtract(atm.Armies);

            return outvar.Subtract(new Armies(1));
        }

        /// <param name="territory">a Territory object</param>
        /// <returns>True if this Territory is a neighbor of given Territory, false otherwise</returns>
        public bool IsNeighbor(BotTerritory territory)
        {
            return Details.ConnectedTo.ContainsKey(territory.ID);
        }

        public int GetAmountOfBordersToOpponentBonus()
        {
            var outvar = 0;
            foreach (var neighbor in this.Neighbors)
            {
                if (neighbor.Details.PartOfBonuses.None(b => this.Details.PartOfBonuses.Contains(b)) && neighbor.Bonuses.Any(o => o.IsOwnedByAnyOpponent()))
                    outvar++;
            }
            return outvar;
        }

        public int GetAmountOfBordersToOwnBonus()
        {
            var outvar = 0;
            foreach (var neighbor in this.Neighbors)
            {
                if (neighbor.Details.PartOfBonuses.None(b => this.Details.PartOfBonuses.Contains(b)) && neighbor.Bonuses.Any(o => o.IsOwnedByMyself()))
                    outvar++;
            }
            return outvar;
        }

        public List<BotTerritory> GetOwnedNeighbors()
        {
            return this.Neighbors.Where(o => o.OwnerPlayerID == BotState.Me.ID).ToList();
        }

        public Armies GetSurroundingIdleArmies()
        {
            var idleArmies = new Armies(0);
            foreach (var neighbor in this.GetOwnedNeighbors())
                idleArmies = idleArmies.Add(neighbor.GetIdleArmies());
            return idleArmies;
        }

        public List<BotTerritory> GetNonOwnedNeighbors()
        {
            return this.Neighbors.Where(o => o.OwnerPlayerID != BotState.Me.ID).ToList();
        }


        public List<BotTerritory> GetOpponentNeighbors()
        {
            var outvar = new List<BotTerritory>();
            foreach (var neighbor in this.Neighbors)
            {
                if (BotState.IsOpponent(neighbor.OwnerPlayerID))
                    outvar.Add(neighbor);
            }
            return outvar;
        }

        /// <returns>A list of this Territory's neighboring Territories</returns>
        public List<BotTerritory> Neighbors
        {
            get
            {
                return Details.ConnectedTo.Keys.Select(o => Parent.Territories[o]).ToList();
            }
        }

        public int getNeededBreakArmies(int opponentDefendingArmies)
        {
            int neededAttackArmies = (int)Math.Round((opponentDefendingArmies - 0.5) / BotState.Settings.OffenseKillRate);
            if (getOwnKills(neededAttackArmies, opponentDefendingArmies) < opponentDefendingArmies)
            {
                neededAttackArmies++;
            }
            return neededAttackArmies;
        }

        public int getOwnKills(int ownAttackingArmies, int opponentDefendingArmies)
        {
            int expectedKills = (int)Math.Round(ownAttackingArmies * BotState.Settings.OffenseKillRate);
            int expectedOwnLosses = (int)Math.Round(opponentDefendingArmies * BotState.Settings.DefenseKillRate);
            if (expectedKills >= opponentDefendingArmies && expectedOwnLosses >= ownAttackingArmies)
            {
                expectedKills = expectedKills - 1;
            }

            return Math.Min(expectedKills, opponentDefendingArmies);
        }

    }
}
