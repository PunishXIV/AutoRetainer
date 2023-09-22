using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.Interop;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using PInvoke;
using System.Windows.Forms;

namespace AutoRetainer.Configuration;

[Serializable]
internal unsafe class Config : IEzConfig
{
    public string CensorSeed = Guid.NewGuid().ToString();
    public Dictionary<ulong, HashSet<string>> SelectedRetainers = new();
    public bool EnableAssigningQuickExploration = false;
    public bool Verbose = false;
    public List<OfflineCharacterData> OfflineData = new();
    //public bool MultipleServiceAccounts = false;
    public bool NoNames = false;
    public int UnsyncCompensation = -5;
    public bool StatsUnifyHQ = false;
    public bool RecordStats = true;
    public bool EnableAutoGCHandin = false; //todo: remove
    public bool ShouldSerializeEnableAutoGCHandin() => false;
    public bool GCHandinNotify = false;
    internal bool BypassSanctuaryCheck = false;
    public bool MultiAllowHET = false;
    public bool MultiHETOnEnable = true;
    public bool UseServerTime = true;
    public bool NoTheme = false;
    public Dictionary<string, AdditionalRetainerData> AdditionalData = new();
    public bool AutoDisable = true;
    public bool Expert = false;
    public List<(ulong CID, string Name)> Blacklist = new();
    public bool HideOverlayIcons = false;
    public bool UnsafeProtection = false;
    public bool CharEqualize = false;
    public bool TimerAllowNegative = false;
    public bool MarketCooldownOverlay = false;

    public bool LoginOverlay = false;
    public float LoginOverlayScale = 1f;
    public float LoginOverlayBPadding = 1.35f;

    public OpenBellBehavior OpenBellBehaviorNoVentures = OpenBellBehavior.Enable_AutoRetainer;
    public OpenBellBehavior OpenBellBehaviorWithVentures = OpenBellBehavior.Enable_AutoRetainer;
    public TaskCompletedBehavior TaskCompletedBehaviorAuto = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    public TaskCompletedBehavior TaskCompletedBehaviorManual = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    public TaskCompletedBehavior TaskCompletedBehaviorAccess = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    //public bool AutoPause = true;
    public bool Stay5 = true;
    public bool NoCurrentCharaOnTop = false;

    public bool UseFrameDelay = true;
    public int Delay = 200;
    public int FrameDelay = 4;

    public bool _dontReassign = false;
    public bool OldRetainerSense = false;
    public bool RetainerSense = false;
    public int RetainerSenseThreshold = 10000;
    public bool MultiModeUIBar = false;
    public bool UIBar = true;

    public LimitedKeys Suppress = LimitedKeys.LeftControlKey;
    public LimitedKeys TempCollectB = LimitedKeys.LeftShiftKey;

    public int RetainerMenuDelay = 0;
    public List<VenturePlan> SavedPlans = new();
    public bool MultiWaitOnLoginScreen = false;
    public UnavailableVentureDisplay UnavailableVentureDisplay = UnavailableVentureDisplay.Hide;

    public bool ShowAdditionalInfo = true;
    public bool RetryItemSearch = false;
    public bool ArtisanIntegration = false;
    public bool DisplayMMType = false;
    public List<SubmarineUnlockPlan> SubmarineUnlockPlans = new();
    public bool HideAirships = false;
    public int DisableRetainerVesselReturn = 0;
    public List<SubmarinePointPlan> SubmarinePointPlans = new();

    internal bool DontReassign
    {
        get
        {
            return _dontReassign || (C.TempCollectB != LimitedKeys.None && IsKeyPressed(C.TempCollectB) && !CSFramework.Instance()->WindowInactive);
        }
        set
        {
            _dontReassign = value;
        }
    }

    public LimitedKeys SellKey = LimitedKeys.None;
    public LimitedKeys EntrustKey = LimitedKeys.None;
    public LimitedKeys RetrieveKey = LimitedKeys.None;
    public LimitedKeys SellMarketKey = LimitedKeys.None;

    public bool NotifyEnableOverlay = false;
    public bool NotifyCombatDutyNoDisplay = true;
    public bool NotifyIncludeAllChara = true;
    public bool NotifyIgnoreNoMultiMode = false;
    public bool NotifyDisplayInChatX = false;
    public bool NotifyDeskopToast = false;
    public bool NotifyFlashTaskbar = false;
    public bool NotifyNoToastWhenRunning = true;
    public bool UnlockFPS = true;
    public bool UnlockFPSUnlimited = false;
    public bool UnlockFPSChillFrames = false;

    public bool ManipulatePriority = false;

    public bool SubsAutoResend = false;
    public bool SubsAutoRepair = false;
    public bool SubsOnlyFinalize = false;
    public bool SubsAutoEnable = false;
    public bool SubsRepairFinalize = false;
    public MultiModeType MultiModeType = MultiModeType.Everything;
    public bool NoErrorCheckPlanner2 = true;
    public WorkshopFailAction FailureNoFuel = WorkshopFailAction.ExcludeChar;
    public WorkshopFailAction FailureNoRepair = WorkshopFailAction.ExcludeVessel;
    public WorkshopFailAction FailureNoInventory = WorkshopFailAction.ExcludeChar;
    public WorkshopFailAction FailureGeneric = WorkshopFailAction.StopPlugin;
    public bool SimpleTweaksCompat = false;

    public MultiModeCommonConfiguration MultiModeRetainerConfiguration = new()
    {
        AdvanceTimer = 60,
        MultiWaitForAll = false,
    };
    public MultiModeCommonConfiguration MultiModeWorkshopConfiguration = new()
    {
        MultiWaitForAll = false,
        AdvanceTimer = 120,
    };
}
