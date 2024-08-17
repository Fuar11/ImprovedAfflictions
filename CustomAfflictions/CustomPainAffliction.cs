using AfflictionComponent.Components;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using ImprovedAfflictions.Pain;
using UnityEngine;

namespace ImprovedAfflictions.CustomAfflictions
{

    internal class CustomPainAffliction
        : CustomAffliction, IDuration
    {

        public CustomPainAffliction(string name, string causeText, string description, string? descriptionNoHeal, string spriteName, AfflictionBodyArea location, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems, float duration, float painLevel, float frequency, float fxLevel) : base(name, causeText, description, descriptionNoHeal, spriteName, location, instantHeal, remedyItems, altRemedyItems)
        {

            Duration = duration;

            m_PainLevel = painLevel;
            m_StartingPainLevel = painLevel;

            m_PulseFxFrequencySeconds = frequency;
            m_PulseFxIntensity = fxLevel;

            Mod.painManager.m_PainStartingLevel += painLevel;
            PainEffects.UpdatePainEffects();
        }

        public float Duration { get; set; }
        public float EndTime { get; set; }

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
            return m_StartingPainLevel / InterfaceDuration.EndTime;
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
