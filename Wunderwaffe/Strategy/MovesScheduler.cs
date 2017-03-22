using System;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.Wunderwaffe.Bot;
using WarLight.Shared.AI.Wunderwaffe.Move;
using WarLight.Shared.AI.Wunderwaffe.Bot.Cards;

namespace WarLight.Shared.AI.Wunderwaffe.Strategy
{
    public class MovesScheduler
    {
        public BotMain BotState;
        private bool OrderPrioPlayed = false;
        private bool OrderDelayPlayed = false;

        public MovesScheduler(BotMain state)
        {
            this.BotState = state;
        }

        private List<BotOrderGeneric> OrderPriorityCardPlays = new List<BotOrderGeneric>();
        private List<BotOrderGeneric> OrderDelayCardPlays = new List<BotOrderGeneric>();

        private List<BotOrderAttackTransfer> EarlyAttacks = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> SupportMovesWhereOpponentMightBreak = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> SupportMovesWhereOpponentMightGetAGoodAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> CrushingAttackMovesToSlipperyTerritories = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> SupportMovesWhereOpponentMightAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> DelayAttackMoves = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> SafeAttackMovesWithGoodAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> NormalSupportMoves = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> BigExpansionMovesNonAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> TransferMoves = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> NonOpponentBorderingSmallExpansionMovesNonAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> OpponentBorderingSmallExpansionMovesNonAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> BigExpansionMovesWithAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> NonOpponentBorderingSmallExpansionMovesWithAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> OpponentBorderingSmallExpansionMovesWithAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> SafeAttackMovesWithPossibleBadAttack = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> RiskyAttackMoves = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> TransferingExpansionMoves = new List<BotOrderAttackTransfer>();
        private List<BotOrderAttackTransfer> SnipeMoves = new List<BotOrderAttackTransfer>();

        /// <summary>Schedules the AttackTransferMoves.</summary>
        /// <remarks>Schedules the AttackTransferMoves.</remarks>
        /// <param name="movesSoFar"></param>
        /// <returns></returns>
        public Moves ScheduleMoves(Moves movesSoFar)
        {
            return GetSortedMoves(movesSoFar.Orders);
        }


        private Moves GetSortedMoves(List<BotOrder> movesSoFar)
        {
            Moves sortedMoves = new Moves();
            sortedMoves.Orders.AddRange(GetSortedDeployment(movesSoFar));

            List<BotOrder> unhandledMoves = movesSoFar.Where(o => !(o is BotOrderDeploy)).ToList();
            BotMap movesMap = BotState.VisibleMap.GetMapCopy();

            while (unhandledMoves.Count > 0)
            {
                var nextMove = GetNextMove(unhandledMoves, movesMap);
                unhandledMoves.Remove(nextMove);
                sortedMoves.AddOrder(nextMove);

                if (nextMove is BotOrderAttackTransfer)
                {
                    BotState.MapUpdater.UpdateMap((BotOrderAttackTransfer)nextMove, movesMap, BotTerritory.DeploymentType.Conservative);
                }
            }
            return sortedMoves;
        }

        private List<BotOrder> GetSortedDeployment(List<BotOrder> allOrders)
        {
            var deploymentsNextOpponent = new List<BotOrder>();
            var deploymentsInBackground = new List<BotOrder>();

            foreach (var deploy in allOrders.OfType<BotOrderDeploy>())
            {
                if (deploy.Territory.GetOpponentNeighbors().Count == 0)
                {
                    deploymentsInBackground.Add(deploy);
                }
                else
                {
                    deploymentsNextOpponent.Add(deploy);
                }
            }
            var allDeployments = new List<BotOrder>();
            allDeployments.AddRange(deploymentsNextOpponent);
            allDeployments.AddRange(deploymentsInBackground);
            return allDeployments;
        }



