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

    }
}
