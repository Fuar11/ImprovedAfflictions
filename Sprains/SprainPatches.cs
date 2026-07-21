using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Data;
using Il2CppTLD.Stats;
using PainComponent.CustomAfflictions;
using PainComponent.Component;
using ImprovedAfflictions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Il2Cpp.Panel_Debug;
using Random = UnityEngine.Random;

namespace ImprovedAfflictions.Sprains
{
    internal class SprainPatches
    {

        [HarmonyPatch(typeof(SprainedWrist), nameof(SprainedWrist.SprainedWristStart))]

        private static class SprainWOverride
        {

            public static bool Prefix() => false;

            public static void Postfix(ref string causeId, ref AfflictionOptions options, SprainedWrist __instance)
            {
                if (GameManager.GetPlayerManagerComponent().PlayerIsDead() || InterfaceManager.IsPanelEnabled<Panel_ChallengeComplete>() || (GameManager.InCustomMode() && !GameManager.GetCustomMode().m_EnableSprains) || __instance.m_IsNoSprainWristForced)
                {
                    return;
                }
                AfflictionBodyArea afflictionBodyArea = AfflictionBodyArea.HandLeft;
                if (__instance.m_Locations.Contains((int)afflictionBodyArea))
                {
                    afflictionBodyArea = AfflictionBodyArea.HandRight;
                }
                if (__instance.m_Locations.Contains((int)afflictionBodyArea))
                {
                    return;
                }

                if (options.HasFlag(AfflictionOptions.PlayFX))
                {
                    if (options.HasFlag(AfflictionOptions.Stumble))
                    {
                        __instance.DoStumbleEffects();
                    }
                    if (options.HasFlag(AfflictionOptions.DisplayIcon))
                    {
                        PlayerDamageEvent.SpawnDamageEvent(__instance.m_LocalizedDisplayName.m_LocalizationID, "GAMEPLAY_Affliction", "ico_injury_sprainedWrist", InterfaceManager.m_FirstAidRedColor, fadeout: true, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventDisplaySeconds, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventFadeOutSeconds);
                    }
                    GameManager.GetPlayerVoiceComponent().Play(__instance.m_SprainedWristVO, Il2CppVoice.Priority.Critical);
                    GameAudioManager.PlaySound(__instance.m_SprainedWristSFX, __instance.gameObject);
                }

                __instance.UnequipItemInHands();

                float duration = Random.Range(48f, 72f);

                string name = "Sprained Wrist";
                var sprain = new CustomPainAffliction(name, name, AfflictionHelper.GetAfflictionDescription(name), "", "ico_injury_sprainedWrist", afflictionBodyArea, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], duration, 15f, 15f, 0.65f);
                sprain.SetInstanceTypeBasedOnName();
                sprain.Start();

            }
        }

        [HarmonyPatch(typeof(SprainedAnkle), nameof(SprainedAnkle.SprainedAnkleStart))]

        private static class SprainOverride
        {

            public static bool Prefix() => false;
          
            public static void Postfix(ref string causeID, ref AfflictionOptions options, SprainedAnkle __instance)
            {

                if (GameManager.GetPlayerManagerComponent().PlayerIsDead() || InterfaceManager.IsPanelEnabled<Panel_ChallengeComplete>() || (GameManager.InCustomMode() && !GameManager.GetCustomMode().m_EnableSprains))
                {
                    return;
                }
                List<AfflictionBodyArea> list = new List<AfflictionBodyArea>(new AfflictionBodyArea[2]
                {
            AfflictionBodyArea.FootLeft,
            AfflictionBodyArea.FootRight
                });
                for (int i = 0; i < __instance.m_Locations.Count; i++)
                {
                    list.Remove((AfflictionBodyArea)__instance.m_Locations[i]);
                }
                if (list.Count <= 0)
                {
                    return;
                }
                AfflictionBodyArea afflictionBodyArea = list[Random.Range(0, list.Count)];

                if (options.HasFlag(AfflictionOptions.PlayFX))
                {
                    if (options.HasFlag(AfflictionOptions.Stumble))
                    {
                        __instance.DoStumbleEffects();
                    }
                    if (options.HasFlag(AfflictionOptions.DisplayIcon))
                    {
                        PlayerDamageEvent.SpawnDamageEvent(__instance.m_LocalizedDisplayName.m_LocalizationID, "GAMEPLAY_Affliction", "ico_injury_sprainedAnkle", InterfaceManager.m_FirstAidRedColor, fadeout: true, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventDisplaySeconds, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventFadeOutSeconds);
                    }
                    GameManager.GetPlayerVoiceComponent().Play(__instance.m_SprainedAnkleAudioEvent, Il2CppVoice.Priority.Critical);
                    GameAudioManager.PlaySound(__instance.m_SprainedAnkleSFXEvent, __instance.gameObject);
                }

                float duration = Random.Range(48f, 72f);

                string name = "Sprained Ankle";
                var sprain = new CustomSprainAffliction(name, name, AfflictionHelper.GetAfflictionDescription(name), "", "ico_injury_sprainedAnkle", afflictionBodyArea, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], duration, 15f, 15f, 0.65f);
                sprain.SetInstanceTypeBasedOnName();
                sprain.Start();

            }

        }
    }
}
