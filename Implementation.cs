using MelonLoader;
using ModData;
using ImprovedAfflictions.Utils;
using Moment;

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
            case "wareOffPainkiller":
                ph.WareOffPainkillers();
                break;
            case "takeEffectPainkiller":
                ph.TakeEffectPainkillers();
                break;
        }
    }

    public override void OnInitializeMelon()
	{
        Instance = this;
		MelonLogger.Msg("Improved Afflictions is online.");
	}
}
