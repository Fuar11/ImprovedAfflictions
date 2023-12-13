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

        public bool m_PainEffectsCheck;
        public float m_PainkillerStandardAmount = 20f;

        public float m_SecondsSinceLastODFx;
        public float m_ODPulseFxFrequencySeconds = 4f;
        public float m_ODPulseFxIntensity = 2f; //default, might change

        public float m_ConcussionDrugLevel;
        public float m_InsomniaDrugLevel;

        public float m_PainkillerIncrementAmount;
        public float m_PainkillerDecrementStartingAmount;

        public bool m_HasConcussion;

        public void Start()
        {

            LoadData();
            m_PainEffectsCheck = false;

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
                m_SecondsSinceLastODFx += Time.deltaTime;
                IncrementPainkillerLevel(tODMinutes / 20);
                MaybeDoOverdoseEffects();
                m_PainEffectsCheck = false;
            }
            else if (IsOnPainkillers())
            {
                m_PainkillerIncrementAmount = 0;

                if (!m_PainEffectsCheck)
                {
                    ph.UpdatePainEffects();
                    m_PainEffectsCheck = true;
                }

                m_SecondsSinceLastODFx += Time.deltaTime;
                MaybeDoOverdoseEffects();
                DecrementPainkillerLevel(tODHours);

            }
            else 
            { 
                m_PainkillerDecrementStartingAmount = 0;

                //reset back to normal values
                GameManager.GetCameraStatusEffects().m_HeadacheSinSpeed = 3;
                GameManager.GetCameraStatusEffects().m_HeadacheVignetteIntensity = 0.3f;
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
                      
            m_PainkillerLevel -= (m_PainkillerDecrementStartingAmount / (m_PainkillerDecrementStartingAmount / 2)) * numHoursDelta;

            m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);

        }

        public bool PainkillersInEffect()
        {
            return m_PainkillerLevel >= m_PainLevel;
        }

        public bool PainkillersInEffect(float num, bool forIndex = false)
        {

            //if forIndex is true then the value passed is being used as index and not value to check for. 
            if (forIndex)
            {
                return m_PainkillerLevel >= m_PainInstances[(int)num].m_PainLevel;
            }

            return m_PainkillerLevel >= num;
        }

        public bool IsOnPainkillers()
        {
            return m_PainkillerLevel > 0;
        }

        public bool IsHighOnPainkillers()
        {
            return m_PainkillerLevel > 60f;
        }
        public bool IsOverdosing()
        {
            return m_PainkillerLevel > 80f;
        }
        public float GetPainkillerLevel()
        {
            return m_PainkillerLevel;
        }

        public void AdministerPainkillers(float amount, bool instant = false)
        {

            if (instant)
            {
                m_PainkillerLevel += amount;
                m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);
                return;
            }

            m_PainkillerIncrementAmount += m_PainkillerIncrementAmount != 0 ? amount : m_PainkillerLevel + m_PainkillerStandardAmount;
            m_PainkillerDecrementStartingAmount = m_PainkillerIncrementAmount;
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

        public PainAffliction? GetPainInstanceAtLocation(AfflictionBodyArea location)
        {
            return (m_PainInstances.Count == 0) ? null : m_PainInstances.Find(p => p.m_Location == location);
        }

        public PainAffliction? GetPainInstanceAtLocationWithCause(AfflictionBodyArea location, string cause)
        {
            return (m_PainInstances.Count == 0) ? null : m_PainInstances.Find(p => p.m_Location == location && p.m_Cause == cause);
        }


        public float GetTotalPainLevel()
        {
            if (m_PainInstances.Count == 0) return 0;

            float total = 0;

            foreach (var pain in m_PainInstances)
            {
                total += pain.m_PainLevel;
            }

            return total;
        }
        public float GetTotalPainLevelForPainAtLocations(AfflictionBodyArea[] locations, bool checkForStartingLevel = false)
        {
            if (m_PainInstances.Count == 0) return 0;

            float total = 0;

            foreach (var pain in m_PainInstances)
            {
                if(locations.Contains(pain.m_Location)) total += checkForStartingLevel ? pain.m_StartingPainLevel : pain.m_PainLevel;
            }

            return total;

        }

        public void MaybeDoOverdoseEffects()
        {

            PainEffects effects = new PainEffects();

            if (IsHighOnPainkillers())
            {

                AddOverdoseFatigueDebuff();

                if(m_SecondsSinceLastODFx > m_ODPulseFxFrequencySeconds)
                {
                    effects.OverdoseVignette(m_ODPulseFxIntensity);
                    m_SecondsSinceLastODFx = 0;
                }
            }

        }

        public void AddOverdoseFatigueDebuff()
        {
            int decreaseMultiplier = IsOverdosing() ? 45 : 30;
            float fatigueValue = decreaseMultiplier * GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);

            GameManager.GetFatigueComponent().AddFatigue(fatigueValue);

        }


    }
}
