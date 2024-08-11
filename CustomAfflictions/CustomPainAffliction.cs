using AfflictionComponent.Components;
using Il2Cpp;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.Pain;
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
        public CustomPainAffliction(string afflictionName, string cause, string desc, string noHealDesc, AfflictionBodyArea location, string spriteName, bool risk, bool buff, float duration, bool noTimer, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems, float painLevel, float frequency, float fxLevel) : base(afflictionName, cause, desc, noHealDesc, location, spriteName, risk, buff, duration, noTimer, instantHeal, remedyItems, altRemedyItems)
        {
            m_PainLevel = painLevel;
            m_StartingPainLevel = painLevel;

            m_PulseFxFrequencySeconds = frequency;
            m_PulseFxIntensity = fxLevel;

            Mod.painManager.m_PainStartingLevel += painLevel;
        }
        public float m_PainLevel { get; set; }
        public float m_StartingPainLevel { get; set; }
        public float m_PulseFxIntensity { get; set; }
        public float m_PulseFxFrequencySeconds { get; set; }

        public override void OnUpdate()
        {
            float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
            m_PainLevel -= GetPainLevelDecreasePerHour() * tODHours;

            if (Mod.painManager.m_PainkillerLevel < m_PainLevel && Mod.painManager.m_PainkillerIncrementAmount == 0)
            {
                if (!NeedsRemedy()) ResetRemedyItems(ref m_RemedyItems);
            }
            else if(Mod.painManager.m_PainkillerLevel >= m_PainLevel && NeedsRemedy())
            {
                UpdatePainkillersOnTheFly();
            }
        }

        protected override void CureSymptoms()
        {
        }
        protected override void OnCure()
        {
            Mod.painManager.m_PainStartingLevel = Mod.painManager.GetTotalPainStartingLevel();
            PainEffects.UpdatePainEffects();
        }

        public float GetPainLevelDecreasePerHour()
        {
            return m_StartingPainLevel / m_EndTime;
        }

        protected override bool ApplyRemedyCondition()
        {
            if (Mod.painManager.m_PainkillerLevel > m_PainLevel) return true;
            else return false;
            
        }
       
        //for now, as long as the arrays stay public
        private void UpdatePainkillersOnTheFly()
        {
            m_RemedyItems = m_RemedyItems.Select(item => item.Item1 == "GEAR_BottlePainKillers" ? new Tuple<string, int, int>(item.Item1, item.Item2, item.Item3 - 1) : item).ToArray();
        }
    }
}
