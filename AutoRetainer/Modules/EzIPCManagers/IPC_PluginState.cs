using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using ECommons.EzIpcManager;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;

namespace AutoRetainer.Modules.EzIPCManagers;
public class IPC_PluginState
{
    public IPC_PluginState()
    {
        EzIPC.Init(this, $"{Svc.PluginInterface.InternalName}.PluginState");
    }

    [EzIPC] public bool IsBusy() => Utils.IsBusy;
    [EzIPC] public Dictionary<ulong, HashSet<string>> GetEnabledRetainers() => C.SelectedRetainers;
    [EzIPC] public bool AreAnyRetainersAvailableForCurrentChara() => Utils.AnyRetainersAvailableCurrentChara();
    [EzIPC] public void AbortAllTasks() => P.TaskManager.Abort();
    [EzIPC]
    public void DisableAllFunctions()
    {
        MultiMode.Enabled = false;
        SchedulerMain.DisablePlugin();
        VoyageScheduler.Enabled = false;
    }
    [EzIPC] public void EnableMultiMode() => Svc.Commands.ProcessCommand("/autoretainer multi enable");
    [EzIPC] public int GetInventoryFreeSlotCount() => Utils.GetInventoryFreeSlotCount();
    [EzIPC] public void EnqueueHET(bool ignoreTeleportZonecheck, bool noTeleport) => HouseEnterTask.EnqueueTask();
    [EzIPC] public bool CanAutoLogin() => Utils.CanAutoLogin();
    [EzIPC] public bool Relog(string charaNameWithWorld)
    {
        if(Utils.CanAutoLogin())
        {
            var target = C.OfflineData.Where(x => $"{x.Name}@{x.World}" == charaNameWithWorld).FirstOrDefault();
            if(target != null)
            {
                MultiMode.Relog(target, out var err, RelogReason.Command);
                return err == null;
            }
        }
        return false;
    } 
}