        private BotOrder GetNextMove(List<BotOrder> unhandledMoves, BotMap movesMap)
        {
            ClearMoves();
            FillMoveTypes(unhandledMoves, movesMap);
            var semiSortedMoves = new List<BotOrder>();

            var orderPriorityCards = BotState.CardsHandler.GetCards(Shared.AI.Wunderwaffe.Bot.Cards.CardTypes.OrderPriority);
            var orderDelayCards = BotState.CardsHandler.GetCards(Shared.AI.Wunderwaffe.Bot.Cards.CardTypes.OrderDelay);
            foreach (Card orderPrio in orderPriorityCards)
            {
                AILog.Log("MovesScheduler", "Playing order priority card " + orderPrio.CardInstanceId);
                OrderPriorityCardPlays.Add(new BotOrderGeneric(GameOrderPlayCardOrderPriority.Create(orderPrio.CardInstanceId, BotState.Me.ID)));
            }
            foreach (Card orderDelay in orderDelayCards)
            {
                AILog.Log("MovesScheduler", "Playing order delay card " + orderDelay.CardInstanceId);
                OrderDelayCardPlays.Add(new BotOrderGeneric(GameOrderPlayCardOrderDelay.Create(orderDelay.CardInstanceId, BotState.Me.ID)));
            }


            EarlyAttacks = ScheduleAttacksAttackingArmies(EarlyAttacks);
            SupportMovesWhereOpponentMightBreak = ScheduleAttacksAttackingArmies(SupportMovesWhereOpponentMightBreak);
            CrushingAttackMovesToSlipperyTerritories = ScheduleAttacksAttackingArmies(CrushingAttackMovesToSlipperyTerritories);
            CrushingAttackMovesToSlipperyTerritories = ScheduleCrushingAttackToSlipperyTerritory(CrushingAttackMovesToSlipperyTerritories);
            SupportMovesWhereOpponentMightGetAGoodAttack = ScheduleAttacksAttackingArmies(SupportMovesWhereOpponentMightGetAGoodAttack);
            SupportMovesWhereOpponentMightAttack = ScheduleAttacksAttackingArmies(SupportMovesWhereOpponentMightAttack);
            DelayAttackMoves = ScheduleDelayAttacks(DelayAttackMoves);
            SafeAttackMovesWithGoodAttack = ScheduleAttacksAttackingArmies(SafeAttackMovesWithGoodAttack);
            NormalSupportMoves = ScheduleAttacksAttackingArmies(NormalSupportMoves);
            BigExpansionMovesNonAttack = SortExpansionMovesOpponentDistance(BigExpansionMovesNonAttack, false);
            BigExpansionMovesNonAttack = ScheduleAttacksAttackingArmies(BigExpansionMovesNonAttack);
            NonOpponentBorderingSmallExpansionMovesNonAttack = SortExpansionMovesOpponentDistance(NonOpponentBorderingSmallExpansionMovesNonAttack, true);
            NonOpponentBorderingSmallExpansionMovesNonAttack = ScheduleAttacksAttackingArmies(NonOpponentBorderingSmallExpansionMovesNonAttack);
            OpponentBorderingSmallExpansionMovesNonAttack = ScheduleAttacksAttackingArmies(OpponentBorderingSmallExpansionMovesNonAttack);
            BigExpansionMovesWithAttack = SortExpansionMovesOpponentDistance(BigExpansionMovesWithAttack, false);
            BigExpansionMovesWithAttack = ScheduleAttacksAttackingArmies(BigExpansionMovesWithAttack);
            NonOpponentBorderingSmallExpansionMovesWithAttack = SortExpansionMovesOpponentDistance(NonOpponentBorderingSmallExpansionMovesWithAttack, true);
            NonOpponentBorderingSmallExpansionMovesWithAttack = ScheduleAttacksAttackingArmies(NonOpponentBorderingSmallExpansionMovesWithAttack);
            OpponentBorderingSmallExpansionMovesWithAttack = SortExpansionMovesOpponentDistance(OpponentBorderingSmallExpansionMovesWithAttack, true);
            OpponentBorderingSmallExpansionMovesWithAttack = ScheduleAttacksAttackingArmies(OpponentBorderingSmallExpansionMovesWithAttack);
            SafeAttackMovesWithPossibleBadAttack = ScheduleAttacksAttackingArmies(SafeAttackMovesWithPossibleBadAttack);
            RiskyAttackMoves = ScheduleAttacksAttackingArmies(RiskyAttackMoves);

            if (!OrderPrioPlayed && (EarlyAttacks.Count != 0 || SupportMovesWhereOpponentMightAttack.Count != 0 || CrushingAttackMovesToSlipperyTerritories.Count != 0))
                OrderPriorityCardPlays.ForEach(o => semiSortedMoves.Add(o));

            EarlyAttacks.ForEach(o => semiSortedMoves.Add(o));
            SupportMovesWhereOpponentMightBreak.ForEach(o => semiSortedMoves.Add(o));
            CrushingAttackMovesToSlipperyTerritories.ForEach(o => semiSortedMoves.Add(o));
            SupportMovesWhereOpponentMightGetAGoodAttack.ForEach(o => semiSortedMoves.Add(o));
            SupportMovesWhereOpponentMightAttack.ForEach(o => semiSortedMoves.Add(o));


            if (!OrderDelayPlayed && RiskyAttackMoves.Count != 0)
            {
                OrderDelayCardPlays.ForEach(o => semiSortedMoves.Add(o));
            }

            DelayAttackMoves.ForEach(o => semiSortedMoves.Add(o));
            SafeAttackMovesWithGoodAttack.ForEach(o => semiSortedMoves.Add(o));
            NormalSupportMoves.ForEach(o => semiSortedMoves.Add(o));
            BigExpansionMovesNonAttack.ForEach(o => semiSortedMoves.Add(o));
            TransferMoves.ForEach(o => semiSortedMoves.Add(o));
            NonOpponentBorderingSmallExpansionMovesNonAttack.ForEach(o => semiSortedMoves.Add(o));
            OpponentBorderingSmallExpansionMovesNonAttack.ForEach(o => semiSortedMoves.Add(o));
            BigExpansionMovesWithAttack.ForEach(o => semiSortedMoves.Add(o));
            NonOpponentBorderingSmallExpansionMovesWithAttack.ForEach(o => semiSortedMoves.Add(o));
            OpponentBorderingSmallExpansionMovesWithAttack.ForEach(o => semiSortedMoves.Add(o));
            TransferingExpansionMoves.ForEach(o => semiSortedMoves.Add(o));
            SnipeMoves.ForEach(o => semiSortedMoves.Add(o));
            SafeAttackMovesWithPossibleBadAttack.ForEach(o => semiSortedMoves.Add(o));
            RiskyAttackMoves.ForEach(o => semiSortedMoves.Add(o));

            if (semiSortedMoves.Count == 0)
            {
                return unhandledMoves[0];
            }

            var nextMove = semiSortedMoves[0];
            if (nextMove is BotOrderGeneric)
            {
                BotOrderGeneric bog = nextMove.As<BotOrderGeneric>();
                if (bog.Order is GameOrderPlayCardOrderPriority)
                {
                    OrderPrioPlayed = true;
                }
                else if (bog.Order is GameOrderPlayCardOrderDelay)
                {
                    OrderDelayPlayed = true;
                }
            }

            if (nextMove is BotOrderAttackTransfer && movesMap.Territories[nextMove.As<BotOrderAttackTransfer>().To.ID].GetOpponentNeighbors().Count > 0)
            {
                var substituteMove = GetSubstituteMove(nextMove.As<BotOrderAttackTransfer>(), movesMap, unhandledMoves);
                if (substituteMove != null)
                    nextMove = substituteMove;
            }
            return nextMove;
        }





