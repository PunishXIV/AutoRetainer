namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class DebugBailout : DebugUIEntry
{
    public override void Draw()
    {
        ImGui.Checkbox(nameof(BailoutManager.SimulateStuckOnQuit), ref BailoutManager.SimulateStuckOnQuit);
        ImGui.Checkbox(nameof(BailoutManager.SimulateStuckOnVoyagePanel), ref BailoutManager.SimulateStuckOnVoyagePanel);
        ImGuiEx.Text($"NoSelectString: {Environment.TickCount64 - BailoutManager.NoSelectString}");
        ImGuiEx.Text($"LobbyStuck: {Environment.TickCount64 - BailoutManager.CharaSelectStuck}");
    }
}
