using Il2Cpp;
using Random = UnityEngine.Random;
using HarmonyLib;
using static Il2Cpp.Panel_Debug;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.Pain.Component;
using UnityEngine;
using MelonLoader;
using NetTopologySuite.Geometries;

namespace ImprovedAfflictions.Pain
{
    internal class PainHelper
    {

        public void UpdatePainEffects()
        {

            if (GameManager.m_ActiveScene.ToLowerInvariant().Contains("menu") || GameManager.m_ActiveScene.ToLowerInvariant().Contains("boot")) return;

            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            float maxIntensity = 0f;

            for (int num = ac.m_PainInstances.Count - 1; num >= 0; num--)
            {
                PainAffliction pain = ac.GetPainInstance(num);

                //if the pain instance isn't saved, skip to the next one
                if (pain == null) continue;

                //set the current pain effects to the most intense pain in the list
                if (pain.m_PulseFxIntensity >= maxIntensity)
                {
                    painManager.m_PulseFxIntensity = pain.m_PulseFxIntensity;
                    painManager.m_PulseFxFrequencySeconds = pain.m_PulseFxFrequencySeconds;
                    maxIntensity = pain.m_PulseFxIntensity;
                }
            }

            //if painkillers have been taken, dull the pain effects by how much drugs are in your system
           
            painManager.m_PulseFxIntensity /= ac.IsOnPainkillers() ? ac.GetPainkillerLevel() / 10 : 1;
            
        }

