using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Collections;
using ImprovedAfflictions.CustomAfflictions;
using Random = UnityEngine.Random;
using ImprovedAfflictions.Utils;
using static UnityEngine.ParticleSystem;
using static Il2CppSystem.Decimal.DecCalc;

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

                AfflictionHelper ph = new AfflictionHelper();
                string desc = "You've exposed your hands or feet to corrosive chemicals and have suffered severe burns. Take painkillers to numb the pain and wait for them to heal.";

                if (!GameManager.GetPlayerManagerComponent().HasFootwearOn())
                {

                    float duration = Random.Range(96f, 240f);


                    if (__instance.m_Toxicity >= 20 && __instance.m_InHazardZone)
                    {
                        var burn1 = new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", "ico_injury_majorBruising", AfflictionBodyArea.FootLeft, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], duration, 25f, 10f, 0.9f);
                        burn1.SetInstanceTypeBasedOnName();
                        burn1.Start();
                        var burn2 = new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", "ico_injury_majorBruising", AfflictionBodyArea.FootRight, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], duration, 25f, 10f, 0.9f);
                        burn2.SetInstanceTypeBasedOnName();
                        burn2.Start();
                    }
                }

                if (!GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Hands, ClothingLayer.Base))
                {
                    if (__instance.m_Toxicity >= 35 && __instance.m_InHazardZone)
                    {
                        float duration = Random.Range(72f, 120f);


                        var burn1 = new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", "ico_injury_majorBruising", AfflictionBodyArea.HandLeft, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], duration, 25f, 10f, 0.9f);
                        burn1.SetInstanceTypeBasedOnName();
                        burn1.Start();
                        var burn2 = new CustomPainAffliction("Chemical Burns", "Corrosive Chemicals", desc, "", "ico_injury_majorBruising", AfflictionBodyArea.HandRight, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], duration, 25f, 10f, 0.9f);
                        burn2.SetInstanceTypeBasedOnName();
                        burn2.Start();
                    }
                }
            }
        }

    }
}
