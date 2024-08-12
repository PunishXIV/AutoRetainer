using AutoRetainer.Modules.EzIPCManagers;
using AutoRetainer.Services.Lifestream;
using AutoRetainer.UI.NeoUI;
using AutoRetainer.UI.Statistics;

namespace AutoRetainer.Services;
public static class AutoRetainerServiceManager
{
    public static NeoWindow NeoWindow { get; private set; }
    public static EzIPCManager EzIPCManager { get; private set; }
    public static FCPointsUpdater FCPointsUpdater { get; private set; }
    public static FcDataManager FCData { get; private set; }
    public static GilDisplayManager GilDisplay { get; private set; }
    public static VentureStatsManager VentureStats { get; private set; }
    public static LifestreamIPC LifestreamIPC { get; private set; }
    //public static CharaListDebugger CharaListDebugger { get; private set; }

}
