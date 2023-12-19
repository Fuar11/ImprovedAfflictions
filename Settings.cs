using Il2CppNewtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModSettings;
using System.Reflection;
using MelonLoader;
using UnityEngine;

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

        [Name("Food Poisoning Minimum Contraction Duration")]
        [Description("The minimum time it takes to contract food poisoning in hours.")]
        [Slider(0, 48)]
        public int fpMinTime = 10;

        [Name("Food Poisoning Maximum Contraction Duration")]
        [Description("The maximum time it takes to contract food poisoning in hours.")]
        [Slider(0, 48)]
        public int fpMaxTime = 24;

        protected override void OnChange(FieldInfo field, object? oldValue, object? newValue)
        {
            if (field.Name == nameof(fpMinTime)) RefreshValues("fpMaxTime");
            else if (field.Name == nameof(fpMaxTime)) RefreshValues("fpMinTime");

            RefreshGUI();
        }

        internal void RefreshValues(string fieldName)
        {

            if (fieldName == "fpMinTime")
            {
                if(fpMaxTime < fpMinTime) fpMinTime = fpMaxTime - 1;
                fpMinTime = Mathf.Clamp(fpMinTime, 0, 48);
            }
            else if(fieldName == "fpMaxTime")
            {
                if (fpMinTime > fpMaxTime) fpMaxTime = fpMinTime + 1;
                fpMaxTime = Mathf.Clamp(fpMaxTime, 0, 48);
            }
        }
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
