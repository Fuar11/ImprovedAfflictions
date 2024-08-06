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

        [HarmonyPatch(typeof(PlayerStruggle), nameof(PlayerStruggle.ApplyBearDamageAfterStruggleEnds))]

        public class BrokenRibTriggerBear
        {
            public static void Postfix()
            {

                float chance = GameManager.GetDamageProtection().HasBallisticVest() ? 15f : 25f;

                if (GameManager.GetFatigueComponent().m_CurrentFatigue <= 40f) chance += 10f;

                if (Il2Cpp.Utils.RollChance(chance))
                {
                    float recoveryTimeModifier = GameManager.GetDamageProtection().MaybeApplyBrokenRibModifier(DamageReason.Animals, WildlifeType.Moose);
                    GameManager.GetBrokenRibComponent().BrokenRibStart("Bear Attack", displayIcon: true, noVO: true, isMinor: true, autoSave: true, recoveryTimeModifier);
                    GameManager.GetCameraEffects().PainPulse(1f);
                }   
            }

        }
    }
}
