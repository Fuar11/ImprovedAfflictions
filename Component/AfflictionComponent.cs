using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using ImprovedAfflictions.Utils;
using ImprovedAfflictions.Component;
using System.Text.Json;
using MelonLoader;

namespace ImprovedAfflictions.Pain.Component
{
    [RegisterTypeInIl2Cpp]
    internal class AfflictionComponent : MonoBehaviour
    {

        public List<PainAffliction> m_PainInstances = new List<PainAffliction>();

        public float m_PainLevel = 0;
        public float m_PainkillerLevel = 0;

        public bool m_PainkillerEffectsCheck;

        public float m_ConcussionDrugLevel;
        public float m_InsomniaDrugLevel;

        public float m_PainkillerIncrementAmount;
        public float m_PainkillerDecrementStartingAmount;

        public bool m_HasConcussion;

        public void Start()
        {

            LoadData();
            m_PainkillerEffectsCheck = false;

        }

        public void Update()
        {

            float tODMinutes = GameManager.GetTimeOfDayComponent().GetTODMinutes(Time.deltaTime);
            float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
            PainHelper ph = new PainHelper();

            foreach(var pain in m_PainInstances)
            {
                pain.DecreasePainLevel(tODHours);
            }

            //constantly changing
            m_PainLevel = GetTotalPainLevel();

            if (m_PainkillerLevel < m_PainkillerIncrementAmount)
            {
                IncrementPainkillerLevel(tODMinutes / 20);
                m_PainkillerEffectsCheck = false;
            }
            else
            {
                m_PainkillerDecrementStartingAmount = m_PainkillerIncrementAmount;
                m_PainkillerIncrementAmount = 0;

                if (!m_PainkillerEffectsCheck)
                {
                    ph.UpdatePainEffects();
                    m_PainkillerEffectsCheck = true;
                }
                
                DecrementPainkillerLevel(tODHours);

            }


        }

        public void LoadData()
        {
            SaveDataManager sdm = Implementation.sdm;

            string? data = sdm.LoadData("component");

            if (data is not null)
            {

                AfflictionComponentSaveDataProxy? sdp = JsonSerializer.Deserialize<AfflictionComponentSaveDataProxy>(data);

            

                if (sdp is not null)
                {
                    m_PainInstances = sdp.m_PainInstances;
                    m_PainLevel = sdp.m_PainLevel;
                    m_PainkillerLevel = sdp.m_PainkillerLevel;
                    m_HasConcussion = sdp.m_HasConcussion;
                    m_InsomniaDrugLevel = sdp.m_InsomniaDrugLevel;
                    m_ConcussionDrugLevel = sdp.m_ConcussionDrugLevel;
                    m_PainkillerIncrementAmount = sdp.m_PainkillerIncrementAmount;
                    m_PainkillerDecrementStartingAmount = sdp.m_PainkillerDecrementStartingAmount;
                }
            }
        }

        public void SaveData()
        {

            SaveDataManager sdm = Implementation.sdm;

            AfflictionComponentSaveDataProxy sdp = new AfflictionComponentSaveDataProxy(m_PainInstances, m_PainLevel, m_PainkillerLevel, m_ConcussionDrugLevel, m_InsomniaDrugLevel, m_PainkillerIncrementAmount, m_PainkillerDecrementStartingAmount, m_HasConcussion);

            string data = JsonSerializer.Serialize<AfflictionComponentSaveDataProxy>(sdp);

            sdm.Save(data, "component");

        }
        public void IncrementPainkillerLevel(float numMinutesDelta)
        {

            m_PainkillerLevel += m_PainkillerIncrementAmount * numMinutesDelta;

            m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);

        }

        public void DecrementPainkillerLevel(float numHoursDelta)
        {
            m_PainkillerLevel -= 1f * numHoursDelta;

            m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);

        }

        public bool PainkillersInEffect()
        {
            return m_PainkillerLevel >= m_PainLevel;
        }

        public bool IsOnPainkillers()
        {
            return m_PainkillerLevel > 0;
        }

        public float GetPainkillerLevel()
        {
            return m_PainkillerLevel;
        }

        public void AddPainInstance(string cause, AfflictionBodyArea location, float duration, float painLevel, float pulseFxIntensity, float pulseFxFrequencySeconds)
        {
            PainAffliction newPain = new PainAffliction();
            newPain.m_Cause = cause;
            newPain.m_Location = location;
            newPain.m_EndTime = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime) + duration;
            newPain.m_PainLevel = painLevel;
            newPain.m_StartingPainLevel = painLevel;
            newPain.m_PulseFxIntensity = pulseFxIntensity;
            newPain.m_PulseFxFrequencySeconds = pulseFxFrequencySeconds;
            newPain.m_PulseFxMaxDuration = duration;

            m_PainInstances.Add(newPain);
        }

        public PainAffliction GetPainInstance(int index)
        {
            return m_PainInstances[index];
        }

        public float GetTotalPainLevel()
        {
            if (m_PainInstances.Count == 0) return 0;

            float total = 0;

            foreach(var pain in m_PainInstances)
            {
                total += pain.m_PainLevel;
            }

            return total;
        }


    }
}
