using ECommons.EzIpcManager;
using ECommons.Throttlers;

namespace AutoRetainer.Modules.EzIPCManagers;
public class IPC_GCContinuation
{
    public IPC_GCContinuation()
    {
        EzIPC.Init(this, $"{Svc.PluginInterface.InternalName}.GC");
    }

    [EzIPC]
    public void EnqueueInitiation()
    {
        GCContinuation.EnqueueInitiation(true);
    }

    [EzIPC]
    public GCInfo? GetGCInfo()
    {
        if(EzThrottler.Throttle("IPCInformObsoleteFunction", 10000))
        {
            PluginLog.Warning($"Don't use GetGCInfo IPC method, it is now obsolete.");
        }
        return GCContinuation.GetGCInfo();
    }
}
