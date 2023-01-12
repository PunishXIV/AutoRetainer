using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.UI;

internal unsafe class Retainers
{
    static Dictionary<int, (Vector2 start, Vector2 end)> bars = new();
    internal static void Draw()
    {
        if (!(P.retainerManager.Ready && Svc.ClientState.LocalPlayer != null))
        {
            ImGuiEx.Text("Data Not Ready");
            return;
        }

        var slots = Utils.GetInventoryFreeSlotCount();
        var ventures = InventoryManager.Instance()->GetInventoryItemCount(21072);
        ImGuiEx.Text($"Inventory slots: ");
        ImGui.SameLine(0, 0);
        ImGuiEx.Text(slots < P.retainerManager.Count ? ImGuiColors.DalamudRed : slots < 14 * P.retainerManager.Count ? ImGuiColors.DalamudOrange : ImGuiColors.ParsedGreen,
            $"{slots}");
        ImGui.SameLine();
        ImGuiEx.Text(ImGuiColors.DalamudGrey3, "|");
        ImGui.SameLine();
        ImGuiEx.Text("Ventures: ");
        ImGui.SameLine(0, 0);
        ImGuiEx.Text(ventures < 2 * P.retainerManager.Count ? ImGuiColors.DalamudRed : ventures < 24 * P.retainerManager.Count ? ImGuiColors.DalamudOrange : ImGuiColors.ParsedGreen,
            $"{ventures}");
        ImGuiComponents.HelpMarker("The plugin will automatically disable itself at < 2 Ventures or inventory slots available.");
        var storePos = ImGui.GetCursorPos();
        for (var i = 0; i < P.retainerManager.Count; i++)
        {
            if (bars.TryGetValue(i, out var v))
            {
                var ret = P.retainerManager.Retainer(i);
                if (ret.VentureID == 0 || !ret.Available || ret.Name.ToString().IsNullOrEmpty()) continue;
                ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                ImGui.ProgressBar(1f - Math.Min(1f, (float)ret.GetVentureSecondsRemaining(false) / (60f * 60f)),
                    new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
                ImGui.PopStyleColor(2);
            }
        }
        ImGui.SetCursorPos(storePos);
        ImGui.BeginTable("##ertainertable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Venture");
        ImGui.TableSetupColumn("Interaction");
        ImGui.TableHeadersRow();
        var retainers = P.GetSelectedRetainers(Svc.ClientState.LocalContentId);
        for (var i = 0; i < P.retainerManager.Count; i++)
        {
            var ret = P.retainerManager.Retainer(i);
            if (!ret.Available || ret.Name.ToString().IsNullOrEmpty()) continue;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            var start = ImGui.GetCursorPos();
            var selected = retainers.Contains(ret.Name.ToString());
            if (ImGui.Checkbox($"Retainer {(P.config.NoNames ? (i + 1) : ret.Name)}", ref selected))
            {
                if (selected)
                {
                    retainers.Add(ret.Name.ToString());
                }
                else
                {
                    retainers.Remove(ret.Name.ToString());
                }
            }
            var end = ImGui.GetCursorPos();
            bars[i] = (start, end);
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            ImGuiEx.Text($"{(ret.VentureID == 0 ? "No Venture" : Utils.ToTimeString(ret.GetVentureSecondsRemaining(false)))}");
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            ImGuiEx.Text($"{Utils.ToTimeString(Scheduler.GetRemainingBanTime(ret.Name.ToString()))}");
        }
        ImGui.EndTable();
        ImGuiEx.ImGuiLineCentered("AYSButtonClear Interaction Timeouts", delegate
        {
            if (ImGui.SmallButton("Clear Interaction Timeouts"))
            {
                Scheduler.Bans.Clear();
            }
        });
    }
}
