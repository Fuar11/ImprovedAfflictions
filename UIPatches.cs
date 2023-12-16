using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using Random = UnityEngine.Random;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using static Il2Cpp.Panel_Affliction;
using ImprovedAfflictions.Pain;
using Il2CppSystem.Xml.Schema;
using ImprovedAfflictions.FoodPoisoning;
using ImprovedAfflictions.Pain.Component;
using ImprovedAfflictions.Component;
using Il2CppSystem.Security;
using Unity.VisualScripting;

namespace ImprovedAfflictions
{
    internal class UIPatches
    {

        [HarmonyPatch(typeof(AfflictionButton), nameof(AfflictionButton.SetCauseAndEffect))]

        public class AfflictionListUIUpdate
        {

            public static bool Prefix(ref string causeStr, ref AfflictionType affType, ref AfflictionBodyArea location, ref int index, ref string effectName, ref string spriteName)
            {
                return (affType != AfflictionType.SprainPain || (affType == AfflictionType.SprainPain && causeStr.ToLowerInvariant().Contains("broken rib"))) ? true : false;
            }

            public static void Postfix(ref string causeStr, ref AfflictionType affType, ref AfflictionBodyArea location, ref int index, ref string effectName, ref string spriteName, AfflictionButton __instance)
            {
                if(affType != AfflictionType.SprainPain || (affType == AfflictionType.SprainPain && causeStr.ToLowerInvariant().Contains("broken rib"))) return;

                __instance.m_LabelCause.text = GetAfflictionCauseNameBasedOnCause(causeStr, location);
                __instance.m_LabelCause.color = __instance.m_CauseColor;
                __instance.m_AfflictionType = affType;
                __instance.m_AfflictionLocation = location;
                __instance.m_Index = index;
                Color colorBasedOnAffliction = __instance.GetColorBasedOnAffliction(__instance.m_AfflictionType, isHovering: false);
                __instance.m_SpriteEffect.spriteName = GetIconNameBasedOnCause(causeStr);
                __instance.m_LabelEffect.text = GetAfflictionNameBasedOnCause(causeStr, location);
                __instance.UpdateFillBar();
                __instance.m_SpriteEffect.color = colorBasedOnAffliction;
                __instance.m_LabelEffect.color = colorBasedOnAffliction;
            }
        }

        public static string GetAfflictionCauseNameBasedOnCause(string cause, AfflictionBodyArea location)
        {
            if (cause.ToLowerInvariant().Contains("corrosive chemical burns")) return "Corrosive Chemicals";
            else if (cause.ToLowerInvariant().Contains("concussion")) return "Head Trauma";
            else return cause;
        }
        public static string GetIconNameBasedOnCause(string cause)
        {
            //could also go with ico_injury_burn1 for this one vvv
            if (cause.ToLowerInvariant().Contains("corrosive chemical burns")) return "ico_injury_majorBruising";
            else if (cause.ToLowerInvariant().Contains("concussion")) return "ico_injury_diabetes";
            else if (cause.ToLowerInvariant().Contains("bite") || cause.ToLowerInvariant().Contains("scratch") || cause.ToLowerInvariant().Contains("claw")) return "ico_injury_laceration";
            else if (cause.ToLowerInvariant().Contains("fall")) return "ico_CarryRestrictions";
            else return "ico_injury_pain";
        }
        public static string GetAfflictionNameBasedOnCause(string cause, AfflictionBodyArea location)
        {

            if (cause.ToLowerInvariant().Contains("corrosive")) return "Chemical Burns";
            else if (cause.ToLowerInvariant().Contains("fall"))
            {
                if (location == AfflictionBodyArea.FootRight || location == AfflictionBodyArea.FootLeft) return "Sprained Ankle";
                else return "Sprained Wrist";
            }
            else if (cause.ToLowerInvariant().Contains("head trauma")) return "Concussion";
            else return cause;
        }

        [HarmonyPatch(typeof(Panel_FirstAid), nameof(Panel_FirstAid.RefreshRightPage))]

        public class FirstAidInstanceUIUpdate
        {
            public static bool Prefix(Panel_FirstAid __instance)
            {

                if (!__instance.m_SelectedAffButton) return true;

                if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Insomnia || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.InsomniaRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoning || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoningRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ScurvyRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Scurvy || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ResistInsomniaBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatDebuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityDebuff)
                {
                    return true;
                }

                return false;
            }

            public static void Postfix(Panel_FirstAid __instance)
            {

                if (!__instance.m_SelectedAffButton) return;

                if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Insomnia || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.InsomniaRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoning || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoningRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ScurvyRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Scurvy || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ResistInsomniaBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatDebuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityDebuff)
                {
                    return; 
                }

