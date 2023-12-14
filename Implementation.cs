using MelonLoader;
using ModData;
using ImprovedAfflictions.Utils;
using Moment;
using ImprovedAfflictions.Pain;
using System.Text.Json;
using Il2Cpp;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using ImprovedAfflictions.Component;
using ImprovedAfflictions.Pain.Component;

namespace ImprovedAfflictions;
internal sealed class Implementation : MelonMod, Moment.IScheduledEventExecutor
{
    internal static Implementation Instance { get; private set; }

    internal static SaveDataManager sdm = new SaveDataManager();

    public string ScheduledEventExecutorId => "Fuar.ImprovedAfflictions";
    public void Execute(TLDDateTime time, string eventType, string? eventId, string? eventData)
    {
        
        PainHelper ph = new PainHelper();

        switch (eventType)
        {
            case "takeEffectFoodPoisoning":
                sdm.Save("false", "scheduledFoodPoisoning");

                if (eventId.ToLowerInvariant().Contains("soda"))
                {
                    GameManager.GetDysenteryComponent().DysenteryStart(displayIcon: true);
                    sdm.Save(eventId, "dysenteryCause");
                }
                else if (eventId.ToLowerInvariant().Contains("pinnacle") || eventId.ToLowerInvariant().Contains("dog") || eventId.ToLowerInvariant().Contains("milk") || eventId.ToLowerInvariant().Contains("corn"))
                {

                    if (Il2Cpp.Utils.RollChance(50f)) GameManager.GetFoodPoisoningComponent().FoodPoisoningStart(eventId, displayIcon: true);
                    else 
                    {
                        GameManager.GetDysenteryComponent().DysenteryStart(displayIcon: true);
                        sdm.Save(eventId, "dysenteryCause");
                    }
                }
                else
                {
                    GameManager.GetFoodPoisoningComponent().FoodPoisoningStart(eventId, displayIcon: true);
                }
                break;

            /** got inlined
            case "takeEffectAntibiotics":
                GameManager.GetFoodPoisoningComponent().m_AntibioticsTaken = true;
                break; **/
        }
    }

    public override void OnInitializeMelon()
	{
        Instance = this;
		MelonLogger.Msg("Improved Afflictions is online.");
	}

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName.ToLowerInvariant().Contains("menu") || sceneName.ToLowerInvariant().Contains("dlc") || sceneName.ToLowerInvariant().Contains("boot") || sceneName.ToLowerInvariant().Contains("empty")) return;

        if (!sceneName.Contains("_SANDBOX") && !sceneName.Contains("_DLC") && !sceneName.Contains("_WILDLIFE"))
        {
            if (!GameObject.Find("SCRIPT_ConditionSystems").GetComponent<AfflictionComponent>())
            {
                GameObject.Find("SCRIPT_ConditionSystems").AddComponent<AfflictionComponent>();
            }
        }
    }

   

}
