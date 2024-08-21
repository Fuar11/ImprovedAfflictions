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
using ImprovedAfflictions.CustomAfflictions;
using ImprovedAfflictions.Pain;
using AfflictionComponent.Components;
using Newtonsoft.Json;
using MelonLoader;
using Random = UnityEngine.Random;

namespace ImprovedAfflictions.Component
{
    [RegisterTypeInIl2Cpp]
    internal class PainManager : MonoBehaviour
    {
        private PainEffects m_PainEffects = new PainEffects();
        private AfflictionHelper m_PainHelper = new AfflictionHelper();

        public float m_TotalPainLevel;
        public float m_PainStartingLevel = 0;

        public float m_PainkillerLevel = 0f;
        public float m_PainkillerIncrementAmount;
        public float m_PainkillerDecrementStartingAmount;

        public float m_SecondsSinceLastODFx;
        public float m_ODPulseFxFrequencySeconds = 4f;
        public float m_ODPulseFxIntensity = 2f;

        public float m_SecondsSinceLastPulseFx;
        public float m_PulseFxFrequencySeconds;
        public float m_PulseFxIntensity;

        private bool m_PainEffectsCheck = false;
        private string m_PulseFxWwiseRtpcName;

        public AfflictionManager am = GameObject.Find("AfflictionManager").GetComponent<AfflictionManager>();

        public void Start()
        {
            LoadData();
        }

        public void Update()
        {

            if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended) return;

            float tODMinutes = GameManager.GetTimeOfDayComponent().GetTODMinutes(Time.deltaTime);
            float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
            float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();

            //constantly changing
            m_TotalPainLevel = GetTotalPainLevel();

            MaybeApplyOverdoseOrRisk();

            if (m_PainkillerLevel < m_PainkillerIncrementAmount)
            {
                m_SecondsSinceLastODFx += Time.deltaTime;
                IncrementPainkillerLevel(tODMinutes / 20);
                //MaybeDoOverdoseEffects();
                m_PainEffectsCheck = false;
            }
            else if (IsOnPainkillers())
            {
                m_PainkillerIncrementAmount = 0;

                if (!m_PainEffectsCheck)
                {
                    PainEffects.UpdatePainEffects();
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
                    PainEffects.UpdatePainEffects();
                    m_PainEffectsCheck = true;
                }

                //reset back to normal values
                GameManager.GetCameraStatusEffects().m_HeadacheSinSpeed = 3;
                GameManager.GetCameraStatusEffects().m_HeadacheVignetteIntensity = 0.3f;
            }

            MaybeDoPainEffects();

            

        }

        //DATA
        public void LoadData()
        {

            string? data = Mod.sdm.LoadData("component");

            if (data is not null)
            {
                PainManagerSaveDataProxy? ldp = JsonConvert.DeserializeObject<PainManagerSaveDataProxy>(data);

                if(ldp is not null)
                {
                    m_PainkillerLevel = ldp.m_PainkillerLevel;
                    m_PainkillerIncrementAmount = ldp.m_PainkillerIncrementAmount;
                    m_PainkillerDecrementStartingAmount = ldp.m_PainkillerDecrementStartingAmount;
                    m_SecondsSinceLastODFx = ldp.m_SecondsSinceLastODFx;
                    m_SecondsSinceLastPulseFx = ldp.m_SecondsSinceLastPulseFx;
                    m_PulseFxFrequencySeconds = ldp.m_PulseFxFrequencySeconds;
                    m_PulseFxIntensity = ldp.m_PulseFxIntensity;
                }
                else
                {
                    Mod.Logger.Log("Pain manager load data is null", ComplexLogger.FlaggedLoggingLevel.Debug);
                }

            }
        }

