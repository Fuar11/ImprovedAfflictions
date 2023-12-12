using ImprovedAfflictions.Pain.Component;
using ImprovedAfflictions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImprovedAfflictions.Component
{
    internal class AfflictionComponentSaveDataProxy
    {
        public List<PainAffliction> m_PainInstances { get; set; }
        public float m_PainLevel { get; set; }
        public float m_PainkillerLevel { get; set; }
        public float m_ConcussionDrugLevel { get; set; }
        public float m_InsomniaDrugLevel { get; set; }
        public float m_PainkillerIncrementAmount { get; set; }
        public float m_PainkillerDecrementStartingAmount { get; set; }
        public bool m_HasConcussion { get; set; }

        public AfflictionComponentSaveDataProxy(List<PainAffliction> painInstances, float painLevel, float painkillerLevel, float concussionDrugLevel, float insomniaDrugLevel, float painkillerIncrementAmount, float painkillerDecrementStartingAmount, bool hasConcussion)
        {
            m_PainInstances = painInstances;
            m_PainLevel = painLevel;
            m_PainkillerLevel = painkillerLevel;
            m_ConcussionDrugLevel = concussionDrugLevel;
            m_InsomniaDrugLevel = insomniaDrugLevel;
            m_PainkillerIncrementAmount = painkillerIncrementAmount;
            m_PainkillerDecrementStartingAmount = painkillerDecrementStartingAmount;
            m_HasConcussion = hasConcussion;
        }
        public AfflictionComponentSaveDataProxy()
        {

        }

    }
}
