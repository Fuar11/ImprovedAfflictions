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
using MelonLoader;
using UnityEngine;

namespace ImprovedAfflictions.Dysentery
{
    internal class DysenteryPatches
    {

        [HarmonyPatch(typeof(Il2Cpp.Dysentery), nameof(Il2Cpp.Dysentery.DysenteryStart))]

        public class DysenteryOverride
        {

            public static void Postfix(Il2Cpp.Dysentery __instance)
            {

                __instance.m_ThirstIncreasePerHour -= 2f;
                __instance.m_DurationHours += 12f;
                __instance.m_NumHoursRestForCure = __instance.m_DurationHours - 5f;
                __instance.m_DurationHours += GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            }

        }


    }
}
