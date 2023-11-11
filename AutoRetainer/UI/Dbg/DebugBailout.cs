using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal static class DebugBailout
    {
        internal static void Draw()
        {
            ImGui.Checkbox(nameof(BailoutManager.SimulateStuckOnQuit), ref BailoutManager.SimulateStuckOnQuit);
            ImGui.Checkbox(nameof(BailoutManager.SimulateStuckOnVoyagePanel), ref BailoutManager.SimulateStuckOnVoyagePanel);
            ImGuiEx.Text($"NoSelectString: {Environment.TickCount64 - BailoutManager.NoSelectString}");
        }
    }
}
