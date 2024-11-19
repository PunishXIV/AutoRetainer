using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public static unsafe class RetainerTable
{
    public static void Draw(OfflineCharacterData data, List<OfflineRetainerData> retainerData, Dictionary<string, (Vector2 start, Vector2 end)> bars)
    {
        if(ImGui.BeginTable("##retainertable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Job");
            ImGui.TableSetupColumn("Venture");
            ImGui.TableSetupColumn("");
            ImGui.TableHeadersRow();
            var retainers = P.GetSelectedRetainers(data.CID);
            foreach(var ret in retainerData)
            {
                if(ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                var adata = Utils.GetAdditionalData(data.CID, ret.Name);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                var start = ImGui.GetCursorPos();
                var selected = retainers.Contains(ret.Name.ToString());
                if(ImGui.Checkbox($"{Censor.Retainer(ret.Name)}", ref selected))
                {
                    if(selected)
                    {
                        retainers.Add(ret.Name.ToString());
                    }
                    else
                    {
                        retainers.Remove(ret.Name.ToString());
                    }
                }
                {
                    if(C.EntrustPlans.TryGetFirst(s => s.Guid == adata.EntrustPlan, out var plan))
                    {
                        ImGui.SameLine();
                        ImGui.PushFont(UiBuilder.IconFont);
                        Vector4? c = plan.ManualPlan ? ImGuiColors.DalamudOrange : null;
                        if(!C.EnableEntrustManager) c = ImGuiColors.DalamudRed;
                        ImGuiEx.Text(c, Lang.IconDuplicate);
                        ImGui.PopFont();
                        ImGuiEx.Tooltip($"Entrust plan \"{plan.Name}\" is active." + (plan.ManualPlan ? "\nThis is manual processing plan" : ""));
                    }
                }
                if(adata.WithdrawGil)
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.Text(Lang.IconGil);
                    ImGui.PopFont();
                }
                Svc.PluginInterface.GetIpcProvider<ulong, string, object>(ApiConsts.OnRetainerPostVentureTaskDraw).SendMessage(data.CID, ret.Name);
                if(adata.IsVenturePlannerActive())
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.Text(Lang.IconPlanner);
                    ImGui.PopFont();
                    if(ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        VentureUtils.BuildUnwrappedList(adata, data, ret);
                        ImGui.EndTooltip();
                    }
                }
                var end = ImGui.GetCursorPos();
                bars[$"{data.CID}{ret.Name}"] = (start, end);
                ImGui.TableNextColumn();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);

                if(ThreadLoadImageHandler.TryGetIconTextureWrap(ret.Job == 0 ? 62143 : 062100 + ret.Job, true, out var t))
                {
                    ImGui.Image(t.ImGuiHandle, new(24, 24));
                }
                else
                {
                    ImGui.Dummy(new(24, 24));
                }
                if(ret.Level > 0)
                {
                    ImGui.SameLine(0, 2);
                    var level = $"{Lang.CharLevel}{ret.Level}";
                    var add = "";
                    var cap = ret.Level < Player.MaxLevel && data.GetJobLevel(ret.Job) == ret.Level;
                    if(cap) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                    ImGuiEx.TextV(level.ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
                    if(cap) ImGui.PopStyleColor();
                    if(C.ShowAdditionalInfo && add != "")
                    {
                        ImGui.SameLine();
                        ImGuiEx.Text(add);
                    }
                }
                ImGui.TableNextColumn();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                if(ret.VentureID != 0 && C.ShowAdditionalInfo)
                {
                    var parts = VentureUtils.GetVentureById(ret.VentureID).GetFancyVentureNameParts(data, ret, out _);
                    if(!parts.Name.IsNullOrEmpty())
                    {
                        ImGuiEx.Text($"{(parts.Level != 0 ? $"{Lang.CharLevel}{parts.Level} " : "")}{parts.Name}");
                        ImGui.SameLine();
                    }
                }
                ImGuiEx.Text($"{(!ret.HasVenture ? "No Venture" : Utils.ToTimeString(ret.GetVentureSecondsRemaining(C.TimerAllowNegative)))}");
                ImGui.TableNextColumn();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                var n = $"{data.CID} {ret.Name} settings";
                if(ImGuiEx.IconButton(FontAwesomeIcon.Cogs, $"{data.CID} {ret.Name}"))
                {
                    ImGui.OpenPopup(n);
                }
                if(ImGuiEx.BeginPopupNextToElement(n))
                {
                    RetainerConfig.Draw(ret, data, adata);
                    ImGui.EndPopup();
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButton(Lang.IconPlanner, $"{data.CID} {ret.Name} planner"))
                {
                    P.VenturePlanner.Open(data, ret);
                }
            }
            ImGui.EndTable();
        }
    }
}
