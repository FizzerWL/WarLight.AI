using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Prod.MakeOrders
{
    public class DefendAttack
    {
        BotMain Bot;
        public List<PossibleAttack> WeightedMoves;

        public DefendAttack(BotMain bot)
        {
            this.Bot = bot;
            this.WeightedMoves = WeightAttacks();
        }

        public void Go(int incomeToUse)
        {
            if (WeightedMoves.None())
            {
                AILog.Log("DefendAttack", "No attacks or defenses to do, skipping DefendAttack");
                return; //No attacks possible
            }

            //Divide 50/50 between offense and defense.  Defense armies could still be used for offense if we happen to attack there
            int armiesToOffense = SharedUtility.Round(incomeToUse * (Bot.IsFFA ? 0.3 : 0.6));
            int armiesToDefense = incomeToUse - armiesToOffense;

            AILog.Log("DefendAttack", armiesToOffense + " armies go to offense, " + armiesToDefense + " armies go to defense");

            //Find defensive opportunities.
            var orderedDefenses = WeightedMoves.OrderByDescending(o => o.DefenseImportance).ToList();
            var orderedAttacks = WeightedMoves.OrderByDescending(o => o.OffenseImportance).ToList();


            DoDefense(armiesToDefense, orderedDefenses);
            DoOffense(armiesToOffense, orderedAttacks);

        }

        private void DoDefense(int armies, List<PossibleAttack> orderedDefenses)
        {
            AILog.Log("Defense", orderedDefenses.Count + " defend ops:");
            foreach (var defend in orderedDefenses.Take(10))
                AILog.Log("Defense", " - " + defend);



#if IOS
            //Work around the "attacking to JIT compile method" for below WeightedRandom call
            if (RandomUtility.RandomNumber(2) == -1)
                new List<PossibleAttack>().Select(o => o.DefenseImportance).ToList();
#endif

            if (orderedDefenses.Count == 0)
                AILog.Log("Defense", "No defenses");
            else
            {
                var allDefenses = new Dictionary<TerritoryIDType, int>();
                for (int i = 0; i < armies; i++)
                {
                    var defend = orderedDefenses.WeightedRandom(o => o.DefenseImportance);
                    if (Bot.Orders.TryDeploy(defend.From, 1))
                        allDefenses.AddTo(defend.From, 1);
                }

                AILog.Log("Defense", "Defended " + allDefenses.Count + " territories: " + allDefenses.OrderByDescending(o => o.Value).Select(o => Bot.TerrString(o.Key) + " with " + o.Value).JoinStrings(", "));
            }


        }

        private void DoOffense(int armiesToOffense, List<PossibleAttack> orderedAttacks)
        {
            AILog.Log("Offense", orderedAttacks.Count + " attack ops: ");
            foreach (var attack in orderedAttacks.Take(10))
                AILog.Log("Offense", " - " + attack);



            foreach (var attack in orderedAttacks)
            {
                int attackWith = Bot.ArmiesToTake(Bot.Standing.Territories[attack.To].NumArmies);

                //TODO: If we're attacking a human opponent, add a few more to what's required so we're not as predictable.   Also take into account the offense weight.

                int have = Bot.MakeOrders.GetArmiesAvailable(attack.From);
                int need = Math.Max(0, attackWith - have);

                if (need > armiesToOffense)
                {
                    //We can't swing it. Just deploy the rest and quit. Will try again next turn.
                    if (armiesToOffense > 0 && Bot.Orders.TryDeploy(attack.From, armiesToOffense))
                    {
                        AILog.Log("Offense", "Could not attack from " + Bot.TerrString(attack.From) + " to " + Bot.TerrString(attack.To) + " with " + attackWith + ". Short by " + need + ".  Just deploying " + armiesToOffense + " to the source and quitting attacks.");
                        armiesToOffense = 0;
                    }
                }
                else
                {
                    //We can attack.  First deploy however many we needed
                    if (need > 0)
                    {
                        if (!Bot.Orders.TryDeploy(attack.From, need))
                            continue;
                        armiesToOffense -= need;
                        Assert.Fatal(armiesToOffense >= 0);
                    }

                    //Now issue the attack
                    Bot.Orders.AddAttack(attack.From, attack.To, AttackTransferEnum.AttackTransfer, attackWith, false);
                    AILog.Log("Offense", "Attacking from " + Bot.TerrString(attack.From) + " to " + Bot.TerrString(attack.To) + " with " + attackWith + " by deploying " + need);
                }
            }
        }

        private List<PossibleAttack> WeightAttacks()
        {
            var weightedNeighbors = WeightNeighbors();

            //build all possible attacks
            List<PossibleAttack> ret = Bot.Territories
                .Where(o => o.OwnerPlayerID == Bot.PlayerID)
                .SelectMany(us =>
                    Bot.Map.Territories[us.ID].ConnectedTo
                    .Select(k => Bot.Standing.Territories[k])
                    .Where(k => k.OwnerPlayerID != TerritoryStanding.NeutralPlayerID && !Bot.IsTeammateOrUs(k.OwnerPlayerID))
                    .Select(k => new PossibleAttack(Bot, us.ID, k.ID))).ToList();


            foreach (PossibleAttack a in ret)
                a.Weight(weightedNeighbors);

            NormalizeWeights(ret);

            return ret;
        }

        private Dictionary<PlayerIDType, int> WeightNeighbors()
        {
            var ret = Bot.Neighbors.Values
                .Where(o => !Bot.IsTeammateOrUs(o.ID)) //Exclude teammates
                .Where(o => o.ID != TerritoryStanding.NeutralPlayerID) //Exclude neutral
                .ToDictionary(o => o.ID, o => o.NeighboringTerritories.Select(n => n.NumArmies.NumArmies).Sum());  //Sum each army they have on our borders as the initial weight

            foreach (var borderTerr in Bot.BorderTerritories)
            {
                //Subtract one weight for each defending army we have next to that player

                Bot.Map.Territories[borderTerr.ID].ConnectedTo
                    .Select(o => Bot.Standing.Territories[o])
                    .Where(o => !Bot.IsTeammateOrUs(o.OwnerPlayerID)) //Ignore our own and teammates
                    .Select(o => o.OwnerPlayerID)
                    .Where(o => o != TerritoryStanding.NeutralPlayerID)
                    .Distinct()
                    .ForEach(o => ret[o] = ret[o] - Bot.Standing.Territories[borderTerr.ID].NumArmies.NumArmies);
            }

            return ret;
        }


        private void NormalizeWeights(IEnumerable<PossibleAttack> attacks)
        {
            if (!attacks.Any())
                return;

            var sub = attacks.Select(o => Math.Min(o.OffenseImportance, o.DefenseImportance)).Min() - 1;
            foreach (var a in attacks)
            {
                a.DefenseImportance -= sub;
                a.OffenseImportance -= sub;
            }
        }

    }
}
