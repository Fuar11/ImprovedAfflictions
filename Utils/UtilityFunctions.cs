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

    }
}
