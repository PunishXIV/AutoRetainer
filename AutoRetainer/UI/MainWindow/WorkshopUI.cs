using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainer.UI.MainWindow.MultiModeTab;
using AutoRetainerAPI.Configuration;
using Dalamud.Game;
using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using Lumina.Excel.Sheets;
using PunishLib.ImGuiMethods;


namespace AutoRetainer.UI.MainWindow;

internal static unsafe class WorkshopUI
{
    private static float StatusTextWidth = 0;
    private static List<(ulong cid, ulong frame, Vector2 start, Vector2 end, float percent)> Bars = [];
    internal static void Draw()
    {
        List<OverlayTextData> overlayTexts = [];
        var sortedData = new List<OfflineCharacterData>();
        if(C.NoCurrentCharaOnTop)
        {
            sortedData = C.OfflineData;
        }
        else
        {
            if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
            {
                sortedData.Add(cdata);
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
        foreach(var data in sortedData.Where(x => x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count > 0 && !x.ExcludeWorkshop))
        {
            var search = Ref<string>.Get("SearchChara");
            if(search != "" && !$"{data.Name}@{data.World}".Contains(search, StringComparison.OrdinalIgnoreCase)) continue;
            ImGui.PushID($"Player{data.CID}");
            var rCurPos = ImGui.GetCursorPos();
            float pad = 0;
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.ButtonCheckbox($"\uf21a##{data.CID}", ref data.WorkshopEnabled, 0xFF097000);
            ImGui.PopFont();
            ImGuiEx.Tooltip($"Enable submersibles in multi mode on this character");
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
            ImGui.SameLine(0, 3);
            if(ImGuiEx.IconButton(FontAwesomeIcon.UserCog))
            {
                ImGui.OpenPopup($"popup{data.CID}");
            }
            ImGuiEx.Tooltip($"Configure Character");
            ImGui.SameLine(0, 3);

            if(ImGui.BeginPopup($"popup{data.CID}"))
            {
                CharaConfig.Draw(data, false);
                ImGui.EndPopup();
            }

            if(data.NumSubSlots > data.GetVesselData(VoyageType.Submersible).Count)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.TextV(ImGuiColors.DalamudYellow, "\uf6e3");
                ImGui.PopFont();
                ImGuiEx.Tooltip($"You can construct new submersible ({data.GetVesselData(VoyageType.Submersible).Count}/{data.NumSubSlots})");
                ImGui.SameLine(0, 3);
            }

            if(data.IsNotEnoughSubmarinesEnabled())
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.TextV(ImGuiColors.DalamudOrange, "\ue4ac");
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Some of your submersibles are not enabled");
                ImGui.SameLine(0, 3);
            }

            if(data.IsThereNotAssignedSubmarine())
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.TextV(ImGuiColors.DalamudOrange, "\ue4ab");
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Some of your submersibles are not undertaking voyage");
                ImGui.SameLine(0, 3);
            }

            if(data.AreAnySuboptimalBuildsFound())
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.TextV(ImGuiColors.DalamudOrange, "\uf0ad");
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Unoptimal configurations are found");
                ImGui.SameLine(0, 3);
            }

            if(data.AreAnyInvalidRedeploysActive())
            {
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.TextV(ImGuiColors.DalamudRed, FontAwesomeIcon.ArrowsSpin.ToIconString());
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Redeploy is active while some unlock plans are set as enforced.");
                ImGui.SameLine(0, 3);
            }

