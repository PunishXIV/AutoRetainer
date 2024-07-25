using ECommons.Throttlers;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugThrottle : DebugSectionBase
{
    public override void Draw()
    {
        EzThrottler.ImGuiPrintDebugInfo();
        ImGui.Separator();
        FrameThrottler.ImGuiPrintDebugInfo();
    }
}
