using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.Interop;

namespace AutoRetainer.PluginData;

[Serializable]
internal unsafe class Config
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
    public HashSet<ulong> WhitelistedAccounts = [];

    public bool ShouldSerializeEnableAutoGCHandin()
    {
        return false;
    }

    public bool GCHandinNotify = false;
    internal bool BypassSanctuaryCheck = false;
    public bool MultiHETOnEnable = true;
    public bool UseServerTime = true;
    public bool NoTheme = false;
    public Dictionary<string, AdditionalRetainerData> AdditionalData = [];
    public bool AutoDisable = true;
    public List<(ulong CID, string Name)> Blacklist = [];
    public bool HideOverlayIcons = false;
    public bool UnsafeProtection = false;
    public bool CharEqualize = false;
    public bool LongestVentureFirst = false;
    public bool CappedLevelsLast = false;
    public bool TimerAllowNegative = false;
    public bool MarketCooldownOverlay = false;

    public bool LoginOverlay = false;
    public float LoginOverlayScale = 1f;
    public float LoginOverlayBPadding = 1.35f;
    public bool LoginOverlayAllSearch = false;

    public OpenBellBehavior OpenBellBehaviorNoVentures = OpenBellBehavior.Enable_AutoRetainer;
    public OpenBellBehavior OpenBellBehaviorWithVentures = OpenBellBehavior.Enable_AutoRetainer;
    public TaskCompletedBehavior TaskCompletedBehaviorAuto = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    public TaskCompletedBehavior TaskCompletedBehaviorManual = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    public TaskCompletedBehavior TaskCompletedBehaviorAccess = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    //public bool AutoPause = true;
    public bool Stay5 = true;
    public bool NoCurrentCharaOnTop = false;

    public int ExtraFrameDelay = 0;

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
    public string AutoLogin = "";
    public int AutoLoginDelay = 10;
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
    public bool AllowManualPostprocess = false;
    public bool AllowSimpleTeleport = false;

    public List<EntrustPlan> EntrustPlans = [];
    public bool DontLogout = false;

    public TeleportOptions GlobalTeleportOptions = new();
    public bool SharedHET = false;
    internal bool SkipItemConfirmations => true;
    public ulong LastLoggedInChara = 0;

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

    public List<LevelAndPartsData> LevelAndPartsData = [];
    public bool EnableAutomaticSubRegistration = false;
    public bool EnableAutomaticComponentsAndPlanChange = false;

    public bool StatusBarMSI = false;
    public int StatusBarIconWidth = 96;

    [Obsolete] public bool IMEnableCofferAutoOpen = false;
    [Obsolete] public bool IMEnableAutoVendor = false;
    [Obsolete] public bool IMEnableContextMenu = false;
    [Obsolete] public bool IMSkipVendorIfRetainer = false;
    [Obsolete] public List<uint> IMAutoVendorHard = [];
    [Obsolete] public List<uint> IMAutoVendorHardIgnoreStack = [];
    [Obsolete] public List<uint> IMAutoVendorSoft = [];
    [Obsolete] public List<uint> IMProtectList = [];
    [Obsolete] public int IMAutoVendorHardStackLimit = 20;
    [Obsolete] public bool IMDry = false;
    [Obsolete] public bool IMEnableItemDesynthesis = false;
    [Obsolete] public bool IMEnableNpcSell = false;
    [Obsolete] public bool AllowSellFromArmory = false;

    public InventoryManagementSettings DefaultIMSettings = new();
    public List<InventoryManagementSettings> AdditionalIMSettings = [];
    public bool IMMigrated = false;

    public Vector2 WindowSize;
    public Vector2 WindowPos;
    public bool PinWindow = false;
    public bool DisplayOnStart = false;

    public bool ResolveConnectionErrors = false;
    public int ConnectionErrorsRetry = 10;
    public bool ConnectionErrorsBlacklist = true;
    public bool EnableEntrustManager = true;
    public bool EnableEntrustChat = false;

    public bool HETWhenDisabled = false;
    public bool UseTitleScreenButton = false;
    public bool NoCharaSearch = false;
    public bool NoTeleportHetWhenNextToBell = false;
    public bool NoGradient = false;
    public bool No2ndInstanceNotify = false;

    public bool FCChestGilCheck = false;
    public int FCChestGilCheckCd = 24;
    public Dictionary<ulong, long> FCChestGilCheckTimes = [];
    public Dictionary<ExcelWorldHelper.Region, long> LockoutTime = [];
    public GCExchangePlan DefaultGCExchangePlan = new();
    public List<GCExchangePlan> AdditionalGCExchangePlans = [];

    public bool EnableRetainerSort = false;
    public List<RetainersVisualOrder> RetainersVisualOrders = [];
    public bool EnableDeployablesSort = false;
    public List<DeployablesVisualOrder> DeployablesVisualOrders = [];
}
