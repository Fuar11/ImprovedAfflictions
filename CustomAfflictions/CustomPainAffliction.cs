using AfflictionComponent.Components;
using AfflictionComponent.Interfaces;
using AfflictionComponent.Resources;
using Il2Cpp;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.Pain;
using ImprovedAfflictions.Utils;
using UnityEngine;

namespace ImprovedAfflictions.CustomAfflictions
{

    internal class CustomPainAffliction
        : CustomAffliction, IDuration, IRemedies, IInstance
    {

        public CustomPainAffliction(string name, string causeText, string description, string? descriptionNoHeal, string spriteName, AfflictionBodyArea location, bool instantHeal, Tuple<string, int, int>[] remedyItems, float duration, float painLevel, float frequency, float fxLevel) : base(name, causeText, description, descriptionNoHeal, spriteName, location)
        {

            Duration = duration;

            SetInstanceTypeBasedOnName();

            RemedyItems = remedyItems;
            AltRemedyItems = [];
            InstantHeal = instantHeal;

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
        public bool InstantHeal { get; set; }
        public Tuple<string, int, int>[] RemedyItems { get; set; }
        public Tuple<string, int, int>[] AltRemedyItems { get; set; }
        public InstanceType type { get; set; }

        public override void OnUpdate()
        {
            float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
            m_PainLevel -= GetPainLevelDecreasePerHour() * tODHours;

            if (Mod.painManager.m_PainkillerLevel < m_PainLevel && Mod.painManager.m_PainkillerIncrementAmount == 0)
            {
                if (!NeedsRemedy())
                {
                    ResetRemedyItems(this);
                }
            }
            else if(Mod.painManager.m_PainkillerLevel >= m_PainLevel && NeedsRemedy())
            {
                UpdatePainkillersOnTheFly();
            }
        }

        public void CureSymptoms()
        {
        }
        public void OnCure()
        {
            Mod.painManager.m_PainStartingLevel = Mod.painManager.GetTotalPainStartingLevel();
            PainEffects.UpdatePainEffects();
        }

        public float GetPainLevelDecreasePerHour()
        {
            return m_StartingPainLevel / EndTime;
        }

        protected override bool ApplyRemedyCondition()
        {
            if (Mod.painManager.m_PainkillerLevel > m_PainLevel) return true;
            else return false;
            
        }
       
        private void UpdatePainkillersOnTheFly()
        {
            RemedyItems = RemedyItems.Select(item => item.Item1 == "GEAR_BottlePainKillers" ? new Tuple<string, int, int>(item.Item1, item.Item2, item.Item3 - 1) : item).ToArray();
        }

        private void SetInstanceTypeBasedOnName()
        {
            if(m_Name is null)
            {
                Mod.Logger.Log("Name is null", ComplexLogger.FlaggedLoggingLevel.Error);

                if (type != InstanceType.Open) Mod.Logger.Log("Type is not default", ComplexLogger.FlaggedLoggingLevel.Debug);
                return;
            }

            if (m_Name.ToLowerInvariant().Contains("concussion")) type = InstanceType.Single;
            else if (m_Name.ToLowerInvariant().Contains("bite")) type = InstanceType.Open;
            else if (m_Name.ToLowerInvariant().Contains("chemical")) type = InstanceType.SingleLocation;
            else if (m_Name.ToLowerInvariant().Contains("sprain")) type = InstanceType.SingleLocation;
        }

        public void OnFoundExistingInstance(CustomAffliction aff)
        {
            if(aff is CustomPainAffliction paff)
            {
                AfflictionHelper.ResetPainAffliction(paff);
            }
        }
    }
}
