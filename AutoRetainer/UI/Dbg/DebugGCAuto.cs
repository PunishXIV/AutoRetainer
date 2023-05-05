using AutoRetainer.Scheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal static unsafe class DebugGCAuto
    {
        internal static void Draw()
        {
            ImGuiEx.Text($"GetGCSealMultiplier: {Utils.GetGCSealMultiplier()}");
            if (ImGui.Button(nameof(GCHandlers.SetMaxVenturesExchange))) GCHandlers.SetMaxVenturesExchange();
        }
    }
}