        public void UpdatePainInstance(int index, PainAffliction instanceToUpdate)
        {
            float newDuration = Random.Range(instanceToUpdate.m_PulseFxMaxDuration, instanceToUpdate.m_MaxDuration);
            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            SprainPain.Instance newInstance = new SprainPain.Instance();
            newInstance.m_Location = painManager.m_ActiveInstances[index].m_Location;
            newInstance.m_Cause = painManager.m_ActiveInstances[index].m_Cause;
            newInstance.m_EndTime = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + newDuration;

            painManager.m_ActiveInstances[index] = newInstance;

            instanceToUpdate.m_PulseFxMaxDuration = newDuration;

            string cause = instanceToUpdate.m_Cause;
            AfflictionBodyArea location = instanceToUpdate.m_Location;

            if (cause.ToLowerInvariant() != "fall" && cause.ToLowerInvariant() != "broken rib" && cause.ToLowerInvariant() != "console" && !cause.ToLowerInvariant().Contains("chemical"))
            {
                PlayerDamageEvent.SpawnDamageEvent(UIPatches.GetAfflictionNameBasedOnCause(cause, location), "GAMEPLAY_Affliction", UIPatches.GetIconNameBasedOnCause(cause), InterfaceManager.m_FirstAidRedColor, fadeout: true, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventDisplaySeconds, InterfaceManager.GetPanel<Panel_HUD>().m_DamageEventFadeOutSeconds);
            }

            ac.UpdatePainInstance(index, instanceToUpdate);
        }
        public PainAffliction GetPainInstance(AfflictionBodyArea location, string cause, ref int index)
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                if (inst.m_Location == location && inst.m_Cause == cause)
                {

                    if (inst.m_Cause == "concussion") continue;

                    PainAffliction pain = ac.GetPainInstance(i);

                    index = i;

                    if (pain == null) return null;
                    else return pain;
                    
                }
            }
            return null;
        }
        public bool IsOnPainkillers()
        {

            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            if (ac.PainkillersInEffect())
            {
                return true;
            }

            return false;
        }
        public bool HasPainAtLocation(AfflictionBodyArea location, string cause)
        {
            foreach (SprainPain.Instance pain in GameManager.GetSprainPainComponent().m_ActiveInstances)
            {
                if (pain.m_Location == location && pain.m_Cause == cause)
                {
                    return true;
                }
            }
            return false;
        }
        public void UpdatePainAtLocation(AfflictionBodyArea location, string cause)
        {

            float newDuration = Random.Range(96f, 240f);

            foreach (SprainPain.Instance pain in GameManager.GetSprainPainComponent().m_ActiveInstances)
            {
                if (pain.m_Location == location && pain.m_Cause == cause)
                {
                    pain.m_EndTime = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + newDuration;
                }
            }

        }
        public bool CanClimbRope()
        {

            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();


            AfflictionBodyArea[] handRight = { AfflictionBodyArea.HandRight };
            AfflictionBodyArea[] handLeft = { AfflictionBodyArea.HandLeft };

            //both hands
            float handRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(handRight);
            float handRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(handRight, true);
            float handRightPainDifference = (handRightPainLevel / handRightStartingPainLevel) * 100;

            float handLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(handLeft);
            float handLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(handLeft, true);
            float handLeftPainDifference = (handLeftPainLevel / handLeftStartingPainLevel) * 100;

            AfflictionBodyArea[] armLeft = { AfflictionBodyArea.ArmLeft };
            AfflictionBodyArea[] armRight = { AfflictionBodyArea.ArmRight };

            //both arms
            float armLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(armLeft);
            float armLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(armLeft, true);
            float armLeftPainDifference = (armLeftPainLevel / armLeftStartingPainLevel) * 100;

            float armRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(armRight);
            float armRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(armRight, true);
            float armRightPainDifference = (armRightPainLevel / armRightStartingPainLevel) * 100;

            AfflictionBodyArea[] legLeft = { AfflictionBodyArea.LegLeft};
            AfflictionBodyArea[] legRight = { AfflictionBodyArea.LegRight };

            //both legs
            float legLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(legLeft);
            float legLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(legLeft, true);
            float legLeftPainDifference = (legLeftPainLevel / legLeftStartingPainLevel) * 100;

            float legRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(legRight);
            float legRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(legRight, true);
            float legRightPainDifference = (legRightPainLevel / legRightStartingPainLevel) * 100;

            AfflictionBodyArea[] footLeft = { AfflictionBodyArea.FootLeft };
            AfflictionBodyArea[] footRight = { AfflictionBodyArea.FootRight };

            // Both feet
            float footLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(footLeft);
            float footLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(footLeft, true);
            float footLeftPainDifference = (footLeftPainLevel / footLeftStartingPainLevel) * 100;

            float footRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(footRight);
            float footRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(footRight, true);
            float footRightPainDifference = (footRightPainLevel / footRightStartingPainLevel) * 100;

            if ((handLeftPainDifference > 30 && handRightPainDifference > 30) && (!ac.PainkillersInEffect(handRightPainLevel) && !ac.PainkillersInEffect(handLeftPainLevel))) return false;
            else if (armLeftPainDifference > 30 && handLeftPainDifference > 30 && (!ac.PainkillersInEffect(armLeftPainLevel) && !ac.PainkillersInEffect(handLeftPainLevel))) return false;
            else if (armRightPainDifference > 30 && handRightPainDifference > 30 && (!ac.PainkillersInEffect(armRightPainLevel) && !ac.PainkillersInEffect(handRightPainLevel))) return false;
            else if (armLeftPainDifference > 30 && armRightPainDifference > 30 && !ac.PainkillersInEffect(armRightPainLevel) && !ac.PainkillersInEffect(armLeftPainLevel)) return false;

            if ((footLeftPainDifference > 30 && footRightPainDifference > 30) && !ac.PainkillersInEffect(legRightPainLevel) && !ac.PainkillersInEffect(legLeftPainLevel)) return false;
            else if (legLeftPainDifference > 30 && footLeftPainDifference > 30 && !ac.PainkillersInEffect(legLeftPainLevel) && !ac.PainkillersInEffect(footLeftPainLevel)) return false;
            else if (legRightPainDifference > 30 && footRightPainDifference > 30 && !ac.PainkillersInEffect(legRightPainLevel) && !ac.PainkillersInEffect(footRightPainLevel)) return false;
            else if (legLeftPainDifference > 30 && legRightPainDifference > 30 && !ac.PainkillersInEffect(legRightPainLevel) && !ac.PainkillersInEffect(legLeftPainLevel)) return false;

            return true;
        }
        public static bool CanCarryTravois()
        {
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();
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
        }
        public static bool CanClimbRocks()
        {
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            AfflictionBodyArea[] handRight = { AfflictionBodyArea.HandRight };
            AfflictionBodyArea[] handLeft = { AfflictionBodyArea.HandLeft };

            //both hands
            float handRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(handRight);
            float handRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(handRight, true);
            float handRightPainDifference = (handRightPainLevel / handRightStartingPainLevel) * 100;

            float handLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(handLeft);
            float handLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(handLeft, true);
            float handLeftPainDifference = (handLeftPainLevel / handLeftStartingPainLevel) * 100;

            AfflictionBodyArea[] armLeft = { AfflictionBodyArea.ArmLeft };
            AfflictionBodyArea[] armRight = { AfflictionBodyArea.ArmRight };

            //both arms
            float armLeftPainLevel = ac.GetTotalPainLevelForPainAtLocations(armLeft);
            float armLeftStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(armLeft, true);
            float armLeftPainDifference = (armLeftPainLevel / armLeftStartingPainLevel) * 100;

            float armRightPainLevel = ac.GetTotalPainLevelForPainAtLocations(armRight);
            float armRightStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(armRight, true);
            float armRightPainDifference = (armRightPainLevel / armRightStartingPainLevel) * 100;

            if ((handLeftPainDifference > 30 && handRightPainDifference > 30) && (!ac.PainkillersInEffect(handRightPainLevel) && !ac.PainkillersInEffect(handLeftPainLevel))) return false;
            else if (armLeftPainDifference > 30 && handLeftPainDifference > 30 && (!ac.PainkillersInEffect(armLeftPainLevel) && !ac.PainkillersInEffect(handLeftPainLevel))) return false;
            else if (armRightPainDifference > 30 && handRightPainDifference > 30 && (!ac.PainkillersInEffect(armRightPainLevel) && !ac.PainkillersInEffect(handRightPainLevel))) return false;
            else if ((armLeftPainDifference > 30 && armRightPainDifference > 30) && !ac.PainkillersInEffect(armRightPainLevel) && !ac.PainkillersInEffect(armLeftPainLevel)) return false;

            return true;
        }
        
        public bool HasConcussionInEffect()
        {
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            PainAffliction concussion = GetConcussion();

            if ((concussion.m_PainLevel / concussion.m_StartingPainLevel) * 100 < 30) return false;

            if (ac.PainkillersInEffect(concussion.m_PainLevel)) return false;
            else return true;

        }
        public PainAffliction GetConcussion()
        {
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            foreach(var p in ac.m_PainInstances)
            {
                if (p.m_Cause.ToLowerInvariant().Contains("concussion") || p.m_Cause.ToLowerInvariant().Contains("trauma")) return p;
            }

            return null;

        }
        public void MaybeConcuss(float chance)
        {
            GearItem hardHat = GameManager.GetInventoryComponent().GearInInventory("GEAR_MinersHelmet", 1);

            if (hardHat != null)
            {
                if (hardHat.m_ClothingItem.IsWearing())
                {
                    chance /= 2;
                }
            }

            if (Il2Cpp.Utils.RollChance(chance))
            {

                GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.Head, "concussion", AfflictionOptions.PlayFX | AfflictionOptions.DoAutoSave | AfflictionOptions.DisplayIcon);
                GameManager.GetCameraEffects().PainPulse(1f);
            }

        }
        public PainAffliction GetBrokenRibPain(ref int index)
        {

            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                if (inst.m_Cause == "broken rib")
                {

                    PainAffliction pain = ac.GetPainInstance(i);

                    index = i;

                    return pain;
                }
            }
            return null;

        }
        public void EndBrokenRibPain()
        {

            SprainPain painManager = GameManager.GetSprainPainComponent();


            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                if (inst.m_Cause == "broken rib")
                {
                    painManager.CureAffliction(inst);
                }
            }

        }
        public string GetAfflictionDescription(string name)
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
