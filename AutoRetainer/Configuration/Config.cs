using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.Interop;

namespace AutoRetainer.Configuration;

[Serializable]
internal unsafe class Config : IEzConfig
{
    public string CensorSeed = Guid.NewGuid().ToString();
    public Dictionary<ulong, HashSet<string>> SelectedRetainers = [];
    public bool EnableAssigningQuickExploration = false;
    public bool Verbose = false;
    public List<OfflineCharacterData> OfflineData = [];
    //public bool MultipleServiceAccounts = false;
    public bool NoNames = false;
    public int UnsyncCompensation = -5;
    public bool StatsUnifyHQ = false;
    public bool RecordStats = true;
    public bool AutoGCContinuation = false;
    public bool ShouldSerializeEnableAutoGCHandin() => false;
    public bool GCHandinNotify = false;
    internal bool BypassSanctuaryCheck = false;
    public bool ExpertMultiAllowHET = true;
    public bool MultiHETOnEnable = true;
    public bool UseServerTime = true;
    public bool NoTheme = false;
    public Dictionary<string, AdditionalRetainerData> AdditionalData = [];
    public bool AutoDisable = true;
    public bool Expert = false;
    public List<(ulong CID, string Name)> Blacklist = [];
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
    public int FrameDelay = 8;

    public bool _dontReassign = false;
    public bool OldRetainerSense = false;
    public bool RetainerSense = false;
    public int RetainerSenseThreshold = 10000;
    public bool MultiModeUIBar = false;
    public bool UIBar = true;

    public LimitedKeys Suppress = LimitedKeys.LeftControlKey;
    public LimitedKeys TempCollectB = LimitedKeys.LeftShiftKey;

    public int RetainerMenuDelay = 0;
    public List<VenturePlan> SavedPlans = [];
    public bool MultiWaitOnLoginScreen = false;
    public UnavailableVentureDisplay UnavailableVentureDisplay = UnavailableVentureDisplay.Hide;

    public bool ShowAdditionalInfo = true;
    public bool RetryItemSearch = false;
    public bool ArtisanIntegration = false;
    public bool DisplayMMType = false;
    public List<SubmarineUnlockPlan> SubmarineUnlockPlans = [];
    public bool HideAirships = false;
    public int DisableRetainerVesselReturn = 0;
    public List<SubmarinePointPlan> SubmarinePointPlans = [];
    public int MultiMinInventorySlots = 2;
    public bool IgnoreEsc = false;

    public int UIWarningRetSlotNum = 20;
    public int UIWarningRetVentureNum = 50;
    public int UIWarningDepTanksNum = 300;
    public int UIWarningDepRepairNum = 100;
    public int UIWarningDepSlotNum = 20;
    public int TargetMSPTIdle = 0;
    public int TargetMSPTRunning = 0;
    public bool NoFPSLockWhenActive = true;
    public bool ExtraFPSLockRange = false;
    public bool FpsLockOnlyShutdownTimer = false;
    public bool ShutdownMakesNightMode = false;

    public bool ShowDeployables = false;
    public int BailoutTimeout = 5;
    public bool EnableBailout = true;
    public bool EnableCharaSelectBailout = true;

    public bool NightMode = false;
    public bool NightModePersistent = false;
    public bool ShowNightMode = false;
    public bool NightModeRetainers = false;
    public bool NightModeDeployables = true;
    internal bool NightModeFPSLimit = true;

    internal bool ExtraDebug = false;

    public bool OldStatusIcons = false;
    public int MinGilDisplay = 10000;
    public bool GilOnlyChars = false;

    public bool MultiAutoStart = false;
    public bool MultiDisableOnRelog = false;
    public bool MultiNoPreferredReset = false;
    public bool MultiPreferredCharLast = true;
    public bool VoyageDisableCalcParallel = false;
    public bool VoyageDisableCalcMultithreading = false;
    public Dictionary<ulong, FCData> FCData = [];
    public bool UpdateStaleFCData = false;
    public bool DisplayOnlyWalletFC = false;

    public bool LeastMBSFirst = false;

    public string DefaultSubmarineUnlockPlan = "";
    public bool AcceptedDisclamer = false;
    public bool AllowPrivateTeleport = false;
    public bool AllowFcTeleport = false;
    public bool AllowManualPostprocess = false;

    public List<EntrustPlan> EntrustPlans = [];
    public bool AllowSellFromArmory = false;

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

    public bool SubsAutoResend2 = true;
    //public bool SubsAutoRepair = true;
    //public bool SubsOnlyFinalize = false;
    //public bool SubsAutoEnable = false;
    //public bool SubsRepairFinalize = true;
    public MultiModeType MultiModeType = MultiModeType.Everything;
    public bool NoErrorCheckPlanner2 = true;
    public WorkshopFailAction FailureNoFuel = WorkshopFailAction.ExcludeChar;
    public WorkshopFailAction FailureNoRepair = WorkshopFailAction.ExcludeVessel;
    public WorkshopFailAction FailureNoInventory = WorkshopFailAction.ExcludeChar;
    public WorkshopFailAction FailureGeneric = WorkshopFailAction.StopPlugin;
    internal bool SimpleTweaksCompat = true;
    public bool FinalizeBeforeResend = false;
    public bool AlertNotAllEnabled = true;
    public bool AlertNotDeployed = true;
    public List<UnoptimalVesselConfiguration> UnoptimalVesselConfigurations = [];

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

    public bool StatusBarMSI = false;
    public int StatusBarIconWidth = 96;

    public bool IMEnableCofferAutoOpen = false;
    public bool IMEnableAutoVendor = false;
    public bool IMEnableContextMenu = false;
    public List<uint> IMAutoVendorHard = [];
    public List<uint> IMAutoVendorHardIgnoreStack = [];
    public List<uint> IMAutoVendorSoft = [];
    public List<uint> IMProtectList = [];
    public int IMAutoVendorHardStackLimit = 20;
    public bool IMDry = false;
    public bool IMEnableItemDesynthesis = false;
    public bool IMEnableNpcSell = false;

    public Vector2 WindowSize;
    public Vector2 WindowPos;
    public bool PinWindow = false;
    public bool DisplayOnStart = false;
}
