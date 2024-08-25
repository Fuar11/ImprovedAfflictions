using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using Random = UnityEngine.Random;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using UnityEngine;
using AfflictionComponent;
using MelonLoader;
using Il2CppEpic.OnlineServices;

namespace ImprovedAfflictions.FoodPoisoning
{
    internal class FoodPoisoningPatches
    {

        [HarmonyPatch(typeof(Il2Cpp.FoodPoisoning), nameof(Il2Cpp.FoodPoisoning.FoodPoisoningStart))]

        public class FoodPoisoningStartUpdate
        {

            public static void Prefix(Il2Cpp.FoodPoisoning __instance)
            {
                __instance.m_DurationHoursMax = 48f;
                __instance.m_DurationHoursMin = 12f;
            }

            public static void Postfix(Il2Cpp.FoodPoisoning __instance)
            {
                __instance.m_NumHoursRestForCure = Random.Range(__instance.m_DurationHoursMin, __instance.m_DurationHours);
                __instance.m_DurationHours += GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused(); 
                __instance.m_StopDegradingConditionAtPercent = 11f;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.FoodPoisoning), nameof(Il2Cpp.FoodPoisoning.UpdateFoodPoisoning))]

        public class FoodPoisoningUpdateUpdate
        {

            public static bool Prefix() { return false;}

            public static void Postfix(Il2Cpp.FoodPoisoning __instance)
            {

                FoodPoisoningHelper foodPoisoningHelper = new FoodPoisoningHelper();

                __instance.m_ElapsedHours = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                if (__instance.m_ElapsedHours > __instance.m_DurationHours)
                {
                    __instance.FoodPoisoningEnd();
                    return;
                }
                if (__instance.m_AntibioticsTaken && __instance.m_ElapsedRest > __instance.m_NumHoursRestForCure - 0.1f)
                {
                    __instance.FoodPoisoningEnd();
                    return;
                }
                bool flag = true;
                /**if (__instance.m_AntibioticsTaken && GameManager.GetPlayerManagerComponent().PlayerIsSleeping())
                {
                    flag = true;
                } **/
                if (GameManager.GetConditionComponent().GetNormalizedCondition() < __instance.m_StopDegradingConditionAtPercent / 100f)
                {
                    flag = false;
                }
                if (flag)
                {
                    float num = foodPoisoningHelper.CalculateConditionToDrain() * GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
                    GameManager.GetConditionComponent().AddHealth(0f - num, DamageSource.FoodPoisoning);
                }
                if (!GameManager.GetPlayerManagerComponent().PlayerIsSleeping())
                {

                    float fatigueIncreasePerHour = __instance.HasTakenAntibiotics() ? 20f : __instance.m_FatigueIncreasePerHour;

                    float fatigueValue = fatigueIncreasePerHour * GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
                    GameManager.GetFatigueComponent().AddFatigue(fatigueValue);
                }
            }
        }


        /** inlined
        [HarmonyPatch(typeof(Il2Cpp.FoodPoisoning), nameof(Il2Cpp.FoodPoisoning.TakeAntibiotics))]

        public class ScheduleAntibiotics
        {

            public static bool Prefix() { return false; }
            public static void Postfix()
            {

                MelonLogger.Msg("Taking antibiotics");

                int val = Random.Range(3, 8);
                Moment.Moment.ScheduleRelative(Mod.Instance, new Moment.EventRequest((0, val, 0), "takeEffectAntibiotics"));
            }

        } **/

        [HarmonyPatch(typeof(GearItem), nameof(GearItem.RollForFoodPoisoning))]

        public class FoodPoisoningChanceUpdate
        {

            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(ref bool __result, ref float startingCalories, GearItem __instance)
            {

                //completely overrides initial food poisoning check 

                if (!__instance.m_FoodItem)
                {
                    __result = false;
                    return;
                }

                if (startingCalories < 35f)
                {
                    __result =  false;
                    return;
                }
                if (!__instance.m_FoodItem.m_IsRawMeat && __instance.GetNormalizedCondition() > 0.45f)
                {
                    __result = false;
                    return;
                }

                if (__instance.IsWornOut())
                {
                    __result = false;
                    return;
                } 

                //float percent = 100f - __instance.GetNormalizedCondition();

                __result = false;

            }

        }

        
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.EatingComplete_Internal))]

        public class FoodPoisoningContractDelay
        {

            public static void Postfix(PlayerManager __instance) 
            {

                FoodPoisoningHelper fph = new FoodPoisoningHelper();
                SaveDataManager sdm = Mod.sdm;

                if (fph.RollFoodPoisoningChance(__instance.m_FoodItemEaten, __instance.m_FoodItemEatenStartingCalories) && sdm.LoadData("scheduledFoodPoisoning") != "true" && !GameManager.GetFoodPoisoningComponent().HasTakenAntibiotics())
                {
                    int val = Random.Range(Settings.settings.fpMinTime, Settings.settings.fpMaxTime);
                    sdm.Save("true", "scheduledFoodPoisoning");
                    Moment.Moment.ScheduleRelative(Mod.Instance, new Moment.EventRequest((0, val, 0), "takeEffectFoodPoisoning", __instance.m_FoodItemEaten.DisplayName));
                }
            }


            public static void GetAfflictions(FirstAidItem fid, ref Il2CppSystem.Collections.Generic.List<Affliction> list)
            {

                Panel_Affliction pa = InterfaceManager.GetPanel<Panel_Affliction>();

                Panel_Affliction.RefreshtListOfShownAfflictionTypes();
                if(list == null)
                {
                    return;
                }
                list.Clear();
                foreach(AfflictionType shownAfflictionType in Panel_Affliction.s_ShownAfflictionTypes)
                {
                    int afflictionsCount = Panel_Affliction.GetAfflictionsCount(shownAfflictionType);
                    for (int i = 0; i < afflictionsCount; i++)
                    {
                        Affliction currentAffliction = Panel_Affliction.GetCurrentAffliction(shownAfflictionType, i);
                        Panel_Affliction.MaybeAddOrFilter(fid, list, currentAffliction);
                    }
                }


            }


        }

        [HarmonyPatch(typeof(AfflictionComponent.Utilities.VanillaOverrides), nameof(AfflictionComponent.Utilities.VanillaOverrides.FoodPoisoningMethod))]

        private static class FoodPoisoningOverride
        {
            public static void Postfix(ref Panel_FirstAid __instance, ref int selectedAfflictionIndex, ref int num, ref int num4)
            {

                Panel_FirstAid panel = InterfaceManager.GetPanel<Panel_FirstAid>();
                panel.m_ObjectRestRemaining.SetActive(false);
                num4 = Mathf.CeilToInt(FoodPoisoningHelper.GetRemainingHours() * 60f);
            }
        }
    }
}
