using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using Random = UnityEngine.Random;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using ImprovedAfflictions.Pain;

namespace ImprovedAfflictions.BrokenRibs
{
    internal class BrokenRibs
    {

        [HarmonyPatch(typeof(BrokenRib), nameof(BrokenRib.Update))]

        public class BrokenRibsCureUpdate
        {

            public static bool Prefix() { return false; }

            public static void Postfix(BrokenRib __instance)
            {

                if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended || GameManager.GetPlayerManagerComponent().m_God || !__instance.HasBrokenRib())
                {
                    return;
                }
                for (int num = __instance.m_CausesLocIDs.Count - 1; num >= 0; num--)
                {
                    if (__instance.m_BandagesApplied[num] >= __instance.m_BandagesRequiredPerInstance && __instance.m_ElapsedRestList[num] > __instance.m_NumHoursRestForCureList[num] - 0.1f)
                    {
                        __instance.BrokenRibEnd(num);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BrokenRib), nameof(BrokenRib.BrokenRibStart))]

        public class BrokenRibAddPain
        {

            public static void Postfix()
            {
                GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.Chest, "broken rib", AfflictionOptions.PlayFX | AfflictionOptions.DoAutoSave | AfflictionOptions.DisplayIcon);

            }

        }


        [HarmonyPatch(typeof(BrokenRib), nameof(BrokenRib.AddRest))]

        public class RestModifier
        {

            public static bool Prefix() { return false; }

            public static void Postfix(BrokenRib __instance, ref float hours)
            {

                for (int num = __instance.m_CausesLocIDs.Count - 1; num >= 0; num--)
                {
                    __instance.m_ElapsedRestList[num] += hours;
                    if (__instance.m_BandagesApplied[num] >= __instance.m_BandagesRequiredPerInstance && __instance.m_ElapsedRestList[num] > __instance.m_NumHoursRestForCureList[num] - 0.1f)
                    {
                        __instance.BrokenRibEnd(num);
                    }
                }

            }
        }


        [HarmonyPatch(typeof(PlayerStruggle), nameof(PlayerStruggle.ApplyBearDamageAfterStruggleEnds))]

        public class BrokenRibTriggerBear
        {
            public static void Postfix(ref string causeLocId)
            {

                float chance = GameManager.GetDamageProtection().HasBallisticVest() ? 15f : 25f;

                if (GameManager.GetFatigueComponent().m_CurrentFatigue <= 40f) chance += 10f;

                if (Il2Cpp.Utils.RollChance(chance))
                {
                    float recoveryTimeModifier = GameManager.GetDamageProtection().MaybeApplyBrokenRibModifier(DamageReason.Animals, WildlifeType.Moose);
                    GameManager.GetBrokenRibComponent().BrokenRibStart(causeLocId, displayIcon: true, noVO: true, isMinor: true, autoSave: true, recoveryTimeModifier);
                    GameManager.GetCameraEffects().PainPulse(1f);
                }   
            }

        }
    }
}
