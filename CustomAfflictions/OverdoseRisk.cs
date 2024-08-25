using AfflictionComponent.Components;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using ImprovedAfflictions.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedAfflictions.CustomAfflictions
{
    internal class OverdoseRisk : CustomAffliction, IRiskPercentage
    {

        public bool Risk { get; set; }
        private float m_RiskPercentage;
        private float m_LastUpdateTime;

        

        private PainManager pm = Mod.painManager;

        public OverdoseRisk(string name, string causeText, string description, string? descriptionNoHeal, string spriteName, AfflictionBodyArea location, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems) : base(name, causeText, description, descriptionNoHeal, spriteName, location)
        {
            Risk = true;
        }

        public override void OnUpdate()
        {
            UpdateRiskValue();

            if (m_RiskPercentage <= 0) Cure();
            else if (m_RiskPercentage >= 100) Cure(false);
        }

        float CalculatePercentage(float x)
        {
            if (x < 60) return 0f;
            if (x > 80) return 100f;

            return ((x - 60f) / 20f) * 100f;
        }

        protected void CureSymptoms()
        {
        }

        protected void OnCure()
        {
        }

        public float GetRiskValue() => m_RiskPercentage;

        public void UpdateRiskValue()
        {
            m_RiskPercentage = CalculatePercentage(Mod.painManager.m_PainkillerLevel);
        }

    }
}
