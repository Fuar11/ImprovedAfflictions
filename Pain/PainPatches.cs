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

                        if (ph.HasConcussion()) effects.HeadTraumaPulse(__instance.m_PulseFxIntensity);
                        else if(__instance.m_PulseFxIntensity >= 1f)
                        {
                            effects.IntensePainPulse(__instance.m_PulseFxIntensity);
                        }
                        else
                        {
                            GameManager.GetCameraEffects().SprainPulse(__instance.m_PulseFxIntensity);
                        }

                        __instance.m_PulseFxFrequencySeconds = Random.Range(__instance.m_SecondsSinceLastPulseFx, 30f);
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

                    float val = ph.IsOnPainkillers() ? 100f : 50f;

                    float in_value = GameManager.GetCameraStatusEffects().m_SprainAmountSin * val;
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


                if (cause.ToLowerInvariant() == "console" || cause.ToLowerInvariant().Contains("bite")) //animal bites
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
                        __instance.m_AfflictionDurationHours = Random.Range(24f, 96f);
                        __instance.m_PulseFxIntensity = 0.8f;
                        __instance.m_PulseFxFrequencySeconds = 14f;
                    }
                    else if (location == AfflictionBodyArea.ArmRight || location == AfflictionBodyArea.ArmLeft)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(32f, 96f);
                        __instance.m_PulseFxIntensity = 0.6f;
                        __instance.m_PulseFxFrequencySeconds = 16f;
                    }
                    else if (location == AfflictionBodyArea.LegRight || location == AfflictionBodyArea.LegLeft)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(48f, 96f);
                        __instance.m_PulseFxIntensity = 0.6f;
                        __instance.m_PulseFxFrequencySeconds = 16f;
                    }
                    else if (location == AfflictionBodyArea.FootRight || location == AfflictionBodyArea.FootLeft)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(24f, 96f);
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
                painInstance.m_PulseFxIntensity = __instance.m_PulseFxIntensity;
                painInstance.m_PulseFxFrequencySeconds = __instance.m_PulseFxFrequencySeconds;
                painInstance.m_PulseFxMaxDuration = __instance.m_AfflictionDurationHours;

                string suffix = "";
                if (__instance.m_ActiveInstances.Count > 0)
                {
                   suffix = (__instance.m_ActiveInstances.Count + 1).ToString();
                }
                else
                {
                    suffix = (__instance.m_ActiveInstances.Count).ToString();
                }

                

                string dataToSave = JsonSerializer.Serialize(painInstance);
                sdm.Save(dataToSave, suffix);

                Moment.Moment.ScheduleRelative(Implementation.Instance, new Moment.EventRequest((0, 0, 5), "wareOffPainkiller"));

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

                //schedule painkillers to take effects in 20 minutes
                Moment.Moment.ScheduleRelative(Implementation.Instance, new Moment.EventRequest((0, 0, 20), "takeEffectPainkiller"));

                //schedule painkillers to last for x amount of hours
                Moment.Moment.ScheduleRelative(Implementation.Instance, new Moment.EventRequest((0, 10, 0), "wareOffPainkiller"));

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

                PainSaveDataProxy pain = new PainSaveDataProxy();

                pain.m_PulseFxIntensity = 0f;
                pain.m_PulseFxFrequencySeconds = 0f;
                
                string dataToSave = JsonSerializer.Serialize(pain);

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

        //adds concussion on chance when falling off rope
        [HarmonyPatch(typeof(FallDamage), nameof(FallDamage.ApplyFallDamage))]
        public class ConcussionTrigger
        {

            public static bool Prefix() { return false; }

            public static void Postfix(ref float height, ref float damageOverride, FallDamage __instance)
            {

                PainHelper ph = new PainHelper();

                if (GameManager.GetPlayerManagerComponent().m_God)
                {
                    return;
                }
                float num = Mathf.Abs(height);
                if (num < __instance.m_HeightStartDamage)
                {
                    if (num > 0.5f)
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_NoDamage, Il2CppVoice.Priority.Critical);
                    }
                    return;
                }
                Condition conditionComponent = GameManager.GetConditionComponent();
                float num2 = (num - __instance.m_HeightStartDamage) * (__instance.m_FallFromRope ? __instance.m_DamagePerMeterFromRope : __instance.m_DamagePerMeter);
                if (__instance.m_DieOnNextFall)
                {
                    if (__instance.m_FallFromRope)
                    {
                        num2 = conditionComponent.m_CurrentHP * __instance.m_FatalRopeFallHealthDropPercentage * 0.01f;
                    }
                    else
                    {
                        num2 = float.PositiveInfinity;
                        __instance.m_IgnoreDamageReduction = true;
                    }
                    __instance.m_DieOnNextFall = false;
                }
                else if (__instance.m_FallFromRope)
                {
                    num2 = Mathf.Min(num2, Mathf.Min(conditionComponent.m_CurrentHP * __instance.m_FatalRopeFallHealthDropPercentage * 0.01f, conditionComponent.m_MaxHP * __instance.m_MaxRopeDamagePercentage * 0.01f));
                }
                if (!Il2Cpp.Utils.IsZero(damageOverride))
                {
                    num2 = damageOverride;
                }
                float num3 = Mathf.Clamp01(num2 / (conditionComponent.m_MaxHP * 0.5f));
                if (num3 > 0.25f)
                {
                    GameManager.GetCameraEffects().PainPulse(num3);
                }
                if (!__instance.m_FallFromRope)
                {
                    GameManager.GetFootStepSoundsComponent().LeaveFootPrint(GameManager.GetPlayerTransform().position, isLeft: false, bothFeet: true, num3);
                }
                GameManager.GetConditionComponent().AddHealth(0f - num2, DamageSource.Falling);
                __instance.ResetIgnoreDamageReduction();
                if (num2 > 0f)
                {
                    if (__instance.m_FallFromRope)
                    {
                        StatsManager.IncrementValue(StatID.NumRopeFalls);
                    }
                    else
                    {
                        StatsManager.IncrementValue(StatID.FallCount);
                    }
                }
                if (!GameManager.GetPlayerManagerComponent().PlayerIsDead() && !GameManager.GetConditionComponent().IsConsideredDead())
                {
                    PlayerDamageEvent.SpawnDamageEvent("GAMEPLAY_DamageEventMinorBruising", "GAMEPLAY_Affliction", "ico_injury_minorBruising", InterfaceManager.m_FirstAidRedColor, fadeout: true, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventDisplaySeconds, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventFadeOutSeconds);
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    if (Il2Cpp.Utils.IsZero(damageOverride))
                    {
                        if (__instance.MaybeSprainAnkle())
                        {
                            flag = true;
                        }
                        if (__instance.MaybeSprainWrist())
                        {
                            flag2 = true;
                        }
                        if (__instance.m_FallFromRope)
                        {
                            //maybe add concussion to player when falling from rope
                            ph.MaybeConcuss();

                            if (__instance.MaybeSprainAnkle())
                            {
                                flag = true;
                            }
                            if (__instance.MaybeSprainWrist())
                            {
                                flag2 = true;
                            }
                            if (__instance.MaybeBloodLoss())
                            {
                                flag3 = true;
                            }
                        }   
                    }
                    if (flag)
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_AnkleSprain, Il2CppVoice.Priority.Critical);
                    }
                    else if (flag2)
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_WristSprain, Il2CppVoice.Priority.Critical);
                    }
                    else if (flag3)
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_BloodLoss, Il2CppVoice.Priority.Critical);
                    }
                    else if (num2 > 50f)
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_HeavyDamage, Il2CppVoice.Priority.Critical);
                    }
                    else if (num2 > 20f)
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_MediumDamage, Il2CppVoice.Priority.Critical);
                    }
                    else
                    {
                        GameManager.GetPlayerVoiceComponent().Play(__instance.m_LightDamage, Il2CppVoice.Priority.Critical);
                    }
                }
                if (Il2Cpp.Utils.IsZero(damageOverride))
                {
                    __instance.ApplyClothingDamage(num);
                }
                __instance.m_FallFromRope = false;
            }
        }

        [HarmonyPatch(typeof(PlayerStruggle), nameof(PlayerStruggle.ApplyDamageAfterMooseAttack))]

        public class ConcussionTriggerMoose
        {
            public static void Postfix()
            {
                PainHelper ph = new PainHelper();
                ph.MaybeConcuss();
            }
        }

        [HarmonyPatch(typeof(PlayerStruggle), nameof(PlayerStruggle.ApplyBearDamageAfterStruggleEnds))]

        public class ConcussionTriggerBear
        {
            public static void Postfix()
            {
                PainHelper ph = new PainHelper();
                ph.MaybeConcuss();
            }

        }

    }
}
