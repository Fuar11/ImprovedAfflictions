using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using HarmonyLib;
using Il2Cpp;
using Il2CppNewtonsoft.Json;
using ImprovedAfflictions.Utils;
using UnityEngine;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MelonLoader;
using ImprovedAfflictions.Pain.Component;
using Il2CppTLD.BigCarry;
using Il2CppTLD.Interactions;

namespace ImprovedAfflictions.Pain
{
    internal class PainDebuffs
    {

        //pain debuffs

        [HarmonyPatch(typeof(Panel_BreakDown), nameof(Panel_BreakDown.UpdateDurationLabel))]
        public class UpdateBreakdownLabel
        {
            private static void Postfix(Panel_BreakDown __instance)
            {
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                AfflictionBodyArea[] hands = { AfflictionBodyArea.HandLeft, AfflictionBodyArea.HandRight };
                AfflictionBodyArea[] upperLimbs = { AfflictionBodyArea.HandLeft, AfflictionBodyArea.HandRight, AfflictionBodyArea.ArmLeft, AfflictionBodyArea.ArmLeft };

                //just hands
                float handsPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands);
                float handsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands, true);
                float handsPainDifference = (handsPainLevel / handsStartingPainLevel) * 100;

                //hands and arms

                float upperLimbsPainLevel = ac.GetTotalPainLevelForPainAtLocations(upperLimbs);
                float upperLimbsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(upperLimbs, true);
                float upperLimbsPainDifference = (upperLimbsPainLevel / upperLimbsStartingPainLevel) * 100;

