using Il2Cpp;
using Random = UnityEngine.Random;
using HarmonyLib;
using static Il2Cpp.Panel_Debug;
using UnityEngine;
using MelonLoader;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.CustomAfflictions;

namespace ImprovedAfflictions.Utils
{
    internal class AfflictionHelper
    {

        static PainManager pm = Mod.painManager;

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

        public static void ResetPainAffliction(CustomPainAffliction aff)
        {
            Mod.painManager.m_PainStartingLevel -= aff.m_PainLevel;
            aff.ResetAffliction(false);
            aff.m_PainLevel = aff.m_StartingPainLevel;
        }

        public static bool HasPainAffliction(string name)
        {
            if (pm.am.m_Afflictions.Count == 0) return false;

            foreach (CustomPainAffliction aff in pm.am.m_Afflictions.OfType<CustomPainAffliction>())
            {
                return aff.m_Name == name ? true : false;
            }

            return false;
        }
        public static bool HasPainAfflictionAtLocation(string name, AfflictionBodyArea location)
        {
            if (pm.am.m_Afflictions.Count == 0) return false;

            foreach (CustomPainAffliction aff in pm.am.m_Afflictions.OfType<CustomPainAffliction>())
            {
                if (aff.m_Name == name && aff.m_Location == location) return true;
            }

            return false;
        }

        public static List<CustomPainAffliction> GetPainAfflictions(string name)
        {
            PainManager pm = Mod.painManager;

            if (pm.am.m_Afflictions.Count == 0) return new List<CustomPainAffliction>();

            return pm.am.m_Afflictions.OfType<CustomPainAffliction>().Where(aff => aff.m_Name == name).ToList();
        }
        public static List<CustomPainAffliction> GetPainAfflictionsAtLocation(string name, AfflictionBodyArea location)
        {
            PainManager pm = Mod.painManager;

            if (pm.am.m_Afflictions.Count == 0) return new List<CustomPainAffliction>();

            return pm.am.m_Afflictions.OfType<CustomPainAffliction>().Where(aff => aff.m_Name == name && aff.m_Location == location).ToList();
        }

        public bool CanClimbRope()
        {
            PainManager pm = Mod.painManager;

            AfflictionBodyArea[] handRight = { AfflictionBodyArea.HandRight };
            AfflictionBodyArea[] handLeft = { AfflictionBodyArea.HandLeft };

            //both hands
            float handRightPainLevel = pm.GetTotalPainLevelForPainAtLocations(handRight);
            float handRightStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(handRight, true);
            float handRightPainDifference = handRightPainLevel / handRightStartingPainLevel * 100;

            float handLeftPainLevel = pm.GetTotalPainLevelForPainAtLocations(handLeft);
            float handLeftStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(handLeft, true);
            float handLeftPainDifference = handLeftPainLevel / handLeftStartingPainLevel * 100;

            AfflictionBodyArea[] armLeft = { AfflictionBodyArea.ArmLeft };
            AfflictionBodyArea[] armRight = { AfflictionBodyArea.ArmRight };

            //both arms
            float armLeftPainLevel = pm.GetTotalPainLevelForPainAtLocations(armLeft);
            float armLeftStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(armLeft, true);
            float armLeftPainDifference = armLeftPainLevel / armLeftStartingPainLevel * 100;

            float armRightPainLevel = pm.GetTotalPainLevelForPainAtLocations(armRight);
            float armRightStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(armRight, true);
            float armRightPainDifference = armRightPainLevel / armRightStartingPainLevel * 100;

            AfflictionBodyArea[] legLeft = { AfflictionBodyArea.LegLeft };
            AfflictionBodyArea[] legRight = { AfflictionBodyArea.LegRight };

            //both legs
            float legLeftPainLevel = pm.GetTotalPainLevelForPainAtLocations(legLeft);
            float legLeftStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(legLeft, true);
            float legLeftPainDifference = legLeftPainLevel / legLeftStartingPainLevel * 100;

            float legRightPainLevel = pm.GetTotalPainLevelForPainAtLocations(legRight);
            float legRightStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(legRight, true);
            float legRightPainDifference = legRightPainLevel / legRightStartingPainLevel * 100;

            AfflictionBodyArea[] footLeft = { AfflictionBodyArea.FootLeft };
            AfflictionBodyArea[] footRight = { AfflictionBodyArea.FootRight };

            // Both feet
            float footLeftPainLevel = pm.GetTotalPainLevelForPainAtLocations(footLeft);
            float footLeftStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(footLeft, true);
            float footLeftPainDifference = footLeftPainLevel / footLeftStartingPainLevel * 100;

            float footRightPainLevel = pm.GetTotalPainLevelForPainAtLocations(footRight);
            float footRightStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(footRight, true);
            float footRightPainDifference = footRightPainLevel / footRightStartingPainLevel * 100;

            if (handLeftPainDifference > 30 && handRightPainDifference > 30 && !pm.PainkillersInEffect(handRightPainLevel) && !pm.PainkillersInEffect(handLeftPainLevel)) return false;
            else if (armLeftPainDifference > 30 && handLeftPainDifference > 30 && !pm.PainkillersInEffect(armLeftPainLevel) && !pm.PainkillersInEffect(handLeftPainLevel)) return false;
            else if (armRightPainDifference > 30 && handRightPainDifference > 30 && !pm.PainkillersInEffect(armRightPainLevel) && !pm.PainkillersInEffect(handRightPainLevel)) return false;
            else if (armLeftPainDifference > 30 && armRightPainDifference > 30 && !pm.PainkillersInEffect(armRightPainLevel) && !pm.PainkillersInEffect(armLeftPainLevel)) return false;

            if (footLeftPainDifference > 30 && footRightPainDifference > 30 && !pm.PainkillersInEffect(legRightPainLevel) && !pm.PainkillersInEffect(legLeftPainLevel)) return false;
            else if (legLeftPainDifference > 30 && footLeftPainDifference > 30 && !pm.PainkillersInEffect(legLeftPainLevel) && !pm.PainkillersInEffect(footLeftPainLevel)) return false;
            else if (legRightPainDifference > 30 && footRightPainDifference > 30 && !pm.PainkillersInEffect(legRightPainLevel) && !pm.PainkillersInEffect(footRightPainLevel)) return false;
            else if (legLeftPainDifference > 30 && legRightPainDifference > 30 && !pm.PainkillersInEffect(legRightPainLevel) && !pm.PainkillersInEffect(legLeftPainLevel)) return false;

            return true;
        }

