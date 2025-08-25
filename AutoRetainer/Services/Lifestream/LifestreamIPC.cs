using ECommons.EzIpcManager;

namespace AutoRetainer.Services.Lifestream;
public class LifestreamIPC
{
    private LifestreamIPC()
    {
        EzIPC.Init(this, "Lifestream", SafeWrapper.AnyException);
    }

    [EzIPC] public Func<uint, byte, bool> Teleport;
    [EzIPC] public Func<bool> TeleportToHome;
    [EzIPC] public Func<bool> TeleportToFC;
    [EzIPC] public Func<bool> TeleportToApartment;
    [EzIPC] public Func<bool> IsBusy;
    /// <summary>
    /// city aetheryte id
    /// </summary>
    [EzIPC] public Func<int, uint> GetResidentialTerritory;
    /// <summary>
    /// content id
    /// </summary>
    [EzIPC] public Func<ulong, (HousePathData Private, HousePathData FC)> GetHousePathData;
    [EzIPC] public Func<HousePathData> GetSharedHousePathData;
    /// <summary>
    /// territory, plot
    /// </summary>
    [EzIPC] public Func<uint, int, Vector3?> GetPlotEntrance;
    /// <summary>
    /// type(home=1, fc=2, apartment=3), mode(enter house=2)
    /// </summary>
    [EzIPC] public Action<int, int?> EnqueuePropertyShortcut;
    [EzIPC] public Func<(int Kind, int Ward, int Plot)?> GetCurrentPlotInfo;

    [EzIPCEvent]
    public void OnHouseEnterError()
    {
        PluginLog.Warning($"Received house enter error from Lifestream. Current character will be excluded from multi mode.");
        if(Data != null)
        {
            Data.Enabled = false;
            Data.WorkshopEnabled = false;
        }
    }

    [EzIPC] public Action<int?> EnqueueInnShortcut;
    [EzIPC] public Func<bool?> HasApartment;
    [EzIPC] public Action<bool> EnterApartment;
    [EzIPC] public Func<bool?> HasPrivateHouse;
    [EzIPC] public Func<bool?> HasSharedEstate;
    [EzIPC] public Func<bool?> HasFreeCompanyHouse;
    [EzIPC] public Func<bool> CanMoveToWorkshop;
    [EzIPC] public Action MoveToWorkshop;
    [EzIPC] public Action<string> ExecuteCommand;
}
