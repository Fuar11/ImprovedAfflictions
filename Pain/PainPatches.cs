using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Random = UnityEngine.Random;
using System.Text.Json;
using ImprovedAfflictions.Utils;
using ImprovedAfflictions.Component;


namespace ImprovedAfflictions.Pain
{
    internal class PainPatches
    {


        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]

        public class SavePainData
        {

            public static void Postfix()
            {
                Mod.painManager.SaveData();
            }

        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.TreatAfflictionWithFirstAid))]

        public class PainkillerModifier
        {
            public static void Postfix(PlayerManager __instance)
            {
                PainManager pm = Mod.painManager;

                if (__instance.m_FirstAidItemUsed.name.ToLowerInvariant().Contains("painkiller"))
                {
                    float amount = __instance.m_FirstAidItemUsed.m_GearItem.m_CurrentHP < 45 ? 20f * ((__instance.m_FirstAidItemUsed.m_GearItem.m_CurrentHP + 20) / 100) : 20f;

                    pm.AdministerPainkillers(amount);
                }

            }

        }

        [HarmonyPatch(typeof(EmergencyStim), nameof(EmergencyStim.ApplyEmergencyStim))]

        public class AdministerStimulants
        {
            public static void Postfix()
            {
                Mod.painManager.AdministerPainkillers(50f, true);
            }
        }
    }
}
