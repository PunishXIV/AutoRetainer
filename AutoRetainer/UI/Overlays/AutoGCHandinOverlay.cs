using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.UI.Overlays;

internal unsafe class AutoGCHandinOverlay : Window
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
        ImGui.Checkbox("Enable Automatic Expert Delivery", ref AutoGCHandin.Operation);
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var d) && !AutoGCHandin.Operation) 
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGuiEx.EnumCombo("##mode", ref d.GCDeliveryType, x => x != GCDeliveryType.Disabled);
            if (d.GCDeliveryType == GCDeliveryType.Hide_Gear_Set_Items)
            {
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.Text($"\uf071");
                ImGui.PopFont();
            }
            if (d.GCDeliveryType == GCDeliveryType.Show_All_Items)
            {
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.Text($"\uf071\uf071\uf071");
                ImGui.PopFont();
            }
        }
        //1078	Priority Seal Allowance	Company seals earned are increased.	ui/icon/016000/016518.tex	0	0	All Classes	1	dk05th_stup0t		False	False	False	False	False	False	False	False	False	0	1	False	False	15	0	False	0	False	0	False	0	0	0	False
        if (!Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 1078) && InventoryManager.Instance()->GetInventoryItemCount(14946) > 0)
        {
            ImGui.SameLine();
            ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudRed, ImGuiColors.DalamudYellow), $"You can use Priority Seal Allowance");
        }
        height = ImGui.GetWindowSize().Y;
    }

    public override void PostDraw()
    {
        //ImGui.PopStyleVar();
    }
}
