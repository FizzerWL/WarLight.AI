///*
//* This code was auto-converted from a java project.
//*/

//using System;
//using System.Linq;
//using System.Collections.Generic;
//using WarLight.AI.Wunderwaffe.Bot;
//using WarLight.AI.Wunderwaffe.Evaluation;

//using WarLight.AI.Wunderwaffe.Move;


//namespace WarLight.AI.Wunderwaffe.Strategy
//{
//    public class MovesScheduler
//    {
//        public BotState BotState;
//        public MovesScheduler(BotState state)
//        {
//            this.BotState = state;
//        }

//        private List<BotOrderAttackTransfer> earlyAttacks = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> supportMovesWhereOpponentMightBreak = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> supportMovesWhereOpponentMightGetAGoodAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> crushingAttackMovesToSlipperyTerritories = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> supportMovesWhereOpponentMightAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> delayAttackMoves = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> safeAttackMovesWithGoodAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> normalSupportMoves = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> bigExpansionMovesNonAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> transferMoves = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> nonOpponentBorderingSmallExpansionMovesNonAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> opponentBorderingSmallExpansionMovesNonAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> bigExpansionMovesWithAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> nonOpponentBorderingSmallExpansionMovesWithAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> opponentBorderingSmallExpansionMovesWithAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> safeAttackMovesWithPossibleBadAttack = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> riskyAttackMoves = new List<BotOrderAttackTransfer>();
//        private List<BotOrderAttackTransfer> transferingExpansionMoves = new List<BotOrderAttackTransfer>();

