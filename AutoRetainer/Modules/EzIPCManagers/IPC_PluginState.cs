using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using ECommons.EzIpcManager;

namespace AutoRetainer.Modules.EzIPCManagers;
public class IPC_PluginState
{
    public IPC_PluginState()
    {
        EzIPC.Init(this, $"{Svc.PluginInterface.InternalName}.PluginState");
    }

    [EzIPC]
    public bool IsBusy()
    {
        return Utils.IsBusy;
    }

    [EzIPC]
    public Dictionary<ulong, HashSet<string>> GetEnabledRetainers()
    {
        return C.SelectedRetainers;
    }

    [EzIPC]
    public bool AreAnyRetainersAvailableForCurrentChara()
    {
        return Utils.AnyRetainersAvailableCurrentChara();
    }

    [EzIPC]
    public void AbortAllTasks()
    {
        P.TaskManager.Abort();
    }

    [EzIPC]
    public void DisableAllFunctions()
    {
        MultiMode.Enabled = false;
        SchedulerMain.DisablePlugin();
        VoyageScheduler.Enabled = false;
    }
    [EzIPC]
    public bool GetMultiModeStatus()
    {
        return MultiMode.Enabled;
    }

    [EzIPC]
    public void EnableMultiMode()
    {
        Svc.Commands.ProcessCommand("/autoretainer multi enable");
    }

    [EzIPC]
    public int GetInventoryFreeSlotCount()
    {
        return Utils.GetInventoryFreeSlotCount();
    }

    [EzIPC]
    public void EnqueueHET(Action onFailure)
    {
        TaskNeoHET.Enqueue(onFailure);
    }

    [EzIPC]
    public bool CanAutoLogin()
    {
        return Utils.CanAutoLogin();
    }

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

    [EzIPC]
    public bool GetOptionRetainerSense()
    {
        return C.RetainerSense;
    }

    [EzIPC]
    public void SetOptionRetainerSense(bool value)
    {
        C.RetainerSense = value;
    }

    [EzIPC]
    public int GetOptionRetainerSenseThreshold()
    {
        return C.RetainerSenseThreshold;
    }

    [EzIPC]
    public void SetOptionRetainerSenseThreshold(int value)
    {
        C.RetainerSenseThreshold = value;
    }

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
        return Data.GetIMSettings().IMProtectList.Contains(itemId);
    }

    [EzIPC]
    public bool? AreAnyEnabledVesselsNotDeployed(ulong contentId)
    {
        var data = C.OfflineData.FirstOrDefault(x => x.CID == contentId);
        if(data == null) return null;
        return VoyageUtils.AreAnyEnabledVesselsNotDeployed(data);
    }

    [EzIPC]
    public bool? AreAnyEnabledVesselsReady(ulong contentId)
    {
        var data = C.OfflineData.FirstOrDefault(x => x.CID == contentId);
        if(data == null) return null;
        if(data.AreAnyEnabledVesselsReturnInNext(C.MultiModeWorkshopConfiguration.AdvanceTimer, C.MultiModeWorkshopConfiguration.MultiWaitForAll))
        {
            return true;
        }
        return false;
    }
}
