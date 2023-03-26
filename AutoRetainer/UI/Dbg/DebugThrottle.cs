using ECommons.Throttlers;

namespace AutoRetainer.UI.Dbg;

internal static unsafe class DebugThrottle
{
    internal static void Draw()
    {
        EzThrottler.ImGuiPrintDebugInfo();
    }
}
