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


namespace ImprovedAfflictions.Pain
{
    internal class PainPatches
    {

        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.Update))]

        public class UpdateOverride
        {

            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(SprainPain __instance)
            {
                PainHelper ph = new PainHelper();
                PainEffects effects = new PainEffects();

                if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
                {
                    return;
                }
                
                float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                for (int num = __instance.m_ActiveInstances.Count - 1; num >= 0; num--)
                {
                    SprainPain.Instance inst = __instance.m_ActiveInstances[num];
                    if (hoursPlayedNotPaused > inst.m_EndTime)
                    {
                        __instance.CureAffliction(inst);
                    }
                }
                if (__instance.HasSprainPain())
                {
                    __instance.m_SecondsSinceLastPulseFx += Time.deltaTime;
                    if (__instance.m_SecondsSinceLastPulseFx > __instance.m_PulseFxFrequencySeconds)
                    {

                        MelonLogger.Msg("Active intensity level: {0}", __instance.m_PulseFxIntensity);

                        if (ph.HasConcussion()) effects.HeadTraumaPulse(2f);
                        else if(__instance.m_PulseFxIntensity >= 1f)
                        {
                            effects.IntensePainPulse(__instance.m_PulseFxIntensity);
                        }
                        else
                        {
                            GameManager.GetCameraEffects().SprainPulse(__instance.m_PulseFxIntensity);
                        }

                        __instance.m_SecondsSinceLastPulseFx = 0f;
                    }
                }
                else
                {
                    GameManager.GetCameraEffects().SprainPulse(0f);
                    __instance.m_SecondsSinceLastPulseFx = 0f;
                }
                if (!string.IsNullOrEmpty(__instance.m_PulseFxWwiseRtpcName))
                {
                    float in_value = GameManager.GetCameraStatusEffects().m_SprainAmountSin * 100f;
                    GameObject soundEmitterFromGameObject = GameAudioManager.GetSoundEmitterFromGameObject(GameManager.GetPlayerObject());
                    AkSoundEngine.SetRTPCValue(__instance.m_PulseFxWwiseRtpcName, in_value, soundEmitterFromGameObject);
                }

            }

        }


        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.ApplyAffliction))]

        public class PainModifier
        {
            public static void Prefix(ref AfflictionBodyArea location, ref string cause, SprainPain __instance)
            {


                if (cause.ToLowerInvariant() == "console" || cause.ToLowerInvariant().Contains("bite"))
                {
                    if (location == AfflictionBodyArea.Head)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(96f, 124f);
                        __instance.m_PulseFxIntensity = 1.1f;
                        __instance.m_PulseFxFrequencySeconds = 8f;
                    }
                    else if (location == AfflictionBodyArea.Neck)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 1f;
                        __instance.m_PulseFxFrequencySeconds = 9.5f;
                    }
                    else if (location == AfflictionBodyArea.Chest)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.9f;
                        __instance.m_PulseFxFrequencySeconds = 10f;
                    }
                    else if (location == AfflictionBodyArea.HandRight || location == AfflictionBodyArea.HandLeft)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.8f;
                        __instance.m_PulseFxFrequencySeconds = 14f;
                    }
                    else if (location == AfflictionBodyArea.LegRight || location == AfflictionBodyArea.LegLeft)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.6f;
                        __instance.m_PulseFxFrequencySeconds = 16f;
                    }
                    else if (location == AfflictionBodyArea.FootRight || location == AfflictionBodyArea.FootLeft)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.85f;
                        __instance.m_PulseFxFrequencySeconds = 12f;
                    }
                    else
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 72f);
                        __instance.m_PulseFxIntensity = 0.5f;
                        __instance.m_PulseFxFrequencySeconds = 18f;
                    }
                }
                else if(cause.ToLowerInvariant() == "fall") //sprains
                {
                    __instance.m_AfflictionDurationHours = Random.Range(96f, 124f);
                    __instance.m_PulseFxIntensity = 0.65f;
                    __instance.m_PulseFxFrequencySeconds = 15f;
                }
                else if (cause.ToLowerInvariant() == "concussion") //head trauma or concussion
                {
                    __instance.m_AfflictionDurationHours = Random.Range(96f, 240f);
                    __instance.m_PulseFxIntensity = 2f;
                    __instance.m_PulseFxFrequencySeconds = 6f;
                }

                SaveDataManager sdm = Implementation.sdm;

                PainSaveDataProxy painInstance = new PainSaveDataProxy();
                painInstance.m_RemedyApplied = false;
                painInstance.m_PulseFxIntensity = __instance.m_PulseFxIntensity;
                painInstance.m_PulseFxFrequencySeconds = __instance.m_PulseFxFrequencySeconds;

                string suffix = (__instance.m_ActiveInstances.Count + 1).ToString();

                string dataToSave = JsonSerializer.Serialize(painInstance);
                sdm.Save(dataToSave, suffix);

                //update pain effects when new pain is afflicted
                PainHelper ph = new PainHelper();
                ph.UpdatePainEffects();
            }

        }


        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.TakePainKillers))]

        public class PainKillerModifier
        {
            public static bool Prefix()
            {
                return false;
            }
            public static void Postfix(SprainPain __instance, ref int index)
            {
                float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();

                if (__instance.GetRemainingHours(index) <= 12f) //if there's 12 hours or less left on the pain area, painkiller will instantly relieve it
                {
                    var value = __instance.m_ActiveInstances[index];
                    float num = (value.m_EndTime - hoursPlayedNotPaused) * (100f - __instance.m_TreatmentPercent) * 0.01f;
                    value.m_EndTime = hoursPlayedNotPaused + num;
                    __instance.m_ActiveInstances[index] = value;
                    return;
                }

                SaveDataManager sdm = Implementation.sdm;

                PainSaveDataProxy painInstance = new PainSaveDataProxy();
                painInstance.m_RemedyApplied = true;
                painInstance.m_PulseFxIntensity = __instance.m_PulseFxIntensity;
                painInstance.m_PulseFxFrequencySeconds = __instance.m_PulseFxFrequencySeconds;

                string dataToSave = JsonSerializer.Serialize(painInstance);
                sdm.Save(dataToSave, index.ToString());

                //update pain effects when painkillers are taken
                PainHelper ph = new PainHelper();
                ph.UpdatePainEffects();

                //schedule painkillers to last for x amount of hours
                Moment.Moment.ScheduleRelative(Implementation.Instance, new Moment.EventRequest((0, 10, 0), "wareOffPainkiller", index.ToString()));

            }
        }

        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.CureAffliction))]

        public class RemovePainData
        {
            public static bool Prefix()
            {
                return false;
            }
            public static void Postfix(SprainPain __instance, ref SprainPain.Instance inst)
            {

                SaveDataManager sdm = Implementation.sdm;

                string suffix = __instance.m_ActiveInstances.IndexOf(inst).ToString();

                PainSaveDataProxy painInstance = new PainSaveDataProxy();
                painInstance.m_RemedyApplied = false;
                painInstance.m_PulseFxIntensity = __instance.m_PulseFxIntensity;
                painInstance.m_PulseFxFrequencySeconds = __instance.m_PulseFxFrequencySeconds;

                string dataToSave = JsonSerializer.Serialize(painInstance);

                //overwrites the pain instance about to be removed with a blank slate
                sdm.Save(dataToSave, suffix);

                __instance.m_ActiveInstances.Remove(inst);
                PlayerDamageEvent.SpawnAfflictionEvent("GAMEPLAY_SprainPain", "GAMEPLAY_Healed", "ico_injury_pain", InterfaceManager.m_FirstAidBuffColor);
                InterfaceManager.GetPanel<Panel_FirstAid>().UpdateDueToAfflictionHealed();

                PainHelper ph = new PainHelper();
                ph.UpdatePainEffects();
            }

        }

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
            
                GameManager.GetSprainPainComponent().ApplyAffliction(__instance.GetLocationOfLastAdded(), cause, options);
               
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

       

    }
}