//        /// <summary>Schedules the AttackTransferMoves.</summary>
//        /// <remarks>Schedules the AttackTransferMoves.</remarks>
//        /// <param name="movesSoFar"></param>
//        /// <returns></returns>
//        public Moves ScheduleMoves(Moves movesSoFar)
//        {
//            var outvar = new Moves();
//            earlyAttacks.Clear();
//            supportMovesWhereOpponentMightBreak.Clear();
//            supportMovesWhereOpponentMightGetAGoodAttack.Clear();
//            supportMovesWhereOpponentMightAttack.Clear();
//            crushingAttackMovesToSlipperyTerritories.Clear();
//            delayAttackMoves.Clear();
//            safeAttackMovesWithGoodAttack.Clear();
//            normalSupportMoves.Clear();
//            bigExpansionMovesNonAttack.Clear();
//            transferMoves.Clear();
//            bigExpansionMovesWithAttack.Clear();
//            nonOpponentBorderingSmallExpansionMovesNonAttack.Clear();
//            opponentBorderingSmallExpansionMovesNonAttack.Clear();
//            nonOpponentBorderingSmallExpansionMovesWithAttack.Clear();
//            opponentBorderingSmallExpansionMovesWithAttack.Clear();
//            safeAttackMovesWithPossibleBadAttack.Clear();
//            riskyAttackMoves.Clear();
//            transferingExpansionMoves.Clear();
//            FillMoveTypes(movesSoFar);
//            outvar.PlaceArmiesMoves = movesSoFar.PlaceArmiesMoves;
//            earlyAttacks = ScheduleAttacksAttackingArmies(earlyAttacks);
//            supportMovesWhereOpponentMightBreak = ScheduleAttacksAttackingArmies(supportMovesWhereOpponentMightBreak);
//            crushingAttackMovesToSlipperyTerritories = ScheduleAttacksAttackingArmies(crushingAttackMovesToSlipperyTerritories);
//            crushingAttackMovesToSlipperyTerritories = ScheduleCrushingAttackToSlipperyTerritory(crushingAttackMovesToSlipperyTerritories);
//            supportMovesWhereOpponentMightGetAGoodAttack = ScheduleAttacksAttackingArmies(supportMovesWhereOpponentMightGetAGoodAttack);
//            supportMovesWhereOpponentMightAttack = ScheduleAttacksAttackingArmies(supportMovesWhereOpponentMightAttack);
//            delayAttackMoves = ScheduleDelayAttacks(delayAttackMoves);
//            safeAttackMovesWithGoodAttack = ScheduleAttacksAttackingArmies(safeAttackMovesWithGoodAttack);
//            normalSupportMoves = ScheduleAttacksAttackingArmies(normalSupportMoves);
//            bigExpansionMovesNonAttack = SortExpansionMovesOpponentDistance(bigExpansionMovesNonAttack, false);
//            bigExpansionMovesNonAttack = ScheduleAttacksAttackingArmies(bigExpansionMovesNonAttack);
//            nonOpponentBorderingSmallExpansionMovesNonAttack = SortExpansionMovesOpponentDistance(nonOpponentBorderingSmallExpansionMovesNonAttack, true);
//            nonOpponentBorderingSmallExpansionMovesNonAttack = ScheduleAttacksAttackingArmies(nonOpponentBorderingSmallExpansionMovesNonAttack);
//            opponentBorderingSmallExpansionMovesNonAttack = ScheduleAttacksAttackingArmies(opponentBorderingSmallExpansionMovesNonAttack);
//            bigExpansionMovesWithAttack = SortExpansionMovesOpponentDistance(bigExpansionMovesWithAttack, false);
//            bigExpansionMovesWithAttack = ScheduleAttacksAttackingArmies(bigExpansionMovesWithAttack);
//            nonOpponentBorderingSmallExpansionMovesWithAttack = SortExpansionMovesOpponentDistance(nonOpponentBorderingSmallExpansionMovesWithAttack, true);
//            nonOpponentBorderingSmallExpansionMovesWithAttack = ScheduleAttacksAttackingArmies(nonOpponentBorderingSmallExpansionMovesWithAttack);
//            opponentBorderingSmallExpansionMovesWithAttack = SortExpansionMovesOpponentDistance(opponentBorderingSmallExpansionMovesWithAttack, true);
//            opponentBorderingSmallExpansionMovesWithAttack = ScheduleAttacksAttackingArmies(opponentBorderingSmallExpansionMovesWithAttack);
//            safeAttackMovesWithPossibleBadAttack = ScheduleAttacksAttackingArmies(safeAttackMovesWithPossibleBadAttack);
//            riskyAttackMoves = ScheduleAttacksAttackingArmies(riskyAttackMoves);
//            outvar.AttackTransferMoves.AddRange(earlyAttacks);
//            outvar.AttackTransferMoves.AddRange(supportMovesWhereOpponentMightBreak);
//            outvar.AttackTransferMoves.AddRange(crushingAttackMovesToSlipperyTerritories);
//            outvar.AttackTransferMoves.AddRange(supportMovesWhereOpponentMightGetAGoodAttack);
//            outvar.AttackTransferMoves.AddRange(supportMovesWhereOpponentMightAttack);
//            outvar.AttackTransferMoves.AddRange(delayAttackMoves);
//            outvar.AttackTransferMoves.AddRange(safeAttackMovesWithGoodAttack);
//            outvar.AttackTransferMoves.AddRange(normalSupportMoves);
//            outvar.AttackTransferMoves.AddRange(bigExpansionMovesNonAttack);
//            outvar.AttackTransferMoves.AddRange(transferMoves);
//            outvar.AttackTransferMoves.AddRange(nonOpponentBorderingSmallExpansionMovesNonAttack);
//            outvar.AttackTransferMoves.AddRange(opponentBorderingSmallExpansionMovesNonAttack);
//            outvar.AttackTransferMoves.AddRange(bigExpansionMovesWithAttack);
//            outvar.AttackTransferMoves.AddRange(nonOpponentBorderingSmallExpansionMovesWithAttack);
//            outvar.AttackTransferMoves.AddRange(opponentBorderingSmallExpansionMovesWithAttack);
//            outvar.AttackTransferMoves.AddRange(transferingExpansionMoves);
//            outvar.AttackTransferMoves.AddRange(safeAttackMovesWithPossibleBadAttack);
//            outvar.AttackTransferMoves.AddRange(riskyAttackMoves);
//            return outvar;
//        }

//        /// <summary>
//        /// Schedules the attacks with 1 in a way that we first attack territories
//        /// bordering multiple of our territories (since the stack might move).
//        /// </summary>
//        /// <remarks>
//        /// Schedules the attacks with 1 in a way that we first attack territories
//        /// bordering multiple of our territories (since the stack might move).
//        /// </remarks>
//        /// <param name="delayAttacks"></param>
//        /// <returns></returns>
//        private List<BotOrderAttackTransfer> ScheduleDelayAttacks(List<BotOrderAttackTransfer> delayAttacks)
//        {
//            var outvar = new List<BotOrderAttackTransfer>();
//            var delayAttacksToLonelyTerritory = new List<BotOrderAttackTransfer>();
//            var delayAttacksToNonLonelyTerritory = new List<BotOrderAttackTransfer>();
//            foreach (BotOrderAttackTransfer atm in delayAttacks)
//            {
//                if (atm.To.GetOwnedNeighbors().Count == 1)
//                    delayAttacksToLonelyTerritory.Add(atm);
//                else
//                    delayAttacksToNonLonelyTerritory.Add(atm);
//            }
//            outvar.AddRange(delayAttacksToNonLonelyTerritory);
//            outvar.AddRange(delayAttacksToLonelyTerritory);
//            return outvar;
//        }