        private void ClearMoves()
        {
            EarlyAttacks.Clear();
            SupportMovesWhereOpponentMightBreak.Clear();
            SupportMovesWhereOpponentMightGetAGoodAttack.Clear();
            SupportMovesWhereOpponentMightAttack.Clear();
            CrushingAttackMovesToSlipperyTerritories.Clear();
            DelayAttackMoves.Clear();
            SafeAttackMovesWithGoodAttack.Clear();
            NormalSupportMoves.Clear();
            BigExpansionMovesNonAttack.Clear();
            TransferMoves.Clear();
            BigExpansionMovesWithAttack.Clear();
            NonOpponentBorderingSmallExpansionMovesNonAttack.Clear();
            OpponentBorderingSmallExpansionMovesNonAttack.Clear();
            NonOpponentBorderingSmallExpansionMovesWithAttack.Clear();
            OpponentBorderingSmallExpansionMovesWithAttack.Clear();
            SafeAttackMovesWithPossibleBadAttack.Clear();
            RiskyAttackMoves.Clear();
            TransferingExpansionMoves.Clear();
            SnipeMoves.Clear();
        }

        /// <summary>
        /// Schedules the attacks with 1 in a way that we first attack territories bordering multiple of our territories (since the
        /// stack might move).
        /// </summary>
        /// <remarks>
        /// Schedules the attacks with 1 in a way that we first attack territories bordering multiple of our territories (since the
        /// stack might move).
        /// </remarks>
        /// <param name="delayAttacks"></param>
        /// <returns></returns>
        private List<BotOrderAttackTransfer> ScheduleDelayAttacks(List<BotOrderAttackTransfer> delayAttacks)
        {
            var outvar = new List<BotOrderAttackTransfer>();
            List<BotOrderAttackTransfer> delayAttacksToLonelyTerritory = new List<BotOrderAttackTransfer>();
            List<BotOrderAttackTransfer> delayAttacksToNonLonelyTerritory = new List<BotOrderAttackTransfer>();
            foreach (BotOrderAttackTransfer atm in delayAttacks)
            {
                if (atm.To.GetOwnedNeighbors().Count == 1)
                    delayAttacksToLonelyTerritory.Add(atm);
                else
                    delayAttacksToNonLonelyTerritory.Add(atm);
            }
            outvar.AddRange(delayAttacksToNonLonelyTerritory);
            outvar.AddRange(delayAttacksToLonelyTerritory);
            return outvar;
        }

