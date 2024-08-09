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
using ImprovedAfflictions.CustomAfflictions;
using Random = UnityEngine.Random;

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
                string desc = "You've exposed your hands or feet to corrosive chemicals and have suffered severe burns. Take painkillers to numb the pain and wait for them to heal.";

                if (!GameManager.GetPlayerManagerComponent().HasFootwearOn())
                {

                    if (__instance.m_Toxicity >= 20 && __instance.m_InHazardZone)
                    {
                        float duration = Random.Range(96f, 240f);
                        new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", AfflictionBodyArea.FootLeft, "ico_injury_majorBruising", false, false, duration, false, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], 25f);
                        new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", AfflictionBodyArea.FootRight, "ico_injury_majorBruising", false, false, duration, false, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], 25f);
                    }
                }

                if (!GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Hands, ClothingLayer.Base))
                {
                    if (__instance.m_Toxicity >= 35 && __instance.m_InHazardZone) 
                    {
                        float duration = Random.Range(72f, 120f);
                        new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", AfflictionBodyArea.HandLeft, "ico_major_bruising", false, false, duration, false, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], 25f);
                        new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", AfflictionBodyArea.HandRight, "ico_major_bruising", false, false, duration, false, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], 25f);
                    }
                }
            }
        }

    }
}
