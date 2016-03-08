using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public class PlayerInvite
    {
        public static readonly TeamIDType NoTeam = (TeamIDType)(-1);

        public string InviteString;
        public TeamIDType Team;
        public SlotType? Slot;

        public static PlayerInvite Create(PlayerIDType id, TeamIDType team, SlotType? slot)
        {
            var pi = new PlayerInvite();
            pi.InviteString = "00" + id + "00";
            pi.Team = team;
            pi.Slot = slot;
            return pi;
        }

        public static PlayerInvite Create(string email, TeamIDType team, SlotType? slot)
        {
            var pi = new PlayerInvite();
            pi.InviteString = email;
            pi.Team = team;
            pi.Slot = slot;
            return pi;
        }

    }
}
