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
        public float m_PainStartingLevel = 0;
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

            if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended) return;

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

                if (!m_PainEffectsCheck)
                {
                    ph.UpdatePainEffects();
                    m_PainEffectsCheck = true;
                }

                //reset back to normal values
                GameManager.GetCameraStatusEffects().m_HeadacheSinSpeed = 3;
                GameManager.GetCameraStatusEffects().m_HeadacheVignetteIntensity = 0.3f;
            }



        }

        //DATA
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
                    m_PainStartingLevel = sdp.m_PainStartingLevel;
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

            AfflictionComponentSaveDataProxy sdp = new AfflictionComponentSaveDataProxy(m_PainInstances, m_PainLevel, m_PainStartingLevel, m_PainkillerLevel, m_ConcussionDrugLevel, m_InsomniaDrugLevel, m_PainkillerIncrementAmount, m_PainkillerDecrementStartingAmount, m_HasConcussion);

            string data = JsonSerializer.Serialize<AfflictionComponentSaveDataProxy>(sdp);

            sdm.Save(data, "component");

        }

        //PAINKILLERS
        public void IncrementPainkillerLevel(float numMinutesDelta)
        {

            m_PainkillerLevel += m_PainkillerIncrementAmount * numMinutesDelta;

            m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);

        }

        public void DecrementPainkillerLevel(float numHoursDelta)
        {
                      
            m_PainkillerLevel -= (m_PainkillerDecrementStartingAmount / (m_PainkillerDecrementStartingAmount / 2)) * numHoursDelta;

            m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);

            if (m_PainkillerLevel == 0) m_PainEffectsCheck = false;
        }

        public bool PainkillersInEffect()
        {
            return m_PainkillerLevel >= m_PainLevel;
        }

        public bool PainkillersInEffect(float num, bool forIndex = false)
        {

            if (num == 0) return false;

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

        public string GetPainkillerLevelPercent()
        {
            string result = Mathf.Clamp(Mathf.FloorToInt(m_PainkillerLevel), 0, 100).ToString();
           
            return result + "%";
        }

        public void AdministerPainkillers(float amount, bool instant = false)
        {
            if (instant)
            {
                m_PainkillerLevel += amount;
                m_PainkillerLevel = Mathf.Clamp(m_PainkillerLevel, 0f, 100f);

                if(m_PainkillerIncrementAmount != 0)
                {
                    m_PainkillerIncrementAmount += m_PainkillerLevel;
                    m_PainkillerIncrementAmount = Mathf.Clamp(m_PainkillerIncrementAmount, 0f, 100f);
                }

                m_PainkillerDecrementStartingAmount += m_PainkillerLevel;
                m_PainkillerDecrementStartingAmount = Mathf.Clamp(m_PainkillerDecrementStartingAmount, 0f, 100f);
                m_PainEffectsCheck = false;
                return;
            }

            m_PainkillerIncrementAmount += m_PainkillerIncrementAmount != 0 ? amount : m_PainkillerLevel + amount;
            m_PainkillerIncrementAmount = Mathf.Clamp(m_PainkillerIncrementAmount, 0f, 100f);
            m_PainkillerDecrementStartingAmount = m_PainkillerIncrementAmount;
        }

        //PAIN
        public void AddPainInstance(string cause, AfflictionBodyArea location, float duration, float maxDuration, float painLevel, float pulseFxIntensity, float pulseFxFrequencySeconds)
        {

            PainAffliction newPain = new PainAffliction();
            newPain.m_Cause = cause;
            newPain.m_Location = location;
            newPain.m_EndTime = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime) + duration;
            newPain.m_MaxDuration = maxDuration;
            newPain.m_PainLevel = painLevel;
            newPain.m_StartingPainLevel = painLevel;
            newPain.m_PulseFxIntensity = pulseFxIntensity;
            newPain.m_PulseFxFrequencySeconds = pulseFxFrequencySeconds;
            newPain.m_PulseFxMaxDuration = duration;

            m_PainStartingLevel += newPain.m_PainLevel;

            m_PainInstances.Add(newPain);
        }

        public void UpdatePainInstance(int index, PainAffliction pi)
        {
            m_PainStartingLevel -= m_PainInstances[index].m_PainLevel;
            m_PainInstances[index] = pi;
            m_PainStartingLevel += pi.m_PainLevel;
        }
        public void CurePainInstance(int index)
        {
            m_PainInstances.RemoveAt(index);
            m_PainStartingLevel = GetTotalPainStartingLevel();
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
        public float GetTotalPainStartingLevel()
        {
            if (m_PainInstances.Count == 0) return 0;

            float total = 0;

            foreach (var pain in m_PainInstances)
            {
                total += pain.m_StartingPainLevel;
            }

            return total;
        }

        //OVERDOSE
        public void MaybeDoOverdoseEffects()
        {

            PainEffects effects = new PainEffects();

            if (IsHighOnPainkillers())
            {

                ApplyPainkillerDamage();

                if(m_SecondsSinceLastODFx > m_ODPulseFxFrequencySeconds)
                {
                    effects.OverdoseVignette(m_ODPulseFxIntensity);
                    m_SecondsSinceLastODFx = 0;
                }

                Condition cond = GameManager.GetConditionComponent();

                if (IsOverdosing())
                {
                    float intensityFactor = 9f;

                    if (m_PainkillerLevel > 95f) intensityFactor = 8f;

                    float blurFrac = 1f - Mathf.Clamp01((intensityFactor - 0) / (cond.m_HPToStartBlur - cond.m_HPForMaxBlur));
                    Vector3 velocity = GameManager.GetVpFPSPlayer().Controller.Velocity;
                    float speedFrac = 1f - Mathf.Clamp01(velocity.magnitude / cond.m_MaxVelocityForSpeedFracCalc);

                    cond.ApplyLowHealthStagger(blurFrac, speedFrac);

                }
                else
                {

                    if(GameManager.GetVpFPSCamera().m_BasePitch != 0f &&
                    GameManager.GetVpFPSCamera().m_BaseRoll != 0f)
                    {
                        cond.ResetLowHealthPitchAndRoll();
                    }
                }

            }

        }

        public void ApplyPainkillerDamage()
        {

            float fatigueIncreasePerHour = IsOverdosing() ? 45 : 30;
            float conditionDrainPerHour = IsOverdosing() ? 5f : 1f;

            float val = conditionDrainPerHour * GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
            GameManager.GetConditionComponent().AddHealth(0f - val, DamageSource.Player);

            if (!GameManager.GetPlayerManagerComponent().PlayerIsSleeping())
            {
                float fatigueValue = fatigueIncreasePerHour * GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
                GameManager.GetFatigueComponent().AddFatigue(fatigueValue);
            }



        }


    }
}
