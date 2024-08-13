using AfflictionComponent.Components;
using AfflictionComponent.Interfaces.Risk;
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

        private float m_RiskPercentage;
        private float m_LastUpdateTime;
        public OverdoseRisk(string afflictionName, string cause, string desc, string noHealDesc, AfflictionBodyArea location, string spriteName, bool risk, bool buff, float duration, bool noTimer, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems) : base(afflictionName, cause, desc, noHealDesc, location, spriteName, risk, buff, duration, noTimer, instantHeal, remedyItems, altRemedyItems)
        {
        }

        private PainManager pm = Mod.painManager;

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

        protected override void CureSymptoms()
        {
        }

        protected override void OnCure()
        {
        }

        public float GetRiskValue() => m_RiskPercentage;

        public void UpdateRiskValue()
        {
            m_RiskPercentage = CalculatePercentage(Mod.painManager.m_PainkillerLevel);
        }

    }
}
