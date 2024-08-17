using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using Random = UnityEngine.Random;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using ImprovedAfflictions.CustomAfflictions;

namespace ImprovedAfflictions.Sprains
{
    internal class SprainPatches
    {
        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.ApplyAffliction))]

        private static class SprainPainOverride
        {

            public static bool Prefix(ref AfflictionBodyArea location)
            {
                float duration = Random.Range(48f, 72f);

                string name = location == AfflictionBodyArea.HandLeft || location == AfflictionBodyArea.HandRight ? "Sprained Wrist" : "Sprained Ankle";

                if (AfflictionHelper.ResetIfHasAffliction(name, location, true)) return false;

                new CustomPainAffliction(name, name, AfflictionHelper.GetAfflictionDescription(name), "", "ico_CarryRestrictions", location, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], duration, 10f, 15f, 0.65f).Start();

                return false;
            }

        }

    }
}
