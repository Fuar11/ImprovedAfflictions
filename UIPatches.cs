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
using Il2CppTLD.IntBackedUnit;

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
                if (affType != AfflictionType.SprainPain || (affType == AfflictionType.SprainPain && causeStr.ToLowerInvariant().Contains("broken rib"))) return;

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

                if (Affliction.IsRisk(__instance.m_SelectedAffButton.m_AfflictionType) || Affliction.IsBeneficial(__instance.m_SelectedAffButton.m_AfflictionType) || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CheatDeath || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.DiminishedState || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.WeakConstitution || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.WeakJoints || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.UnsettledSleep || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.SourStomach || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.SevereLacerations || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CabinFever || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.BloodLoss || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Burns || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.BurnsElectric || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Insomnia || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.InsomniaRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoning || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoningRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ScurvyRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Scurvy || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ResistInsomniaBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatDebuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityDebuff)
                {
                    return true;
                }

                return false;
            }

            public static void Postfix(Panel_FirstAid __instance)
            {

                if (!__instance.m_SelectedAffButton) return;

                if (Affliction.IsRisk(__instance.m_SelectedAffButton.m_AfflictionType) || Affliction.IsBeneficial(__instance.m_SelectedAffButton.m_AfflictionType) || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CheatDeath || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.DiminishedState || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.WeakConstitution || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.WeakJoints || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.UnsettledSleep || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.SourStomach || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.SevereLacerations || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CabinFever || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.BloodLoss || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Burns || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.BurnsElectric || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Insomnia || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.InsomniaRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoning || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ChemicalPoisoningRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ScurvyRisk || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.Scurvy || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.ResistInsomniaBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatDebuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.FoodStatBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityBuff || __instance.m_SelectedAffButton.m_AfflictionType == AfflictionType.CarryCapacityDebuff)
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

                if (Affliction.IsBeneficial(__instance.m_SelectedAffButton.m_AfflictionType))
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
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), brokenRibComponent.GetRestAmountRemaining(selectedAfflictionIndex), brokenRibComponent.GetNumHoursRestForCure(selectedAfflictionIndex));
                            num = (int)brokenRibComponent.GetLocation(selectedAfflictionIndex);
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
            dysenteryComponent.GetWaterAmountRemaining().m_Units < 10000000,
            dysenteryComponent.HasTakenAntibiotics()
                            };
                            int[] remedyNumRequired = new int[2] { 1, 2 };
                            string[] altRemedySprites = new string[2] { "GEAR_WaterSupplyPotable", "GEAR_ReishiTea" };
                            bool[] altRemedyComplete = new bool[2]
                            {
            dysenteryComponent.GetWaterAmountRemaining().m_Units < 10000000,    
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
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), infectionComponent.GetRestAmountRemaining(selectedAfflictionIndex), infectionComponent.m_NumHoursRestForCure);
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
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), 0f, 0f);
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
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), sprainedAnkleComponent.GetRestAmountRemaining(selectedAfflictionIndex), sprainedAnkleComponent.m_NumHoursRestForCure);
                            num = (int)sprainedAnkleComponent.GetLocation(selectedAfflictionIndex);
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
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), sprainedWristComponent.GetRestAmountRemaining(selectedAfflictionIndex), sprainedWristComponent.m_NumHoursRestForCure);
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
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), 0f, 0f);
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
                            PainHelper ph = new PainHelper();

                            bool HasTakenPainkillers = ac.PainkillersInEffect(selectedAfflictionIndex, true);
                            float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();

                            //if painkillers have been taken, the UI will show that accordingly

                            __instance.m_LabelAfflictionDescriptionNoRest.text = "";
                            __instance.m_LabelAfflictionDescription.text = ph.GetAfflictionDescription(__instance.m_SelectedAffButton.m_LabelEffect.text);
                            string[] remedySprites = new string[1] { "GEAR_BottlePainKillers" };
                            bool[] remedyComplete = new bool[1] { HasTakenPainkillers };
                            int[] remedyNumRequired = new int[1] { 2 };
                            string[] altRemedySprites = new string[1] { "GEAR_RoseHipTea" };
                            bool[] altRemedyComplete = new bool[1] { HasTakenPainkillers };
                            int[] altRemedyNumRequired = new int[1] { 1 };
                            __instance.SetItemsNeeded(remedySprites, remedyComplete, remedyNumRequired, altRemedySprites, altRemedyComplete, altRemedyNumRequired, new ItemLiquidVolume(0), 0f, 0f);
                            num = (int)ac.m_PainInstances[selectedAfflictionIndex].m_Location;
                            num4 = Mathf.CeilToInt((ac.m_PainInstances[selectedAfflictionIndex].m_EndTime - hoursPlayedNotPaused) * 60f);

                            if(num4 >= 9999999)
                            {
                                num4 = 0;
                            }
                            
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

        [HarmonyPatch(typeof(Panel_Affliction), nameof(Panel_Affliction.RequiresPainKiller))]

        public class RequiresPainkillerOverride
        {
            public static void Postfix(ref bool __result)
            {
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                if (ac.m_PainInstances.Count > 0) __result = true;
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
