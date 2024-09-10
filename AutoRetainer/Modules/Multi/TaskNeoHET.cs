using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Multi;
public static class TaskNeoHET
{
    public static void Enqueue()
    {
        PluginLog.Debug($"Enqueued HouseEnterTask from {new StackTrace().GetFrames().Select(x => x.GetMethod()?.Name).Prepend("      ").Print("\n")}");
    }
}