//        private List<BotOrderAttackTransfer> ScheduleCrushingAttackToSlipperyTerritory(List <BotOrderAttackTransfer> attacks)
//        {
//            var outvar = new List<BotOrderAttackTransfer>();
//            var copy = new List<BotOrderAttackTransfer>();
//            copy.AddRange(attacks);
//            while (!copy.IsEmpty())
//            {
//                var bestAttack = copy[0];
//                foreach (BotOrderAttackTransfer atm in copy)
//                {
//                    if (GetSlipperyOpponentTerritoryNumber(atm.To) > GetSlipperyOpponentTerritoryNumber(bestAttack.To))
//                        bestAttack = atm;
//                }
//                outvar.Add(bestAttack);
//                copy.Remove(bestAttack);
//            }
//            return outvar;
//        }

//        private List<BotOrderAttackTransfer> ScheduleAttacksAttackingArmies(List<BotOrderAttackTransfer> attackTransferMoves)
//        {
//            var outvar = new List<BotOrderAttackTransfer>();
//            var copy = new List<BotOrderAttackTransfer>();
//            copy.AddRange(attackTransferMoves);
//            while (!copy.IsEmpty())
//            {
//                var biggestAttack = copy[0];
//                foreach (var atm in copy)
//                {
//                    if (atm.Armies.AttackPower > biggestAttack.Armies.AttackPower)
//                        biggestAttack = atm;
//                }
//                outvar.Add(biggestAttack);
//                copy.Remove(biggestAttack);
//            }
//            return outvar;
//        }

//        private void FillMoveTypes(Moves movesSoFar)
//        {
//            foreach (var atm in movesSoFar.AttackTransferMoves)
//            {
//                if (BotState.IsOpponent(atm.To.OwnerPlayerID)) // Opponent attack moves
//                {
//                    if (atm.Message == AttackMessage.EarlyAttack)
//                        earlyAttacks.Add(atm);
//                    else if (atm.Armies.SpecialUnits.Length == 0 && atm.Armies.NumArmies == 1)
//                        delayAttackMoves.Add(atm);
//                    else if (!CanOpponentAttackTerritory(atm.To.OwnerPlayerID, atm.From) && GetSlipperyOpponentTerritoryNumber(atm.To) > -1 && IsProbablyCrushingMove(atm))
//                        crushingAttackMovesToSlipperyTerritories.Add(atm);
//                    else if (!CanOpponentAttackTerritory(atm.To.OwnerPlayerID, atm.From) && IsAlwaysGoodAttackMove(atm))
//                        safeAttackMovesWithGoodAttack.Add(atm);
//                    else if (!CanOpponentAttackTerritory(atm.To.OwnerPlayerID, atm.From) && !IsAlwaysGoodAttackMove(atm))
//                        safeAttackMovesWithPossibleBadAttack.Add(atm);
//                    else if (CanOpponentAttackTerritory(atm.To.OwnerPlayerID, atm.From))
//                        riskyAttackMoves.Add(atm);
//                }
//                else if (atm.To.OwnerPlayerID == BotState.Me.ID) // Transfer moves
//                {
//                    if (CanOpponentBreakTerritory(atm.To))
//                        supportMovesWhereOpponentMightBreak.Add(atm);
//                    else if (atm.To.GetOpponentNeighbors().Count > 0 && CanOpponentGetAGoodAttack(atm.To))
//                        supportMovesWhereOpponentMightGetAGoodAttack.Add(atm);
//                    else if (atm.To.GetOpponentNeighbors().Count > 0 && BotState.Opponents.Any(o => CanOpponentAttackTerritory(o.ID,  atm.To)))
//                        supportMovesWhereOpponentMightAttack.Add(atm);
//                    else if (atm.To.GetOpponentNeighbors().Count > 0 && BotState.Opponents.None(o => CanOpponentAttackTerritory(o.ID, atm.To)))
//                        normalSupportMoves.Add(atm);
//                    else if (atm.To.GetOpponentNeighbors().Count == 0)
//                        transferMoves.Add(atm);
//                }
//                else if (atm.To.OwnerPlayerID == TerritoryStanding.NeutralPlayerID) // Expansion moves
//                {
//                    if (Math.Round(atm.Armies.AttackPower * BotState.Settings.OffensiveKillRate) < atm.To.Armies.DefensePower)
//                        transferingExpansionMoves.Add(atm);
//                    else if (atm.Armies.AttackPower > 3 && !CanOpponentAttackTerritory(atm.From))
//                        bigExpansionMovesNonAttack.Add(atm);
//                    else if (atm.Armies.AttackPower <= 3 && atm.To.GetOpponentNeighbors().Count == 0 && !CanOpponentAttackTerritory(atm.From))
//                        nonOpponentBorderingSmallExpansionMovesNonAttack.Add(atm);
//                    else if (atm.Armies.AttackPower <= 3 && atm.To.GetOpponentNeighbors().Count > 0 && !CanOpponentAttackTerritory(atm.From))
//                        opponentBorderingSmallExpansionMovesNonAttack.Add(atm);
//                    else if (atm.Armies.AttackPower > 3 && CanOpponentAttackTerritory(atm.From))
//                        bigExpansionMovesWithAttack.Add(atm);
//                    else if (atm.Armies.AttackPower <= 3 && atm.To.GetOpponentNeighbors().Count == 0 && CanOpponentAttackTerritory(atm.From))
//                        nonOpponentBorderingSmallExpansionMovesWithAttack.Add(atm);
//                    else if (atm.Armies.AttackPower <= 3 && atm.To.GetOpponentNeighbors().Count > 0 && CanOpponentAttackTerritory(atm.From))
//                        opponentBorderingSmallExpansionMovesWithAttack.Add(atm);
//                }
//                else
//                    throw new Exception("Unknown " + atm.To.OwnerPlayerID);
//            }
//        }

