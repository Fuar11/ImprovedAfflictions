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
using AfflictionComponent.Components;
using ImprovedAfflictions.CustomAfflictions;
using Random = UnityEngine.Random;
using ComplexLogger;

namespace ImprovedAfflictions;
internal sealed class Mod : MelonMod, Moment.IScheduledEventExecutor
{
    internal static Mod Instance { get; private set; }
    internal static ComplexLogger<Mod> Logger = new();
    internal static PainManager painManager;
    internal static SaveDataManager sdm = new SaveDataManager();

    public string ScheduledEventExecutorId => "Fuar.ImprovedAfflictions";
    public void Execute(TLDDateTime time, string eventType, string? eventId, string? eventData)
    {
        
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
        }
    }

    public override void OnInitializeMelon()
	{
        Instance = this;
		MelonLogger.Msg("Improved Afflictions is online.");
        Settings.OnLoad();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (sceneName.ToLowerInvariant().Contains("boot") || sceneName.ToLowerInvariant().Contains("empty")) return;
        if (sceneName.ToLowerInvariant().Contains("menu"))
        {
            UnityEngine.Object.Destroy(GameObject.Find("PainManager"));
            painManager = null;
            return;
        }

        if (!sceneName.Contains("_SANDBOX") && !sceneName.Contains("_DLC") && !sceneName.Contains("_WILDLIFE"))
        {
            if (painManager == null)
            {
                GameObject PainManager = new() { name = "PainManager", layer = vp_Layer.Default };
                UnityEngine.Object.Instantiate(PainManager, GameManager.GetVpFPSPlayer().transform);
                UnityEngine.Object.DontDestroyOnLoad(PainManager);
                painManager = PainManager.AddComponent<PainManager>();
            }
        }
    }

    public override void OnUpdate()
    {
        if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Keypad7))
        {
            float duration = Random.Range(96f, 240f);
            string desc = "You've sufferred head trauma and are suffering from a concussion. Take painkillers to numb the debilitating effects while your head rests to heal.";

            if (AfflictionHelper.ResetIfHasAffliction("Concussion", AfflictionBodyArea.Head, false)) return;

            new CustomPainAffliction("Concussion", "Head Trauma", desc, "", AfflictionBodyArea.Head, "ico_injury_diabetes", false, false, duration, false, false, [Tuple.Create("GEAR_BottlePainKillers", 2, 1)], [], 40f);
        }
    }
}
