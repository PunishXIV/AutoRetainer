using ECommons.EzIpcManager;

namespace AutoRetainer.Modules.EzIPCManagers;
public class IPC_GCContinuation
{
		public IPC_GCContinuation()
		{
				EzIPC.Init(this, $"{Svc.PluginInterface.InternalName}.GC");
		}

		[EzIPC] public void EnqueueInitiation() => GCContinuation.EnqueueInitiation();
		[EzIPC] public GCInfo? GetGCInfo() => GCContinuation.GetGCInfo();
}