        private List<BotOrderAttackTransfer> ScheduleCrushingAttackToSlipperyTerritory(List<BotOrderAttackTransfer> attacks)
        {
            var outvar = new List<BotOrderAttackTransfer>();
            var copy = new List<BotOrderAttackTransfer>();
            copy.AddRange(attacks);
            while (copy.Count > 0)
            {
                var bestAttack = copy[0];
                foreach (BotOrderAttackTransfer atm in copy)
                {
                    if (GetSlipperyOpponentTerritoryNumber(atm.To) > GetSlipperyOpponentTerritoryNumber(bestAttack.To))
                        bestAttack = atm;
                }
                outvar.Add(bestAttack);
                copy.Remove(bestAttack);
            }
            return outvar;
        }

        private List<BotOrderAttackTransfer> ScheduleAttacksAttackingArmies(List<BotOrderAttackTransfer> attackTransferMoves)
        {
            var outvar = new List<BotOrderAttackTransfer>();
            var copy = new List<BotOrderAttackTransfer>();
            copy.AddRange(attackTransferMoves);
            while (copy.Count > 0)
            {
                var biggestAttack = copy[0];
                foreach (var atm in copy)
                {
                    if (atm.Armies.AttackPower > biggestAttack.Armies.AttackPower)
                        biggestAttack = atm;
                }
                outvar.Add(biggestAttack);
                copy.Remove(biggestAttack);
            }
            return outvar;
        }

        /// <summary>Tries to find an attack move to make the support move obsolete.</summary>
        private BotOrderAttackTransfer GetSubstituteMove(BotOrderAttackTransfer supportMove, BotMap movesMap, List<BotOrder> unhandledMoves)
        {
            var territoryToDefend = supportMove.To;
            var mmTerritoryToDefend = movesMap.Territories[territoryToDefend.ID];
            if (mmTerritoryToDefend.GetOpponentNeighbors().Count > 1)
                return null;

            var mmOpponentTerritory = mmTerritoryToDefend.GetOpponentNeighbors()[0];
            foreach (var unhandledMove in unhandledMoves.OfType<BotOrderAttackTransfer>())
            {
                if (unhandledMove.To.ID == mmOpponentTerritory.ID)
                {
                    if (!CanOpponentAttackTerritory(mmOpponentTerritory.OwnerPlayerID, unhandledMove.From))
                    {
                        if (unhandledMove.Armies.AttackPower * BotState.Settings.OffenseKillRate > mmOpponentTerritory.Armies.DefensePower + BotState.GetGuessedOpponentIncome(mmOpponentTerritory.OwnerPlayerID, BotState.VisibleMap) + 3)
                        {
                            return unhandledMove;
                        }
                    }
                }
            }
            return null;
        }

