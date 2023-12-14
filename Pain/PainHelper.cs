using Il2Cpp;
using Random = UnityEngine.Random;
using HarmonyLib;
using static Il2Cpp.Panel_Debug;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.Pain.Component;
using UnityEngine;
using MelonLoader;

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
            float newDuration = Random.Range(instanceToUpdate.m_PulseFxMaxDuration, 240f);
            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            SprainPain.Instance newInstance = new SprainPain.Instance();
            newInstance.m_Location = painManager.m_ActiveInstances[index].m_Location;
            newInstance.m_Cause = painManager.m_ActiveInstances[index].m_Cause;
            newInstance.m_EndTime = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + newDuration;

            painManager.m_ActiveInstances[index] = newInstance;

            instanceToUpdate.m_PulseFxMaxDuration = newDuration;

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


            AfflictionBodyArea[] hands = { AfflictionBodyArea.HandLeft, AfflictionBodyArea.HandRight };

            //just hands
            float handsPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands);
            float handsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(hands, true);
            float handsPainDifference = (handsPainLevel / handsStartingPainLevel) * 100;
            
            AfflictionBodyArea[] rLimbs = { AfflictionBodyArea.ArmRight, AfflictionBodyArea.HandRight };

            //right arm and hand
            float rLimbsPainLevel = ac.GetTotalPainLevelForPainAtLocations(rLimbs);
            float rLimbsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(rLimbs, true);
            float rLimbsPainDifference = (rLimbsPainLevel / rLimbsStartingPainLevel) * 100;

            AfflictionBodyArea[] lLimbs = { AfflictionBodyArea.ArmLeft, AfflictionBodyArea.HandLeft };

            //left arm and hand
            float lLimbsPainLevel = ac.GetTotalPainLevelForPainAtLocations(lLimbs);
            float lLimbsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(lLimbs, true);
            float lLimbsPainDifference = (lLimbsPainLevel / lLimbsStartingPainLevel) * 100;

            AfflictionBodyArea[] arms = { AfflictionBodyArea.ArmLeft, AfflictionBodyArea.ArmRight };

            //just arms
            float armsPainLevel = ac.GetTotalPainLevelForPainAtLocations(arms);
            float armsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(arms, true);
            float armsPainDifference = (armsPainLevel / armsStartingPainLevel) * 100;

            AfflictionBodyArea[] legs = { AfflictionBodyArea.LegLeft, AfflictionBodyArea.LegRight };

            //just arms
            float legsPainLevel = ac.GetTotalPainLevelForPainAtLocations(legs);
            float legsStartingPainLevel = ac.GetTotalPainLevelForPainAtLocations(legs, true);
            float legsPainDifference = (legsPainLevel / legsStartingPainLevel) * 100;

            
            if (handsPainDifference > 30 && !ac.PainkillersInEffect(handsPainLevel)) return false;
            else if (lLimbsPainDifference > 30 && !ac.PainkillersInEffect(lLimbsPainLevel)) return false;
            else if (rLimbsPainDifference > 30 && !ac.PainkillersInEffect(rLimbsPainLevel)) return false;
            else if (armsPainDifference > 30 && !ac.PainkillersInEffect(armsPainLevel)) return false;

            if (legsPainDifference > 30 && !ac.PainkillersInEffect(legsPainLevel)) return false;

            return true;
        }

        public bool HasConcussion()
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            foreach (SprainPain.Instance inst in painManager.m_ActiveInstances)
            {
                if (inst.m_Cause.ToLowerInvariant() == "concussion")
                {
                    return true;
                }
            }
            return false;
        }

        public PainAffliction GetConcussion()
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                if (inst.m_Cause == "concussion")
                {

                    PainAffliction pain = ac.GetPainInstance(i);

                    return pain;
                }
            }
            return null;

        }

        public void MaybeConcuss()
        {
            if (Il2Cpp.Utils.RollChance(60f))
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
