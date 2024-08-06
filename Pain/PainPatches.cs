using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Random = UnityEngine.Random;
using System.Text.Json;
using ImprovedAfflictions.Utils;
using ImprovedAfflictions.Component;


namespace ImprovedAfflictions.Pain
{
    internal class PainPatches
    {


        /**
        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.ApplyAffliction))]

        public class PainOverride
        {
            public static bool Prefix(ref AfflictionBodyArea location, ref string cause, ref AfflictionOptions opts, SprainPain __instance)
            {

                opts = AfflictionOptions.None;

                PainHelper ph = new PainHelper();
                PainManager ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<PainManager>();
                int index = 0;

                PainAffliction existingInstance = null;

                if (cause == "concussion") existingInstance = ph.GetConcussion();
                else if (cause == "broken rib") existingInstance = ph.GetBrokenRibPain(ref index);
                else existingInstance = ph.GetPainInstance(location, cause, ref index);

                if (existingInstance != null)
                {
                    ac.UpdatePainInstance(index, existingInstance);
                    ph.UpdatePainEffects();
                    return false;
                }

                float painLevel = 0;
                float maxDuration = 0;

                if (cause.ToLowerInvariant() == "console" || cause.ToLowerInvariant().Contains("bite")) //animal bites
                {
                    painLevel = 15f;
                    maxDuration = 96f;

                    if (location == AfflictionBodyArea.Head)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(96f, 124f);
                        maxDuration = 124f;
                        __instance.m_PulseFxIntensity = 1.1f;
                        __instance.m_PulseFxFrequencySeconds = 8f;
                        painLevel = 21f;
                    }
                    else if (location == AfflictionBodyArea.Neck)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 1f;
                        __instance.m_PulseFxFrequencySeconds = 9.5f;
                        painLevel = 20f;
                    }
                    else if (location == AfflictionBodyArea.Chest)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.9f;
                        __instance.m_PulseFxFrequencySeconds = 10f;
                    }
                    else if (location == AfflictionBodyArea.HandRight || location == AfflictionBodyArea.HandLeft)
                    {
                        AfflictionBodyArea locationToCheck = location == AfflictionBodyArea.HandRight ? AfflictionBodyArea.ArmRight : AfflictionBodyArea.ArmLeft;
                        if (ac.GetPainInstanceAtLocationWithCause(locationToCheck, cause) != null) painLevel -= 5;

                        __instance.m_AfflictionDurationHours = Random.Range(24f, 96f);
                        __instance.m_PulseFxIntensity = 0.8f;
                        __instance.m_PulseFxFrequencySeconds = 14f;
                    }
                    else if (location == AfflictionBodyArea.ArmRight || location == AfflictionBodyArea.ArmLeft)
                    {

                        AfflictionBodyArea locationToCheck = location == AfflictionBodyArea.ArmRight ? AfflictionBodyArea.HandRight : AfflictionBodyArea.HandLeft;
                        if (ac.GetPainInstanceAtLocationWithCause(locationToCheck, cause) != null) painLevel -= 5;

                        __instance.m_AfflictionDurationHours = Random.Range(24f, 96f);
                        __instance.m_PulseFxIntensity = 0.6f;
                        __instance.m_PulseFxFrequencySeconds = 16f;
                    }
                    else if (location == AfflictionBodyArea.LegRight || location == AfflictionBodyArea.LegLeft)
                    {

                        AfflictionBodyArea locationToCheck = location == AfflictionBodyArea.LegRight ? AfflictionBodyArea.FootRight : AfflictionBodyArea.FootLeft;
                        if (ac.GetPainInstanceAtLocationWithCause(locationToCheck, cause) != null) painLevel -= 5;

                        __instance.m_AfflictionDurationHours = Random.Range(32f, 96f);
                        __instance.m_PulseFxIntensity = 0.6f;
                        __instance.m_PulseFxFrequencySeconds = 16f;
                    }
                    else if (location == AfflictionBodyArea.FootRight || location == AfflictionBodyArea.FootLeft)
                    {

                        AfflictionBodyArea locationToCheck = location == AfflictionBodyArea.FootRight ? AfflictionBodyArea.LegRight : AfflictionBodyArea.LegLeft;
                        if (ac.GetPainInstanceAtLocationWithCause(locationToCheck, cause) != null) painLevel -= 5;

                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.85f;
                        __instance.m_PulseFxFrequencySeconds = 12f;
                    }
                    else
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 72f);
                        maxDuration = 72f;
                        __instance.m_PulseFxIntensity = 0.5f;
                        __instance.m_PulseFxFrequencySeconds = 18f;
                        painLevel = 5f;
                    }
                }
                else if (cause.ToLowerInvariant().Contains("fall")) //sprains
                {
                    __instance.m_AfflictionDurationHours = Random.Range(48f, 72f);
                    maxDuration = 72f;
                    __instance.m_PulseFxIntensity = 0.65f;
                    __instance.m_PulseFxFrequencySeconds = 15f;
                    painLevel = 10f;
                }
                else if (cause.ToLowerInvariant() == "concussion") //head trauma or concussion
                {
                    __instance.m_AfflictionDurationHours = Random.Range(96f, 240f);
                    maxDuration = 240f;
                    __instance.m_PulseFxIntensity = 2f;
                    __instance.m_PulseFxFrequencySeconds = 6f;
                    painLevel = 40f;
                }
                else if (cause.ToLowerInvariant() == "broken rib") //broken ribs
                {
                    __instance.m_AfflictionDurationHours = 999999999f;
                    __instance.m_PulseFxIntensity = 1.7f;
                    __instance.m_PulseFxFrequencySeconds = 8f;
                    painLevel = 35f;
                }
                else if (cause.ToLowerInvariant() == "corrosive chemical burns") //chemical burns
                {
                    painLevel = 25f;
                    if (location == AfflictionBodyArea.FootLeft || location == AfflictionBodyArea.FootRight)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(96f, 240f);
                        maxDuration = 240f;
                    }
                    else
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(72f, 120f);
                        maxDuration = 120f;
                    }

                    __instance.m_PulseFxIntensity = 0.9f;
                    __instance.m_PulseFxFrequencySeconds = 10f;
                }
                

                ac.AddPainInstance(cause, location, __instance.m_AfflictionDurationHours, maxDuration, painLevel , __instance.m_PulseFxIntensity, __instance.m_PulseFxFrequencySeconds);



                if (ac.m_PainInstances.Count == 1)
                {
                    ac.m_PainManager.m_SecondsSinceLastPulseFx = ac.m_PainManager.m_PulseFxFrequencySeconds - ac.m_PainManager.m_PulseFxStartDelaySeconds;
                    MelonLogger.Msg("Seconds since last pulse fx on add for the first time: {0}", ac.m_PainManager.m_SecondsSinceLastPulseFx);
                }

                if (cause.ToLowerInvariant() != "fall" || cause.ToLowerInvariant() != "broken rib" || cause.ToLowerInvariant() != "console")
                {
                    PlayerDamageEvent.SpawnDamageEvent(UIPatches.GetAfflictionNameBasedOnCause(cause, location), "GAMEPLAY_Affliction", UIPatches.GetIconNameBasedOnCause(cause), InterfaceManager.m_FirstAidRedColor, fadeout: true, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventDisplaySeconds, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventFadeOutSeconds);
                }

                //update pain effects when new pain is afflicted
                ph.UpdatePainEffects();
                return false;
            }

        } **/


        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.TreatAfflictionWithFirstAid))]

        public class PainkillerModifier
        {
            public static void Postfix(PlayerManager __instance)
            {
                PainManager pm = Mod.painManager;

                if (__instance.m_FirstAidItemUsed.name.ToLowerInvariant().Contains("painkiller"))
                {
                    float amount = __instance.m_FirstAidItemUsed.m_GearItem.m_CurrentHP < 45 ? 20f * ((__instance.m_FirstAidItemUsed.m_GearItem.m_CurrentHP + 20) / 100) : 20f;

                    pm.AdministerPainkillers(amount);
                }

            }

        }

        [HarmonyPatch(typeof(EmergencyStim), nameof(EmergencyStim.ApplyEmergencyStim))]

        public class AdministerStimulants
        {
            public static void Postfix()
            {
                Mod.painManager.AdministerPainkillers(50f, true);
            }
        }
    }
}
