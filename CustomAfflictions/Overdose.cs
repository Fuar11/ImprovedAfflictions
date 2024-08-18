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
        public Overdose(string name, string causeText, string description, string? descriptionNoHeal, string spriteName, AfflictionBodyArea location, bool instantHeal, Tuple<string, int, int>[] remedyItems, Tuple<string, int, int>[] altRemedyItems) : base(name, causeText, description, descriptionNoHeal, spriteName, location)
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
