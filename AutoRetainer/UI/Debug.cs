using AutoRetainer.UI.Dbg;

namespace AutoRetainer.UI;

internal unsafe static class Debug
{
    internal static void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "Anything can happen here.");
        Safe(delegate
        {
            ImGuiEx.EzTabBar("DebugBar",
                ("Venture", DebugVenture.Draw, null, true),
                ("Scheduler", DebugScheduler.Draw, null, true),
                ("Multi", DebugMulti.Draw, null, true),
                ("Throttle", DebugThrottle.Draw, null, true),
                ("IPC", DebugIPC.Draw, null, true),
                ("Mist", DebugMisc.Draw, null, true)
                );
        });
    }
}
