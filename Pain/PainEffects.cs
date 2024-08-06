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

namespace ImprovedAfflictions.Pain
{
    internal class PainEffects
    {

        //NEED TO ADD MY OWN

        /**
        public void IntensePainPulse(float amount)
        {

            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            GameManager.GetCameraStatusEffects().m_WaterTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_WaterTarget, amount * 1.1f);
            GameManager.GetCameraStatusEffects().m_SprainTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_SprainTarget, amount);
            if(ac.m_PainkillerLevel < 60f) GameManager.GetCameraStatusEffects().m_HeadacheTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_HeadacheTarget, amount);
            GameManager.GetCameraStatusEffects().m_SprainVignetteColor = Color.white;

        }

        public void HeadTraumaPulse(float amount)
        {
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            GameManager.GetCameraStatusEffects().m_WaterTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_WaterTarget, amount * 2f);
            GameManager.GetCameraStatusEffects().m_SprainTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_SprainTarget, amount);
            if (ac.m_PainkillerLevel < 60f) GameManager.GetCameraStatusEffects().m_HeadacheTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_HeadacheTarget, amount);
            GameManager.GetCameraStatusEffects().m_SprainVignetteColor = ac.m_PainkillerLevel < 60f ? Color.black : Color.white;
        }

        public void OverdoseVignette(float amount)
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
        **/
    }
}
