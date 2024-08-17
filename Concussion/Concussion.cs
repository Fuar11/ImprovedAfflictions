using Il2Cpp;
using Il2CppTLD.Stats;
using ImprovedAfflictions.CustomAfflictions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using HarmonyLib;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.Utils;

namespace ImprovedAfflictions.Concussion
{
    internal static class Concussion
    {
        static PainManager pm = Mod.painManager;
        public static string KEY = "Concussion";
        public static void MaybeConcuss(float chance)
        {
            GearItem hardHat = GameManager.GetInventoryComponent().GearInInventory("GEAR_MinersHelmet", 1);

            if (hardHat != null)
            {
                if (hardHat.m_ClothingItem.IsWearing())
                {
                    chance /= 2;
                }
            }

            if (Il2Cpp.Utils.RollChance(chance))
            {

                if (AfflictionHelper.ResetIfHasAffliction(KEY, AfflictionBodyArea.Head, false)) return;

                float duration = Random.Range(96f, 240f);
                string desc = "You've sufferred head trauma and are suffering from a concussion. Take painkillers to numb the debilitating effects while your head rests to heal.";
                //apply concussion here
                new CustomPainAffliction(KEY, "Head Trauma", desc, "", "ico_injury_diabetes", AfflictionBodyArea.Head, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], duration, 40f, 6f, 2.5f);
                GameManager.GetCameraEffects().PainPulse(1f);
            }

        }

        public static bool HasConcussion(bool checkForPainkillers)
        {
            if (pm.am.m_Afflictions.Count == 0) return false;

            foreach (CustomPainAffliction aff in pm.am.m_Afflictions.OfType<CustomPainAffliction>())
            {
                if (aff.m_Name == KEY)
                {
                    return checkForPainkillers ? !pm.PainkillersInEffect(aff.m_PainLevel) : true;
                }
            }

            return false;
        }

       

        //adds concussion on chance when falling off rope
        [HarmonyPatch(typeof(FallDamage), nameof(FallDamage.ApplyFallDamage))]
        public class ConcussionTrigger
        {
            public static bool Prefix() { return false; }
            public static void Postfix(ref float height, ref float damageOverride, FallDamage __instance)
            {

                AfflictionHelper ph = new AfflictionHelper();

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
                            MaybeConcuss(90f);

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
                MaybeConcuss(80f);
            }
        }


        [HarmonyPatch(typeof(PlayerStruggle), nameof(PlayerStruggle.ApplyBearDamageAfterStruggleEnds))]
        public class ConcussionTriggerBear
        {
            public static void Postfix()
            {
                MaybeConcuss(60f);
            }

        }

    }
}
