using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using ImprovedAfflictions.Component;
using Random = System.Random;


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

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UseFirstAidItem))]

        public class PainkillerModifier
        {
            public static void Postfix(PlayerManager __instance)
            {
                PainManager pm = Mod.painManager;

                Mod.Logger.Log("Using first aid item", ComplexLogger.FlaggedLoggingLevel.Debug);

                if (__instance.m_FirstAidItemUsed.name.ToLowerInvariant().Contains("painkiller"))
                {
                    float amount = __instance.m_FirstAidItemUsed.m_GearItem.m_CurrentHP < 45 ? 20f * ((__instance.m_FirstAidItemUsed.m_GearItem.m_CurrentHP + 20) / 100) : 20f;

                    Mod.Logger.Log("Big testo", ComplexLogger.FlaggedLoggingLevel.Debug);

                    pm.AdministerPainkillers(amount);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UseFoodInventoryItem))]

        public class FeelTheRoses
        {

            public static void Postfix(PlayerManager __instance)
            {
                if (__instance.m_FoodItemEaten.name.ToLowerInvariant().Contains("rosehiptea"))
                {
                    Random rand = new Random();

                    int amount = rand.Next(5, 10);

                    Mod.painManager.AdministerPainkillers(amount);
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
