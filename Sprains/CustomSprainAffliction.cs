using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using PainComponent.CustomAfflictions;

namespace ImprovedAfflictions.Sprains
{
    internal class CustomSprainAffliction : CustomPainAffliction, ILimp
    {
        public CustomSprainAffliction(string name, string causeText, string description, string? descriptionNoHeal, string spriteName, AfflictionBodyArea location, bool instantHeal, Tuple<string, int, int>[] remedyItems, float duration, float painLevel, float frequency, float fxLevel) : base(name, causeText, description, descriptionNoHeal, spriteName, location, instantHeal, remedyItems, duration, painLevel, frequency, fxLevel)
        {
        }

        public bool IsLimping { get; set; }

        public override void OnUpdate()
        {

            base.OnUpdate();

            if(Mod.painManager.PainkillersInEffect(m_PainLevel) || (m_PainLevel / m_StartingPainLevel) * 100 > 30)
            {
                if(!IsLimping) IsLimping = true;
            }
            else if(IsLimping) IsLimping = false;

        }
    }
}
