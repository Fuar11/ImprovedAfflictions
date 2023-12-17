using Il2CppNewtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModSettings;

namespace ImprovedAfflictions
{
    public enum Active
    {
        Enabled,
        Disabled
    }
    internal class CustomSettings : JsonModSettings
    {

        [Section("Food Poisoning")]

        [Name("Food Poisoning Threshold")]
        [Description("Food poisoning contraction threshold is decreased from 45% to 25% for all food items on Interloper or any difficulty with very high decay rate.")]
        [Choice("Disabled", "Enabled")]
        public Active fpThreshold = Active.Disabled;

        [Name("Food Poisoning Chance Offset")]
        [Description("Food poisoning chance is offset by 20%. E.g. 45% condition = 25% chance of food poisoning instead of 55%. On Interloper or any difficulty with very high decay rate.")]
        [Choice("Disabled", "Enabled")]
        public Active fpOffset = Active.Disabled;

    }

    static class Settings
    {

        internal static CustomSettings settings;
        internal static void OnLoad()
        {
            settings = new CustomSettings();
            settings.AddToModSettings("Improved Afflictions", MenuType.Both);
        }


    }

}
