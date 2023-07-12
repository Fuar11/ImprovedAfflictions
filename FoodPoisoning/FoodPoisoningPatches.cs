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
using MelonLoader;
using Il2CppEpic.OnlineServices;

namespace ImprovedAfflictions.FoodPoisoning
{
    internal class FoodPoisoningPatches
    {

        [HarmonyPatch(typeof(GearItem), nameof(GearItem.RollForFoodPoisoning))]

        public class FoodPoisoningChanceUpdate
        {

            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(ref bool __result, ref float startingCalories, GearItem __instance)
            {

                if (!__instance.m_FoodItem)
                {
                    __result = false;
                }
                if (startingCalories < 35f)
                {
                    __result =  false;
                }
                if (!__instance.m_FoodItem.m_IsRawMeat && __instance.GetNormalizedCondition() > 0.6f)
                {
                    __result = false;
                }

                if (__instance.IsWornOut()) __result = true;

                float percent = 100f - __instance.GetNormalizedCondition();
                
                __result = Il2Cpp.Utils.RollChance(percent);

            }

        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.EatingComplete_Internal))]

        public class FoodPoisoningContractDelay
        {
            public static bool Prefix() { return false; }
            public static void Postfix(ref bool success, ref bool playerCancel, ref float progress, PlayerManager __instance) 
            {

                GameManager.GetHungerComponent().ClearAddReserveCaloriesOverTime();
                if (InterfaceManager.GetPanel<Panel_Inventory>().IsEnabled())
                {
                    InterfaceManager.GetPanel<Panel_Inventory>().MarkDirty();
                }
                if (!__instance.m_FoodItemEaten)
                {
                    return;
                }
                if (__instance.m_FoodItemEaten.m_FoodItem.m_MustConsumeAll)
                {
                    if (!Il2Cpp.Utils.Approximately(progress, 1f))
                    {
                        return;
                    }
                    GameManager.GetHungerComponent().AddReserveCalories(__instance.m_FoodItemEaten.m_FoodItem.m_CaloriesRemaining);
                    GameManager.GetThirstComponent().AddThirst(-1f * __instance.m_FoodItemEaten.m_FoodItem.m_ReduceThirst);
                    __instance.m_FoodItemEaten.m_FoodItem.m_CaloriesRemaining = 0f;
                }
                __instance.m_FoodItemEaten.m_FoodItem.m_Packaged = false;
                float num = (__instance.m_FoodItemEatenStartingCalories - __instance.m_FoodItemEaten.m_FoodItem.m_CaloriesRemaining) / __instance.m_FoodItemEaten.m_FoodItem.m_CaloriesTotal;
                if (__instance.m_FoodItemEaten.m_FoodItem.m_IsMeat || __instance.m_FoodItemEaten.m_FoodItem.m_IsFish)
                {
                    float itemWeightKG = __instance.m_FoodItemEaten.GetItemWeightKG();
                    float increment = __instance.m_FoodItemEatenStartingWeight - itemWeightKG;
                    if (__instance.m_FoodItemEaten.m_FoodItem.m_IsMeat)
                    {
                        StatsManager.IncrementValue(StatID.MeatConsumed, increment);
                    }
                    if (__instance.m_FoodItemEaten.m_FoodItem.m_IsFish)
                    {
                        StatsManager.IncrementValue(StatID.FishConsumed, increment);
                    }
                }
                GameManager.GetAchievementManagerComponent().AteFoodItem(__instance.m_FoodItemEaten.m_FoodItem);
                SpecialEvent.EatingComplete(__instance.m_FoodItemEaten);
                if (__instance.m_FoodItemEaten.m_FirstAidItem)
                {
                    __instance.m_FirstAidItemUsed = __instance.m_FoodItemEaten.m_FirstAidItem;
                    __instance.m_AfflictionSelected = Affliction.InvalidAffliction;
                    Il2CppSystem.Collections.Generic.List<Affliction> list = new Il2CppSystem.Collections.Generic.List<Affliction>();
                    GetAfflictions(__instance.m_FirstAidItemUsed, ref list);
                    if (list.Count == 1)
                    {
                        __instance.m_AfflictionSelected = list[0];
                    }
                    else if (list.Count > 1)
                    {
                        int index = Random.Range(0, list.Count);
                        __instance.m_AfflictionSelected = list[index];
                    }
                    __instance.OnFirstAidComplete(success, playerCancel: false, progress);
                }
                else
                {
                    GameManager.GetInventoryComponent().OnConsumed(__instance.m_FoodItemEaten);
                }
                GameManager.GetFeatStraightToHeart().IncrementItemConsumed(__instance.m_FoodItemEaten, num);
                __instance.HandleEatingCompleteStackableFoodItem(__instance.m_FoodItemEaten);
                if (__instance.ShouldDestroyFoodAfterEating(__instance.m_FoodItemEaten))
                {
                    GameManager.GetInventoryComponent().DestroyGear(__instance.m_FoodItemEaten.gameObject);
                    __instance.m_FoodItemEaten.m_FoodItem.DoGearHarvestAfterFinishEating();
                }
                __instance.m_FoodItemEaten.ApplyBuffs(num);
                bool flag = false;
                /**if (GameManager.GetSkillCooking().NoParasitesOrFoodPosioning() && !__instance.m_FoodItemEaten.m_FoodItem.m_IsRawMeat)
                {
                    flag = true;
                } **/
                if (!flag)
                {
                    if (__instance.m_FoodItemEaten.RollForFoodPoisoning(__instance.m_FoodItemEatenStartingCalories))
                    {

                        int val = Random.Range(16, 24);
                        MelonLogger.Msg("Scheduling food poisoning!");
                        Moment.Moment.ScheduleRelative(Implementation.Instance, new Moment.EventRequest((0, val, 0), "takeEffectFoodPoisoning", __instance.m_FoodItemEaten.DisplayName));
                    }
                    GameManager.GetIntestinalParasitesComponent().AddRiskPercent(__instance.m_FoodItemEaten.m_FoodItem.m_ParasiteRiskPercentIncrease);
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


    }
}
