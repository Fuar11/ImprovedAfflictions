using Il2Cpp;
using Il2Cppgw.proto.utils;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImprovedAfflictions.Utils
{
    internal class PainHelper
    {

        public bool HasConcussion()
        {
            SprainPain painManager = GameManager.GetSprainPainComponent();
            foreach(SprainPain.Instance inst in painManager.m_ActiveInstances)
            {
                if (inst.m_Cause.ToLowerInvariant() == "concussion") 
                {
                    MelonLogger.Msg("Has concussion!");
                        return true; 
                }
            }
            return false;
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
                    MelonLogger.Error("Error retrieving data, might not be saved yet.");
                    continue;
                }

                PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

                //if the pain instance isn't saved or it's remedied, skip to the next one
                if (pain == null || pain.m_RemedyApplied) continue;

                //set the current pain effects to the most intense pain in the list
                if (pain.m_PulseFxIntensity >= maxIntensity)
                {
                    painManager.m_PulseFxIntensity = pain.m_PulseFxIntensity;
                    painManager.m_PulseFxFrequencySeconds = pain.m_PulseFxFrequencySeconds;
                    maxIntensity = pain.m_PulseFxIntensity;
                }
            }

            //if all of the pain instances are remedied but not gone, set minimal pain effect
            if(maxIntensity == 0f && painManager.HasSprainPain())
            {
                painManager.m_PulseFxIntensity = 0.3f;
            }

            
        }

        public void WareOffPainkillers(string index)
        {

            SaveDataManager sdm = Implementation.sdm;

            var data = sdm.LoadPainData(index);

            if (data == null)
            {
                MelonLogger.Error("Unable to ware off painkillers since data cannot be retrieved from Mod Data file");
                return;
            }

            PainSaveDataProxy? pain = JsonSerializer.Deserialize<PainSaveDataProxy>(data);

            if (pain == null || pain.m_RemedyApplied == false) return;

            PainSaveDataProxy painToSave = pain;
            painToSave.m_RemedyApplied = false;

            var dataToSave = JsonSerializer.Serialize(painToSave);

            sdm.Save(dataToSave, index);

            UpdatePainEffects();

        }
    }
}