        private void FillMoveTypes(List<BotOrder> unhandledMoves, BotMap movesMap)
        {
            foreach (var atm in unhandledMoves.OfType<BotOrderAttackTransfer>())
            {
                var mmToTerritory = movesMap.Territories[atm.To.ID];
                var mmFromTerritory = movesMap.Territories[atm.From.ID];
                if (atm.Message == AttackMessage.EarlyAttack)
                    EarlyAttacks.Add(atm);
                else
                {
                    // Opponent attack moves
                    if (BotState.IsOpponent(mmToTerritory.OwnerPlayerID))
                    {
                        if (atm.Armies.AttackPower == 1)
                            DelayAttackMoves.Add(atm);
                        else if (!CanOpponentAttackTerritory(mmToTerritory.OwnerPlayerID, atm.From) && GetSlipperyOpponentTerritoryNumber(atm.To) > -1 && IsProbablyCrushingMove(atm))
                            CrushingAttackMovesToSlipperyTerritories.Add(atm);
                        else if (!CanOpponentAttackTerritory(mmToTerritory.OwnerPlayerID, atm.From) && IsAlwaysGoodAttackMove(atm))
                            SafeAttackMovesWithGoodAttack.Add(atm);
                        else if (!CanOpponentAttackTerritory(mmToTerritory.OwnerPlayerID, atm.From) && !IsAlwaysGoodAttackMove(atm))
                            SafeAttackMovesWithPossibleBadAttack.Add(atm);
                        else if (CanOpponentAttackTerritory(mmToTerritory.OwnerPlayerID, atm.From))
                            RiskyAttackMoves.Add(atm);
                    }
                    else if (mmToTerritory.OwnerPlayerID == BotState.Me.ID) // Transfer moves
                    {
                        if (mmToTerritory.GetOpponentNeighbors().Count > 0 && CanOpponentBreakTerritory(mmToTerritory))
                            SupportMovesWhereOpponentMightBreak.Add(atm);
                        else if (mmToTerritory.GetOpponentNeighbors().Count > 0 && CanOpponentGetAGoodAttack(mmToTerritory))
                            SupportMovesWhereOpponentMightGetAGoodAttack.Add(atm);
                        else if (mmToTerritory.GetOpponentNeighbors().Count > 0 && CanAnyOpponentAttackTerritory(mmToTerritory))
                            SupportMovesWhereOpponentMightAttack.Add(atm);
                        else if (mmToTerritory.GetOpponentNeighbors().Count > 0 && !CanAnyOpponentAttackTerritory(mmToTerritory))
                            NormalSupportMoves.Add(atm);
                        else if (mmToTerritory.GetOpponentNeighbors().Count == 0)
                            TransferMoves.Add(atm);
                    }
                    else if (mmToTerritory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID) // Expansion moves
                    {


                        if (atm.Message == AttackMessage.Snipe)
                            SnipeMoves.Add(atm);
                        else if (Math.Round(atm.Armies.AttackPower * BotState.Settings.OffenseKillRate) < mmToTerritory.Armies.DefensePower)
                            TransferingExpansionMoves.Add(atm);
                        else if (IsBigExpansionStep(atm) && !CanAnyOpponentAttackTerritory(mmFromTerritory))
                            BigExpansionMovesNonAttack.Add(atm);
                        else if (!IsBigExpansionStep(atm) && mmToTerritory.GetOpponentNeighbors().Count == 0 && !CanAnyOpponentAttackTerritory(mmFromTerritory))
                            NonOpponentBorderingSmallExpansionMovesNonAttack.Add(atm);
                        else if (atm.Armies.AttackPower <= 3 && mmToTerritory.GetOpponentNeighbors().Count > 0 && !CanAnyOpponentAttackTerritory(mmFromTerritory))
                            OpponentBorderingSmallExpansionMovesNonAttack.Add(atm);
                        else if (IsBigExpansionStep(atm) && CanAnyOpponentAttackTerritory(mmFromTerritory))
                            BigExpansionMovesWithAttack.Add(atm);
                        else if (!IsBigExpansionStep(atm) && mmToTerritory.GetOpponentNeighbors().Count == 0 && CanAnyOpponentAttackTerritory(mmFromTerritory))
                            NonOpponentBorderingSmallExpansionMovesWithAttack.Add(atm);
                        else if (!IsBigExpansionStep(atm) && mmToTerritory.GetOpponentNeighbors().Count > 0 && CanAnyOpponentAttackTerritory(mmFromTerritory))
                            OpponentBorderingSmallExpansionMovesWithAttack.Add(atm);
                    }
                }
            }
        }

