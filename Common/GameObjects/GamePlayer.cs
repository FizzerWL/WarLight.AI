using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public enum GamePlayerState : byte
    {
        Invited = 1,
        Playing = 2,
        Eliminated = 3,
        Won = 4,
        Declined = 5,
        RemovedByHost = 6,
        SurrenderAccepted = 7,
        Booted = 8,
        EndedByVote = 9
    }

    public class GamePlayer
    {
        public PlayerIDType ID;
        public GamePlayerState State;
        public TeamIDType Team;
        public ushort ScenarioID;
        public bool IsAI;
        public bool HumanTurnedIntoAI;
        public bool HasCommittedOrders;

        public bool IsAIOrHumanTurnedIntoAI
        {
            get { return IsAI || HumanTurnedIntoAI; }
        }

        public GamePlayer(PlayerIDType id, GamePlayerState state, TeamIDType team, ushort scenarioID, bool isAI, bool isHumanTurnedIntoAI, bool hasCommittedOrders)
        {
            this.ID = id;
            this.State = state;
            this.Team = team;
            this.ScenarioID = scenarioID;
            this.IsAI = isAI;
            this.HumanTurnedIntoAI = isHumanTurnedIntoAI;
            this.HasCommittedOrders = hasCommittedOrders;
        }
    }
}
