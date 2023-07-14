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

namespace ImprovedAfflictions.Pain
{
    internal class PainDebuffs
    {

        [HarmonyPatch(typeof(Panel_BreakDown), nameof(Panel_BreakDown.UpdateDurationLabel))]
        public class UpdateBreakdownLabel
        {
            private static void Postfix(Panel_BreakDown __instance)
            {
                PainHelper ph = new PainHelper();
                SaveDataManager sdm = Implementation.sdm;

                string data = sdm.LoadData("painkillers");

                if (data == null) return;

                PainkillerSaveDataProxy? pk = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);


                if (ph.HasPainAtLocation(AfflictionBodyArea.HandLeft) || ph.HasPainAtLocation(AfflictionBodyArea.HandRight))
                {
                    if(!pk.m_RemedyApplied)
                    {
                        __instance.m_DurationHours *= 1.1f;
                    }
                    else
                    {
                        __instance.m_DurationHours *= 1.02f;
                    }
                }
                else if(ph.HasPainAtLocation(AfflictionBodyArea.ArmLeft) || ph.HasPainAtLocation(AfflictionBodyArea.ArmRight))
                {
                    if (__instance.m_BreakDown.name.Contains("Limb") || __instance.m_BreakDown.name.Contains("Crate") || __instance.m_BreakDown.name.Contains("PalletPile") || __instance.m_BreakDown.name.Contains("Plank"))
                    {
                        if (!pk.m_RemedyApplied)
                        {
                            __instance.m_DurationHours *= 1.25f;
                        }
                        else
                        {
                            __instance.m_DurationHours *= 1.05f;
                        }
                    }
                }

            }
        }

        [HarmonyPatch(typeof(Panel_Crafting), nameof(Panel_Crafting.GetModifiedCraftingDuration))]
        public class UpdateCraftingDuration
        {
            private static void Postfix(Panel_Crafting __instance, ref int __result)
            {

                PainHelper ph = new PainHelper();
                SaveDataManager sdm = Implementation.sdm;

                string data = sdm.LoadData("painkillers");

                if (data == null) return;

                PainkillerSaveDataProxy? pk = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);

                if (ph.HasPainAtLocation(AfflictionBodyArea.HandLeft) || ph.HasPainAtLocation(AfflictionBodyArea.HandRight))
                {
                    float multi = pk.m_RemedyApplied ? 1.02f : 1.07f;

                    __result = (int)(__result * multi);
                }
            }
        }

        [HarmonyPatch(typeof(vp_FPSController), nameof(vp_FPSController.GetSlopeMultiplier))]

        public class MovementSpeedModifier
        {
            public static void Postfix(ref float __result)
            {

                PainHelper ph = new PainHelper();
                SaveDataManager sdm = Implementation.sdm;

                string data = sdm.LoadData("painkillers");

                if (data == null) return;

                PainkillerSaveDataProxy? pk = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);

                if (ph.HasPainAtLocation(AfflictionBodyArea.LegLeft) || ph.HasPainAtLocation(AfflictionBodyArea.LegRight))
                {
                    if (GameManager.GetPlayerManagerComponent().PlayerIsSprinting())
                    {
                        float multi1 = pk.m_RemedyApplied ? 0.9f : 0.7f;

                        __result *= multi1;
                    }
                    else if (GameManager.GetPlayerManagerComponent().PlayerIsWalking())
                    {
                        float multi2 = pk.m_RemedyApplied ? 0.9f : 0.8f;

                        __result *= multi2;
                    }
                }

                if (ph.HasPainAtLocation(AfflictionBodyArea.FootLeft) || ph.HasPainAtLocation(AfflictionBodyArea.FootRight))
                {
                    if (GameManager.GetPlayerManagerComponent().PlayerIsSprinting())
                    {
                        float multi1 = pk.m_RemedyApplied ? 0.9f : 0.8f;

                        __result *= multi1;
                    }
                    else if (GameManager.GetPlayerManagerComponent().PlayerIsWalking())
                    {
                        float multi2 = pk.m_RemedyApplied ? 0.9f : 0.8f;

                        __result *= multi2;
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

                float multiUp = 1f;
                float multiDown = 1f;

                if (ph.HasPain())
                {

                    if (ph.HasPainAtLocation(AfflictionBodyArea.Head) || ph.HasPainAtLocation(AfflictionBodyArea.Neck))
                    {
                        multiUp *= 1f;
                        multiDown *= 1f;
                    }
                    if (ph.HasPainAtLocation(AfflictionBodyArea.ArmLeft) || ph.HasPainAtLocation(AfflictionBodyArea.ArmRight))
                    {
                        multiUp *= 0.7f;
                        multiDown *= 0.75f;
                    }
                    if (ph.HasPainAtLocation(AfflictionBodyArea.LegLeft) || ph.HasPainAtLocation(AfflictionBodyArea.LegRight))
                    {
                        multiUp *= 0.8f;
                        multiDown *= 0.85f;
                    }
                    if (ph.HasPainAtLocation(AfflictionBodyArea.HandLeft) || ph.HasPainAtLocation(AfflictionBodyArea.HandRight))
                    {
                        multiUp *= 0.8f;
                        multiDown *= 0.85f;
                    }
                    if (ph.HasPainAtLocation(AfflictionBodyArea.FootLeft) || ph.HasPainAtLocation(AfflictionBodyArea.FootRight))
                    {
                        multiUp *= 0.9f;
                        multiDown *= 0.95f;
                    }
                    if (ph.HasPainAtLocation(AfflictionBodyArea.Chest) || ph.HasPainAtLocation(AfflictionBodyArea.Stomach))
                    {
                        multiUp *= 0.9f;
                        multiDown *= 0.95f;
                    }
                }

                if (ph.HasPain() && ph.IsOnPainkillers())
                {
                    multiUp += 0.1f;
                    multiDown += 0.2f;
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
       
    }
}
