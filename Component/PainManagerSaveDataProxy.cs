using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedAfflictions.Component
{
    public class PainManagerSaveDataProxy
    {

        public float m_PainkillerLevel { get; set; }
        public float m_PainkillerIncrementAmount { get; set; }
        public float m_PainkillerDecrementStartingAmount { get; set; }

        public float m_SecondsSinceLastODFx { get; set; }

        public float m_SecondsSinceLastPulseFx { get; set; }
        public float m_PulseFxFrequencySeconds { get; set; }
        public float m_PulseFxIntensity { get; set; }

        public PainManagerSaveDataProxy(float painkillerLevel, float painkillerIncrementAmount, float painkillerDecrementStartingAmount, float secondsSinceLastODFx, float secondsSinceLastPulseFx, float pulseFxFrequencySeconds, float pulseFxIntensity)
        {
            m_PainkillerLevel = painkillerLevel;
            m_PainkillerIncrementAmount = painkillerIncrementAmount;
            m_PainkillerDecrementStartingAmount = painkillerDecrementStartingAmount;
            m_SecondsSinceLastODFx = secondsSinceLastODFx;
            m_SecondsSinceLastPulseFx = secondsSinceLastPulseFx;
            m_PulseFxFrequencySeconds = pulseFxFrequencySeconds;
            m_PulseFxIntensity = pulseFxIntensity;
        }

        public PainManagerSaveDataProxy()
        {
        }

        public override string ToString()
        {
            return $"Painkiller Level: {m_PainkillerLevel}, " +
           $"Painkiller Increment Amount: {m_PainkillerIncrementAmount}, " +
           $"Painkiller Decrement Starting Amount: {m_PainkillerDecrementStartingAmount}, " +
           $"Seconds Since Last ODFx: {m_SecondsSinceLastODFx}, " +
           $"Seconds Since Last PulseFx: {m_SecondsSinceLastPulseFx}, " +
           $"PulseFx Frequency Seconds: {m_PulseFxFrequencySeconds}, " +
           $"PulseFx Intensity: {m_PulseFxIntensity}";
        }

    }
}
