using Il2Cpp;
using Il2CppTLD.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedAfflictions.Utils
{
    internal class UtilityFunctions
    {

        public static float MapPercentageToVariable(double percentage, float minVariableValue = 1.0f, float maxVariableValue = 2.0f)
        {

            double minPercentage = 0;
            double maxPercentage = 100;
           
            float variable = (float)((percentage - minPercentage) / (maxPercentage - minPercentage) * (maxVariableValue - minVariableValue) + minVariableValue);

            variable = Math.Max(minVariableValue, Math.Min(maxVariableValue, variable));

            return variable;
        }

        public static bool IsInterloperOrFastDecayRate()
        {
            if (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.Interloper || GameManager.InCustomMode() && GameManager.GetCustomMode().m_ItemDecayRate == Il2CppTLD.Gameplay.Tunable.CustomTunableLMHV.VeryHigh) return true;
            else return false;
        }

        public static string GetAfflictionDescription(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "wolf bite":
                    return "You are suffering from a wolf bite. Take painkillers to numb the pain and wait for the wound to heal.";
                case "bear bite":
                    return "You are suffering from a bear bite. Take painkillers to numb the pain and wait for the wound to heal.";
                case "sprained wrist":
                    return "You're suffering from a sprained wrist, while the sprain can be stabilized, the pain will last for a while. Taking painkillers can numb the effects of the pain.";
                case "sprained ankle":
                    return "You're suffering from a sprained ankle, while the sprain can be stabilized, the pain will last for a while. Taking painkillers can numb the effects of the pain.";
                case "broken rib":
                    return "You've broken one or more of your ribs, the pain will last for a while. Movement and mobility will be hindered while suffering from the pain, take painkillers to numb the effects.";
                case "concussion":
                    return "You've sufferred head trauma and are suffering from a concussion. Take painkillers to numb the debilitating effects while your head rests to heal.";
                case "chemical burns":
                    return "You've exposed your hands or feet to corrosive chemicals and have suffered severe burns. Take painkillers to numb the pain and wait for them to heal.";
                default: return Localization.Get("GAMEPLAY_SprainPainDesc");
            }

        }
    }
}
