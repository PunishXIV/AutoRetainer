using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI.Configuration;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI
{
    internal static unsafe class WorkshopUI
    {
        static List<(ulong cid, ulong frame, Vector2 start, Vector2 end, float percent)> bars = new();
        internal static void Draw()
        {
            var sortedData = new List<OfflineCharacterData>();
            if (C.NoCurrentCharaOnTop)
            {
                sortedData = C.OfflineData;
            }
            else
            {
                if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
                {
                    sortedData.Add(cdata);
                }
                foreach (var x in C.OfflineData)
                {
                    if (x.CID != Svc.ClientState.LocalContentId)
                    {
                        sortedData.Add(x);
                    }
                }
            }
            foreach (var data in sortedData.Where(x => x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count > 0 && !x.ExcludeWorkshop))
            {
                ImGui.PushID($"Player{data.CID}");
                var rCurPos = ImGui.GetCursorPos();
                float pad = 0;
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox($"\uf21a##{data.CID}", ref data.WorkshopEnabled, 0xFF097000);
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Enable submersibles in multi mode on this character");
                ImGui.SameLine(0,3);
                if (ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen))
                {
                    if (MultiMode.Active)
                    {
                        foreach (var z in C.OfflineData)
                        {
                            z.Preferred = false;
                        }
                        Notify.Warning("Preferred character has been reset");
                    }
                    if (MultiMode.Relog(data, out var error))
                    {
                        Notify.Success("Relogging...");
                    }
                    else
                    {
                        Notify.Error(error);
                    }
                }
                ImGui.SameLine(0, 3);

                var initCurpos = ImGui.GetCursorPos();
                var lst = data.GetVesselData(VoyageType.Airship).Where(s => data.GetEnabledVesselsData(VoyageType.Airship).Contains(s.Name))
                    .Union(data.GetVesselData(VoyageType.Submersible).Where(x => data.GetEnabledVesselsData(VoyageType.Submersible).Contains(x.Name)))
                    .Where(x => x.ReturnTime != 0).OrderBy(z => z.GetRemainingSeconds());
                //if (EzThrottler.Throttle("log")) PluginLog.Information($"{lst.Select(x => x.Name).Print()}");
                var lowestRetainer = C.MultiModeWorkshopConfiguration.MultiWaitForAll?lst.LastOrDefault():lst.FirstOrDefault();
                if (lowestRetainer != default)
                {
                    var prog = 1f - ((float)lowestRetainer.GetRemainingSeconds() / (60f * 60f * 24f));
                    prog.ValidateRange(0f, 1f);
                    var pcol = prog == 1f ? GradientColor.Get(0xbb500000.ToVector4(), 0xbb005000.ToVector4()) : 0xbb500000.ToVector4();
                    ImGui.PushStyleColor(ImGuiCol.PlotHistogram, pcol);
                    ImGui.ProgressBar(prog, new(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y * 2), "");
                    ImGui.PopStyleColor();
                    ImGui.SetCursorPos(initCurpos);
                }

                if (data.NumSubSlots > data.GetVesselData(VoyageType.Submersible).Count)
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.TextV(ImGuiColors.DalamudYellow, "\uf6e3");
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"You can construct new submarine ({data.GetVesselData(VoyageType.Submersible).Count}/{data.NumSubSlots})");
                    ImGui.SameLine(0, 3);
                }

                if (ImGuiEx.CollapsingHeader(Censor.Character(data.Name, data.World)))
                {
                    pad = ImGui.GetStyle().FramePadding.Y;
                    DrawTable(data);
                }

                var rightText = $"R: {data.RepairKits} | C: {data.Ceruleum} | I: {data.InventorySpace}";
                var subNum = data.GetEnabledVesselsData(VoyageType.Submersible).Count();
                var col = data.RepairKits < C.UIWarningDepRepairNum || data.Ceruleum < C.UIWarningDepTanksNum || data.InventorySpace < C.UIWarningDepSlotNum;
                var cur = ImGui.GetCursorPos();
                ImGui.SameLine();
                ImGui.SetCursorPos(new(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + pad));
                ImGuiEx.Text(col?ImGuiColors.DalamudOrange:null, rightText);

                ImGui.PopID();
            }
            bars.RemoveAll(x => x.frame != Svc.PluginInterface.UiBuilder.FrameCount);

            ImGuiEx.ImGuiLineCentered("WorkshopUI planner button", () =>
            {
                if (ImGui.Button("Open unlock plan editor"))
                {
                    P.SubmarineUnlockPlanUI.IsOpen = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Open point plan editor"))
                {
                    P.SubmarinePointPlanUI.IsOpen = true;
                }
            });

            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox($"Resend airships and submarines on voyage CP access", ref C.SubsAutoResend);
                ImGui.Checkbox($"Only finalize reports", ref C.SubsOnlyFinalize);
                ImGui.Checkbox($"When resending, auto-repair vessels", ref C.SubsAutoRepair);
                ImGui.Checkbox($"Even when only finalizing, repair vessels", ref C.SubsRepairFinalize);
                //ImGui.Checkbox($"Experimental compatibility with SimpleTweaks destination letters tweak", ref C.SimpleTweaksCompat);
                ImGui.Checkbox($"Hide airships", ref C.HideAirships);
                ImGui.SetNextItemWidth(60);
                ImGui.DragInt("Don't process retainers if vessels return in, minutes", ref C.DisableRetainerVesselReturn.ValidateRange(0, 60));
            }

            if (ImGui.CollapsingHeader("Public debug"))
            {
                try
                {
                    if (!P.TaskManager.IsBusy)
                    {
                        if (ImGui.Button("Resend currently selected submarine on previous voyage"))
                        {
                            TaskDeployOnPreviousVoyage.Enqueue();
                        }
                        if (ImGui.Button("Select best path"))
                        {
                            TaskCalculateAndPickBestExpRoute.Enqueue();
                        }
                        if (ImGui.Button("Select best path with 1 unlock included"))
                        {
                            TaskCalculateAndPickBestExpRoute.Enqueue(VoyageUtils.GetSubmarineUnlockPlanByGuid(Data.GetAdditionalVesselData(Utils.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedUnlockPlan) ?? new());
                        }
                        if (ImGui.Button("Select unlock path (up to 5)"))
                        {
                            TaskDeployOnUnlockRoute.EnqueuePickOrCalc(VoyageUtils.GetSubmarineUnlockPlanByGuid(Data.GetAdditionalVesselData(Utils.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedUnlockPlan) ?? new(), UnlockMode.MultiSelect);
                        }
                        if (ImGui.Button("Select unlock path (only 1)"))
                        {
                            TaskDeployOnUnlockRoute.EnqueuePickOrCalc(VoyageUtils.GetSubmarineUnlockPlanByGuid(Data.GetAdditionalVesselData(Utils.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedUnlockPlan) ?? new(), UnlockMode.SpamOne);
                        }
                        if (ImGui.Button("Select point planner path"))
                        {
                            var plan = VoyageUtils.GetSubmarinePointPlanByGuid(Data.GetAdditionalVesselData(Utils.Read(CurrentSubmarine.Get()->Name), VoyageType.Submersible).SelectedPointPlan);
                            if (plan != null)
                            {
                                TaskDeployOnPointPlan.EnqueuePick(plan);
                            }
                            else
                            {
                                DuoLog.Error($"No plan selected!");
                            }
                        }
                        foreach (var x in Data.OfflineSubmarineData)
                        {
                            if (ImGui.Button($"Repair {x.Name} submarine's broken components"))
                            {
                                if (VoyageUtils.GetCurrentWorkshopPanelType() == PanelType.Submersible)
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
                        if (ImGui.Button("Approach bell"))
                        {
                            TaskInteractWithNearestBell.Enqueue(false);
                        }

                        if (ImGui.Button("Approach panel"))
                        {
                            TaskInteractWithNearestPanel.Enqueue(false);
                        }

                        if(ImGui.Button("Redeploy current vessel on previous voyage"))
                        {
                            TaskRedeployPreviousLog.Enqueue();
                        }

                        if (ImGui.Button($"Deploy current submarine on best experience route")) TaskDeployOnBestExpVoyage.Enqueue();

                    }
                    else
                    {
                        ImGuiEx.Text(EColor.RedBright, $"Currently executing: {P.TaskManager.CurrentTaskName}");
                    }
                }
                catch(Exception e)
                {
                    ImGuiEx.TextWrapped(e.ToString());
                }
            }
            if (ImGui.CollapsingHeader("Debug"))
            {
                try
                {
                    var h = HousingManager.Instance()->WorkshopTerritory;
                    if (h != null) 
                    { 
                        foreach(var x in h->Submersible.DataListSpan)
                        {
                            ImGuiEx.Text($"{MemoryHelper.ReadStringNullTerminated((nint)x.Name)}/{x.ReturnTime}/{*x.CurrentExplorationPoints}");
                        }
                    }
                    if (ImGui.Button("Erase offline data"))
                    {
                        Data.OfflineAirshipData.Clear();
                        Data.OfflineSubmarineData.Clear();
                    }
                    if (ImGui.Button("Repair 1")) VoyageScheduler.TryRepair(0);
                    if (ImGui.Button("Repair 2")) VoyageScheduler.TryRepair(1);
                    if (ImGui.Button("Repair 3")) VoyageScheduler.TryRepair(2);
                    if (ImGui.Button("Repair 4")) VoyageScheduler.TryRepair(3);
                    if (ImGui.Button("Close repair")) VoyageScheduler.CloseRepair();
                    //if (ImGui.Button("Trigger auto repair")) TaskRepairAll.EnqueueImmediate();
                    ImGui.InputText("data1", ref data1, 50);
                    ImGuiEx.EnumCombo("data2", ref data2);
                    if(CurrentSubmarine.Get() != null)
                    {
                        ImGuiEx.Text($"{CurrentSubmarine.Get()->CurrentExp}/{CurrentSubmarine.Get()->NextLevelExp}");
                    }
                    ImGuiEx.Text($"Is voyage panel: {VoyageUtils.IsInVoyagePanel()}, {Lang.PanelName}");
                    if (ImGui.Button("IsVesselNeedsRepair"))
                    {
                        try
                        {
                            DuoLog.Information($"{VoyageUtils.GetIsVesselNeedsRepair(data1, data2, out var log).Print()}\n{log.Join("\n")}");
                        }
                        catch (Exception e)
                        {
                            e.LogDuo();
                        }
                    }
                    if (ImGui.Button("GetSubmarineIndexByName"))
                    {
                        try
                        {
                            DuoLog.Information($"{VoyageUtils.GetVesselIndexByName(data1, VoyageType.Submersible)}");
                        }
                        catch (Exception e)
                        {
                            e.LogDuo();
                        }
                    }
                    ImGuiEx.Text($"Bell: {Utils.GetReachableRetainerBell(false)}");
                    ImGuiEx.Text($"Bell(true): {Utils.GetReachableRetainerBell(true)}");
                    ImGuiEx.TextWrapped($"Enabled subs: {Data.GetVesselData(VoyageType.Submersible).Select(x => $"{x.Name}, {x.GetRemainingSeconds()}").Print()}");
                    ImGuiEx.Text($"AnyEnabledVesselsAvailable: {Data.AnyEnabledVesselsAvailable()}");
                    ImGuiEx.Text($"Panel type: {VoyageUtils.GetCurrentWorkshopPanelType()}");
                    if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
                    {
                        var button = addon->UldManager.NodeList[3]->GetAsAtkComponentButton();
                        ImGuiEx.Text($"Button: {button->IsEnabled}");
                    }
                    if (ImGui.Button("Interact with nearest panel"))
                    {
                        TaskInteractWithNearestPanel.Enqueue();
                    }
                    ImGuiEx.Text($"bars\n{bars.Select(x => $"{x.cid},{x.start},{x.end},{x.frame},{x.percent}").Join("\n")}");
                }
                catch(Exception e)
                {
                    ImGuiEx.TextWrapped(e.ToString());
                }
            }
        }
        static string data1 = "";
        static VoyageType data2 = VoyageType.Airship;


        static void DrawTable(OfflineCharacterData data)
        {
            var storePos = ImGui.GetCursorPos();
            foreach (var v in bars.Where(x => x.cid == data.CID))
            {
                ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                ImGui.ProgressBar(1f - Math.Min(1f, v.percent),
                    new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
                ImGui.PopStyleColor(2);
            }
            ImGui.SetCursorPos(storePos);
            if (ImGui.BeginTable("##retainertable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Structure");
                ImGui.TableSetupColumn("Voyage");
                ImGui.TableSetupColumn("");
                ImGui.TableHeadersRow();
                for (var i = 0; i < data.OfflineAirshipData.Count; i++)
                {
                    var vessel = data.OfflineAirshipData[i];
                    DrawRow(data, vessel, VoyageType.Airship);
                }
                for (var i = 0; i < data.OfflineSubmarineData.Count; i++)
                {
                    var vessel = data.OfflineSubmarineData[i];
                    DrawRow(data, vessel, VoyageType.Submersible);
                }
                ImGui.EndTable();
            }
        }

        static void DrawRow(OfflineCharacterData data, OfflineVesselData vessel, VoyageType type)
        {
            if (type == VoyageType.Airship && data.GetEnabledVesselsData(type).Count == 0 && C.HideAirships) return;
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
            if (disabled) ImGui.BeginDisabled();
            ImGuiEx.CollectionCheckbox($"{vessel.Name}##sub", vessel.Name, enabled);
            if (disabled) ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (adata.VesselBehavior == VesselBehavior.Finalize)
            {
                ImGuiEx.Text(Lang.IconAnchor);
            }
            else if (adata.VesselBehavior == VesselBehavior.Redeploy)
            {
                ImGuiEx.Text(Lang.IconResend);
            }
            else if (adata.VesselBehavior == VesselBehavior.LevelUp)
            {
                ImGuiEx.Text(Lang.IconLevelup);
            }
            else if (adata.VesselBehavior == VesselBehavior.Unlock)
            {
                ImGuiEx.Text(Lang.IconUnlock);
                ImGui.SameLine();
                if(adata.UnlockMode == UnlockMode.WhileLevelling)
                {
                    ImGuiEx.Text(Lang.IconLevelup);
                }
                else if (adata.UnlockMode == UnlockMode.SpamOne)
                {
                    ImGuiEx.Text(Lang.IconRepeat);
                }
                else if (adata.UnlockMode == UnlockMode.MultiSelect)
                {
                    ImGuiEx.Text(Lang.IconPath);
                }
                else
                {
                    ImGuiEx.Text(Lang.IconWarning);
                }
            }
            else if (adata.VesselBehavior == VesselBehavior.Use_plan)
            {
                ImGuiEx.Text(Lang.IconPlanner);
            }
            else
            {
                ImGuiEx.Text(Lang.IconWarning);
            }
            ImGui.PopFont();
            var end = ImGui.GetCursorPos();
            var p = (float)vessel.GetRemainingSeconds() / (60f * 60f * 24f);
            if(vessel.ReturnTime != 0) bars.Add((data.CID, Svc.PluginInterface.UiBuilder.FrameCount, start, end, vessel.ReturnTime==0?0:p.ValidateRange(0f, 1f)));
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            if (adata.Level > 0)
            {
                var lvlf = 0;
                if(adata.CurrentExp > 0 && adata.NextLevelExp > 0)
                {
                    lvlf = (int)((float)adata.CurrentExp * 100f / (float)adata.NextLevelExp);
                }
                ImGuiEx.TextV(Lang.CharLevel + $"{adata.Level}".ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
                ImGui.SameLine(0, 0);
                ImGuiEx.Text(ImGuiColors.DalamudGrey3, $".{lvlf:D2}".ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
                ImGui.SameLine(0, 0);
                ImGuiEx.Text(VoyageUtils.GetSubmarineBuild(adata));
            }
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);

            if (vessel.ReturnTime == 0)
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
                        var d = Svc.Data.GetExcelSheet<SubmarineExplorationPretty>(Dalamud.ClientLanguage.Japanese).GetRow(x);
                        if(d != null && d.Location.ToString().Length > 0)
                        {
                            points.Add(d.Location.ToString());
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
            if (ImGuiEx.IconButton(FontAwesomeIcon.Cogs, $"{data.CID} {vessel.Name}"))
            {
                ImGui.OpenPopup(n);
            }
            if (ImGuiEx.BeginPopupNextToElement(n))
            {
                ImGui.CollapsingHeader($"{vessel.Name} - {Censor.Character(data.Name)} Configuration  ##conf", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
                ImGuiEx.Text($"Vessel behavior:");
                ImGuiEx.EnumCombo("##vbeh", ref adata.VesselBehavior);
                if (adata.VesselBehavior == VesselBehavior.Unlock)
                {
                    ImGuiEx.Text($"Unlock mode:");
                    ImGuiEx.EnumCombo("##umode", ref adata.UnlockMode, Lang.UnlockModeNames);
                    var currentPlan = VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan);
                    var text = Environment.TickCount64 % 2000 > 1000 ? "Unlocking every point (default plan)" : "No or unknown plan selected";
                    if (ImGui.BeginCombo("##uplan", currentPlan?.Name ?? text))
                    {
                        if (ImGui.Button("Open editor"))
                        {
                            P.SubmarineUnlockPlanUI.IsOpen = true;
                            P.SubmarineUnlockPlanUI.SelectedPlanGuid = adata.SelectedUnlockPlan;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Clear plan"))
                        {
                            adata.SelectedUnlockPlan = Guid.Empty.ToString();
                        }
                        foreach (var x in C.SubmarineUnlockPlans)
                        {
                            if (ImGui.Selectable($"{x.Name}##{x.GUID}"))
                            {
                                adata.SelectedUnlockPlan = x.GUID;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
                if (adata.VesselBehavior == VesselBehavior.Use_plan)
                {
                    var currentPlan = VoyageUtils.GetSubmarinePointPlanByGuid(adata.SelectedPointPlan);
                    if (ImGui.BeginCombo("##uplan", currentPlan.GetPointPlanName()))
                    {
                        if (ImGui.Button("Open editor"))
                        {
                            P.SubmarinePointPlanUI.IsOpen = true;
                            P.SubmarinePointPlanUI.SelectedPlanGuid = adata.SelectedPointPlan;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Clear plan"))
                        {
                            adata.SelectedPointPlan = Guid.Empty.ToString();
                        }
                        foreach (var x in C.SubmarinePointPlans)
                        {
                            if (ImGui.Selectable($"{x.GetPointPlanName()}##{x.GUID}"))
                            {
                                adata.SelectedPointPlan = x.GUID;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }
    }
}
