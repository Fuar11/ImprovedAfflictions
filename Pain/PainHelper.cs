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

        //may no longer be needed
        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.Deserialize))]

        public class UpdatePainEffectsOnLoad
        {
            public static void Postfix()
            {
               //PainHelper ph = new PainHelper();
               //ph.UpdatePainEffects();
            }
        }


        public void UpdatePainEffects()
        {

            if (GameManager.m_ActiveScene.ToLowerInvariant().Contains("menu")) return;

            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            float maxIntensity = 0f;

            for (int num = painManager.m_ActiveInstances.Count - 1; num >= 0; num--)
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
           
            painManager.m_PulseFxIntensity /= ac.GetPainkillerLevel() / 10;
            
        }

        public PainAffliction GetPainInstance(AfflictionBodyArea location, ref int index)
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            AfflictionComponent ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>();

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];


                if (inst.m_Location == location)
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

            ac.m_PainInstances[index] = instanceToUpdate;
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

        
        public bool HasPainAtLocation(AfflictionBodyArea location)
        {
            foreach (SprainPain.Instance pain in GameManager.GetSprainPainComponent().m_ActiveInstances)
            {
                if (pain.m_Location == location) return true;
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

        public bool HasPain()
        {
            if (GameManager.GetSprainPainComponent().HasSprainPain()) return true;
            return false;
        }
        public bool CanClimbRope()
        {
            if (IsOnPainkillers())
            {
                return true;
            }
            if (HasPainAtLocation(AfflictionBodyArea.HandLeft) && HasPainAtLocation(AfflictionBodyArea.HandRight)) return false;
            else if (HasPainAtLocation(AfflictionBodyArea.HandLeft) && HasPainAtLocation(AfflictionBodyArea.ArmLeft)) return false;
            else if (HasPainAtLocation(AfflictionBodyArea.HandRight) && HasPainAtLocation(AfflictionBodyArea.ArmRight)) return false;
            else if (HasPainAtLocation(AfflictionBodyArea.ArmLeft) && HasPainAtLocation(AfflictionBodyArea.ArmRight)) return false;

            if (HasPainAtLocation(AfflictionBodyArea.FootLeft) && HasPainAtLocation(AfflictionBodyArea.FootLeft)) return false;

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

    }
}
