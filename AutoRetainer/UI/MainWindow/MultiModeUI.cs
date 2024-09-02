using AutoRetainer.Internal;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
using Lumina.Excel.GeneratedSheets;
using PunishLib.ImGuiMethods;
using ThreadLoadImageHandler = ECommons.ImGuiMethods.ThreadLoadImageHandler;

namespace AutoRetainer.UI.MainWindow;

internal static unsafe class MultiModeUI
{
    internal static bool JustRelogged = false;
    private static Dictionary<string, (Vector2 start, Vector2 end)> bars = [];
    private static float StatusTextWidth = 0f;
    internal static void Draw()
    {
        List<OverlayTextData> overlayTexts = [];
        SharedUI.DrawExcludedNotification(true, false);
        C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
        var sortedData = new List<OfflineCharacterData>();
        var shouldExpand = false;
        var doExpand = JustRelogged && !C.NoCurrentCharaOnTop;
        JustRelogged = false;
        if(C.NoCurrentCharaOnTop)
        {
            sortedData = C.OfflineData;
        }
        else
        {
            if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
            {
                sortedData.Add(cdata);
                shouldExpand = true;
            }
            foreach(var x in C.OfflineData)
            {
                if(x.CID != Svc.ClientState.LocalContentId)
                {
                    sortedData.Add(x);
                }
            }
        }
        UIUtils.DrawSearch();
        for(var index = 0; index < sortedData.Count; index++)
        {
            var data = sortedData[index];
            if(data.World.IsNullOrEmpty() || data.ExcludeRetainer) continue;
            var search = Ref<string>.Get("SearchChara");
            if(search != "" && !$"{data.Name}@{data.World}".Contains(search, StringComparison.OrdinalIgnoreCase)) continue;
            ImGui.PushID(data.CID.ToString());
            var rCurPos = ImGui.GetCursorPos();
            var colen = false;
            if(data.Enabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);
                colen = true;
            }
            if(ImGuiEx.IconButton(Lang.IconMultiMode))
            {
                data.Enabled = !data.Enabled;
            }
            if(colen) ImGui.PopStyleColor();
            ImGuiEx.Tooltip($"Enable multi-mode for this character");
            ImGui.SameLine(0, 3);
            if(ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen))
            {
                if(MultiMode.Relog(data, out var error, RelogReason.ConfigGUI))
                {
                    Notify.Success("Relogging...");
                }
                else
                {
                    Notify.Error(error);
                }
            }
            if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                Copy($"/ays relog {data.Name}@{data.World}");
            }
            ImGuiEx.Tooltip($"Left click - relog to this character\nRight click - copy relog command into clipboard");
            ImGui.SameLine(0, 3);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Cog))
            {
                ImGui.OpenPopup($"popup{data.CID}");
            }
            ImGuiEx.Tooltip($"Configure Character");
            ImGui.SameLine(0, 3);

            if(ImGui.BeginPopup($"popup{data.CID}"))
            {
                SharedUI.DrawMultiModeHeader(data);
                if(ImGuiGroup.BeginGroupBox("General Character Specific Settings"))
                {
                    SharedUI.DrawServiceAccSelector(data);
                    SharedUI.DrawPreferredCharacterUI(data);
                    ImGui.Checkbox("List & Process Retainers in Display Order", ref data.ShowRetainersInDisplayOrder);

                    ImGuiEx.Text($"Automatic Grand Company Expert Delivery:");
                    if(!AutoGCHandin.Operation)
                    {
                        ImGuiEx.SetNextItemWidthScaled(200f);
                        ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);
                    }
                    else
                    {
                        ImGuiEx.Text($"Can't change this now");
                    }
                    /*if(ImGui.Button("Entrust Duplicates Exclusions"))
                    {
                        P.DuplicateBlacklistSelector.IsOpen = true;
                        P.DuplicateBlacklistSelector.SelectedData = data;
                    }*/
                    ImGuiGroup.EndGroupBox();
                }
                SharedUI.DrawExcludeReset(data);
                ImGui.EndPopup();
            }
            data.DrawDCV();
            UIUtils.DrawTeleportIcons(data.CID);

            var initCurpos = ImGui.GetCursorPos();
            var lowestRetainer = C.MultiModeRetainerConfiguration.MultiWaitForAll ? data.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).LastOrDefault() : data.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).FirstOrDefault();
            if(lowestRetainer != default)
            {
                var prog = Math.Max(0, (3600 - lowestRetainer.GetVentureSecondsRemaining(false)) / 3600f);
                var pcol = prog == 1f ? GradientColor.Get(0xbb500000.ToVector4(), 0xbb005000.ToVector4()) : 0xbb500000.ToVector4();
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, pcol);
                ImGui.ProgressBar(prog, new(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y * 2), "");
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(initCurpos);
            }
            var col = data.Preferred;
            if(col)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, GradientColor.Get(ImGui.GetStyle().Colors[(int)ImGuiCol.Text], ImGuiColors.ParsedGreen));
            }
            if(shouldExpand && doExpand)
            {
                ImGui.SetNextItemOpen(index == 0);
            }
            if(ImGui.CollapsingHeader(data.GetCutCharaString(StatusTextWidth) + $"###workshop{data.CID}" + $"###chara{data.CID}"))
            {
                SetAsPreferred(data);
                if(col)
                {
                    ImGui.PopStyleColor();
                    col = false;
                }
                var enabledRetainers = data.GetEnabledRetainers();
                ImGui.PushID(data.CID.ToString());

                var storePos = ImGui.GetCursorPos();
                var retainerData = data.RetainerData;
                if(data.ShowRetainersInDisplayOrder)
                {
                    retainerData = retainerData.OrderBy(x => x.DisplayOrder).ToList();
                }
                foreach(var ret in retainerData)
                {
                    if(bars.TryGetValue($"{data.CID}{ret.Name}", out var v))
                    {
                        if(!ret.HasVenture || ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                        ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
                        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                        ImGui.ProgressBar(1f - Math.Min(1f, ret.GetVentureSecondsRemaining(false) / (60f * 60f)),
                            new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
                        ImGui.PopStyleColor(2);
                    }
                }
                ImGui.SetCursorPos(storePos);
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
                                ImGuiEx.Text(plan.ManualPlan ? ImGuiColors.DalamudOrange : null, Lang.IconDuplicate);
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
                            if(adata.Ilvl > -1 && !VentureUtils.IsDoL(ret.Job))
                            {
                                add += $"{Lang.CharItemLevel}{adata.Ilvl}";
                            }
                            if((adata.Gathering > -1 || adata.Perception > -1) && VentureUtils.IsDoL(ret.Job))
                            {
                                add += $"{Lang.CharPlant}{adata.Gathering}/{adata.Perception}";
                            }
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
                                var c = parts.YieldRate == 4 ? ImGuiColors.ParsedGreen : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                                ImGuiEx.Text(c, $"{(parts.Level != 0 ? $"{Lang.CharLevel}{parts.Level} " : "")}{parts.Name}");
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
                            ImGui.CollapsingHeader($"{Censor.Retainer(ret.Name)} - {Censor.Character(data.Name)} Configuration  ##conf", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
                            ImGuiEx.Text($"Additional Post-venture Tasks:");
                            //ImGui.Checkbox($"Entrust Duplicates", ref adata.EntrustDuplicates);
                            var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == adata.EntrustPlan);
                            ImGuiEx.TextV($"Entrust Items:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(150f);
                            if(ImGui.BeginCombo($"##select", selectedPlan?.Name ?? "Disabled", ImGuiComboFlags.HeightLarge))
                            {
                                if(ImGui.Selectable("Disabled")) adata.EntrustPlan = Guid.Empty;
                                for(int i = 0; i < C.EntrustPlans.Count; i++)
                                {
                                    var plan = C.EntrustPlans[i];
                                    ImGui.PushID(plan.Guid.ToString());
                                    if(ImGui.Selectable(plan.Name, plan == selectedPlan))
                                    {
                                        adata.EntrustPlan = plan.Guid;
                                    }
                                    ImGui.PopID();
                                }
                                ImGui.EndCombo();
                            }
                            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "Copy entrust plan to..."))
                            {
                                ImGui.OpenPopup($"CopyEntrustPlanTo");
                            }
                            if(ImGui.BeginPopup("CopyEntrustPlanTo"))
                            {
                                if(ImGui.Selectable("To all other retainers of this character"))
                                {
                                    int cnt = 0;
                                    foreach(var x in data.RetainerData)
                                    {
                                        cnt++;
                                        Utils.GetAdditionalData(data.CID, x.Name).EntrustPlan = adata.EntrustPlan;
                                    }
                                    Notify.Info($"Changed {cnt} retainers");
                                }
                                if(ImGui.Selectable("To all other retainers without entrust plan of this character"))
                                {
                                    foreach(var x in data.RetainerData)
                                    {
                                        var cnt = 0;
                                        if(!C.EntrustPlans.Any(s => s.Guid == adata.EntrustPlan))
                                        {
                                            Utils.GetAdditionalData(data.CID, x.Name).EntrustPlan = adata.EntrustPlan;
                                            cnt++;
                                        }
                                        Notify.Info($"Changed {cnt} retainers");
                                    }
                                }
                                if(ImGui.Selectable("To all other retainers of ALL characters"))
                                {
                                    var cnt = 0;
                                    foreach(var offlineData in C.OfflineData)
                                    {
                                        foreach(var x in offlineData.RetainerData)
                                        {
                                            Utils.GetAdditionalData(offlineData.CID, x.Name).EntrustPlan = adata.EntrustPlan;
                                            cnt++;
                                        }
                                    }
                                    Notify.Info($"Changed {cnt} retainers");
                                }
                                if(ImGui.Selectable("To all other retainers without entrust plan of ALL characters"))
                                {
                                    var cnt = 0;
                                    foreach(var offlineData in C.OfflineData)
                                    {
                                        foreach(var x in offlineData.RetainerData)
                                        {
                                            var a = Utils.GetAdditionalData(data.CID, x.Name);
                                            if(!C.EntrustPlans.Any(s => s.Guid == a.EntrustPlan))
                                            {
                                                a.EntrustPlan = adata.EntrustPlan;
                                                cnt++;
                                            }
                                        }
                                    }
                                    Notify.Info($"Changed {cnt} retainers");
                                }
                                ImGui.EndPopup();
                            }
                            ImGui.Checkbox($"Withdraw/Deposit Gil", ref adata.WithdrawGil);
                            if(adata.WithdrawGil)
                            {
                                if(ImGui.RadioButton("Withdraw", !adata.Deposit)) adata.Deposit = false;
                                if(ImGui.RadioButton("Deposit", adata.Deposit)) adata.Deposit = true;
                                ImGuiEx.SetNextItemWidthScaled(200f);
                                ImGui.InputInt($"Amount, %", ref adata.WithdrawGilPercent.ValidateRange(1, 100), 1, 10);
                            }
                            ImGui.Separator();
                            Svc.PluginInterface.GetIpcProvider<ulong, string, object>(ApiConsts.OnRetainerSettingsDraw).SendMessage(data.CID, ret.Name);
                            if(C.Verbose)
                            {
                                if(ImGui.Button("Fake ready"))
                                {
                                    ret.VentureEndsAt = 1;
                                }
                                if(ImGui.Button("Fake unready"))
                                {
                                    ret.VentureEndsAt = P.Time + 60 * 60;
                                }
                            }
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
                ImGui.Dummy(new(2, 2));
                ImGui.PopID();
            }
            else
            {
                SetAsPreferred(data);
                if(col)
                {
                    ImGui.PopStyleColor();
                    col = false;
                }
            }
            ImGui.SameLine(0, 0);
            List<(bool, string)> texts = [(data.Ventures < C.UIWarningRetVentureNum, $"V: {data.Ventures}"), (data.InventorySpace < C.UIWarningRetSlotNum, $"I: {data.InventorySpace}")];
            if(C.CharEqualize && MultiMode.Enabled)
            {
                texts.Insert(0, (false, $"C: {MultiMode.CharaCnt.GetOrDefault(data.CID)}"));
            }
            overlayTexts.Add((new Vector2(ImGui.GetContentRegionMax().X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + ImGui.GetStyle().FramePadding.Y), [.. texts]));
            ImGui.NewLine();
            ImGui.PopID();
        }
        StatusTextWidth = 0f;
        UIUtils.DrawOverlayTexts(overlayTexts, ref StatusTextWidth);

        if(C.Verbose && ImGui.CollapsingHeader("Debug"))
        {
            ImGuiEx.Text($"GetCurrentTargetCharacter: {MultiMode.GetCurrentTargetCharacter()}");
            //ImGuiEx.Text($"Yes Already: {YesAlready.IsEnabled()}");
            ImGuiEx.Text($"IsCurrentCharacterDone: {MultiMode.IsCurrentCharacterDone()}");
            ImGuiEx.Text($"NextInteraction: {Math.Max(0, MultiMode.NextInteractionAt - Environment.TickCount64)}");
            ImGuiEx.Text($"EnsureCharacterValidity: {MultiMode.EnsureCharacterValidity(true)}");
            ImGuiEx.Text($"IsInteractionAllowed: {MultiMode.IsInteractionAllowed()}");
            ImGuiEx.Text($"GetPreferredCharacter: {MultiMode.GetPreferredCharacter()}");
            ImGuiEx.Text($"IsAllRetainersHaveMoreThan15Mins: {MultiMode.IsAllRetainersHaveMoreThan15Mins()}");
            ImGuiEx.Text($"Target ?? Preferred: {MultiMode.GetCurrentTargetCharacter() ?? MultiMode.GetPreferredCharacter()}");
            //ImGuiEx.Text($"GetAutoAfkOpt: {MultiMode.GetAutoAfkOpt()}");
            //ImGuiEx.Text($"AutoAfkValue: {ConfigModule.Instance()->GetIntValue(145)}");
            ImGuiEx.Text($"LastLongin: {MultiMode.LastLogin:X16}");
            ImGuiEx.Text($"AnyRetainersAvailable: {MultiMode.AnyRetainersAvailable()}");
            ImGuiEx.Text($"IsAnySelectedRetainerFinishesWithin, 60: {MultiMode.IsAnySelectedRetainerFinishesWithin(60)}");
            ImGuiEx.Text($"IsAnySelectedRetainerFinishesWithin, 5*60: {MultiMode.IsAnySelectedRetainerFinishesWithin(5 * 60)}");
            foreach(var data in C.OfflineData)
            {
                ImGuiEx.Text($"Chatacter {data}\n  GetNeededVentureAmount: {data.GetNeededVentureAmount()}");
            }
        }
    }

    internal static void SetAsPreferred(OfflineCharacterData x)
    {
        if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            if(x.Preferred)
            {
                x.Preferred = false;
            }
            else
            {
                C.OfflineData.Each(x => x.Preferred = false);
                x.Preferred = true;
            }
        }
    }
}
