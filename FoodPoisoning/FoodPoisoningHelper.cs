using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using UnityEngine;
using MelonLoader;
using Il2Cpp;
using ImprovedAfflictions.Pain;

namespace ImprovedAfflictions.FoodPoisoning
{
    internal class FoodPoisoningHelper
    {

        public float GetRemainingHours()
        {
            float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            return GameManager.GetFoodPoisoningComponent().m_DurationHours - hoursPlayedNotPaused;
        }

        public float CalculateConditionToDrain()
        {

            float conditionPerHour = GameManager.GetPlayerManagerComponent().PlayerIsSleeping() ? 7f : 10f;

            float fatigueAmt = GameManager.GetFatigueComponent().m_CurrentFatigue;
            float hungerAmt = GameManager.GetHungerComponent().m_CurrentReserveCalories;
            float thirstAmt = GameManager.GetThirstComponent().m_CurrentThirst;

            if (fatigueAmt < 30f && !GameManager.GetPlayerManagerComponent().PlayerIsSleeping()) conditionPerHour += 1f;
            if (hungerAmt < 35f) conditionPerHour += 1f;
            if (thirstAmt < 25f) conditionPerHour += 1f;

            if (GameManager.GetSprainPainComponent().GetAfflictionsCount() > 3) conditionPerHour += 0.5f;

            if (GameManager.GetFoodPoisoningComponent().HasTakenAntibiotics()) conditionPerHour -= 2f;

            return conditionPerHour;
        }

        public bool RollFoodPoisoningChance(GearItem gi, float startingCalories)
        {

            if (!gi.m_FoodItem)
            {
                return false;
            }

            if (startingCalories < 35f)
            {
                return false;
            }
            if (!gi.m_FoodItem.m_IsRawMeat && gi.GetNormalizedCondition() > 0.45f)
            {
                return false;
            }

            if (gi.IsWornOut())
            {
                return true;
            }

            float percent = 100f - gi.GetNormalizedCondition();

            //if player has scurvy, they have a much higher chance of getting food poisoning, regardless of food type
            if (GameManager.GetScurvyComponent().HasAffliction()) percent += 25;

                //20% offset for interloper or difficulties with a fast decay rate
                if (UtilityFunctions.IsInterloperOrFastDecayRate()) 
                {
                    percent -= 20;
                }

            return Il2Cpp.Utils.RollChance(percent);
        }
    }
}
