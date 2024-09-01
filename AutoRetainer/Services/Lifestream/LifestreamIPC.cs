﻿using ECommons.EzIpcManager;

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
    /// <summary>
    /// territory, plot
    /// </summary>
    [EzIPC] public Func<uint, int, Vector3?> GetPlotEntrance;
    /// <summary>
    /// type(home=1, fc=2), mode(enter house=2)
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
}
