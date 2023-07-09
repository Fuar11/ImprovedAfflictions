using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace ImprovedAfflictions.Pain
{
    internal class PainEffects
    {


        public void IntensePainPulse(float amount)
        {

            GameManager.GetCameraStatusEffects().m_WaterTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_WaterTarget, amount * 1.1f);
            GameManager.GetCameraStatusEffects().m_SprainTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_SprainTarget, amount);

        }

        public void HeadTraumaPulse(float amount)
        {
            GameManager.GetCameraStatusEffects().m_WaterTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_WaterTarget, amount * 2f);
            GameManager.GetCameraStatusEffects().m_SprainTarget = Mathf.Max(GameManager.GetCameraStatusEffects().m_SprainTarget, amount);
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

    }
}
