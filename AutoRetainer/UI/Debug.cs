using AutoRetainer.UI.Dbg;
using AutoRetainer.UI.Settings;

namespace AutoRetainer.UI;

internal unsafe static class Debug
{
    internal static void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "Anything can happen here.");
        Safe(delegate
        {
            ImGuiEx.EzTabBar("DebugBar",
                ("Beta", Beta.Draw, null, true),
                ("Venture", DebugVenture.Draw, null, true),
                ("Scheduler", DebugScheduler.Draw, null, true),
                ("Multi", DebugMulti.Draw, null, true),
                ("Throttle", DebugThrottle.Draw, null, true),
                ("IPC", DebugIPC.Draw, null, true),
                ("Misc", DebugMisc.Draw, null, true),
                ("GCAuto", DebugGCAuto.Draw, null, true),
                ("Artisan", DebugArtisan.Draw, null, true),
                ("Voyage", DebugVoyage.Draw, null, true),
                ("AtkReader", DebugReader.Draw, null, true)
                );
        });
    }
}