                for (int i = 0; i < __instance.m_FakButtons.Length; i++)
                {
                    __instance.m_FakButtons[i].SetNeeded(needed: false);
                }
                __instance.m_SpecialTreatmentWindow.SetActive(false);
                __instance.m_BuffWindow.SetActive(false);
                if (__instance.m_ScrollListEffects.m_ScrollObjects.Count == 0)
                {
                    NGUITools.SetActive(__instance.m_RightPageHealthyObject, state: true);
                    __instance.HideRightPage();
                    return;
                }
                NGUITools.SetActive(__instance.m_RightPageHealthyObject, state: false);
                if (__instance.m_SelectedAffButton == null)
                {
                    __instance.HideRightPage();
                    return;
                }
                if (Affliction.AfflictionTypeIsBuff(__instance.m_SelectedAffButton.m_AfflictionType))
                {
                    if (!Panel_Affliction.HasAffliction(__instance.m_SelectedAffButton.m_AfflictionType))
                    {
                        __instance.HideRightPage();
                        return;
                    }
                }
                else if (!GameManager.GetConditionComponent().HasSpecificAffliction(__instance.m_SelectedAffButton.m_AfflictionType))
                {
                    __instance.HideRightPage();
                    return;
                }
                NGUITools.SetActive(__instance.m_RightPageObject, state: true);
                for (int j = 0; j < __instance.m_BodyIconList.Length; j++)
                {
                    __instance.m_BodyIconList[j].width = __instance.m_BodyIconWidthOriginal;
                    __instance.m_BodyIconList[j].height = __instance.m_BodyIconHeightOriginal;
                }
                int num = -1;

                if(__instance.m_SelectedAffButton.m_AfflictionType != AfflictionType.SprainPain || (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.SprainPain && __instance.m_SelectedAffButton.m_LabelCause.text.ToLowerInvariant().Contains("broken rib")))
                {
                    __instance.m_LabelAfflictionName.text = Affliction.LocalizedNameFromAfflictionType(__instance.m_SelectedAffButton.m_AfflictionType, __instance.m_SelectedAffButton.GetAfflictionIndex());
                }
                else
                {
                    __instance.m_LabelAfflictionName.text = GetAfflictionNameBasedOnCause(__instance.m_SelectedAffButton.m_LabelCause.text, __instance.m_SelectedAffButton.m_AfflictionLocation);
                }

