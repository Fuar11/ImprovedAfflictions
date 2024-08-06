using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using UnityEngine;
using Random = UnityEngine.Random;
using ImprovedAfflictions.CustomAfflictions;

namespace ImprovedAfflictions.Bites
{
    internal class Bite
    {

        //adds pain whenever blood loss is contracted
        [HarmonyPatch(typeof(BloodLoss), nameof(BloodLoss.BloodLossStart))]

        public class BloodLossPain
        {

            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(ref string cause, ref bool displayIcon, ref AfflictionOptions options, BloodLoss __instance)
            {

                if (GameManager.GetPlayerManagerComponent().PlayerIsDead() || InterfaceManager.IsPanelEnabled<Panel_ChallengeComplete>())
                {
                    return;
                }

                Il2CppSystem.Collections.Generic.List<AfflictionBodyArea> bodyAreasToPreventBloodLoss = GameManager.GetDamageProtection().GetBodyAreasToPreventBloodLoss();
                StatsManager.IncrementValue(StatID.BloodLoss);
                if (__instance.m_ShouldOverrideArea)
                {
                    if (bodyAreasToPreventBloodLoss.Contains(__instance.m_OverrideArea))
                    {
                        return;
                    }
                    __instance.m_ShouldOverrideArea = false;
                    if (__instance.m_Locations.Contains((int)__instance.m_OverrideArea))
                    {
                        return;
                    }
                    __instance.m_Locations.Add((int)__instance.m_OverrideArea);
                }
                else
                {
                    List<AfflictionBodyArea> list = new List<AfflictionBodyArea>((AfflictionBodyArea[])Enum.GetValues(typeof(AfflictionBodyArea)));
                    foreach (AfflictionBodyArea item in bodyAreasToPreventBloodLoss)
                    {
                        list.Remove(item);
                    }
                    for (int i = 0; i < __instance.m_Locations.Count; i++)
                    {
                        list.Remove((AfflictionBodyArea)__instance.m_Locations[i]);
                    }
                    if (list.Count <= 0)
                    {
                        return;
                    }
                    __instance.m_Locations.Add((int)list[Random.Range(0, list.Count)]);
                }
                __instance.m_CausesLocIDs.Add(cause);
                __instance.m_ElapsedHoursList.Add(0f);
                __instance.m_DurationHoursList.Add(Random.Range(__instance.m_DurationHoursMin, __instance.m_DurationHoursMax));
                if (displayIcon && Condition.ShouldPlayFx(options))
                {
                    PlayerDamageEvent.SpawnDamageEvent(__instance.m_LocalizedDisplayName.m_LocalizationID, "GAMEPLAY_Affliction", "ico_injury_bloodLoss", InterfaceManager.m_FirstAidRedColor, fadeout: true, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventDisplaySeconds, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventFadeOutSeconds);
                }
                GameManager.GetLogComponent().AddAffliction(AfflictionType.BloodLoss, cause);
                AfflictionBodyArea location = __instance.GetLocationOfLastAdded();
                string desc;
                switch (cause.ToLowerInvariant())
                {
                    case "wolf":
                        desc = "You are suffering from a wolf bite. Take painkillers to numb the pain and wait for the wound to heal.";

                        new CustomPainAffliction("Wolf Bite", "Wolf Attack", desc, "", location, "ico_injury_laceration", false, false, GetDurationByLocation(location), false, false, [Tuple.Create("GEAR_BottlePainkillers", 2, 1)], [], GetPainLevelByLocation(location));
                        break;
                    case "bear":
                        desc = "You are suffering from a bear bite. Take painkillers to numb the pain and wait for the wound to heal.";

                        new CustomPainAffliction("Bear Bite", "Bear Attack", desc, "", location, "ico_injury_laceration", false, false, GetDurationByLocation(location), false, false, [Tuple.Create("GEAR_BottlePainkillers", 2, 1)], [], GetPainLevelByLocation(location));
                        break;
                }

                if (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.ChallengeHunted)
                {
                    if (Time.timeSinceLevelLoad > 120f && Condition.ShouldDoAutoSave(options))
                    {
                        GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
                    }
                }
                else if (Condition.ShouldDoAutoSave(options))
                {
                    GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
                }
                if (Condition.ShouldPlayFx(options))
                {
                    string eventName = __instance.m_SoundToPlayAboveThreshold;
                    if (GameManager.GetConditionComponent().m_CurrentHP < __instance.m_HPThresholdForSound)
                    {
                        eventName = __instance.m_SoundToPlayBelowThreshold;
                    }
                    GameManager.GetPlayerVoiceComponent().Play(eventName, Il2CppVoice.Priority.Critical);
                }
            }


        }

        public static float GetPainLevelByLocation(AfflictionBodyArea location)
        {
            switch (location)
            {
                case AfflictionBodyArea.Head: return 21;
                case AfflictionBodyArea.Neck: return 20;
                case AfflictionBodyArea.Chest: return 18;
                default: return 15;
            }

        }

        public static float GetDurationByLocation(AfflictionBodyArea location)
        {

            if (location == AfflictionBodyArea.Head) return Random.Range(96f, 124f);
            else if (location == AfflictionBodyArea.Neck || location == AfflictionBodyArea.Chest) return Random.Range(48f, 96f);
            else if (location == AfflictionBodyArea.HandRight || location == AfflictionBodyArea.HandLeft || location == AfflictionBodyArea.ArmLeft || location == AfflictionBodyArea.ArmRight) return Random.Range(24f, 96f);
            else if (location == AfflictionBodyArea.LegRight || location == AfflictionBodyArea.LegLeft) return Random.Range(32f, 96f);
            else if (location == AfflictionBodyArea.FootRight || location == AfflictionBodyArea.FootLeft) return Random.Range(48f, 96f);
            else return Random.Range(48f, 72f);

        }

    }
}
