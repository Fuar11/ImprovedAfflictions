﻿using Il2Cpp;
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

namespace ImprovedAfflictions.Utils
{
    internal class PainHelper
    {

        [HarmonyPatch(typeof(QualitySettingsManager), nameof(QualitySettingsManager.ApplyCurrentQualitySettings))]

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
                string data = sdm.LoadPainData(num.ToString());

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

            string data2 = sdm.LoadPainData("painkillers");

            if (data2 == null) return;

            PainkillerSaveDataProxy? painkillerData = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data2);


            //if painkillers have been taken, dull the pain effects
            if (painkillerData != null && painkillerData.m_RemedyApplied)
            {
                MelonLogger.Msg("Dulling pain: {0}", painManager.m_PulseFxIntensity);
                painManager.m_PulseFxIntensity /= 1.5f;
                MelonLogger.Msg("Dulled pain to: {0}", painManager.m_PulseFxIntensity);
            }


        }

        public void WareOffPainkillers()
        {

            SaveDataManager sdm = Implementation.sdm;

            var data = sdm.LoadPainData("painkillers");

            if (data == null)
            {
                MelonLogger.Error("Unable to ware off painkillers since data cannot be retrieved from Mod Data file");
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

            var data = sdm.LoadPainData("painkillers");

            if (data == null)
            {
                MelonLogger.Error("Unable to ware off painkillers since data cannot be retrieved from Mod Data file");
                return;
            }

            PainkillerSaveDataProxy? painkillerData = JsonSerializer.Deserialize<PainkillerSaveDataProxy>(data);

            if(painkillerData == null || painkillerData.m_RemedyApplied == true) return;

            painkillerData.m_RemedyApplied = true;


            string dataToSave = JsonSerializer.Serialize(painkillerData);
            sdm.Save(dataToSave, "painkillers");

            //update pain effects when painkillers are taken
            PainHelper ph = new PainHelper();
            ph.UpdatePainEffects();

        }

        public bool HasPainAtLocation(AfflictionBodyArea location)
        {
            foreach(SprainPain.Instance pain in GameManager.GetSprainPainComponent().m_ActiveInstances)
            {
                if (pain.m_Location == location) return true;
            }
            return false;
        }

        public bool HasConcussion()
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            foreach (SprainPain.Instance inst in painManager.m_ActiveInstances)
            {
                if (inst.m_Cause.ToLowerInvariant() == "concussion")
                {
                    MelonLogger.Msg("Has concussion!");
                    return true;
                }
            }
            return false;
        }

        public void MaybeConcuss()
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            SaveDataManager sdm = Implementation.sdm;

            if (HasConcussion())
            {
                if (Il2Cpp.Utils.RollChance(40f))
                {
                    for (int i = 0; i < painManager.m_ActiveInstances.Count; i++)
                    {
                        SprainPain.Instance inst = painManager.m_ActiveInstances[i];

                        if (inst.m_Location == AfflictionBodyArea.Head)
                        {
                            if (inst.m_Cause.ToLowerInvariant() == "concussion")
                            {
                                string data = sdm.LoadPainData(i.ToString());
                                PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

                                if (pain == null) return;

                                //if player already has concussion and a new one is triggered, simply reset the timer of the existing one and remove painkiller effect
                                float newDuration = Random.Range(pain.m_PulseFxMaxDuration, 240f);

                                inst.m_EndTime = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + newDuration;
                                GameManager.GetCameraEffects().PainPulse(1f);

                                pain.m_PulseFxMaxDuration = newDuration;
                                pain.m_PulseFxIntensity += 0.2f;

                                data = JsonSerializer.Serialize(pain);
                                sdm.Save(data, i.ToString());

                                PainkillerSaveDataProxy? painkiller = new PainkillerSaveDataProxy();
                                painkiller.m_RemedyApplied = false;

                                string data2 = JsonSerializer.Serialize(painkiller);
                                sdm.Save(data2, "painkillers");
                            }
                        }
                    }
                }
            }
            else
            {
                if (Il2Cpp.Utils.RollChance(60f))
                {
                    GameManager.GetSprainPainComponent().ApplyAffliction(AfflictionBodyArea.Head, "concussion", AfflictionOptions.PlayFX | AfflictionOptions.DoAutoSave | AfflictionOptions.DisplayIcon);
                    GameManager.GetCameraEffects().PainPulse(1f);
                }
            }
        }

    }
}
