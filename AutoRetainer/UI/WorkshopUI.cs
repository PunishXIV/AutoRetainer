using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
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

                if (ImGuiEx.CollapsingHeader(Censor.Character(data.Name, data.World)))
                {
                    pad = ImGui.GetStyle().FramePadding.Y;
                    DrawTable(data);
                }

                var rightText = $"R: {data.RepairKits} | C: {data.Ceruleum} | I: {data.InventorySpace}";
                var cur = ImGui.GetCursorPos();
                ImGui.SameLine();
                ImGui.SetCursorPos(new(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + pad));
                ImGuiEx.Text(rightText);

                ImGui.PopID();
            }
            bars.RemoveAll(x => x.frame != Svc.PluginInterface.UiBuilder.FrameCount);
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox($"Resend airships and submarines on voyage CP access", ref C.SubsAutoResend);
                ImGui.Checkbox($"Only finalize reports", ref C.SubsOnlyFinalize);
                ImGui.Checkbox($"When resending, auto-repair vessels", ref C.SubsAutoRepair);
                ImGui.Checkbox($"Even when only finalizing, repair vessels", ref C.SubsRepairFinalize);
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
                        foreach (var x in Data.OfflineSubmarineData)
                        {
                            if (ImGui.Button($"Repair {x.Name} submarine's broken components"))
                            {
                                if (VoyageUtils.GetCurrentWorkshopPanelType() == PanelType.Submersible)
                                {
                                    TaskSelectVesselByName.Enqueue(x.Name);
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
                        Utils.GetCurrentCharacterData().OfflineAirshipData.Clear();
                        Utils.GetCurrentCharacterData().OfflineSubmarineData.Clear();
                    }
                    if (ImGui.Button("Repair 1")) VoyageScheduler.TryRepair(0);
                    if (ImGui.Button("Repair 2")) VoyageScheduler.TryRepair(1);
                    if (ImGui.Button("Repair 3")) VoyageScheduler.TryRepair(2);
                    if (ImGui.Button("Repair 4")) VoyageScheduler.TryRepair(3);
                    if (ImGui.Button("Close repair")) VoyageScheduler.CloseRepair();
                    //if (ImGui.Button("Trigger auto repair")) TaskRepairAll.EnqueueImmediate();
                    ImGui.InputText("data1", ref data1, 50);
                    ImGuiEx.EnumCombo("data2", ref data2);
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
            ImGui.PushID($"{data.CID}/{vessel.Name}/{type}");
            var enabled = type == VoyageType.Airship ? data.EnabledAirships : data.EnabledSubs;
            var finalize = type == VoyageType.Airship ? data.FinalizeAirships : data.FinalizeSubs;

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
            if (finalize.Contains(vessel.Name))
            {
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.Text("\uf13d");
                ImGui.PopFont();
            }
            var end = ImGui.GetCursorPos();
            var p = (float)vessel.GetRemainingSeconds() / (60f * 60f * 24f);
            if(vessel.ReturnTime != 0) bars.Add((data.CID, Svc.PluginInterface.UiBuilder.FrameCount, start, end, vessel.ReturnTime==0?0:p.ValidateRange(0f, 1f)));
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            var adata = data.GetAdditionalVesselData(vessel.Name, type);
            if (adata.Level > 0)
            {
                var level = $"{Lang.CharLevel}{adata.Level}";
                ImGuiEx.TextV(level.ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
            }
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);

            if (vessel.ReturnTime == 0)
            {
                ImGuiEx.Text($"No voyage");
            }
            else
            {
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
                ImGuiEx.CollectionCheckbox($"Only finalize this vessel", vessel.Name, finalize);
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }
    }
}
