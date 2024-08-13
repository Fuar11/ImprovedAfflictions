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
    internal class Overdose : CustomAffliction
    {
        public Overdose(string afflictionName, string cause, string desc, string noHealDesc, AfflictionBodyArea location, string spriteName, bool risk, bool buff, float duration, bool noTimer, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems) : base(afflictionName, cause, desc, noHealDesc, location, spriteName, risk, buff, duration, noTimer, instantHeal, remedyItems, altRemedyItems)
        {
        }

        public override void OnUpdate()
        {
            if (!Mod.painManager.IsOverdosing()) Cure();
        }

        protected override void CureSymptoms()
        {
        }

        protected override void OnCure()
        {
        }
    }
}