                if (Affliction.IsRisk(__instance.m_SelectedAffButton.m_AfflictionType))
                {
                    __instance.m_LabelAfflictionName.color = InterfaceManager.m_FirstAidRiskColor;
                    if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.InfectionRisk)
                    {
                        InfectionRisk infectionRiskComponent = GameManager.GetInfectionRiskComponent();
                        UILabel labelAfflictionName = __instance.m_LabelAfflictionName;
                        labelAfflictionName.text = labelAfflictionName.text + " (" + infectionRiskComponent.GetCurrentRisk(__instance.m_SelectedAffButton.GetIndex()) + "%)";
                    }
                    else if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.HypothermiaRisk)
                    {
                        int num2 = (int)Mathf.Clamp(GameManager.GetHypothermiaComponent().GetHypothermiaRiskValue() * 100f, 1f, 99f);
                        UILabel labelAfflictionName = __instance.m_LabelAfflictionName;
                        labelAfflictionName.text = labelAfflictionName.text + " (" + num2 + "%)";
                    }
                    else if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.IntestinalParasitesRisk)
                    {
                        IntestinalParasites intestinalParasitesComponent = GameManager.GetIntestinalParasitesComponent();
                        UILabel labelAfflictionName = __instance.m_LabelAfflictionName;
                        labelAfflictionName.text = labelAfflictionName.text + " (" + intestinalParasitesComponent.GetCurrentRisk() + "%)";
                    }
                    else if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CabinFeverRisk)
                    {
                        CabinFever cabinFeverComponent = GameManager.GetCabinFeverComponent();
                        UILabel labelAfflictionName = __instance.m_LabelAfflictionName;
                        labelAfflictionName.text = labelAfflictionName.text + " (" + cabinFeverComponent.GetCurrentRisk() + "%)";
                    }
                    else if (__instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FrostbiteRisk)
                    {
                        int num3 = Mathf.Clamp((int)(GameManager.GetFrostbiteComponent().GetFrostbiteRiskValue((int)__instance.m_SelectedAffButton.m_AfflictionLocation) * 100f), 1, 99);
                        UILabel labelAfflictionName = __instance.m_LabelAfflictionName;
                        labelAfflictionName.text = labelAfflictionName.text + " (" + num3 + "%)";
                    }
                }
                else if (Affliction.IsBeneficial(__instance.m_SelectedAffButton.m_AfflictionType))
                {
                    __instance.m_LabelAfflictionName.color = InterfaceManager.m_FirstAidBuffColor;
                }
                else
                {
                    __instance.m_LabelAfflictionName.color = InterfaceManager.m_FirstAidRedColor;
                }
                int num4 = 0;
                int selectedAfflictionIndex = __instance.GetSelectedAfflictionIndex();
                switch (__instance.m_SelectedAffButton.m_AfflictionType)
                {
                    case AfflictionType.BloodLoss:
                        {
                            BloodLoss bloodLossComponent = GameManager.GetBloodLossComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = bloodLossComponent.m_Description;
                            __instance.m_LabelAfflictionDescription.text = "";
                            string[] remedySprites = new string[1] { "GEAR_HeavyBandage" };
                            bool[] remedyComplete = new bool[1] { !bloodLossComponent.RequiresBandage() };
                            int[] remedyNumRequired = new int[1] { 1 };
                            string[] altRemedySprites = null;
                            bool[] altRemedyComplete = null;
                            int[] altRemedyNumRequired = null;
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)bloodLossComponent.GetLocation(selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.BrokenRib:
                        {
                            BrokenRib brokenRibComponent = GameManager.GetBrokenRibComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = brokenRibComponent.m_Description;
                            string[] remedySprites = new string[1] { "GEAR_HeavyBandage" };
                            bool[] remedyComplete = new bool[1]
                            {
                            !brokenRibComponent.RequiresBandage(selectedAfflictionIndex)
                            };
                            int[] remedyNumRequired = new int[1]
                            {
                            brokenRibComponent.GetRequiredBandages(selectedAfflictionIndex)
                            };
                            string[] altRemedySprites = null;
                            bool[] altRemedyComplete = null;
                            int[] altRemedyNumRequired = null;
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, brokenRibComponent.GetRestAmountRemaining(selectedAfflictionIndex), brokenRibComponent.GetNumHoursRestForCure(selectedAfflictionIndex));
                            num = (int)brokenRibComponent.GetLocation(selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.Burns:
                        {
                            Il2Cpp.Burns burnsComponent = GameManager.GetBurnsComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = burnsComponent.m_LocalizedDescription.Text();
                            __instance.m_LabelAfflictionDescription.text = "";
                            string[] remedySprites = new string[2] { "GEAR_HeavyBandage", "GEAR_BottlePainKillers" };
                            bool[] remedyComplete = new bool[2]
                            {
                            !burnsComponent.RequiresBandage(),
                            !burnsComponent.RequiresPainkillers()
                            };
                            int[] remedyNumRequired = new int[2] { 1, 2 };
                            string[] altRemedySprites = new string[2] { "GEAR_HeavyBandage", "GEAR_RoseHipTea" };
                            bool[] altRemedyComplete = new bool[2]
                            {
                            !burnsComponent.RequiresBandage(),
                            !burnsComponent.RequiresPainkillers()
                            };
                            int[] altRemedyNumRequired = new int[2] { 1, 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.Burns, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.BurnsElectric:
                        {
                            BurnsElectric burnsElectricComponent = GameManager.GetBurnsElectricComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = burnsElectricComponent.m_Description;
                            __instance.m_LabelAfflictionDescription.text = "";
                            string[] remedySprites = new string[2] { "GEAR_HeavyBandage", "GEAR_BottlePainKillers" };
                            bool[] remedyComplete = new bool[2]
                            {
                            !burnsElectricComponent.RequiresBandage(),
                            !burnsElectricComponent.RequiresPainkillers()
                            };
                            int[] remedyNumRequired = new int[2] { 1, 2 };
                            string[] altRemedySprites = new string[2] { "GEAR_HeavyBandage", "GEAR_RoseHipTea" };
                            bool[] altRemedyComplete = new bool[2]
                            {
                            !burnsElectricComponent.RequiresBandage(),
                            !burnsElectricComponent.RequiresPainkillers()
                            };
                            int[] altRemedyNumRequired = new int[2] { 1, 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.BurnsElectric, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.Dysentery:
                        {

                            SaveDataManager sdm = Implementation.sdm;

                            Il2Cpp.Dysentery dysenteryComponent = GameManager.GetDysenteryComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = dysenteryComponent.m_Description;

                            string cause = sdm.LoadData("dysenteryCause");

                            if (cause != "" || cause is not null)
                            {
                                __instance.m_SelectedAffButton.m_LabelCause.text = cause;
                            }


                            string[] remedySprites = new string[2] { "GEAR_WaterSupplyPotable", "GEAR_BottleAntibiotics" };
                            bool[] remedyComplete = new bool[2]
                            {
            dysenteryComponent.GetWaterAmountRemaining() < 0.01f,
            dysenteryComponent.HasTakenAntibiotics()
                            };
                            int[] remedyNumRequired = new int[2] { 1, 2 };
                            string[] altRemedySprites = new string[2] { "GEAR_WaterSupplyPotable", "GEAR_ReishiTea" };
                            bool[] altRemedyComplete = new bool[2]
                            {
            dysenteryComponent.GetWaterAmountRemaining() < 0.01f,
            dysenteryComponent.HasTakenAntibiotics()
                            };
                            int[] altRemedyNumRequired = new int[2] { 1, 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, dysenteryComponent.GetWaterAmountRemaining(), 0f, 0f);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.Dysentery, selectedAfflictionIndex);
                            float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                            num4 = Mathf.CeilToInt((GameManager.GetDysenteryComponent().m_DurationHours - hoursPlayedNotPaused) * 60f);
                            break;
                        }
                    case AfflictionType.Infection:
                        {
                            Infection infectionComponent = GameManager.GetInfectionComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = infectionComponent.m_Description;
                            string[] remedySprites = new string[1] { "GEAR_BottleAntibiotics" };
                            bool[] remedyComplete = new bool[1] { infectionComponent.HasTakenAntibiotics(selectedAfflictionIndex) };
                            int[] remedyNumRequired = new int[1] { 2 };
                            string[] altRemedySprites = new string[1] { "GEAR_ReishiTea" };
                            bool[] altRemedyComplete = new bool[1] { infectionComponent.HasTakenAntibiotics(selectedAfflictionIndex) };
                            int[] altRemedyNumRequired = new int[1] { 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, infectionComponent.GetRestAmountRemaining(selectedAfflictionIndex), infectionComponent.m_NumHoursRestForCure);
                            num = (int)infectionComponent.GetLocation(selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.FoodPoisioning:
                        {
                            Il2Cpp.FoodPoisoning foodPoisoningComponent = GameManager.GetFoodPoisoningComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = foodPoisoningComponent.m_Description;
                            string[] remedySprites = new string[1] { "GEAR_BottleAntibiotics" };
                            bool[] remedyComplete = new bool[1] { foodPoisoningComponent.HasTakenAntibiotics() };
                            int[] remedyNumRequired = new int[1] { 2 };
                            string[] altRemedySprites = new string[1] { "GEAR_ReishiTea" };
                            bool[] altRemedyComplete = new bool[1] { foodPoisoningComponent.HasTakenAntibiotics() };
                            int[] altRemedyNumRequired = new int[1] { 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.FoodPoisioning, selectedAfflictionIndex);
                            FoodPoisoningHelper foodPoisoningHelper = new FoodPoisoningHelper();
                            num4 = Mathf.CeilToInt(foodPoisoningHelper.GetRemainingHours() * 60f);
                            break;
                        }
                    case AfflictionType.SprainedAnkle:
                        {
                            SprainedAnkle sprainedAnkleComponent = GameManager.GetSprainedAnkleComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = sprainedAnkleComponent.Description;
                            string[] remedySprites = new string[1] { "GEAR_HeavyBandage" };
                            bool[] remedyComplete = new bool[1];
                            int[] remedyNumRequired = new int[1] { 1 };
                            string[] altRemedySprites = null;
                            bool[] altRemedyComplete = null;
                            int[] altRemedyNumRequired = null;
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, sprainedAnkleComponent.GetRestAmountRemaining(selectedAfflictionIndex), sprainedAnkleComponent.m_NumHoursRestForCure);
                            num = (int)sprainedAnkleComponent.GetLocation(selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.InfectionRisk:
                        {
                            InfectionRisk infectionRiskComponent2 = GameManager.GetInfectionRiskComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = infectionRiskComponent2.m_Description;
                            __instance.m_LabelAfflictionDescription.text = "";
                            string[] remedySprites = new string[1] { "GEAR_BottleHydrogenPeroxide" };
                            bool[] remedyComplete = new bool[1] { !infectionRiskComponent2.RequiresAntiseptic(selectedAfflictionIndex) };
                            int[] remedyNumRequired = new int[1] { 1 };
                            string[] altRemedySprites = new string[1] { "GEAR_OldMansBeardDressing" };
                            bool[] altRemedyComplete = new bool[1] { !infectionRiskComponent2.RequiresAntiseptic(selectedAfflictionIndex) };
                            int[] altRemedyNumRequired = new int[1] { 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)infectionRiskComponent2.GetLocation(selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.SprainedWrist:
                        {
                            SprainedWrist sprainedWristComponent = GameManager.GetSprainedWristComponent();
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = sprainedWristComponent.m_Description;
                            string[] remedySprites = new string[1] { "GEAR_HeavyBandage" };
                            bool[] remedyComplete = new bool[1];
                            int[] remedyNumRequired = new int[1] { 1 };
                            string[] altRemedySprites = null;
                            bool[] altRemedyComplete = null;
                            int[] altRemedyNumRequired = null;
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, sprainedWristComponent.GetRestAmountRemaining(selectedAfflictionIndex), sprainedWristComponent.m_NumHoursRestForCure);
                            num = (int)sprainedWristComponent.GetLocation(selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.Hypothermia:
                        {
                            NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                            string text2 = Localization.Get("GAMEPLAY_HypothermiaTreatment");
                            text2 = text2.Replace("{time-val}", Mathf.RoundToInt(GameManager.GetHypothermiaComponent().GetWarmTimeAmountRemaining()).ToString());
                            __instance.m_LabelSpecialTreatment.text = text2;
                            text2 = GameManager.GetHypothermiaComponent().m_Description;
                            text2 = text2.Replace("{time-val}", Mathf.RoundToInt(GameManager.GetHypothermiaComponent().GetNumHoursWarmForCure()).ToString());
                            __instance.m_LabelSpecialTreatmentDescription.text = text2;
                            __instance.m_SpecialTreatmentWindow.SetActive(true);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.Hypothermia, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.WarmingUp:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_WarmingUpDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        num4 = Mathf.CeilToInt(GameManager.GetPlayerManagerComponent().GetFreezingBuffTimeRemainingHours() * 60f);
                        break;
                    case AfflictionType.ReducedFatigue:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_FatigueReducedDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        num4 = ((!GameManager.GetEmergencyStimComponent().GetEmergencyStimActive()) ? Mathf.CeilToInt(GameManager.GetPlayerManagerComponent().GetFatigueBuffTimeRemainingHours() * 60f) : Mathf.CeilToInt(GameManager.GetEmergencyStimComponent().GetActiveHoursRemaining() * 60f));
                        break;
                    case AfflictionType.ImprovedRest:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_ImprovedRestDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        num4 = Mathf.CeilToInt(GameManager.GetPlayerManagerComponent().GetConditionRestBuffTimeRemainingHours() * 60f);
                        break;
                    case AfflictionType.HypothermiaRisk:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelSpecialTreatment.text = Localization.Get("GAMEPLAY_HypothermiaRiskTreatment");
                        __instance.m_LabelSpecialTreatmentDescription.text = Localization.Get("GAMEPLAY_HypothermiaRiskDesc");
                        __instance.m_SpecialTreatmentWindow.SetActive(true);
                        num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.HypothermiaRisk, selectedAfflictionIndex);
                        break;
                    case AfflictionType.CabinFever:
                        {
                            NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                            string text = Localization.Get("GAMEPLAY_CabinFeverTreatment");
                            text = text.Replace("{time-val}", Mathf.CeilToInt(GameManager.GetCabinFeverComponent().GetTimeAmountRemaining()).ToString());
                            __instance.m_LabelSpecialTreatment.text = text;
                            text = Localization.Get("GAMEPLAY_CabinFeverDesc");
                            text = text.Replace("{time-val}", GameManager.GetCabinFeverComponent().m_NumHoursToPreventIndoorRest.ToString());
                            __instance.m_LabelSpecialTreatmentDescription.text = text;
                            __instance.m_SpecialTreatmentWindow.SetActive(true);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.CabinFever, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.CabinFeverRisk:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelSpecialTreatment.text = Localization.Get("GAMEPLAY_CabinFeverRiskTreatment");
                        __instance.m_LabelSpecialTreatmentDescription.text = Localization.Get("GAMEPLAY_CabinFeverRiskDesc");
                        __instance.m_SpecialTreatmentWindow.SetActive(true);
                        num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.CabinFeverRisk, selectedAfflictionIndex);
                        break;
                    case AfflictionType.IntestinalParasites:
                        {
                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = Localization.Get("GAMEPLAY_IntestinalParasitesDesc");
                            string[] remedySprites = new string[1] { "GEAR_BottleAntibiotics" };
                            bool[] remedyComplete = new bool[1];
                            int[] remedyNumRequired = new int[1] { 2 };
                            string[] altRemedySprites = new string[1] { "GEAR_ReishiTea" };
                            bool[] altRemedyComplete = new bool[1];
                            int[] altRemedyNumRequired = new int[1] { 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.IntestinalParasites, selectedAfflictionIndex);
                            __instance.m_MultipleDosesObject.SetActive(true);
                            __instance.m_LabelDosesRemaining.text = GameManager.GetIntestinalParasitesComponent().GetNumDosesRemaining().ToString();
                            __instance.m_LabelDosesRequired.text = GameManager.GetIntestinalParasitesComponent().GetNumDosesRequired().ToString();
                            if (GameManager.GetIntestinalParasitesComponent().HasTakenDoseToday() && !__instance.m_TreatmentDontHaveItemsLabel.enabled)
                            {
                                __instance.m_UsedAntibioticsAlreadyLabel.enabled = true;
                                __instance.m_TreatmentButtonMultiLeft.SetActive(false);
                                __instance.m_TreatmentButtonMultiRight.SetActive(false);
                                __instance.m_TreatmentWidgetMultiLeft.GetComponent<BoxCollider>().enabled = true;
                                __instance.m_TreatmentWidgetMultiRight.GetComponent<BoxCollider>().enabled = true;
                            }
                            break;
                        }
                    case AfflictionType.IntestinalParasitesRisk:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelSpecialTreatment.text = Localization.Get("GAMEPLAY_IntestinalParasitesRiskTreatment");
                        __instance.m_LabelSpecialTreatmentDescription.text = Localization.Get("GAMEPLAY_IntestinalParasitesRiskDesc");
                        __instance.m_SpecialTreatmentWindow.SetActive(true);
                        num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.IntestinalParasitesRisk, selectedAfflictionIndex);
                        break;
                    case AfflictionType.FrostbiteRisk:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelSpecialTreatment.text = Localization.Get("GAMEPLAY_FrostbiteRiskTreatment");
                        __instance.m_LabelSpecialTreatmentDescription.text = Localization.Get("GAMEPLAY_FrostbiteRiskDesc");
                        __instance.m_SpecialTreatmentWindow.SetActive(true);
                        num = (int)__instance.m_SelectedAffButton.m_AfflictionLocation;
                        break;
                    case AfflictionType.Frostbite:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        num = (int)__instance.m_SelectedAffButton.m_AfflictionLocation;
                        if (GameManager.GetFrostbiteComponent().NumInstancesFrostbiteAtLocation(num) > 1)
                        {
                            UILabel labelAfflictionName2 = __instance.m_LabelAfflictionName;
                            labelAfflictionName2.text = labelAfflictionName2.text + " x" + GameManager.GetFrostbiteComponent().NumInstancesFrostbiteAtLocation(num);
                        }
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_FrostbiteDebuffDesc");
                        __instance.m_BuffWindow.SetActive(true);
                        break;
                    case AfflictionType.WellFed:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_WellFedDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        break;
                    case AfflictionType.ConditionOverTimeBuff:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_ConditionOverTimeBuffDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        num4 = Mathf.CeilToInt(GameManager.GetPlayerManagerComponent().GetConditionPerHourTimeRemainingHours() * 60f);
                        break;
                    case AfflictionType.SprainRisk:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelSpecialTreatment.text = Localization.Get("GAMEPLAY_SprainRiskTreatment");
                        __instance.m_LabelSpecialTreatmentDescription.text = Localization.Get("GAMEPLAY_SprainRiskDesc");
                        __instance.m_SpecialTreatmentWindow.SetActive(true);
                        num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.SprainRisk, selectedAfflictionIndex);
                        break;
                    case AfflictionType.EnergyBoost:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_EnergyBoostDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        num4 = ((!GameManager.GetEmergencyStimComponent().GetEmergencyStimActive()) ? Mathf.CeilToInt(GameManager.GetEnergyBoostComponent().GetEnergyBoostTimeRemainingHours() * 60f) : Mathf.CeilToInt(GameManager.GetEmergencyStimComponent().GetActiveHoursRemaining() * 60f));
                        break;
                    case AfflictionType.Headache:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_HeadacheDesc");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        num4 = Mathf.CeilToInt(GameManager.GetHeadacheComponent().GetActiveHoursRemaining(selectedAfflictionIndex) * 60f);
                        __instance.m_BuffWindow.SetActive(true);
                        break;
                    case AfflictionType.SprainPain: 
                        {

                            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
                            SprainPain sprainPainComponent = GameManager.GetSprainPainComponent();
                            PainHelper ph = new PainHelper();

                            bool HasTakenPainkillers = ac.PainkillersInEffect(selectedAfflictionIndex, true);

                            //if painkillers have been taken, the UI will show that accordingly

                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = ph.GetAfflictionDescription(__instance.m_SelectedAffButton.m_LabelEffect.text);
                            string[] remedySprites = new string[1] { "GEAR_BottlePainKillers" };
                            bool[] remedyComplete = new bool[1] { HasTakenPainkillers };
                            int[] remedyNumRequired = new int[1] { 2 };
                            string[] altRemedySprites = new string[1] { "GEAR_RoseHipTea" };
                            bool[] altRemedyComplete = new bool[1] { HasTakenPainkillers };
                            int[] altRemedyNumRequired = new int[1] { 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, 0f, 0f, 0f);
                            num = (int)sprainPainComponent.GetLocation(selectedAfflictionIndex);
                            num4 = Mathf.CeilToInt(GameManager.GetSprainPainComponent().GetRemainingHours(selectedAfflictionIndex) * 60f);

                            if(num4 >= 9999999)
                            {
                                num4 = 0;
                            }
                            
                            break;
                        }
                    case AfflictionType.Anxiety:
                        {
                            NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                            Anxiety anxietyComponent = GameManager.GetAnxietyComponent();
                            __instance.m_LabelSpecialTreatment.text = anxietyComponent.GetAfflictionTreatment();
                            __instance.m_LabelSpecialTreatmentDescription.text = anxietyComponent.GetAfflictionDescription();
                            __instance.m_SpecialTreatmentWindow.SetActive(true);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.Anxiety, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.Fear:
                        {
                            NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                            Fear fearComponent = GameManager.GetFearComponent();
                            __instance.m_LabelSpecialTreatment.text = fearComponent.GetAfflictionTreatment();
                            __instance.m_LabelSpecialTreatmentDescription.text = fearComponent.GetAfflictionDescription();
                            __instance.m_SpecialTreatmentWindow.SetActive(true);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.Fear, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.ToxicFog:
                        {
                            NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                            ToxicFog toxicFogComponent = GameManager.GetToxicFogComponent();
                            __instance.m_LabelSpecialTreatment.text = toxicFogComponent.GetAfflictionTreatment();
                            __instance.m_LabelSpecialTreatmentDescription.text = toxicFogComponent.GetAfflictionDescription();
                            __instance.m_SpecialTreatmentWindow.SetActive(true);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.ToxicFog, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.SprainProtection:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = Localization.Get("GAMEPLAY_CramponsBenefitDescription");
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        break;
                    case AfflictionType.Suffocating:
                        {
                            NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                            Suffocating suffocatingComponent = GameManager.GetSuffocatingComponent();
                            __instance.m_LabelSpecialTreatment.text = suffocatingComponent.m_SufocatingTreatment.Text();
                            __instance.m_LabelSpecialTreatmentDescription.text = suffocatingComponent.m_SufocatingDescription.Text();
                            __instance.m_SpecialTreatmentWindow.SetActive(true);
                            num = (int)Panel_Affliction.GetAfflictionLocation(AfflictionType.Suffocating, selectedAfflictionIndex);
                            break;
                        }
                    case AfflictionType.DamageProtection:
                        NGUITools.SetActive(__instance.m_RightPageObject, state: false);
                        __instance.m_LabelBuffDescription.text = GameManager.GetDamageProtection().GetAfflictionDescription(selectedAfflictionIndex);
                        num = (int)Panel_Affliction.GetAfflictionLocation(__instance.m_SelectedAffButton.m_AfflictionType, selectedAfflictionIndex);
                        __instance.m_BuffWindow.SetActive(true);
                        break;
                }
                __instance.m_LabelCause.text = string.Format("{0} {1}", Localization.Get("GAMEPLAY_AfflictionCausedBy"), __instance.m_SelectedAffButton.m_LabelCause.text);
                if (num >= 0)
                {
                    __instance.m_BodyIconList[num].width = (int)((float)__instance.m_BodyIconWidthOriginal * 1.5f);
                    __instance.m_BodyIconList[num].height = (int)((float)__instance.m_BodyIconHeightOriginal * 1.5f);
                    __instance.UpdateBodyIconActiveAnimation(num, __instance.m_SelectedAffButton.m_AfflictionType);
                    __instance.UpdateBodyIconColors(__instance.m_SelectedAffButton, isButtonSelected: true, num);
                    __instance.UpdateAllButSelectedBodyIconColors();
                }
                else
                {
                    __instance.m_BodyIconActiveAnimationObj.SetActive(false);
                }
                if (num4 > 0)
                {
                    Il2Cpp.Utils.SetActive(__instance.m_DurationWidgetParentObj, active: true);
                    int num5 = num4 / 60;
                    int num6 = num4 % 60;
                    __instance.m_DurationWidgetHoursLabel.text = num5.ToString();
                    __instance.m_DurationWidgetMinutesLabel.text = num6.ToString();
                }
                else
                {
                    Il2Cpp.Utils.SetActive(__instance.m_DurationWidgetParentObj, active: false);
                }
            }
        }

        [HarmonyPatch(typeof(AfflictionButton), nameof(AfflictionButton.UpdateFillBar))]

        public class PainkillerFillBar
        {
            //to-do
        }

        [HarmonyPatch(typeof(Panel_Affliction), "UpdateSelectedAffliction", new Type[] { typeof(Affliction) })]

        public class FirstAidPanelTextUpdate
        {

            public static void Postfix(Affliction affliction, Panel_Affliction __instance)
            {

                if (affliction.m_AfflictionType != AfflictionType.SprainPain || (affliction.m_AfflictionType == AfflictionType.SprainPain && affliction.m_Cause.ToLowerInvariant().Contains("broken rib"))) return;

                __instance.m_Label.text = GetAfflictionNameBasedOnCause(affliction.m_Cause, affliction.m_Location);
                __instance.m_LabelCause.text = GetAfflictionCauseNameBasedOnCause(affliction.m_Cause, affliction.m_Location);
                __instance.m_LabelLocation.text = LocalizedNameFromAfflictionLocation(affliction.m_Location);
                if (__instance.m_AfflictionButtonColorReferences)
                {
                    Color colorBasedOnAffliction = __instance.m_AfflictionButtonColorReferences.GetColorBasedOnAffliction(affliction.m_AfflictionType, isHovering: true);
                    __instance.m_LabelCause.color = colorBasedOnAffliction;
                    __instance.m_LabelLocation.color = colorBasedOnAffliction;
                }
            }
        }

        [HarmonyPatch(typeof(AfflictionCoverflow), nameof(AfflictionCoverflow.SetAffliction))]

        public class FirstAidPanelSpritesUpdate
        {

            public static void Postfix(ref Affliction affliction, AfflictionCoverflow __instance)
            {
                __instance.m_SpriteEffect.spriteName = affliction.m_AfflictionType == AfflictionType.SprainPain && !affliction.m_Cause.ToLowerInvariant().Contains("broken rib") ? GetIconNameBasedOnCause(affliction.m_Cause) : Affliction.SpriteNameFromAfflictionType(affliction.m_AfflictionType);
            }


        }

        [HarmonyPatch(typeof(Panel_FirstAid), nameof(Panel_FirstAid.Initialize))]

        public class AddBloodDrugLevelLabel : MonoBehaviour
        {

            public static void Postfix(Panel_FirstAid __instance)
            {
                //make calories section smaller
                GameObject statusBars = __instance.gameObject.transform.GetChild(2).gameObject;
                GameObject caloriesSection = statusBars.transform.GetChild(12).gameObject;
                GameObject caloriesBg = caloriesSection.transform.GetChild(3).gameObject;

                Vector3 scale = caloriesBg.transform.localScale;
                scale.x -= 0.2f;
                caloriesBg.transform.localScale = scale;

                GameObject bloodDrugLevelSection = Instantiate(caloriesSection, caloriesSection.transform.parent);
                bloodDrugLevelSection.name = "Blood Drug Level";
                
                /**UISprite pillIcon = bloodDrugLevelSection.transform.GetChild(0).GetComponent<UISprite>();
                pillIcon.mSpriteName = "ico_units_pill";
                pillIcon.gameObject.active = true; **/

                Vector3 pos = caloriesSection.transform.position;
                pos.x -= 0.33f;
                bloodDrugLevelSection.transform.position = pos;

                /**
                GameObject conditionBarSection = __instance.gameObject.transform.GetChild(3).gameObject;
                GameObject conditionBarSpawner = conditionBarSection.transform.GetChild(1).gameObject;

                GenericStatusBarSpawner gsbs = conditionBarSpawner.AddComponent<GenericStatusBarSpawner>();
                gsbs.m_EmptySpriteName = "ico_units_pill";
                gsbs.m_MainSpriteName = "ico_units_pill";
                gsbs.m_StatusBarType = StatusBar.StatusBarType.Condition;
                gsbs.m_Prefab = conditionBarSpawner.transform.GetChild(0).gameObject;

                gsbs.enabled = true;
                gsbs.Awake(); **/
            }

        }

        [HarmonyPatch(typeof(Panel_FirstAid), nameof(Panel_FirstAid.RefreshStatusLabels))]

        public class UpdateBloodDrugLevelOnUI
        {

            public static void Postfix(Panel_FirstAid __instance)
            {
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
                GameObject statusBars = __instance.gameObject.transform.GetChild(2).gameObject;
                GameObject bloodDrugLevel = statusBars.transform.GetChild(13).gameObject;


                if (bloodDrugLevel)
                {
                    bloodDrugLevel.transform.GetChild(1).GetComponent<UILabel>().text = "BLOOD DRUG LEVEL";
                    bloodDrugLevel.transform.GetChild(2).GetComponent<UILabel>().text = ac.GetPainkillerLevelPercent();
                }
            }

        }


        /**
        [HarmonyPatch(typeof(GenericStatusBarSpawner), nameof(GenericStatusBarSpawner.AssignValuesToSpawnedObject))]

        public class BloodDrugLevelBarMove
        {

            public static void Postfix(GenericStatusBarSpawner __instance)
            {

                if(__instance.m_EmptySpriteName == "ico_units_pill")
                {
                    Vector3 position = __instance.m_SpawnedObject.transform.position;
                    position.y -= 0.10f;
                    __instance.m_SpawnedObject.transform.position = position;
                }

            }

        }

        [HarmonyPatch(typeof(StatusBar), nameof(StatusBar.GetFillValuesCondition))]

        public class OverrideConditionFillValue
        {

            public static void Postfix(ref float __result, StatusBar __instance)
            {

                MelonLogger.Msg("Hellooooooooo?");

                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                if (__instance.m_SpriteWhenEmpty.mSpriteName == "ico_units_pill")
                {
                    __result = ac.m_PainkillerLevel;
                }
            }

        } **/

    }
}
