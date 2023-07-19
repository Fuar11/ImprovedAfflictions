using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using Random = UnityEngine.Random;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using MelonLoader;

namespace ImprovedAfflictions.Hypothermia
{
    internal class HypothermiaPatches
    {

        [HarmonyPatch(typeof(Freezing), nameof(Freezing.CalculateBodyTemperature))]

        public class HypothermiaTemperatureReduction
        {

            public static void Postfix(float __result)
            {

                if (GameManager.GetHypothermiaComponent().HasHypothermia())
                {
                    MelonLogger.Msg("Current result: {0}", __result);
                    __result -= 5f;
                    MelonLogger.Msg("New result: {0}", __result);
                }

            }

        }

    }
}
