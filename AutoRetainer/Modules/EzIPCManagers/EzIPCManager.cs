namespace AutoRetainer.Modules.EzIPCManagers;
public sealed class EzIPCManager
{
    public IPC_GCContinuation IPC_GCContinuation = new();
    public IPC_PluginState IPC_PluginState = new();
}
