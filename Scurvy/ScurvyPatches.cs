using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using MelonLoader;
using UnityEngine;

namespace ImprovedAfflictions.Scurvy
{
    internal class ScurvyPatches
    {

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UseFoodInventoryItem))]

        public class ScurvyCalorieReduction
        {

            public static void Prefix(ref GearItem gi) 
            {

                if (!GameManager.GetScurvyComponent().HasAffliction()) return;

                //if food has no or little vitamin c, reduce the amount by half if player has scurvy
                if(gi.m_FoodItem.m_Nutrients.Count == 0 || gi.m_FoodItem.m_Nutrients[0].m_Amount <= 20)
                {
                    gi.m_FoodItem.m_CaloriesRemaining /= 2;
                }
            }

        }

        [HarmonyPatch(typeof(ScurvyManager), nameof(ScurvyManager.Update))]

        public class ScurvyFatigueAndCarryCapacityReduction
        {

            public static void Prefix(ScurvyManager __instance)
            {
                __instance.m_FatigueMultiplier = 2;
                __instance.m_CarryCapacityDebuff = new Il2CppTLD.IntBackedUnit.ItemWeight(-7000000000);
            }

        }

        

        [HarmonyPatch(typeof(Freezing), nameof(Freezing.CalculateBodyTemperature))]

        public class ScurvyTemperatureReduction
        {

            public static void Postfix(float __result)
            {
                __result -= 10;
            }

        }

       
    }
}
