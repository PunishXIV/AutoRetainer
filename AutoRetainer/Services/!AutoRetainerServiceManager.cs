using AutoRetainer.Modules.EzIPCManagers;
using AutoRetainer.Services.Lifestream;
using AutoRetainer.UI.NeoUI;
using AutoRetainer.UI.Overlays;
using AutoRetainer.UI.Statistics;

namespace AutoRetainer.Services;
public static class AutoRetainerServiceManager
{
    public static NeoWindow NeoWindow;
    public static EzIPCManager EzIPCManager;
    public static FCPointsUpdater FCPointsUpdater;
    public static FcDataManager FCData;
    public static GilDisplayManager GilDisplay;
    public static VentureStatsManager VentureStats;
    public static LifestreamIPC LifestreamIPC;
    //public static EventLogger EventLogger;
    public static AutoBuyFuelOverlay AutoBuyFuelOverlay;
    public static TitleScreenButton TitleScreenButton;
    public static AddonWatcher AddonWatcher;
    public static DataMigrator DataMigrator;
}
