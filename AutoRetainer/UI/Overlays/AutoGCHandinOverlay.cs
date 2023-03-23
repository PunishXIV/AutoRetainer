namespace AutoRetainer.UI.Overlays;

internal class AutoGCHandinOverlay : Window
{
    internal float height;
    public AutoGCHandinOverlay() : base("AutoRetainer GC Handin overlay", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.AlwaysAutoResize, true)
    {
        RespectCloseHotkey = false;
    }

    public override void PreDraw()
    {
        //ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    public override void Draw()
    {
        ImGui.Checkbox("Automatically hand in all listed items", ref AutoGCHandin.Operation);
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var d) && !AutoGCHandin.Operation) 
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGuiEx.EnumCombo("##mode", ref d.GCDeliveryType, x => x != GCDeliveryType.Disabled);
        }
        height = ImGui.GetWindowSize().Y;
    }

    public override void PostDraw()
    {
        //ImGui.PopStyleVar();
    }
}
