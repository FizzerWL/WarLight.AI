 /*
 * This code was auto-converted from a java project.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using WarLight.AI.Wunderwaffe.Bot;



namespace WarLight.AI.Wunderwaffe.Heuristics
{
    /// <summary>The heuristic expansion value of a Bonus.</summary>
    public class BonusExpansionValueHeuristic
    {
        public double ExpansionValue = 0.0;
        public BotMain BotState;

        public BonusExpansionValueHeuristic(BotMain state, BotBonus bonus, PlayerIDType playerID)
        {
            this.BotState = state;
            // public static final int HIGH = 1000;
            // public static final int LOW = 10;
            SetExpansionValue2(bonus, playerID);
        }

        // public void addExtraValueForFirstTurnBonus(Bonus Bonus) {
        // int neutrals = Bonus.NeutralArmies;
        // double initialValue = incomeNeutralsRatio * HIGH;
        // double addition = 0.0;
        // if (neutrals <= 4) {
        // addition = initialValue * 0.8;
        // } else if (neutrals <= 6) {
        // addition = initialValue * 0.6;
        // } else {
        // addition = initialValue * 0.4;
        // }
        // if (Bonus.Amount <= 1) {
        // addition *= 0.6;
        // } else if (Bonus.Amount <= 2) {
        // addition *= 0.9;
        // }
        //
        // expansionValue += addition;
        // }
        private double IncomeNeutralsRatio(BotBonus bonus)
        {
            var income = (double)bonus.Amount;
            var neutrals = (double)bonus.NeutralArmies.DefensePower;

            neutrals += bonus.Territories.Count(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution) * BotState.Settings.InitialNeutralsInDistribution;

            return income / neutrals;
        }

        public void SetExpansionValue2(BotBonus bonus, PlayerIDType playerID)
        {
            this.ExpansionValue = 0.0;
            if (IsExpansionWorthless(bonus, playerID))
                return;

            var points = IncomeNeutralsRatio(bonus) * 200.0;
            if (BotState.NumberOfTurns == -1)
            {
                Assert.Fatal(playerID == BotState.Me.ID); //we only ever call this during territory picking for ourselves.
                if (bonus.AreAllTerritoriesVisible())
                    points += AddExtraValueForFirstTurnBonus(bonus);
            }

            var neutralArmies = bonus.NeutralArmies.DefensePower;

            if (neutralArmies > 8)
                points -= neutralArmies * 4.5;
            else if (neutralArmies > 6)
                points -= neutralArmies * 3.5;
            else if (neutralArmies > 4)
                points -= neutralArmies * 2.5;

            points -= 0.5 * bonus.Territories.Count;

            var immediatelyCounteredTerritories = 0;
            if (playerID == BotState.Me.ID)
                immediatelyCounteredTerritories = bonus.GetOwnedTerritoriesBorderingNeighborsOwnedByOpponentOrDistribution().Count;
            else
                immediatelyCounteredTerritories = bonus.GetOpponentTerritoriesBorderingOwnedNeighbors().Count;


            points -= 7 * immediatelyCounteredTerritories;

            var allCounteredTerritories = GetCounteredTerritories(bonus, playerID);
            points -= 4 * allCounteredTerritories;

            var neighborBonuses = bonus.GetNeighborBonuses();
            foreach (var neighborBonus in neighborBonuses)
            {
                if ((neighborBonus.Territories.Any(o => BotState.IsOpponent(o.OwnerPlayerID) || o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution) && playerID == BotState.Me.ID) || (neighborBonus.ContainsOwnPresence() && BotState.IsOpponent(playerID)))
                    points -= 1;
                else if (neighborBonus.GetOwnedTerritories().Count > 0)
                    points += 0.5;
                else
                    points -= 0.4;
            }

            if (allCounteredTerritories > 0)
                points -= 7;

            if (immediatelyCounteredTerritories > 0)
                points -= Math.Abs(points * 0.1);

            // double value = this.incomeNeutralsRatio * HIGH;
            // double deductions = getDeductions(Bonus, playerID, value);
            // double additions = getAdditions(Bonus, playerID, value);
            // value += additions;
            // value -= deductions;

            var distanceFromUs = bonus.DistanceFrom(terr => terr.OwnerPlayerID == playerID, 3);
            if (distanceFromUs > 2)
            {
                //Penalize weight of bonuses far away
                points *= (12 - distanceFromUs) / 10.0;
            }

            this.ExpansionValue = points;
        }

        //private double GetAdditions(BotBonus bonus, PlayerIDType playerID, double initialValue)
        //{
        //    var additions = 0.0d;
        //    // First turn bonus in picking stage
        //    if (BotState.NumberOfTurns == -1)
        //    {
        //        if (playerID == BotState.Us.ID && Bonus.NeutralArmies <= 6 && Bonus.AreAllTerritoriesVisible())
        //        {
        //            additions += initialValue * 0.8;
        //        }
        //        else
        //        {
        //            if (playerID == BotState.OpponentPlayerID && Bonus.NeutralArmies <= 6 && Bonus.AreAllTerritoriesVisibleToOpponent())
        //            {
        //                additions += initialValue * 0.8;
        //            }
        //        }
        //    }
        //    if (Bonus.NeutralArmies <= 2)
        //    {
        //        additions += initialValue * 0.3;
        //    }
        //    else
        //    {
        //        if (Bonus.NeutralArmies <= 4)
        //        {
        //            additions += initialValue * 0.25;
        //        }
        //        else
        //        {
        //            if (Bonus.NeutralArmies <= 6)
        //            {
        //                additions += initialValue * -0.2;
        //            }
        //            else
        //            {
        //                if (Bonus.NeutralArmies <= 8)
        //                {
        //                    additions += initialValue * -0.3;
        //                }
        //                else
        //                {
        //                    additions += initialValue * -0.4;
        //                }
        //            }
        //        }
        //    }
        //    var otherPlayerNeighbors = false;
        //    foreach (BotBonus neighborBonus in Bonus.GetNeighborBonuses())
        //    {
        //        if ((neighborBonus.ContainsOpponentPresence() && playerID == BotState.Us.ID) || (neighborBonus.ContainsOwnPresence() && playerID == BotState.OpponentPlayerID))
        //            otherPlayerNeighbors = true;
        //    }
        //    if (!otherPlayerNeighbors)
        //    {
        //        additions += initialValue * 0.2;
        //    }
        //    additions += Math.Max(0, 7 - Bonus.Territories.Count);
        //    return additions;
        //}

        //private double GetDeductions(BotBonus bonus, PlayerIDType playerID, double initialValue)
        //{
        //    var containsWasteland = false;
        //    foreach (var territory in Bonus.Territories)
        //    {
        //        if (territory.Armies == 6 && (territory.OwnerPlayerID == TerritoryStanding.FogPlayerID || territory.OwnerPlayerID == TerritoryStanding.NeutralPlayerID))
        //            containsWasteland = true;
        //    }
        //    var immediatelyCounteredTerritories = 0;
        //    if (playerID == BotState.Us.ID)
        //    {
        //        immediatelyCounteredTerritories = Bonus.GetOwnedTerritoriesBorderingOpponentNeighbors().Count;
        //    }
        //    else
        //    {
        //        immediatelyCounteredTerritories = Bonus.GetOpponentTerritoriesBorderingOwnedNeighbors().Count;
        //    }
        //    var allCounteredTerritories = GetCounteredTerritories(Bonus, playerID);
        //    var opponentCanEasilyBreak = CanOtherPlayerEasilyBreak(Bonus, playerID);
        //    var easyBreakDeduction = opponentCanEasilyBreak ? initialValue * 0.4 : 0;
        //    double counteredTerritoriesDeduction = 0;
        //    if (allCounteredTerritories == 1)
        //        counteredTerritoriesDeduction = initialValue * 0.15;
        //    else if (allCounteredTerritories == 2)
        //        counteredTerritoriesDeduction = initialValue * 0.2;
        //    else if (allCounteredTerritories == 3)
        //        counteredTerritoriesDeduction = initialValue * 0.25;

        //    double immediateCounterDeduction = 0;
        //    if (immediatelyCounteredTerritories == 1)
        //        immediateCounterDeduction = initialValue * 0.2;
        //    else if (immediatelyCounteredTerritories == 2)
        //        immediateCounterDeduction = initialValue * 0.25;
        //    else if (immediatelyCounteredTerritories == 3)
        //        immediateCounterDeduction = initialValue * 0.3;

        //    double wastelandDeduction = 0;
        //    if (BotState.NumberOfTurns == -1 && containsWasteland)
        //        wastelandDeduction = initialValue * 0.9;

        //    var maxDeduction = 0.0d;
        //    maxDeduction = Math.Max(Math.Max(wastelandDeduction, immediateCounterDeduction),
        //        Math.Max(easyBreakDeduction, counteredTerritoriesDeduction));
        //    if (Bonus.NeutralArmies > 8)
        //    {
        //        maxDeduction += initialValue * 0.1;
        //        maxDeduction += Bonus.NeutralArmies;
        //    }
        //    return maxDeduction;
        //}

        //private bool CanOtherPlayerEasilyBreak(BotBonus bonus, PlayerIDType playerID)
        //{
        //    var otherPlayerArmiesIdleArmies = 0;
        //    var otherPlayerTerritories = new List<BotTerritory>();

        //    if (playerID == BotState.Us.ID)
        //        otherPlayerTerritories = Bonus.GetOpponentNeighbors();
        //    else
        //        otherPlayerTerritories = Bonus.GetOwnedNeighborTerritories();

        //    foreach (var territory in otherPlayerTerritories)
        //        otherPlayerArmiesIdleArmies += territory.Armies - 1;

        //    return otherPlayerArmiesIdleArmies >= 7 ? true : false;
        //}

        private bool IsExpansionWorthless(BotBonus bonus, PlayerIDType playerID)
        {
            if (bonus.Amount == 0)
                return true;

            if ((BotState.IsOpponent(playerID) && bonus.ContainsOwnPresence()) || (playerID == BotState.Me.ID && bonus.ContainsOpponentPresence()))
                return true;

            if ((playerID == BotState.Me.ID && bonus.IsOwnedByMyself()) || (BotState.IsOpponent(playerID) && bonus.IsOwnedByAnyOpponent()))
                return true;

            return false;
        }

        private int GetCounteredTerritories(BotBonus bonus, PlayerIDType playerID)
        {
            var outvar = 0;
            foreach (var territory in bonus.Territories)
            {
                if (territory.GetOpponentNeighbors().Count > 0 && playerID == BotState.Me.ID)
                    outvar++;
                else if (territory.GetOwnedNeighbors().Count > 0 && BotState.IsOpponent(playerID))
                    outvar++;
            }
            return outvar;
        }

        // public static int getBonusValue(Map temporaryMap, Bonus Bonus) {
        // double incomeNeutralsRatio = getIncomeNeutralsRatio(Bonus);
        // int immediatelyCounteredTerritories = Bonus.OwnedTerritoriesBorderingOpponentNeighbors.size();
        // int allCounteredTerritories = getCounteredTerritories(Bonus);
        // int neutrals = Bonus.NeutralArmies;
        // int allTerritories = Bonus.Territories.size();
        //
        // double points = incomeNeutralsRatio * 200;
        // if (neutrals > 8) {
        // points -= neutrals * 4.5;
        // } else if (neutrals > 6) {
        // points -= neutrals * 3.5;
        // } else if (neutrals > 4) {
        // points -= neutrals * 2.5;
        // }
        // points -= 0.5 * allTerritories;
        // points -= 9 * immediatelyCounteredTerritories;
        // points -= 5 * allCounteredTerritories;
        //
        // List<Bonus> neighborBonuses = Bonus.NeighborBonuses;
        // for (Bonus neighborBonus : neighborBonuses) {
        // if (neighborBonus.containsOpponentPresence()) {
        // points -= 1;
        // } else if (neighborBonus.GetOwnedTerritories().size() > 0) {
        // points += 0.5;
        // } else {
        // points -= 0.4;
        // }
        // }
        //
        // if (allCounteredTerritories > 0) {
        // points -= 12;
        // }
        // if (immediatelyCounteredTerritories > 0) {
        // double abs = Math.abs(points * 0.2);
        // points -= abs;
        // }
        //
        // return (int) points;
        // }
        public virtual double AddExtraValueForFirstTurnBonus(BotBonus bonus)
        {
            var neutrals = bonus.NeutralArmies.DefensePower;
            if (neutrals <= 4)
                return bonus.Amount * 15;
            else if (neutrals <= 6)
                return bonus.Amount * 7;
            else
                return 0;
        }

        // private static int getCounteredTerritories(Bonus Bonus) {
        // int out = 0;
        // for (Territory territory : Bonus.Territories) {
        // if (territory.GetOpponentNeighbors().size() > 0) {
        // out++;
        // }
        // }
        // return out;
        // }
        public override string ToString()
        {
            return "BonusExpansionValue: " + ExpansionValue;
        }
    }
}
