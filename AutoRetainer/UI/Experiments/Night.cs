using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Experiments
{
    internal static class Night
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped($"Night mode:\n" +
                $"- Wait on login screen option is forcefully enabled\n" +
                $"- Built-in FPS limiter restrictions forcefully applied\n" +
                $"- While unfocused and awaiting, game is limited to 0.2 FPS\n" +
                $"- It may look like game hung up, but let it up to 5 seconds to wake up after you reactivate game window.\n" +
                $"- By default, only Deployables are enabled in Night mode\n" +
                $"- After disabling Night mode, Bailout manager will activate to relog you back to the game.");
            ImGui.Checkbox("Show Night mode checkbox", ref C.ShowNightMode);
            ImGui.Checkbox("Do retainers in Night mode", ref C.NightModeRetainers);
            ImGui.Checkbox("Do deployables in Night mode", ref C.NightModeDeployables);
        }
    }
}
