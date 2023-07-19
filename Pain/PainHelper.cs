using Il2Cpp;
using Il2Cppgw.proto.utils;
using Il2CppSystem.Data;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using HarmonyLib;
using Il2CppSWS;
using Il2CppRewired.Utils.Platforms.Windows;
using ImprovedAfflictions.Utils;
using static Il2Cpp.Panel_Debug;

namespace ImprovedAfflictions.Pain
{
    internal class PainHelper
    {

        [HarmonyPatch(typeof(SprainPain), nameof(SprainPain.Deserialize))]

        public class UpdatePainEffectsOnLoad
        {
            public static void Postfix()
            {
                PainHelper ph = new PainHelper();
                ph.UpdatePainEffects();
            }
        }

        public void UpdatePainEffects()
        {

            SprainPain painManager = GameManager.GetSprainPainComponent();

            SaveDataManager sdm = Implementation.sdm;
            float maxIntensity = 0f;

            for (int num = painManager.m_ActiveInstances.Count - 1; num >= 0; num--)
            {
                string data = sdm.LoadData(num.ToString());

                if (data == null)
                {
                    continue;
                }

                PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

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

            //if painkillers have been taken, dull the pain effects
            if (IsOnPainkillers())
            {
                painManager.m_PulseFxIntensity *= 0.5f;
            }
        }

        public PainSaveDataProxy GetPainInstance(AfflictionBodyArea location, ref int index)
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            SaveDataManager sdm = Implementation.sdm;

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];


                if (inst.m_Location == location)
                {

                    if (inst.m_Cause == "concussion") continue;

                    string data = sdm.LoadData(i.ToString());
                    PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

                    index = i;

                    if (pain == null) return null;
                    else return pain;
                    
                }
            }
            return null;
        }

       
        public void UpdatePainInstance(int index, PainSaveDataProxy instanceToUpdate)
        {
            float newDuration = Random.Range(instanceToUpdate.m_PulseFxMaxDuration, 240f);

            SprainPain painManager = GameManager.GetSprainPainComponent();
            painManager.m_ActiveInstances[index].m_EndTime = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + newDuration;
            instanceToUpdate.m_PulseFxMaxDuration = newDuration;

            SaveDataManager sdm = Implementation.sdm;
            string dataToSave = JsonSerializer.Serialize(instanceToUpdate);
            sdm.Save(dataToSave, index.ToString());
        }

        
        public void WareOffPainkillers()
        {
            SaveDataManager sdm = Implementation.sdm;

            var data = sdm.LoadData("painkillers");

            if (data == null)
            {
                return;
            }

            PainkillerSaveDataProxy? painkillerData = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);

            if (painkillerData == null || painkillerData.m_RemedyApplied == false) return;

            PainkillerSaveDataProxy painToSave = painkillerData;
            painToSave.m_RemedyApplied = false;

            var dataToSave = JsonSerializer.Serialize(painToSave);

            sdm.Save(dataToSave, "painkillers");

            UpdatePainEffects();

        }

        public void TakeEffectPainkillers()
        {

            SaveDataManager sdm = Implementation.sdm;

            var data = sdm.LoadData("painkillers");

            if (data == null)
            {
                return;
            }

            PainkillerSaveDataProxy? painkillerData = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);

            if (painkillerData == null || painkillerData.m_RemedyApplied == true) return;

            painkillerData.m_RemedyApplied = true;


            string dataToSave = JsonSerializer.Serialize(painkillerData);
            sdm.Save(dataToSave, "painkillers");

            //update pain effects when painkillers are taken
            PainHelper ph = new PainHelper();
            ph.UpdatePainEffects();

        }

        public bool HasPain()
        {
            if (GameManager.GetSprainPainComponent().HasSprainPain()) return true;
            return false;
        }
        public bool IsOnPainkillers()
        {

            if (GameManager.m_ActiveScene.ToLowerInvariant().Contains("menu")) return false;

            SaveDataManager sdm = Implementation.sdm;

            var data = sdm.LoadData("painkillers");

            if (data == null)
            {
                return false;
            }

            PainkillerSaveDataProxy? painkillerData = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);

            if (painkillerData == null || painkillerData.m_RemedyApplied) return true;

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

        public PainSaveDataProxy GetConcussion()
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            SaveDataManager sdm = Implementation.sdm;

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                if (inst.m_Cause == "concussion")
                {
                    string data = sdm.LoadData(i.ToString());
                    PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

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

        public PainSaveDataProxy GetBrokenRibPain(ref int index)
        {

            SprainPain painManager = GameManager.GetSprainPainComponent();
            SaveDataManager sdm = Implementation.sdm;

            for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
            {
                SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                if (inst.m_Cause == "broken rib")
                {
                    string data = sdm.LoadData(i.ToString());
                    PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

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
