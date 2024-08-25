using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using ImprovedAfflictions.Component;
using AfflictionComponent.Components;
using ImprovedAfflictions.CustomAfflictions;
using static Il2Cpp.Panel_Debug;
using Il2CppSWS;

namespace ImprovedAfflictions.Pain
{
    internal class PainEffects
    {

        //NEED TO ADD MY OWN

        public static void UpdatePainEffects()
        {

            if (GameManager.m_ActiveScene.ToLowerInvariant().Contains("menu") || GameManager.m_ActiveScene.ToLowerInvariant().Contains("boot")) return;

            PainManager pm = Mod.painManager;
            AfflictionManager am = pm.am;

            float maxIntensity = 0f;

            foreach (CustomPainAffliction aff in am.m_Afflictions.OfType<CustomPainAffliction>())
            {

                if (aff.m_PulseFxIntensity >= maxIntensity)
                {
                    pm.m_PulseFxIntensity = aff.m_PulseFxIntensity;
                    pm.m_PulseFxFrequencySeconds = aff.m_PulseFxFrequencySeconds;
                    maxIntensity = aff.m_PulseFxIntensity;
                }

            }

            //if painkillers have been taken, dull the pain effects by how much drugs are in your system

            pm.m_PulseFxIntensity /= pm.GetPainkillerLevel() > 1 ? pm.GetPainkillerLevel() / 10 : 1;

            if (pm.IsOnPainkillers())
            {

                //always less than 1
                float painkillerMulti = pm.GetPainkillerLevel() / pm.m_PainkillerDecrementStartingAmount;

                pm.m_PulseFxIntensity *= pm.IsOnPainkillers() ? painkillerMulti : 1;
            }

        }


            
            public static void IntensePainPulse(float amount)
            {
                PainManager ac = Mod.painManager;

                GameManager.GetCameraStatusEffects().m_WaterTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_WaterTarget, amount * 1.1f);
                GameManager.GetCameraStatusEffects().m_SprainTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_SprainTarget, amount);
                if(ac.m_PainkillerLevel < 60f) GameManager.GetCameraStatusEffects().m_HeadacheTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_HeadacheTarget, amount);
                GameManager.GetCameraStatusEffects().m_SprainVignetteColor = Color.white;

            }

            public static void HeadTraumaPulse(float amount)
            {
                PainManager ac = Mod.painManager;

                GameManager.GetCameraStatusEffects().m_WaterTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_WaterTarget, amount * 2f);
                GameManager.GetCameraStatusEffects().m_SprainTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_SprainTarget, amount);
                if (ac.m_PainkillerLevel < 60f) GameManager.GetCameraStatusEffects().m_HeadacheTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_HeadacheTarget, amount);
                GameManager.GetCameraStatusEffects().m_SprainVignetteColor = ac.m_PainkillerLevel < 60f ? Color.black : Color.white;
            }

            public static void OverdoseVignette(float amount)
            {
                GameManager.GetCameraStatusEffects().m_HeadacheTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_HeadacheTarget, amount);
                GameManager.GetCameraStatusEffects().m_HeadacheSinSpeed = 2.5f;
                GameManager.GetCameraStatusEffects().m_HeadacheVignetteIntensity = 0.2f;
            }

            //overrides pain pulse allowing it to accept any value for the intensity
            [HarmonyPatch(typeof(CameraEffects), nameof(CameraEffects.SprainPulse))]

            public class SprainPulseExtension
            {

                public static bool Prefix()
                {
                    return false;
                }

                public static void Postfix(ref float amount, CameraEffects __instance)
                {
                    __instance.m_CameraStatusEffects.m_SprainTarget = Mathf.Max(__instance.m_CameraStatusEffects.m_SprainTarget, amount);
                }

            }

            [HarmonyPatch(typeof(CameraEffects), nameof(CameraEffects.HeadachePulse))]

            public class HeadachePulseExtension
            {

                public static bool Prefix()
                {
                    return false;
                }

                public static void Postfix(ref float amount, CameraEffects __instance)
                {
                    __instance.m_CameraStatusEffects.m_HeadacheTarget = Mathf.Max(__instance.m_CameraStatusEffects.m_HeadacheTarget, amount);
                }

            }
        }
}
