using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using HarmonyLib;
using ImprovedAfflictions.Pain;
using MelonLoader;
using UnityEngine;
using System.Collections;

namespace ImprovedAfflictions.ChemicalPoisoning
{
    internal class ChemicalPoisoningPatches : MonoBehaviour
    {

        [HarmonyPatch(typeof(Il2Cpp.ChemicalPoisoning), nameof(Il2Cpp.ChemicalPoisoning.HasRisk))]

        public class ChemicalPoisoningRiskSpeedIncrease
        {
            public static void Prefix(Il2Cpp.ChemicalPoisoning __instance)
            {
                __instance.m_ToxicityGainedPerHour = 200;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.ChemicalPoisoning), nameof(Il2Cpp.ChemicalPoisoning.Update))]

        public class ChemicalPoisoningBurns
        {

            public static void Postfix(Il2Cpp.ChemicalPoisoning __instance)
            {

                if (GameManager.m_IsPaused) return;

                PainHelper ph = new PainHelper();
                string cause = "Corrosive Chemical Burns";

                if (!GameManager.GetPlayerManagerComponent().HasFootwearOn())
                {

                    if (__instance.m_Toxicity >= 20 && __instance.m_InHazardZone)
                    {

                        GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.FootLeft, cause, AfflictionOptions.None);
                        GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.FootRight, cause, AfflictionOptions.None);
                    }
                }

                if (!GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Hands, ClothingLayer.Base))
                {
                    if (__instance.m_Toxicity >= 35 && __instance.m_InHazardZone) 
                    {

                        GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.HandLeft, cause, AfflictionOptions.None);
                        GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.HandRight, cause, AfflictionOptions.None);
                    }
                }
            }

            private static IEnumerator ApplyBurnsPain(string location, string cause)
            {
                float waitSeconds = 1f;
                for (float t = 0f; t < waitSeconds; t += Time.deltaTime) yield return null;

                if(location == "hands")
                {
                    GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.HandLeft, cause, AfflictionOptions.None);
                    GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.HandRight, cause, AfflictionOptions.None);
                }
                else
                {
                    GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.FootLeft, cause, AfflictionOptions.None);
                    GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.FootRight, cause, AfflictionOptions.None);
                }
               
            }
        }

    }
}