                if (__instance.m_BreakDown.name.Contains("Limb") || __instance.m_BreakDown.name.Contains("Crate") || __instance.m_BreakDown.name.Contains("PalletPile") || __instance.m_BreakDown.name.Contains("Plank") || __instance.m_BreakDown.name.Contains("Shelf") || __instance.m_BreakDown.name.Contains("Cart") || __instance.m_BreakDown.name.Contains("Ladder") || __instance.m_BreakDown.name.Contains("Chair") || __instance.m_BreakDown.name.Contains("Table"))
                {

                    if (upperLimbsPainDifference > 0)
                    {
                        if (!ac.PainkillersInEffect(upperLimbsPainLevel))
                        {
                            __instance.m_DurationHours *= UtilityFunctions.MapPercentageToVariable(upperLimbsPainDifference, 1f, 1.5f);
                        }
                        else
                        {
                            __instance.m_DurationHours *= UtilityFunctions.MapPercentageToVariable(upperLimbsPainDifference / 2, 1f, 1.5f);
                        }
                    }
                }
                else //anything breaking down not in the list above (usually doesn't involve arm swinging actions)
                {
                    if (handsPainDifference > 0)
                    {
                        if (!ac.PainkillersInEffect(handsPainLevel))
                        {
                            __instance.m_DurationHours *= UtilityFunctions.MapPercentageToVariable(handsPainDifference);
                        }
                        else
                        {
                            __instance.m_DurationHours *= UtilityFunctions.MapPercentageToVariable(handsPainDifference / 2);
                        }
                    }
                }
                __instance.m_DurationLabel.text = Il2Cpp.Utils.GetExpandedDurationString(Mathf.RoundToInt(__instance.m_DurationHours * 60f));

            }
        }

        [HarmonyPatch(typeof(Panel_Crafting), nameof(Panel_Crafting.GetModifiedCraftingDuration))]
        public class UpdateCraftingDuration
        {
            private static void Postfix(Panel_Crafting __instance, ref int __result)
            {

                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                AfflictionBodyArea[] hands = { AfflictionBodyArea.HandLeft, AfflictionBodyArea.HandRight };

                //just hands
                float handsPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands);
                float handsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands, true);
                float handsPainDifference = (handsPainLevel / handsStartingPainLevel) * 100;

                if (handsPainDifference > 0)
                {
                    float multi = ac.PainkillersInEffect(handsPainLevel) ? UtilityFunctions.MapPercentageToVariable(handsPainDifference / 2) : UtilityFunctions.MapPercentageToVariable(handsPainDifference);

                    __result = (int)(__result * multi);
                }
            }
        }

        [HarmonyPatch(typeof(vp_FPSController), nameof(vp_FPSController.GetSlopeMultiplier))]

        public class MovementSpeedModifier
        {
            public static void Postfix(ref float __result)
            {

                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                if (ac.m_PainInstances.Count == 0) return;
                
                SprainedAnkle sprains = new SprainedAnkle();

                //if player has sprained ankle, they are limping which means the speed is already being modified enough
                if (sprains.HasSprainedAnkle()) return;

                AfflictionBodyArea[] feetAndLegs = { AfflictionBodyArea.FootLeft, AfflictionBodyArea.FootRight, AfflictionBodyArea.LegLeft, AfflictionBodyArea.LegRight };

                float lowerLimbsPainLevel = ac.GetTotalPainLevelForPainAtLocations(feetAndLegs);
                float lowerLimbsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(feetAndLegs, true);
                float lowerLimbsPainLevelDifference = (lowerLimbsPainLevel / lowerLimbsStartingPainLevel) * 100;

             
                if (lowerLimbsPainLevelDifference > 0)
                {
                    if (GameManager.GetPlayerManagerComponent().PlayerIsSprinting() || GameManager.GetPlayerManagerComponent().PlayerIsWalking())
                    {
                        float multi1 = ac.PainkillersInEffect(lowerLimbsPainLevel) ? UtilityFunctions.MapPercentageToVariable(lowerLimbsPainLevelDifference / 2, 1f, 1.5f) : UtilityFunctions.MapPercentageToVariable(lowerLimbsPainLevelDifference, 1f, 1.5f);

                        __result /= multi1;
                    }
                }
                else
                {
                    if (ac.IsHighOnPainkillers())
                    {
                        __result /= ac.IsOverdosing() ? 0.8f : 0.9f; 
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RopeClimbPoint), nameof(RopeClimbPoint.PerformInteraction))]

        public class RopeClimbRestriction
        {

            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(RopeClimbPoint __instance)
            {

                PainHelper ph = new PainHelper();

                if (!GameManager.GetEmergencyStimComponent().GetEmergencyStimActive())
                {
                    if (!ph.CanClimbRope())
                    {
                        GameAudioManager.PlayGUIError();
                        HUDMessage.AddMessage("Can't climb rope when injured.");
                        return;
                    }
                    if (GameManager.GetSprainedWristComponent().HasSprainedWrist())
                    {
                        GameAudioManager.PlayGUIError();
                        HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotClimbWithSprainedWrist"));
                        return;
                    }
                    if (GameManager.GetSprainedAnkleComponent().HasSprainedAnkle())
                    {
                        GameAudioManager.PlayGUIError();
                        HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotClimbWithSprainedAnkle"));
                        return;
                    }
                    if (GameManager.GetBrokenRibComponent().HasBrokenRib())
                    {
                        GameAudioManager.PlayGUIError();
                        HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotClimbWithBrokenRib"));
                        return;
                    }
                }
                if (GameManager.GetEncumberComponent().IsEncumbered())
                {
                    GameAudioManager.PlayGUIError();
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotClimbWhenEncumbered"));
                    return;
                }
                if (GameManager.GetPlayerMovementComponent().IsCrouchOrLimpOrWalkForced())
                {
                    return;
                }
                GameManager.GetPlayerClimbRopeComponent().BeginClimbing(__instance.m_Rope);
                return;

            }

        }

        [HarmonyPatch(typeof(PlayerClimbRope), nameof(PlayerClimbRope.SetClimbSpeed))]

        public class ClimbSpeedModifier
        {
            static PainHelper ph = new PainHelper();
            public static void Postfix(PlayerClimbRope __instance)
            {

                if (__instance.m_ClimbingState == ClimbingState.Falling)
                {
                    __instance.m_ClimbSpeed = 0f;
                    return;
                }
                else if(__instance.m_ClimbingState == ClimbingState.Holding)
                {
                    return;
                }

                float multi = GetClimbSpeedMultiplier(__instance);
                    __instance.m_ClimbSpeed *= multi;
            }

            public static float GetClimbSpeedMultiplier(PlayerClimbRope inst)
            {


                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();


                AfflictionBodyArea[] hands = { AfflictionBodyArea.HandLeft, AfflictionBodyArea.HandRight };

                //just hands
                float handsPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands);
                float handsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands, true);
                float handsPainDifference = (handsPainLevel / handsStartingPainLevel) * 100;

                AfflictionBodyArea[] rLimbs = { AfflictionBodyArea.ArmRight, AfflictionBodyArea.HandRight };

                //right arm and hand
                float rLimbsPainLevel = ac.GetTotalPainLevelForPainAtLocations(rLimbs);
                float rLimbsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(rLimbs, true);
                float rLimbsPainDifference = (rLimbsPainLevel / rLimbsStartingPainLevel) * 100;

                AfflictionBodyArea[] lLimbs = { AfflictionBodyArea.ArmLeft, AfflictionBodyArea.HandLeft };

                //left arm and hand
                float lLimbsPainLevel = ac.GetTotalPainLevelForPainAtLocations(lLimbs);
                float lLimbsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(lLimbs, true);
                float lLimbsPainDifference = (lLimbsPainLevel / lLimbsStartingPainLevel) * 100;

                AfflictionBodyArea[] arms = { AfflictionBodyArea.ArmLeft, AfflictionBodyArea.ArmRight };

                //just arms
                float armsPainLevel = ac.GetTotalPainLevelForPainAtLocations(arms);
                float armsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(arms, true);
                float armsPainDifference = (armsPainLevel / armsStartingPainLevel) * 100;

                AfflictionBodyArea[] legs = { AfflictionBodyArea.LegLeft, AfflictionBodyArea.LegRight };

                //just legs
                float legsPainLevel = ac.GetTotalPainLevelForPainAtLocations(legs);
                float legsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(legs, true);
                float legsPainDifference = (legsPainLevel / legsStartingPainLevel) * 100;

                AfflictionBodyArea[] torso = { AfflictionBodyArea.Chest, AfflictionBodyArea.Stomach };

                //just torso
                float torsoPainLevel = ac.GetTotalPainLevelForPainAtLocations(torso);
                float torsoStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(torso, true);
                float torsoPainDifference = (torsoPainLevel / torsoStartingPainLevel) * 100;

                float multiUp = 1f;
                float multiDown = 1f;

                if (ac.GetTotalPainLevel() > 0)
                {

                    if (armsPainDifference > 30)
                    {
                        multiUp /= ac.PainkillersInEffect(armsPainLevel) ? UtilityFunctions.MapPercentageToVariable(armsPainDifference / 2, 1.0f, 1.3f) : UtilityFunctions.MapPercentageToVariable(armsPainDifference, 1.0f, 1.3f);
                        multiDown /= ac.PainkillersInEffect(armsPainLevel) ? UtilityFunctions.MapPercentageToVariable(armsPainDifference / 2, 1.0f, 1.25f) : UtilityFunctions.MapPercentageToVariable(armsPainDifference, 1.0f, 1.25f); 
                    }
                    if (handsPainDifference > 30)
                    {
                        multiUp /= ac.PainkillersInEffect(handsPainLevel) ? UtilityFunctions.MapPercentageToVariable(handsPainDifference / 2, 1.0f, 1.3f) : UtilityFunctions.MapPercentageToVariable(handsPainDifference, 1.0f, 1.3f);
                        multiDown /= ac.PainkillersInEffect(handsPainLevel) ? UtilityFunctions.MapPercentageToVariable(handsPainDifference / 2, 1.0f, 1.25f) : UtilityFunctions.MapPercentageToVariable(handsPainDifference, 1.0f, 1.25f);
                    }
                    if (rLimbsPainDifference > 30)
                    {
                        multiUp /= ac.PainkillersInEffect(rLimbsPainLevel) ? UtilityFunctions.MapPercentageToVariable(rLimbsPainDifference / 2, 1.0f, 1.2f) : UtilityFunctions.MapPercentageToVariable(rLimbsPainDifference, 1.0f, 1.2f);
                        multiDown /= ac.PainkillersInEffect(rLimbsPainLevel) ? UtilityFunctions.MapPercentageToVariable(rLimbsPainDifference / 2, 1.0f, 1.15f) : UtilityFunctions.MapPercentageToVariable(rLimbsPainDifference, 1.0f, 1.15f);
                    }
                    if (lLimbsPainDifference > 30)
                    {
                        multiUp /= ac.PainkillersInEffect(lLimbsPainLevel) ? UtilityFunctions.MapPercentageToVariable(lLimbsPainDifference / 2, 1.0f, 1.2f) : UtilityFunctions.MapPercentageToVariable(lLimbsPainDifference, 1.0f, 1.2f);
                        multiDown /= ac.PainkillersInEffect(lLimbsPainLevel) ? UtilityFunctions.MapPercentageToVariable(lLimbsPainDifference / 2, 1.0f, 1.15f) : UtilityFunctions.MapPercentageToVariable(lLimbsPainDifference, 1.0f, 1.15f);
                    }
                    if (legsPainDifference > 30)
                    {
                        multiUp /= ac.PainkillersInEffect(legsPainLevel) ? UtilityFunctions.MapPercentageToVariable(legsPainDifference / 2, 1.0f, 1.1f) : UtilityFunctions.MapPercentageToVariable(legsPainDifference, 1.0f, 1.1f);
                        multiDown /= ac.PainkillersInEffect(legsPainLevel) ? UtilityFunctions.MapPercentageToVariable(legsPainDifference / 2, 1.0f, 1.05f) : UtilityFunctions.MapPercentageToVariable(legsPainDifference, 1.0f, 1.05f);
                    }
                    if (torsoPainDifference > 30)
                    {
                        multiUp /= ac.PainkillersInEffect(torsoPainLevel) ? UtilityFunctions.MapPercentageToVariable(torsoPainDifference / 2, 1.0f, 1.1f) : UtilityFunctions.MapPercentageToVariable(torsoPainDifference, 1.0f, 1.1f);
                        multiDown /= ac.PainkillersInEffect(torsoPainLevel) ? UtilityFunctions.MapPercentageToVariable(torsoPainDifference / 2, 1.0f, 1.05f) : UtilityFunctions.MapPercentageToVariable(torsoPainDifference, 1.0f, 1.05f);
                    }
                }
                
       
                if (inst.m_ClimbingState == ClimbingState.Up)
                {
                    return multiUp;
                }
                else if (inst.m_ClimbingState == ClimbingState.Down)
                {
                    return multiDown;
                }

                return 1f;
            }
        }

        [HarmonyPatch(typeof(SimpleInteraction), nameof(SimpleInteraction.PerformInteraction))]

        public class RockClimbRestriction
        {

            public static bool Prefix(SimpleInteraction __instance)
            {
                if(__instance.gameObject.name == "INTERACT_CLIMB_ENABLED" || __instance.gameObject.name == "INTERACT_CLIMBDOWN_ENABLED")
                {
                    if (!PainHelper.CanClimbRocks())
                    {
                        GameAudioManager.PlayGUIError();
                        HUDMessage.AddMessage("Can't climb while injured.");
                        return false;
                    }
                    else return true;
                }
                else return true;
            }

        }

        /**

        [HarmonyPatch(typeof(TravoisBigCarryItem), nameof(TravoisBigCarryItem.OnPrepareForCarry))]
        public class TravoisCarryRestriction
        {
            public static bool Prefix()
            {

                MelonLogger.Msg("Travois carry");

                bool flag = PainHelper.CanCarryTravois();

                if (!flag)
                {
                    GameAudioManager.PlayGUIError();
                    HUDMessage.AddMessage("Can't carry travois when injured.");
                    GameManager.GetPlayerManagerComponent().gameObject.GetComponent<BigCarrySystem>().MaybeDropImmediate();
                    return false;
                }
                else return true;
                

            }

        }

        [HarmonyPatch(typeof(BigCarrySystem), nameof(BigCarrySystem.PrepareCarry))]

        public class testingc
        {

            public static void Prefix()
            {
                MelonLogger.Msg("Prepare carry");
            }

        }

        [HarmonyPatch(typeof(BigCarrySystem), nameof(BigCarrySystem.BeginCarry))]

        public class testingb
        {

            public static void Prefix()
            {
                MelonLogger.Msg("Begin carry");
            }

        }

        [HarmonyPatch(typeof(BigCarrySystem), nameof(BigCarrySystem.SetupCarry))]

        public class testing
        {

            public static void Prefix()
            {
                MelonLogger.Msg("Setup carry");
            }

        } **/


        //reading

        [HarmonyPatch(typeof(Panel_Inventory_Examine), nameof(Panel_Inventory_Examine.MaybeAbortReadingWithHUDMessage))]

        public class ReadingChanges
        {

            // true result means abort reading. False means you're allowed to read
            static bool Prefix(ref bool __result)
            {
                __result = ShouldPreventReading();
                return false; // Don't execute original method afterwards
            }

            private static bool ShouldPreventReading()
            {

                PainHelper ph = new PainHelper();

                if (GameManager.GetWeatherComponent().IsTooDarkForAction(ActionsToBlock.Reading))
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooDarkToRead"), false);
                    return true;
                }
                if (GameManager.GetFatigueComponent().IsExhausted())
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooTiredToRead"), false);
                    return true;
                }
                if (GameManager.GetFreezingComponent().IsFreezing())
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooColdToRead"), false);
                    return true;
                }
                if (GameManager.GetHungerComponent().IsStarving())
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooHungryToRead"), false);
                    return true;
                }
                if (GameManager.GetThirstComponent().IsDehydrated())
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooThirstyToRead"), false);
                    return true;
                }
                if (GameManager.GetConditionComponent().GetNormalizedCondition() < 0.1f)
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooWoundedToRead"), false);
                    return true;
                }
                if (GameManager.GetConditionComponent().HasNonRiskAffliction())
                {
                    if(ph.HasConcussionInEffect())
                    {
                        HUDMessage.AddMessage("Can't focus on reading with a concussion", false);
                        return true;
                    }
                    return false;
                }
                return false;
            }

        }

        //painkiller debuffs
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.PlayerCanSprint))]

        public class OverdoseSprintRestriction
        {

            public static void Postfix(ref bool __result)
            {

                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                if (ac.IsOverdosing() && !GameManager.GetEmergencyStimComponent().GetEmergencyStimActive())
                {
                    __result = false;
                }
            }

        }
    }
}
