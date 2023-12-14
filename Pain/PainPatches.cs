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
using ImprovedAfflictions.Pain.Component;
using ImprovedAfflictions.Component;
using UnityEngine.Analytics;
using Il2CppRewired.Utils.Platforms.Windows;

namespace ImprovedAfflictions.Pain
{
    internal class PainPatches
    {

        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.Update))]

        public class UpdateOverride
        {

            public static bool Prefix(SprainPain __instance)
            {
                return false;
            }

            public static void Postfix(SprainPain __instance)
            {

                if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended || GameManager.m_ActiveScene.ToLowerInvariant().Contains("menu") || GameManager.m_ActiveScene.ToLowerInvariant().Contains("boot") || GameManager.m_ActiveScene.ToLowerInvariant().Contains("empty"))
                {
                    return;
                }

                PainHelper ph = new PainHelper();
                PainEffects effects = new PainEffects();
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();


                float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                for (int num = __instance.m_ActiveInstances.Count - 1; num >= 0; num--)
                {
                    SprainPain.Instance inst = __instance.m_ActiveInstances[num];
                    if (hoursPlayedNotPaused > inst.m_EndTime)
                    {
                        ac.CurePainInstance(num);
                        __instance.CureAffliction(inst);
                    }
                }

                //overall pain level is not less than 20 percent of the most recent highest pain level
                if ((ac.m_PainLevel / ac.m_PainStartingLevel) * 100 > 20)
                {
                    __instance.m_SecondsSinceLastPulseFx += Time.deltaTime;
                    if (__instance.m_SecondsSinceLastPulseFx > __instance.m_PulseFxFrequencySeconds)
                    {

                        if (ph.HasConcussion())
                        {
                            effects.HeadTraumaPulse(__instance.m_PulseFxIntensity);
                        }
                        else if (__instance.m_PulseFxIntensity > 1f)
                        {
                            effects.IntensePainPulse(__instance.m_PulseFxIntensity);
                        }
                        else
                        {
                            GameManager.GetCameraEffects().SprainPulse(__instance.m_PulseFxIntensity);
                        }


                        //random variation between pain pulses
                        //__instance.m_PulseFxFrequencySeconds = Random.Range(3f, __instance.m_PulseFxFrequencySeconds + 5f);
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
            public static bool Prefix(ref AfflictionBodyArea location, ref string cause, SprainPain __instance)
            {
                PainHelper ph = new PainHelper();
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
                int index = 0;

                PainAffliction existingInstance = null;

                if (cause == "concussion") existingInstance = ph.GetConcussion();
                else if (cause == "broken rib") existingInstance = ph.GetBrokenRibPain(ref index);
                else existingInstance = ph.GetPainInstance(location, cause, ref index);

                if (existingInstance != null)
                {
                    ph.UpdatePainInstance(index, existingInstance);
                    ph.UpdatePainEffects();
                    return false;
                }

                float painLevel = 0;

                if (cause.ToLowerInvariant() == "console" || cause.ToLowerInvariant().Contains("bite")) //animal bites
                {
                    painLevel = 15f;

                    if (location == AfflictionBodyArea.Head)
                    {
                        __instance.m_AfflictionDurationHours = Random.Range(96f, 124f);
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
                        __instance.m_PulseFxIntensity = 0.5f;
                        __instance.m_PulseFxFrequencySeconds = 18f;
                        painLevel = 5f;
                    }
                }
                else if (cause.ToLowerInvariant().Contains("fall")) //sprains
                {
                    __instance.m_AfflictionDurationHours = Random.Range(96f, 124f);
                    __instance.m_PulseFxIntensity = 0.65f;
                    __instance.m_PulseFxFrequencySeconds = 15f;
                    painLevel = 10f;
                }
                else if (cause.ToLowerInvariant() == "concussion") //head trauma or concussion
                {
                    __instance.m_AfflictionDurationHours = Random.Range(96f, 240f);
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
                    if (location == AfflictionBodyArea.FootLeft || location == AfflictionBodyArea.FootRight) __instance.m_AfflictionDurationHours = Random.Range(96f, 240f);
                    else __instance.m_AfflictionDurationHours = Random.Range(72f, 120f);

                    __instance.m_PulseFxIntensity = 0.9f;
                    __instance.m_PulseFxFrequencySeconds = 10f;
                }

                ac.AddPainInstance(cause, location, __instance.m_AfflictionDurationHours, painLevel, __instance.m_PulseFxIntensity, __instance.m_PulseFxFrequencySeconds);

                //update pain effects when new pain is afflicted
                ph.UpdatePainEffects();
                return true;
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

                /**
                if (__instance.GetRemainingHours(index) <= 12f) //if there's 12 hours or less left on the pain area, painkiller will instantly relieve it
                {
                    var value = __instance.m_ActiveInstances[index];
                    float num = (value.m_EndTime - hoursPlayedNotPaused) * (100f - __instance.m_TreatmentPercent) * 0.01f;
                    value.m_EndTime = hoursPlayedNotPaused + num;
                    __instance.m_ActiveInstances[index] = value;
                    return;
                } **/

               AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
               ac.AdministerPainkillers(ac.m_PainkillerStandardAmount);
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.TreatAfflictionWithFirstAid))]

        public class PainkillerModifier2
        {

            public static void Postfix(PlayerManager __instance)
            {
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                if (__instance.m_FirstAidItemUsed.name.ToLowerInvariant().Contains("painkiller") && __instance.m_AfflictionSelected == Affliction.InvalidAffliction)
                {
                    ac.AdministerPainkillers(ac.m_PainkillerStandardAmount);
                }

            }

        }

        [HarmonyPatch(typeof(EmergencyStim), nameof(EmergencyStim.ApplyEmergencyStim))]

        public class AdministerStimulants
        {
            public static void Postfix()
            {
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
                ac.AdministerPainkillers(50f, true);
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
                PainHelper ph = new PainHelper();
                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

                int index = __instance.m_ActiveInstances.IndexOf(inst);

                ac.CurePainInstance(index);

                __instance.m_ActiveInstances.Remove(inst);
                PlayerDamageEvent.SpawnAfflictionEvent(UIPatches.GetAfflictionNameBasedOnCause(inst.m_Cause, inst.m_Location), "GAMEPLAY_Healed", UIPatches.GetIconNameBasedOnCause(inst.m_Cause), InterfaceManager.m_FirstAidBuffColor);
                InterfaceManager.GetPanel<Panel_FirstAid>().UpdateDueToAfflictionHealed();

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
