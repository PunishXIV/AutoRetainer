using ECommons.Configuration;
using ECommons.MathHelpers;
using PInvoke;
using System.Windows.Forms;

namespace AutoRetainer.Configuration;

[Serializable]
internal unsafe class Config : IEzConfig
{
    public Dictionary<ulong, HashSet<string>> SelectedRetainers = new();
    public bool EnableAssigningQuickExploration = false;
    public bool Verbose = false;
    public List<OfflineCharacterData> OfflineData = new();
    public bool MultiWaitForAll = false;
    //public bool MultipleServiceAccounts = false;
    public bool NoNames = false;
    public int UnsyncCompensation = -5;
    public int AdvanceTimer = 60;
    public bool StatsUnifyHQ = false;
    public bool RecordStats = true;
    public bool EnableAutoGCHandin = false;
    public bool GCHandinNotify = true;
    public bool SS = false;
    internal bool BypassSanctuaryCheck = false;
    public bool MultiAllowHET = false;
    public bool UseServerTime = true;
    public bool NoTheme = false;
    public Dictionary<string, AdditionalRetainerData> AdditionalData = new();
    public bool AutoDisable = true;
    public bool Expert = false;
    public List<(ulong CID, string Name)> Blacklist = new();
    public bool HideOverlayIcons = false;

    public OpenBellBehavior OpenBellBehaviorNoVentures = OpenBellBehavior.Enable_AutoRetainer;
    public OpenBellBehavior OpenBellBehaviorWithVentures = OpenBellBehavior.Enable_AutoRetainer;
    public TaskCompletedBehavior TaskCompletedBehaviorAuto = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    public TaskCompletedBehavior TaskCompletedBehaviorManual = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    public TaskCompletedBehavior TaskCompletedBehaviorAccess = TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled;
    //public bool AutoPause = true;
    public bool Stay5 = true;
    public bool NoCurrentCharaOnTop = false;

    public int Delay = 200;

    public bool _dontReassign = false;
    public bool AutoUseRetainerBell = false;
    public bool MultiModeUIBar = false;
    public bool UIBar = true;

    public Keys Suppress = Keys.Control;
    public Keys TempCollectB = Keys.ShiftKey;
    internal bool DontReassign
    {
        get
        {
            return _dontReassign || (Bitmask.IsBitSet(User32.GetKeyState((int)P.config.TempCollectB), 15) && !CSFramework.Instance()->WindowInactive);
        }
        set
        {
            _dontReassign = value;
        }
    }

    public Keys SellKey = Keys.None;
    public Keys EntrustKey = Keys.None;
    public Keys RetrieveKey = Keys.None;
    public Keys SellMarketKey = Keys.None;

    public bool NotifyEnableOverlay = false;
    public bool NotifyCombatDutyNoDisplay = true;
    public bool NotifyIncludeAllChara = true;
    public bool NotifyIgnoreNoMultiMode = false;
    public bool NotifyDisplayInChatX = false;
    public bool NotifyDeskopToast = false;
    public bool NotifyFlashTaskbar = false;
    public bool NotifyNoToastWhenRunning = true;
}
