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
using Microsoft.Extensions.Logging;
using ImprovedAfflictions.FoodPoisoning;
using ComplexLogger;

namespace ImprovedAfflictions.Dysentery
{
    internal class DysenteryPatches
    {

        [HarmonyPatch(typeof(Il2Cpp.Dysentery), nameof(Il2Cpp.Dysentery.DysenteryStart))]

        public class DysenteryStartOverride
        {

            public static void Postfix(Il2Cpp.Dysentery __instance)
            {

                __instance.m_ThirstIncreasePerHour -= 2f;
                __instance.m_DurationHours += 12f;
                __instance.m_NumHoursRestForCure = __instance.m_DurationHours - 5f;
                __instance.m_DurationHours += GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            }

        }

        [HarmonyPatch(typeof(Il2Cpp.Dysentery), nameof(Il2Cpp.Dysentery.DysenteryEnd))]

        public class DysenteryDescriptionReset
        {

            public static void Postfix(Il2Cpp.Dysentery __instance)
            {
                SaveDataManager sdm = Mod.sdm;
                sdm.Save("", "dysenteryCause");
            }

        }

        [HarmonyPatch(typeof(AfflictionComponent.Utilities.VanillaOverrides), nameof(AfflictionComponent.Utilities.VanillaOverrides.DysenteryMethod))]

        private static class DysenteryOverride
        {
            public static void Prefix(ref Panel_FirstAid __instance, ref int selectedAfflictionIndex, ref int num, ref int num4)
            {
                Panel_FirstAid panel = InterfaceManager.GetPanel<Panel_FirstAid>();
                panel.m_SelectedAffButton.m_LabelCause.text = Mod.sdm.LoadData("dysenteryCause");
            }
        }
    }
}