            if(C.OldStatusIcons)
            {
                if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.TextV("\uf252");
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"Wait for all deployables is globally enabled.");
                    ImGui.SameLine(0, 3);
                }
                else if(data.MultiWaitForAllDeployables)
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.TextV("\uf252");
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"Wait for all deployables is enabled for this character.");
                    ImGui.SameLine(0, 3);
                }
            }

            data.DrawDCV();
            UIUtils.DrawTeleportIcons(data.CID);
            SharedUI.DrawLockout(data);

            var initCurpos = ImGui.GetCursorPos();
            var lst = data.GetVesselData(VoyageType.Airship).Where(s => data.GetEnabledVesselsData(VoyageType.Airship).Contains(s.Name))
                .Union(data.GetVesselData(VoyageType.Submersible).Where(x => data.GetEnabledVesselsData(VoyageType.Submersible).Contains(x.Name)))
                .Where(x => x.ReturnTime != 0).OrderBy(z => z.GetRemainingSeconds());
            //if (EzThrottler.Throttle("log")) PluginLog.Information($"{lst.Select(x => x.Name).Print()}");
            var lowestVessel = (C.MultiModeWorkshopConfiguration.MultiWaitForAll || data.MultiWaitForAllDeployables) && !data.AreAnyEnabledVesselsReturnInNext(C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting * 60) ? lst.LastOrDefault() : lst.FirstOrDefault();
            if(lowestVessel != default)
            {
                var prog = 1f - lowestVessel.GetRemainingSeconds() / (60f * 60f * 24f);
                prog.ValidateRange(0f, 1f);
                var pcol = prog == 1f ? (C.NoGradient ? 0xbb005000.ToVector4() : GradientColor.Get(0xbb500000.ToVector4(), 0xbb005000.ToVector4())) : 0xbb500000.ToVector4();
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, pcol);
                ImGui.ProgressBar(prog, new(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y * 2), "");
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(initCurpos);
            }

            var colpref = UIUtils.PushColIfPreferredCurrent(data);

            if(ImGuiEx.CollapsingHeader(data.GetCutCharaString(StatusTextWidth) + $"###workshop{data.CID}"))
            {
                MultiModeUI.SetAsPreferred(data);
                if(colpref) ImGui.PopStyleColor();
                pad = ImGui.GetStyle().FramePadding.Y;
                DrawTable(data);
            }
            else
            {
                MultiModeUI.SetAsPreferred(data);
                if(colpref) ImGui.PopStyleColor();
            }

            ImGui.SameLine(0, 0);
            List<(bool, string)> texts = [(data.RepairKits < C.UIWarningDepRepairNum, $"R: {data.RepairKits}"), (data.Ceruleum < C.UIWarningDepTanksNum, $"C: {data.Ceruleum}"), (data.InventorySpace < C.UIWarningDepSlotNum, $"I: {data.InventorySpace}")];
            overlayTexts.Add((new Vector2(ImGui.GetContentRegionMax().X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + ImGui.GetStyle().FramePadding.Y), [.. texts]));
            ImGui.NewLine();

            ImGui.PopID();
        }
        StatusTextWidth = 0f;
        UIUtils.DrawOverlayTexts(overlayTexts, ref StatusTextWidth);
        Bars.RemoveAll(x => x.frame != Svc.PluginInterface.UiBuilder.FrameCount);

        ImGuiEx.LineCentered("WorkshopUI planner button", () =>
        {
            if(ImGui.Button("Open Voyage Route Planner"))
            {
                P.SubmarinePointPlanUI.IsOpen = true;
            }
            ImGui.SameLine();
            if(ImGui.Button("Open Voyage Unlockable Planner"))
            {
                P.SubmarineUnlockPlanUI.IsOpen = true;
            }
        });

        if(C.Verbose)
        {
            if(ImGui.CollapsingHeader("Public debug"))
            {
                try
                {
                    if(!P.TaskManager.IsBusy)
                    {
                        /*if (ImGui.Button("Resend currently selected submarine on previous voyage"))
                        {
                            TaskDeployOnPreviousVoyage.Enqueue();
                        }*/
                        if(ImGui.Button("Select best path"))
                        {
                            TaskCalculateAndPickBestExpRoute.Enqueue();
                        }
                        if(ImGui.Button("Select best path with 1 unlock included"))
                        {
                            TaskCalculateAndPickBestExpRoute.Enqueue(VoyageUtils.GetSubmarineUnlockPlanByGuid(Data.GetAdditionalVesselData(GenericHelpers.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedUnlockPlan) ?? new());
                        }
                        if(ImGui.Button("Select unlock path (up to 5)"))
                        {
                            TaskDeployOnUnlockRoute.EnqueuePickOrCalc(VoyageUtils.GetSubmarineUnlockPlanByGuid(Data.GetAdditionalVesselData(GenericHelpers.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedUnlockPlan) ?? new(), UnlockMode.MultiSelect);
                        }
                        if(ImGui.Button("Select unlock path (only 1)"))
                        {
                            TaskDeployOnUnlockRoute.EnqueuePickOrCalc(VoyageUtils.GetSubmarineUnlockPlanByGuid(Data.GetAdditionalVesselData(GenericHelpers.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedUnlockPlan) ?? new(), UnlockMode.SpamOne);
                        }
                        if(ImGui.Button("Select point planner path"))
                        {
                            var plan = VoyageUtils.GetSubmarinePointPlanByGuid(Data.GetAdditionalVesselData(GenericHelpers.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedPointPlan);
                            if(plan != null)
                            {
                                TaskDeployOnPointPlan.EnqueuePick(plan);
                            }
                            else
                            {
                                DuoLog.Error($"No plan selected!");
                            }
                        }
                        foreach(var x in Data.OfflineSubmarineData)
                        {
                            if(ImGui.Button($"Repair {x.Name} submarine's broken components"))
                            {
                                if(VoyageUtils.GetCurrentWorkshopPanelType() == PanelType.Submersible)
                                {
                                    TaskSelectVesselByName.Enqueue(x.Name, VoyageType.Submersible);
                                    TaskIntelligentRepair.Enqueue(x.Name, VoyageType.Submersible);
                                    P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
                                }
                                else
                                {
                                    Notify.Error("You are not in a submersible menu");
                                }
                            }
                        }
                        if(ImGui.Button("Approach bell"))
                        {
                            TaskInteractWithNearestBell.Enqueue(false);
                        }

                        if(ImGui.Button("Approach panel"))
                        {
                            TaskInteractWithNearestPanel.Enqueue(false);
                        }

                        /*if (ImGui.Button("Redeploy current vessel on previous voyage"))
                        {
                            TaskRedeployPreviousLog.Enqueue();
                        }*/

                        //if (ImGui.Button($"Deploy current submarine on best experience route")) TaskDeployOnBestExpVoyage.Enqueue();

                    }
                    else
                    {
                        ImGuiEx.Text(EColor.RedBright, $"Currently executing: {P.TaskManager.CurrentTask?.Name}");
                    }
                }
                catch(Exception e)
                {
                    ImGuiEx.TextWrapped(e.ToString());
                }
            }
        }
    }

    private static string data1 = "";
    private static VoyageType data2 = VoyageType.Airship;

    private static void DrawTable(OfflineCharacterData data)
    {
        var storePos = ImGui.GetCursorPos();
        foreach(var v in Bars.Where(x => x.cid == data.CID))
        {
            ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
            ImGui.ProgressBar(1f - Math.Min(1f, v.percent),
                new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
            ImGui.PopStyleColor(2);
        }
        ImGui.SetCursorPos(storePos);
        if(ImGui.BeginTable("##retainertable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Structure");
            ImGui.TableSetupColumn("Voyage");
            ImGui.TableSetupColumn("");
            ImGui.TableHeadersRow();
            for(var i = 0; i < data.OfflineAirshipData.Count; i++)
            {
                var vessel = data.OfflineAirshipData[i];
                DrawRow(data, vessel, VoyageType.Airship);
            }
            for(var i = 0; i < data.OfflineSubmarineData.Count; i++)
            {
                var vessel = data.OfflineSubmarineData[i];
                DrawRow(data, vessel, VoyageType.Submersible);
            }
            ImGui.EndTable();
        }
    }

    private static void DrawRow(OfflineCharacterData data, OfflineVesselData vessel, VoyageType type)
    {
        if(type == VoyageType.Airship && data.GetEnabledVesselsData(type).Count == 0 && C.HideAirships) return;
        ImGui.PushID($"{data.CID}/{vessel.Name}/{type}");
        var enabled = type == VoyageType.Airship ? data.EnabledAirships : data.EnabledSubs;
        var adata = data.GetAdditionalVesselData(vessel.Name, type);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
        var start = ImGui.GetCursorPos();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.TextV(type == VoyageType.Airship ? "\ue22d" : "\uf21a");
        ImGui.PopFont();
        ImGui.SameLine();
        var disabled = data.OfflineSubmarineData.Count(x => data.EnabledSubs.Contains(x.Name)) + data.OfflineAirshipData.Count(x => data.EnabledAirships.Contains(x.Name)) >= 4 && !enabled.Contains(vessel.Name);
        if(disabled) ImGui.BeginDisabled();
        ImGuiEx.CollectionCheckbox($"{vessel.Name}##sub", vessel.Name, enabled);
        if(disabled) ImGui.EndDisabled();
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if(adata.VesselBehavior == VesselBehavior.Finalize)
        {
            ImGuiEx.Text(Lang.IconAnchor);
        }
        else if(adata.VesselBehavior == VesselBehavior.Redeploy)
        {
            ImGuiEx.Text(Lang.IconResend);
        }
        else if(adata.VesselBehavior == VesselBehavior.LevelUp)
        {
            ImGuiEx.Text(Lang.IconLevelup);
        }
        else if(adata.VesselBehavior == VesselBehavior.Unlock)
        {
            ImGuiEx.Text(Lang.IconUnlock);
            ImGui.SameLine();
            if(adata.UnlockMode == UnlockMode.WhileLevelling)
            {
                ImGuiEx.Text(Lang.IconLevelup);
            }
            else if(adata.UnlockMode == UnlockMode.SpamOne)
            {
                ImGuiEx.Text(Lang.IconRepeat);
            }
            else if(adata.UnlockMode == UnlockMode.MultiSelect)
            {
                ImGuiEx.Text(Lang.IconPath);
            }
            else
            {
                ImGuiEx.Text(Lang.IconWarning);
            }
        }
        else if(adata.VesselBehavior == VesselBehavior.Use_plan)
        {
            var plan = VoyageUtils.GetSubmarinePointPlanByGuid(adata.SelectedPointPlan);
            var valid = plan != null && plan.Points.Count.InRange(1, 5, true);
            var fast = plan != null && plan.Points.SequenceEqual(adata.Points.Where(x => x != 0).Select(x => (uint)x));
            ImGuiEx.Text(valid?(fast?EColor.GreenBright:null) :EColor.RedBright, Lang.IconPlanner);
            ImGui.PushFont(UiBuilder.DefaultFont);
            if(valid)ImGuiEx.Tooltip(plan.Points.Select(x => $"{VoyageUtils.GetSubmarineExploration(x)?.FancyDestination()}").Print("\n"));
            ImGui.PopFont();
        }
        else
        {
            ImGuiEx.Text(Lang.IconWarning);
        }
        ImGui.PopFont();
        if(adata.IndexOverride > 0)
        {
            ImGui.SameLine();
            ImGuiEx.Text(ImGuiColors.DalamudGrey3, $"Index override: {adata.IndexOverride}");
        }
        var end = ImGui.GetCursorPos();
        var p = vessel.GetRemainingSeconds() / (60f * 60f * 24f);
        if(vessel.ReturnTime != 0) Bars.Add((data.CID, Svc.PluginInterface.UiBuilder.FrameCount, start, end, vessel.ReturnTime == 0 ? 0 : p.ValidateRange(0f, 1f)));
        ImGui.TableNextColumn();
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
        if(adata.Level > 0)
        {
            var lvlf = 0;
            if(adata.CurrentExp > 0 && adata.NextLevelExp > 0)
            {
                lvlf = (int)(adata.CurrentExp * 100f / adata.NextLevelExp);
            }
            ImGuiEx.TextV(Lang.CharLevel + $"{adata.Level}".ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey3, $".{lvlf:D2}".ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(adata.IsUnoptimalBuild(out var justification) ? ImGuiColors.DalamudOrange : null, adata.GetSubmarineBuild());
            if(justification != null)
            {
                ImGuiEx.Tooltip(justification);
            }
        }
        ImGui.TableNextColumn();
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);

        if(vessel.ReturnTime == 0)
        {
            ImGuiEx.Text($"No voyage");
        }
        else
        {
            List<string> points = [];
            foreach(var x in adata.Points)
            {
                if(x != 0)
                {
                    var d = Svc.Data.GetExcelSheet<SubmarineExploration>(ClientLanguage.Japanese).GetRowOrDefault(x);
                    if(d != null && d.Value.Location.ToString().Length > 0)
                    {
                        points.Add(d.Value.Location.ToString());
                    }
                }
            }
            ImGuiEx.Text(points.Join(""));
            ImGui.SameLine();
            ImGuiEx.Text(vessel.GetRemainingSeconds() > 0 ? $"{VoyageUtils.Seconds2Time(vessel.GetRemainingSeconds())}" : "Voyage completed");
        }
        ImGui.TableNextColumn();
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
        var n = $"{data.CID} {vessel.Name} settings";
        if(ImGuiEx.IconButton(FontAwesomeIcon.Cogs, $"{data.CID} {vessel.Name}"))
        {
            ImGui.OpenPopup(n);
        }
        if(ImGuiEx.BeginPopupNextToElement(n))
        {
            ImGui.CollapsingHeader($"{vessel.Name} - {Censor.Character(data.Name)} Configuration  ##conf", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
            ImGuiEx.Text($"Vessel behavior:");
            ImGuiEx.EnumCombo("##vbeh", ref adata.VesselBehavior);
            if(adata.VesselBehavior == VesselBehavior.Unlock)
            {
                ImGuiEx.Text($"Unlock mode:");
                ImGuiEx.EnumCombo("##umode", ref adata.UnlockMode, Lang.UnlockModeNames);
                var currentPlan = VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan) ?? VoyageUtils.GetDefaultSubmarineUnlockPlan(false);
                var isDefault = VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan) == null;
                var text = Environment.TickCount64 % 2000 > 1000 ? "Unlocking every point" : "No or unknown plan selected";
                if(ImGui.BeginCombo("##uplan", (currentPlan?.Name ?? text) + (isDefault ? " (default)" : ""), ImGuiComboFlags.HeightLarge))
                {
                    if(ImGui.Button("Open editor"))
                    {
                        P.SubmarineUnlockPlanUI.IsOpen = true;
                        P.SubmarineUnlockPlanUI.SelectedPlanGuid = adata.SelectedUnlockPlan;
                    }
                    ImGui.SameLine();
                    if(ImGui.Button("Clear plan"))
                    {
                        adata.SelectedUnlockPlan = Guid.Empty.ToString();
                    }
                    foreach(var x in C.SubmarineUnlockPlans)
                    {
                        if(ImGui.Selectable($"{x.Name}##{x.GUID}"))
                        {
                            adata.SelectedUnlockPlan = x.GUID;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            if(adata.VesselBehavior == VesselBehavior.Use_plan)
            {
                var currentPlan = VoyageUtils.GetSubmarinePointPlanByGuid(adata.SelectedPointPlan);
                if(ImGui.BeginCombo("##uplan", currentPlan.GetPointPlanName(), ImGuiComboFlags.HeightLarge))
                {
                    if(ImGui.Button("Open editor"))
                    {
                        P.SubmarinePointPlanUI.IsOpen = true;
                        P.SubmarinePointPlanUI.SelectedPlanGuid = adata.SelectedPointPlan;
                    }
                    ImGui.SameLine();
                    if(ImGui.Button("Clear plan"))
                    {
                        adata.SelectedPointPlan = Guid.Empty.ToString();
                    }
                    foreach(var x in C.SubmarinePointPlans)
                    {
                        if(ImGui.Selectable($"{x.GetPointPlanName()}##{x.GUID}"))
                        {
                            adata.SelectedPointPlan = x.GUID;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            ImGui.Separator();
            ImGuiEx.SetNextItemWidthScaled(150f);
            ImGuiEx.SliderInt("Index override", ref adata.IndexOverride, 0, 4, adata.IndexOverride == 0 ? "Disabled" : $"{adata.IndexOverride}");
            ImGuiComponents.HelpMarker($"If your vessel order in AutoRetainer is different than in voyage panel menu, you must use this feature to set correct index to incorrectly ordered vessels. Make sure that index is matching order in control panel.");
            if(ImGui.CollapsingHeader("I have recently renamed this vessel"))
            {
                if(ImGui.BeginCombo("##selprev", "Select previous vessel name", ImGuiComboFlags.HeightLarge))
                {
                    var datas = ((Func<Dictionary<string, AdditionalVesselData>>)delegate
                    {
                        if(type == VoyageType.Airship) return data.AdditionalAirshipData;
                        if(type == VoyageType.Submersible) return data.AdditionalSubmarineData;
                        throw new ArgumentOutOfRangeException(nameof(type));
                    })();
                    foreach(var x in datas)
                    {
                        var d = data.GetVesselData(type).Any(z => z.Name == x.Key);
                        if(d) ImGui.BeginDisabled();
                        if(ImGui.Selectable($"{x.Key}"))
                        {
                            new TickScheduler(() =>
                            {
                                var copyTo = vessel.Name;
                                var newData = x.Value.JSONClone();
                                var toDelete = x.Key;
                                datas[copyTo] = x.Value;
                                datas.Remove(toDelete);
                                Notify.Success($"Moved data from {toDelete} to {copyTo}");
                            });
                        }
                        if(d) ImGui.EndDisabled();
                    }
                    ImGui.EndCombo();
                }
            }
            if(C.Verbose)
            {
                if(ImGui.Button("Fake ready")) vessel.ReturnTime = (uint)P.Time;
                if(ImGui.Button("Fake ready+")) vessel.ReturnTime += 60u * (ImGui.GetIO().KeyCtrl ? 10u : 1u) * (ImGui.GetIO().KeyShift ? 10u : 1u);
                if(ImGui.Button("Fake ready-")) vessel.ReturnTime -= 60u * (ImGui.GetIO().KeyCtrl ? 10u : 1u) * (ImGui.GetIO().KeyShift ? 10u : 1u);
                if(ImGui.Button("Fake unready")) vessel.ReturnTime = (uint)(P.Time + 9999);
            }
            ImGui.EndPopup();
        }
        ImGui.PopID();
    }
}