        // an expansion step is big if it leaves more leftovers than the initial neutral armies
        private bool IsBigExpansionStep(BotOrderAttackTransfer atm)
        {
            int attackPower = atm.Armies.AttackPower;
            int DefendingArmies = atm.To.Armies.AttackPower;
            int losses = (int)Math.Round(DefendingArmies * BotState.Settings.DefenseKillRate);
            return attackPower - losses > DefendingArmies;
        }

        private bool IsAlwaysGoodAttackMove(BotOrderAttackTransfer atm)
        {
            var opponentIncome = BotState.GetGuessedOpponentIncome(atm.To.OwnerPlayerID, BotState.VisibleMap);
            var opponentArmies = atm.To.Armies.DefensePower + opponentIncome;
            // Heuristic since the opponent might have more income than expected
            opponentArmies += 3;
            return Math.Round(atm.Armies.AttackPower * BotState.Settings.OffenseKillRate) >= Math.Round(opponentArmies * BotState.Settings.DefenseKillRate);
        }

        /// <remarks>
        /// Calculates the highest defense territory value of a bordering territory that the opponent might break from his slippery territory. If there is no such territory then returns -1.
        /// </remarks>
        /// <param name="opponentTerritory"></param>
        /// <returns></returns>
        private int GetSlipperyOpponentTerritoryNumber(BotTerritory slipperyOpponentTerritory)
        {
            List<BotTerritory> territoriesOpponentMightBreak = new List<BotTerritory>();
            var opponentIncome = BotState.GetGuessedOpponentIncome(slipperyOpponentTerritory.OwnerPlayerID, BotState.VisibleMap);
            var opponentAttackingArmies = opponentIncome + slipperyOpponentTerritory.Armies.AttackPower - BotState.MustStandGuardOneOrZero;
            var neededArmiesForDefense = (int)Math.Round(opponentAttackingArmies * BotState.Settings.OffenseKillRate);
            foreach (var ownedNeighbor in slipperyOpponentTerritory.GetOwnedNeighbors())
            {
                if (ownedNeighbor.GetArmiesAfterDeploymentAndIncomingMoves().DefensePower < neededArmiesForDefense)
                    territoriesOpponentMightBreak.Add(ownedNeighbor);
            }
            var sortedOwnedNeighbors = BotState.TerritoryValueCalculator.SortDefenseValue(territoriesOpponentMightBreak);
            if (sortedOwnedNeighbors.Count > 0)
                return sortedOwnedNeighbors[0].DefenceTerritoryValue;
            else
                return -1;
        }

        private bool IsProbablyCrushingMove(BotOrderAttackTransfer attack)
        {
            var guessedOpponentArmies = attack.To.GetArmiesAfterDeployment(BotTerritory.DeploymentType.Normal).DefensePower;
            var maximumOpponentArmies = attack.To.Armies.DefensePower + BotState
                .GetGuessedOpponentIncome(attack.To.OwnerPlayerID, BotState.VisibleMap);
            var adjustedOpponentArmies = Math.Max(guessedOpponentArmies, maximumOpponentArmies - 2);
            var isCrushingMove = Math.Round(attack.Armies.AttackPower * BotState.Settings.OffenseKillRate) >= adjustedOpponentArmies;
            return isCrushingMove;
        }

        private bool CanOpponentBreakTerritory(BotTerritory ourTerritory)
        {
            var oppNeighbors = ourTerritory.GetOpponentNeighbors();

            if (oppNeighbors.Count == 0)
                return false;

            foreach (var group in oppNeighbors.GroupBy(o => o.OwnerPlayerID))
            {
                var opponentIncome = BotState.GetGuessedOpponentIncome(group.Key, BotState.VisibleMap);
                var ourArmies = ourTerritory.GetArmiesAfterDeploymentAndIncomingMoves();
                var opponentAttackingArmies = opponentIncome;
                foreach (var opponentNeighbor in group)
                    opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

                if (Math.Round(opponentAttackingArmies * BotState.Settings.OffenseKillRate) >= ourArmies.DefensePower)
                    return true;
            }

            return false;
        }

