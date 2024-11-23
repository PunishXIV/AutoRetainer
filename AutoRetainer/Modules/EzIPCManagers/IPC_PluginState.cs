using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using ECommons.EzIpcManager;
using FFXIVClientStructs.FFXIV.Client.Game;

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
    [EzIPC] public void EnqueueHET(Action onFailure) => TaskNeoHET.Enqueue(onFailure);
    [EzIPC] public bool CanAutoLogin() => Utils.CanAutoLogin();
    [EzIPC]
    public bool Relog(string charaNameWithWorld)
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

    [EzIPC] public bool GetOptionRetainerSense() => C.RetainerSense;
    [EzIPC] public void SetOptionRetainerSense(bool value) => C.RetainerSense = value;
    [EzIPC] public int GetOptionRetainerSenseThreshold() => C.RetainerSenseThreshold;
    [EzIPC] public void SetOptionRetainerSenseThreshold(int value) => C.RetainerSenseThreshold = value;
    [EzIPC]
    public long? GetClosestRetainerVentureSecondsRemaining(ulong CID)
    {
        if(C.SelectedRetainers.TryGetValue(CID, out var enabledRetainers))
        {
            if(C.OfflineData.TryGetFirst(x => x.CID == CID, out var data))
            {
                var selectedRetainers = data.GetEnabledRetainers().Where(z => z.HasVenture).OrderBy(z => z.GetVentureSecondsRemaining());
                if(selectedRetainers.Any()) return selectedRetainers.First().GetVentureSecondsRemaining();
            }
        }
        return null;
    }

    [EzIPC] 
    public bool IsItemProtected(uint itemId)
    {
        return C.IMProtectList.Contains(itemId);
    }
}
