using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.EzIPCManagers;
public class EzIPCManager
{
    public IPC_GCContinuation IPC_GCContinuation = new();
    public IPC_PluginState IPC_PluginState = new();
}