        /**
        public static bool CanCarryTravois()
        {
            PainManager ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<PainManager>();
            AfflictionBodyArea[] armLeft = { AfflictionBodyArea.ArmLeft };
            AfflictionBodyArea[] armRight = { AfflictionBodyArea.ArmRight };
            AfflictionBodyArea[] chest = { AfflictionBodyArea.Chest };

            float armLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(armLeft);
            float armLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(armLeft, true);
            float armLeftPainDifference = (armLeftPainLevel / armLeftStartingPainLevel) * 100;

            float armRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(armRight);
            float armRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(armRight, true);
            float armRightPainDifference = (armRightPainLevel / armRightStartingPainLevel) * 100;

            float chestPainLevel = ac.GetTotalPainLevelForPainAtLocations(chest);
            float chestStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(chest, true);
            float chestPainDifference = (chestPainLevel / chestStartingPainLevel) * 100;

            if (armRightPainDifference > 30 && armLeftPainDifference > 30 || chestPainDifference > 30)
            {
                return false;
            }
            else return true;
        } **/

        public static bool CanClimbRocks()
        {

            PainManager pm = Mod.painManager;

            AfflictionBodyArea[] handRight = { AfflictionBodyArea.HandRight };
            AfflictionBodyArea[] handLeft = { AfflictionBodyArea.HandLeft };

            //both hands
            float handRightPainLevel = pm.GetTotalPainLevelForPainAtLocations(handRight);
            float handRightStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(handRight, true);
            float handRightPainDifference = handRightPainLevel / handRightStartingPainLevel * 100;

            float handLeftPainLevel = pm.GetTotalPainLevelForPainAtLocations(handLeft);
            float handLeftStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(handLeft, true);
            float handLeftPainDifference = handLeftPainLevel / handLeftStartingPainLevel * 100;

            AfflictionBodyArea[] armLeft = { AfflictionBodyArea.ArmLeft };
            AfflictionBodyArea[] armRight = { AfflictionBodyArea.ArmRight };

            //both arms
            float armLeftPainLevel = pm.GetTotalPainLevelForPainAtLocations(armLeft);
            float armLeftStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(armLeft, true);
            float armLeftPainDifference = armLeftPainLevel / armLeftStartingPainLevel * 100;

            float armRightPainLevel = pm.GetTotalPainLevelForPainAtLocations(armRight);
            float armRightStartingPainLevel = pm.GetTotalPainLevelForPainAtLocations(armRight, true);
            float armRightPainDifference = armRightPainLevel / armRightStartingPainLevel * 100;

            if (handLeftPainDifference > 30 && handRightPainDifference > 30 && !pm.PainkillersInEffect(handRightPainLevel) && !pm.PainkillersInEffect(handLeftPainLevel)) return false;
            else if (armLeftPainDifference > 30 && handLeftPainDifference > 30 && !pm.PainkillersInEffect(armLeftPainLevel) && !pm.PainkillersInEffect(handLeftPainLevel)) return false;
            else if (armRightPainDifference > 30 && handRightPainDifference > 30 && !pm.PainkillersInEffect(armRightPainLevel) && !pm.PainkillersInEffect(handRightPainLevel)) return false;
            else if (armLeftPainDifference > 30 && armRightPainDifference > 30 && !pm.PainkillersInEffect(armRightPainLevel) && !pm.PainkillersInEffect(armLeftPainLevel)) return false;

            return true;
        }






    }
}
