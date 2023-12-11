using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using HarmonyLib;
using UnityEngine;
using MelonLoader;
using System.Text.Json.Serialization;

namespace ImprovedAfflictions.Component
{
    internal class PainAffliction
    {

        public string m_Cause { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AfflictionBodyArea m_Location { get; set; }
        public float m_EndTime { get; set; }
        public float m_PainLevel { get; set; }
        public float m_StartingPainLevel { get; set; }
        public float m_PulseFxIntensity { get; set; }
        public float m_PulseFxFrequencySeconds { get; set; }
        public float m_PulseFxMaxDuration { get; set; }


        public void DecreasePainLevel(float tODHours)
        {
            //decrease pain level over time as wound heals
            m_PainLevel -= GetPainLevelDecreasePerHour() * tODHours;

        }

        public float GetPainLevelDecreasePerHour()
        {
            return m_StartingPainLevel / m_EndTime;
        }

    }
}