        public void SaveData()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            Mod.Logger.Log("Saving pain manager data", ComplexLogger.FlaggedLoggingLevel.Debug);
            PainManagerSaveDataProxy dataToSave = new PainManagerSaveDataProxy(m_PainkillerLevel, m_PainkillerIncrementAmount, m_PainkillerDecrementStartingAmount, m_SecondsSinceLastODFx, m_SecondsSinceLastPulseFx, m_PulseFxFrequencySeconds, m_PulseFxIntensity);
            string json = JsonConvert.SerializeObject(dataToSave, settings);
            if (json is null)
            {
                Mod.Logger.Log("JSON DATA IS NULL", ComplexLogger.FlaggedLoggingLevel.Error);
                return;
            }
            Mod.sdm.Save(json, "component");
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
            return m_PainkillerLevel >= m_TotalPainLevel;
        }

        public bool PainkillersInEffect(float num, bool forIndex = false)
        {
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

        public float GetTotalPainLevel()
        {
            if (am.m_Afflictions.Count == 0) return 0;

            float total = 0;

            foreach (CustomPainAffliction aff in am.m_Afflictions.OfType<CustomPainAffliction>())
            {
                total += aff.m_PainLevel;
            }

            return total;
        }
        public float GetTotalPainLevelForPainAtLocations(AfflictionBodyArea[] locations, bool checkForStartingLevel = false)
        {
            if (am.m_Afflictions.Count == 0) return 0;

            float total = 0;

            foreach (CustomPainAffliction aff in am.m_Afflictions.OfType<CustomPainAffliction>())
            {
                if (locations.Contains(aff.m_Location)) total += checkForStartingLevel ? aff.m_StartingPainLevel : aff.m_PainLevel;
            }

            return total;

        }
        public float GetTotalPainStartingLevel()
        {

            if (am.m_Afflictions.Count == 0) return 0;

            float total = 0;

            foreach (CustomPainAffliction aff in am.m_Afflictions.OfType<CustomPainAffliction>())
            {
                total += aff.m_StartingPainLevel;
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
                    PainEffects.OverdoseVignette(m_ODPulseFxIntensity);
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

        
        public void MaybeDoPainEffects()
        {
            //overall pain level is not less than 20 percent of the most recent highest pain level
            if ((m_TotalPainLevel / m_PainStartingLevel) * 100 > 20)
            {
                m_SecondsSinceLastPulseFx += Time.deltaTime;
                if (m_SecondsSinceLastPulseFx > m_PulseFxFrequencySeconds)
                {
                    if (Concussion.Concussion.HasConcussion(true))
                    {
                        PainEffects.HeadTraumaPulse(m_PulseFxIntensity);
                    }
                    else if (m_PulseFxIntensity > 1f)
                    {
                        PainEffects.IntensePainPulse(m_PulseFxIntensity);
                    }
                    else
                    {
                        GameManager.GetCameraEffects().SprainPulse(m_PulseFxIntensity);
                    }

                    //random variation between pain pulses
                    //__instance.m_PulseFxFrequencySeconds = Random.Range(3f, __instance.m_PulseFxFrequencySeconds + 5f);
                    m_SecondsSinceLastPulseFx = 0f;
                }
            }
            else
            {
                GameManager.GetCameraEffects().SprainPulse(0f);
                m_SecondsSinceLastPulseFx = 0f;
            }
            if (!string.IsNullOrEmpty(m_PulseFxWwiseRtpcName))
            {

                float val = IsOnPainkillers() ? 100f : 50f;

                float in_value = GameManager.GetCameraStatusEffects().m_SprainAmountSin * val;
                GameObject soundEmitterFromGameObject = GameAudioManager.GetSoundEmitterFromGameObject(GameManager.GetPlayerObject());
                AkSoundEngine.SetRTPCValue(m_PulseFxWwiseRtpcName, in_value, soundEmitterFromGameObject);
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

        public void MaybeApplyOverdoseOrRisk()
        {

            string riskDesc = "You have consumed too many meds and your blood drug level is rising. You are at risk of an overdose.";
            string riskDesc2 = "Refrain from taking medications and rest to let the drugs leave your system.";

            string odDesc = "You have consumed too much medication and your blood drug level is dangerously high. You are suffering from an overdose.";
            string odDesc2 = "Refrain from taking any medication and rest to let the drugs leave your system.";

            if(IsHighOnPainkillers() && !IsOverdosing() && !am.HasAfflictionOfType(typeof(OverdoseRisk)))
            {
                new OverdoseRisk("Overdose Risk", "Painkillers", riskDesc, riskDesc2, "ico_units_pill", AfflictionBodyArea.Chest, true, [], []).Start();
            }
            else if(IsOverdosing() && !am.HasAfflictionOfType(typeof(Overdose)))
            {
                new Overdose("Overdose", "Painkillers", odDesc, odDesc2, "ico_units_pill", AfflictionBodyArea.Chest, true, [], []).Start();
            }
        }

    }
}