//        private bool IsAlwaysGoodAttackMove(BotOrderAttackTransfer atm)
//        {
//            var opponentIncome = BotState.GetGuessedOpponentIncome(atm.To.OwnerPlayerID, BotState.VisibleMap);
//            var opponentArmies = atm.To.Armies.DefensePower + opponentIncome;
//            // Heuristic since the opponent might have more income than expected
//            //		opponentArmies += 3;
//            return Math.Round(atm.Armies.AttackPower * BotState.Settings.OffensiveKillRate) >= Math.Round(opponentArmies * BotState.Settings.DefensiveKillRate);
//        }

//        /// <remarks>
//        /// Calculates the highest defense territory value of a bordering territory that the opponent might break from his slippery territory. If there is no such territory then returns -1.
//        /// </remarks>
//        /// <param name="opponentTerritory"></param>
//        /// <returns></returns>
//        private int GetSlipperyOpponentTerritoryNumber(BotTerritory slipperyOpponentTerritory)
//        {
//            var territoriesOpponentMightBreak = new List<BotTerritory>();
//            var opponentIncome = BotState.GetGuessedOpponentIncome(slipperyOpponentTerritory.OwnerPlayerID, BotState.VisibleMap);
//            var opponentAttackingArmies = opponentIncome + slipperyOpponentTerritory.Armies.DefensePower - 1;
//            var neededArmiesForDefense = (int)Math.Round(opponentAttackingArmies * BotState.Settings.OffensiveKillRate);
//            foreach (var ownedNeighbor in slipperyOpponentTerritory.GetOwnedNeighbors())
//            {
//                if (ownedNeighbor.GetArmiesAfterDeploymentAndIncomingMoves().DefensePower < neededArmiesForDefense)
//                    territoriesOpponentMightBreak.Add(ownedNeighbor);
//            }

//            var sortedOwnedNeighbors = BotState.TerritoryValueCalculator.SortDefenseValue(territoriesOpponentMightBreak);
//            if (sortedOwnedNeighbors.Count > 0)
//                return sortedOwnedNeighbors[0].DefenceTerritoryValue;
//            else
//                return -1;
//        }

//        private bool IsProbablyCrushingMove(BotOrderAttackTransfer attack)
//        {
//            var guessedOpponentArmies = attack.To.GetArmiesAfterDeployment(BotTerritory.DeploymentType.Normal).DefensePower;
//            var maximumOpponentArmies = attack.To.Armies.DefensePower + BotState.GetGuessedOpponentIncome(attack.To.OwnerPlayerID, BotState.VisibleMap);
//            var adjustedOpponentArmies = Math.Max(guessedOpponentArmies, maximumOpponentArmies - 2);
//            var isCrushingMove = Math.Round(attack.Armies.AttackPower * BotState.Settings.OffensiveKillRate) >= adjustedOpponentArmies;
//            return isCrushingMove;
//        }

//        private bool CanOpponentBreakTerritory(BotTerritory ourTerritory)
//        {
//            var oppNeighbors = ourTerritory.GetOpponentNeighbors();
//            if (oppNeighbors.Count == 0)
//                return false;

//            foreach (var group in oppNeighbors.GroupBy(o => o.OwnerPlayerID))
//            {

