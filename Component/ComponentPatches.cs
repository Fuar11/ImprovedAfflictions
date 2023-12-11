using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Il2Cpp;
using HarmonyLib;
using Unity;
using ImprovedAfflictions.Pain.Component;
using UnityEngine;
using ImprovedAfflictions.Utils;
using MelonLoader;

namespace ImprovedAfflictions.Component
{
    internal class ComponentPatches
    {

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Serialize))]

        public class SaveComponent
        {
            public static void Postfix()
            {
                if (GameManager.m_ActiveScene.ToLowerInvariant().Contains("menu") || GameManager.m_ActiveScene.ToLowerInvariant().Contains("boot") || GameManager.m_ActiveScene.ToLowerInvariant().Contains("empty")) return;

                AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
                ac.SaveData();
            }
        }
    }
}
