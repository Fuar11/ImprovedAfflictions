using AfflictionComponent.Components;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ImprovedAfflictions.CustomAfflictions
{

    internal class CustomPainAffliction
        : CustomAffliction
    {
        public CustomPainAffliction(string afflictionName, string cause, string desc, string noHealDesc, AfflictionBodyArea location, string spriteName, bool risk, bool buff, float duration, bool noTimer, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems, float painLevel) : base(afflictionName, cause, desc, noHealDesc, location, spriteName, risk, buff, duration, noTimer, instantHeal, remedyItems, altRemedyItems)
        {
            m_PainLevel = painLevel;
            m_StartingPainLevel = painLevel;
        }
        public float m_PainLevel { get; set; }
        public float m_StartingPainLevel { get; set; }

        public override void OnUpdate()
        {
            float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
            m_PainLevel -= GetPainLevelDecreasePerHour() * tODHours;

            if (Mod.painManager.m_PainkillerLevel <= m_PainLevel)
            {
                if (!NeedsRemedy()) ResetRemedyItems(m_RemedyItems);
            }
        }

        public override void CureSymptoms()
        {
        }
        public override void OnCure()
        {
        }

        public float GetPainLevelDecreasePerHour()
        {
            return m_StartingPainLevel / m_EndTime;
        }

       
    }
}
