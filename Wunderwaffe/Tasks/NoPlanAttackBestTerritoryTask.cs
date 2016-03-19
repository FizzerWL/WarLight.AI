using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;

using WarLight.Shared.AI.Wunderwaffe.Move;

namespace WarLight.Shared.AI.Wunderwaffe.Tasks
{
    /// <summary>
    /// NoPlanAttackBestTerritoryTask is responsible for attacking the best opponent territory with good attacks and without following a specific plan.
    /// </summary>
    public class NoPlanAttackBestTerritoryTask
    {
        public static Moves CalculateNoPlanAttackBestTerritoryTask(BotMain state, int maxDeployment)
        {
            var outvar = new Moves();
            // If true attacks are possible then go with them
            // If true attacks are possible then go with them
            var sortedOpponentTerritories = state.TerritoryValueCalculator.GetSortedAttackValueTerritories();
            var territoriesToAttack = NoPlanBreakBestTerritoryTask.RemoveTerritoriesThatWeTook
                (state, sortedOpponentTerritories);
            foreach (var territory in territoriesToAttack)
            {
                if (territory.IsVisible)
                {
                    var attackTerritoryMoves = AttackTerritoryTask.CalculateAttackTerritoryTask(state, territory, maxDeployment);
                    if (attackTerritoryMoves != null)
                        return attackTerritoryMoves;

                    // If we can't truly attack then attack with 1's
                    var allowedSmallAttacks = territory.Armies.NumArmies - state.MustStandGuardOneOrZero;
                    allowedSmallAttacks -= GetAlreadyPresentSmallAttacks(territory);
                    foreach (var ownedNeighbor in territory.GetOwnedNeighbors())
                    {
                        if (allowedSmallAttacks > 0 && ownedNeighbor.GetIdleArmies().NumArmies > 0 && !IsTerritoryAlreadySmallAttackedFromOurTerritory(ownedNeighbor, territory))
                        {
                            var alreadyAttacksOpponent = false;
                            foreach (var atm in ownedNeighbor.OutgoingMoves)
                            {
                                if (atm.Armies.AttackPower > 1 && state.IsOpponent(atm.To.OwnerPlayerID))
                                    alreadyAttacksOpponent = true;
                            }

                            if (!alreadyAttacksOpponent)
                            {
                                var atm_1 = new BotOrderAttackTransfer(state.Me.ID, ownedNeighbor, territory, new Armies(1), "NoPlanAttackBestTerritoryTask");
                                outvar.AddOrder(atm_1);
                                allowedSmallAttacks--;
                            }
                        }
                    }
                    if (outvar.Orders.OfType<BotOrderAttackTransfer>().Any())
                        return outvar;
                }
            }
            // If absolutely no attack possible then return null
            return null;
        }

        private static int GetAlreadyPresentSmallAttacks(BotTerritory opponentTerritory)
        {
            var outvar = 0;
            foreach (var atm in opponentTerritory.IncomingMoves)
            {
                if (atm.Armies.AttackPower == 1)
                    outvar++;
            }
            return outvar;
        }

        private static bool IsTerritoryAlreadySmallAttackedFromOurTerritory(BotTerritory ourTerritory, BotTerritory opponentTerritory)
        {
            var alreadySmallAttacked = false;
            foreach (var atm in ourTerritory.OutgoingMoves)
            {
                if (atm.Armies.AttackPower == 1 && atm.To.ID == opponentTerritory.ID)
                    alreadySmallAttacked = true;
            }
            return alreadySmallAttacked;
        }
    }
}