        private bool CanOpponentGetAGoodAttack(BotTerritory ourTerritory)
        {
            var oppNeighbors = ourTerritory.GetOpponentNeighbors();

            if (oppNeighbors.Count == 0)
                return false;

            foreach (var group in oppNeighbors.GroupBy(o => o.OwnerPlayerID))
            {
                var opponentIncome = BotState.GetGuessedOpponentIncome(group.Key, BotState.VisibleMap);
                var ourArmies = ourTerritory.GetArmiesAfterDeploymentAndIncomingMoves();
                var opponentAttackingArmies = opponentIncome;
                foreach (var opponentNeighbor in group)
                    opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

                if (Math.Round(opponentAttackingArmies * BotState.Settings.OffenseKillRate) >= Math.Round(ourArmies.DefensePower * BotState.Settings.DefenseKillRate))
                    return true;
            }
            return false;
        }

        private bool CanAnyOpponentAttackTerritory(BotTerritory ourTerritory)
        {
            var oppNeighbors = ourTerritory.GetOpponentNeighbors();
            if (oppNeighbors.Count == 0)
                return false;

            foreach (var group in oppNeighbors.GroupBy(o => o.OwnerPlayerID))
            {
                var opponentIncome = BotState.GetGuessedOpponentIncome(group.Key, BotState.VisibleMap);
                var ourArmies = ourTerritory.Armies;
                var opponentAttackingArmies = opponentIncome;
                foreach (var opponentNeighbor in group)
                    opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

                if (Math.Round(opponentAttackingArmies * BotState.Settings.OffenseKillRate) >= Math.Round(ourArmies.DefensePower * BotState.Settings.DefenseKillRate))
                    return true;
            }

            return false;
        }

        private bool CanOpponentAttackTerritory(PlayerIDType opponentID, BotTerritory ourTerritory)
        {
            if (ourTerritory.GetOpponentNeighbors().Count == 0)
                return false;
            var opponentIncome = BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap);
            var ourArmies = ourTerritory.Armies;
            var opponentAttackingArmies = opponentIncome;
            foreach (var opponentNeighbor in ourTerritory.GetOpponentNeighbors())
                opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

            return Math.Round(opponentAttackingArmies * BotState.Settings.OffenseKillRate) >= Math.Round(ourArmies.DefensePower * BotState.Settings.DefenseKillRate);
        }

        /// <summary>
        /// Sorts the expansion moves according to the distance of the toTerritory to the direct opponent border (without
        /// blocking neutrals).
        /// </summary>
        /// <remarks>
        /// Sorts the expansion moves according to the distance of the toTerritory to the direct opponent border (without
        /// blocking neutrals).
        /// </remarks>
        /// <param name="unsortedMoves">the unsorted moves</param>
        /// <param name="reverse">if true then the move with the biggest to territory distance is returned first, else returned last
        /// </param>
        /// <returns>sorted moves</returns>
        private List<BotOrderAttackTransfer> SortExpansionMovesOpponentDistance(List
            <BotOrderAttackTransfer> unsortedMoves, bool reverse)
        {
            var outvar = new List<BotOrderAttackTransfer>();
            var temp = new List<BotOrderAttackTransfer>();
            temp.AddRange(unsortedMoves);
            while (temp.Count != 0)
            {
                var extremestDistanceMove = temp[0];
                foreach (BotOrderAttackTransfer atm in temp)
                {
                    var reverseCondition = atm.To.DirectDistanceToOpponentBorder > extremestDistanceMove.To.DirectDistanceToOpponentBorder;
                    var nonReverseCondition = atm.To.DirectDistanceToOpponentBorder < extremestDistanceMove.To.DirectDistanceToOpponentBorder;
                    if ((reverseCondition && reverse) || (nonReverseCondition && !reverse))
                        extremestDistanceMove = atm;
                }
                temp.Remove(extremestDistanceMove);
                outvar.Add(extremestDistanceMove);
            }
            return outvar;
        }
    }
}