//                var opponentIncome = BotState.GetGuessedOpponentIncome(group.Key, BotState.VisibleMap);
//                var ourArmies = ourTerritory.GetArmiesAfterDeploymentAndIncomingMoves();
//                var opponentAttackingArmies = opponentIncome;
//                foreach (var opponentNeighbor in group)
//                    opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;
//                if (Math.Round(opponentAttackingArmies * BotState.Settings.OffensiveKillRate) >= ourArmies.DefensePower)
//                    return true;
//            }

//            return false;
//        }

//        private bool CanOpponentGetAGoodAttack(BotTerritory ourTerritory)
//        {
//            var oppNeightbors = ourTerritory.GetOpponentNeighbors();
//            if (oppNeightbors.Count == 0)
//                return false;

//            foreach (var group in oppNeightbors.GroupBy(o => o.OwnerPlayerID))
//            {
//                var opponentIncome = BotState.GetGuessedOpponentIncome(group.Key, BotState.VisibleMap);
//                var ourArmies = ourTerritory.GetArmiesAfterDeploymentAndIncomingMoves();
//                var opponentAttackingArmies = opponentIncome;
//                foreach (var opponentNeighbor in group)
//                    opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

//                if (Math.Round(opponentAttackingArmies * BotState.Settings.OffensiveKillRate) >= Math.Round
//                        (ourArmies.DefensePower * BotState.Settings.DefensiveKillRate))
//                    return true;
//            }

//            return false;
//        }

//        private bool CanOpponentAttackTerritory(BotTerritory ourTerritory)
//        {
//            var oppNeighbors = ourTerritory.GetOpponentNeighbors();
//            if (oppNeighbors.Count == 0)
//                return false;

//            foreach (var group in oppNeighbors.GroupBy(o => o.OwnerPlayerID))
//            {
//                var opponentIncome = BotState.GetGuessedOpponentIncome(group.Key, BotState.VisibleMap);
//                var ourArmies = ourTerritory.Armies;
//                var opponentAttackingArmies = opponentIncome;
//                foreach (var opponentNeighbor in group)
//                    opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

//                if (Math.Round(opponentAttackingArmies * BotState.Settings.OffensiveKillRate) >= Math.Round(ourArmies.DefensePower * BotState.Settings.DefensiveKillRate))
//                    return true;
//            }

//            return false;
//        }

//        private bool CanOpponentAttackTerritory(PlayerIDType opponentID, BotTerritory ourTerritory)
//        {
//            if (ourTerritory.GetOpponentNeighbors().Count == 0)
//                return false;
//            var opponentIncome = BotState.GetGuessedOpponentIncome(opponentID, BotState.VisibleMap);
//            var ourArmies = ourTerritory.Armies;
//            var opponentAttackingArmies = opponentIncome;
//            foreach (var opponentNeighbor in ourTerritory.GetOpponentNeighbors())
//                opponentAttackingArmies += opponentNeighbor.Armies.AttackPower - BotState.MustStandGuardOneOrZero;

//            return Math.Round(opponentAttackingArmies * BotState.Settings.OffensiveKillRate) >= Math.Round(ourArmies.DefensePower * BotState.Settings.DefensiveKillRate);
//        }

//        /// <summary>
//        /// Sorts the expansion moves according to the distance of the toTerritory to the direct opponent border (without blocking neutrals).
//        /// </summary>
//        /// <param name="unsortedMoves">the unsorted moves</param>
//        /// <param name="reverse">if true then the move with the biggest to territory distance is returned first, else returned last
//        /// </param>
//        /// <returns>sorted moves</returns>
//        private List<BotOrderAttackTransfer> SortExpansionMovesOpponentDistance(List<BotOrderAttackTransfer> unsortedMoves, bool reverse)
//        {
//            var outvar = new List<BotOrderAttackTransfer>();
//            var temp = new List<BotOrderAttackTransfer>();
//            temp.AddRange(unsortedMoves);
//            while (!temp.IsEmpty())
//            {
//                var extremestDistanceMove = temp[0];
//                foreach (BotOrderAttackTransfer atm in temp)
//                {
//                    var reverseCondition = atm.To.DirectDistanceToOpponentBorder > extremestDistanceMove.To.DirectDistanceToOpponentBorder;
//                    var nonReverseCondition = atm.To.DirectDistanceToOpponentBorder < extremestDistanceMove.To.DirectDistanceToOpponentBorder;
//                    if ((reverseCondition && reverse) || (nonReverseCondition && !reverse))
//                        extremestDistanceMove = atm;
//                }
//                temp.Remove(extremestDistanceMove);
//                outvar.Add(extremestDistanceMove);
//            }
//            return outvar;
//        }
//    }
//}
