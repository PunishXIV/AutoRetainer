using AutoRetainer.Offline;
using Dalamud.Configuration;
using System.Windows.Forms;

namespace AutoRetainer;

[Serializable]
internal class Config : IPluginConfiguration
{
    public int Version { get; set; } =  1;
    public Dictionary<ulong, HashSet<string>> SelectedRetainers = new();
    public bool AutoEnableDisable = false;
    public bool TurboMode = false;
    public bool EnableAssigningQuickExploration = false;
    public bool Verbose = false;
    public bool AutoUseRetainerBell = false;
    public bool AutoUseRetainerBellFocusOnly = true;
    public bool _autoCloseRetainerWindow = false;
    public List<OfflineCharacterData> OfflineData = new();
    public bool MultiWaitForAll = false;
    //public bool MultipleServiceAccounts = false;
    public bool NoNames = false;
    public int UnsyncCompensation = -5;
    public int AdvanceTimer = 60;
    public int Speed = 100;
    public bool StatsUnifyHQ = false;
    public bool RecordStats = true;
    public bool EnableAutoGCHandin = false;
    public bool GCHandinNotify = true;
    public bool SS = false;
    internal bool BypassSanctuaryCheck = false;
    public bool MultiAllowHET = false;

    internal bool AutoCloseRetainerWindow
    {
        get
        {
            return _autoCloseRetainerWindow && !ImGui.GetIO().KeyCtrl;
        }
        set
        {
            _autoCloseRetainerWindow = value;
        }
    }

    public bool AutoEnablePluginNearBell = false;
    public bool _dontReassign = false;
    internal bool DontReassign
    {
        get
        {
            return _dontReassign || ImGui.GetIO().KeyCtrl;
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
