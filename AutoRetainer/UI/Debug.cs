using Dalamud.Memory;
using Dalamud;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Utility;
using ClickLib.Clicks;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;

namespace AutoRetainer.UI;

internal unsafe static class Debug
{
    static string dbgRetName = string.Empty;
    internal static void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "Anything can happen here.");
        Safe(delegate
        {
            ImGui.Checkbox($"TaskWithdrawGil.forceCheck", ref TaskWithdrawGil.forceCheck);
            ImGuiEx.Text($"{Svc.Data.GetExcelSheet<LogMessage>().GetRow(4578).Text.ToDalamudString().ExtractText(true)}");
            if(ImGui.Button("Close retainer"))
            {
                DuoLog.Information($"{RetainerHandlers.CloseAgentRetainer()}");
            }
            if (ImGui.CollapsingHeader("Agent"))
            {
                /*var agent = Framework.Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentId.GatheringNote);
                if(agent != null && agent->IsAgentActive())
                {
                    
                    var ptr = (nint)agent->AtkEventInterface.vtbl;
                    ImGuiEx.Text($"Vtable: {ptr:X16}");
                    for(var i = 0; i < 8; i++)
                    {
                        ImGuiEx.Text($"Vfunc{i}: {*(nint*)(ptr + 8 * i)}");
                    }
                }*/
                //in next life I guess
            }
            ImGuiEx.Text($"Utils.AnyRetainersAvailableCurrentChara(): {Utils.AnyRetainersAvailableCurrentChara()}");
            if (ImGui.Button($"SelectAssignVenture"))
            {
                DuoLog.Information($"{RetainerHandlers.SelectAssignVenture()}");
            }
            if (ImGui.Button($"SelectQuit"))
            {
                DuoLog.Information($"{RetainerHandlers.SelectQuit()}");
            }
            if (ImGui.Button($"SelectViewVentureReport"))
            {
                DuoLog.Information($"{RetainerHandlers.SelectViewVentureReport()}");
            }
            if (ImGui.Button($"ClickResultReassign"))
            {
                DuoLog.Information($"{RetainerHandlers.ClickResultReassign()}");
            }
            if (ImGui.Button($"ClickResultConfirm"))
            {
                DuoLog.Information($"{RetainerHandlers.ClickResultConfirm()}");
            }
            if (ImGui.Button($"ClickAskAssign"))
            {
                DuoLog.Information($"{RetainerHandlers.ClickAskAssign()}");
            }
            if (ImGui.Button($"SelectQuickExploration"))
            {
                DuoLog.Information($"{RetainerHandlers.SelectQuickExploration()}");
            }
            if (ImGui.Button($"SelectEntrustItems"))
            {
                DuoLog.Information($"{RetainerHandlers.SelectEntrustItems()}");
            }
            if (ImGui.Button($"SelectEntrustGil"))
            {
                DuoLog.Information($"{RetainerHandlers.SelectEntrustGil()}");
            }
            if (ImGui.Button($"ClickEntrustDuplicates"))
            {
                DuoLog.Information($"{RetainerHandlers.ClickEntrustDuplicates()}");
            }
            if (ImGui.Button($"ClickEntrustDuplicatesConfirm"))
            {
                DuoLog.Information($"{RetainerHandlers.ClickEntrustDuplicatesConfirm()}");
            }
            if (ImGui.Button($"ClickCloseEntrustWindow"))
            {
                DuoLog.Information($"{RetainerHandlers.ClickCloseEntrustWindow()}");
            }
            if (ImGui.Button($"CloseRetainerInventory"))
            {
                DuoLog.Information($"{RetainerHandlers.CloseAgentRetainer()}");
            }
            if (ImGui.Button($"CloseRetainerInventory"))
            {
                DuoLog.Information($"{RetainerHandlers.CloseAgentRetainer()}");
            }
            if (ImGui.Button($"SetWithdrawGilAmount (1%)"))
            {
                DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(1)}");
            }
            if (ImGui.Button($"SetWithdrawGilAmount (50%)"))
            {
                DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(50)}");
            }
            if (ImGui.Button($"SetWithdrawGilAmount (99%)"))
            {
                DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(99)}");
            }
            if (ImGui.Button($"SetWithdrawGilAmount (100%)"))
            {
                DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(100)}");
            }
            if (ImGui.Button($"WithdrawGilOrCancel"))
            {
                DuoLog.Information($"{RetainerHandlers.WithdrawGilOrCancel()}");
            }
            if (ImGui.Button($"WithdrawGilOrCancel (force cancel)"))
            {
                DuoLog.Information($"{RetainerHandlers.WithdrawGilOrCancel(true)}");
            }

            ImGui.Separator();

            if (ImGui.Button($"TaskEntrustDuplicates"))
            {
                TaskEntrustDuplicates.Enqueue();
            }
            if (ImGui.Button($"TaskAssignQuickVenture"))
            {
                TaskAssignQuickVenture.Enqueue();
            }
            if (ImGui.Button($"TaskReassignVenture"))
            {
                TaskReassignVenture.Enqueue();
            }
            if (ImGui.Button($"TaskWithdrawGil (50%)"))
            {
                TaskWithdrawGil.Enqueue(50);
            }

            if (TryGetAddonByName<AddonSelectString>("SelectString", out var sel))
            {
                var entries = Utils.GetEntries(sel);
                foreach (var x in entries)
                {
                    var index = entries.IndexOf(x);
                    if (ImGui.SmallButton($"{x} / {index}") && index >= 0)
                    {
                        ClickSelectString.Using((nint)sel).SelectItem((ushort)index);
                    }
                }
            }
            ImGui.Separator();
            ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText()}");
            ImGuiEx.Text($"Server time: {Framework.GetServerTime()}");
            ImGuiEx.Text($"PC time: {DateTimeOffset.Now.ToUnixTimeSeconds()}");
            if (ImGui.Button("InstallInteractHook"))
            {
                P.Memory.InstallInteractHook();
            }
            if (ImGui.CollapsingHeader("HET"))
            {
                ImGuiEx.Text($"Nearest entrance: {Utils.GetNearestEntrance(out var d)}, d={d}");
                if(ImGui.Button("Enter house"))
                {
                    HouseEnterTask.EnqueueTask();
                }
            }
            if(ImGui.CollapsingHeader("Estate territories"))
            {
                ImGuiEx.Text(ResidentalAreas.List.Select(x => GenericHelpers.GetTerritoryName(x)).Join("\n"));
                ImGuiEx.Text($"In residental area: {ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType)}");
            }
            if (ImGui.CollapsingHeader("Task debug"))
            {
                ImGuiEx.Text($"Busy: {P.TaskManager.IsBusy}, abort in {P.TaskManager.AbortAt - Environment.TickCount64}");
                if (ImGui.Button($"Generate random numbers 1/500"))
                {
                    P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 500); InternalLog.Verbose($"Gen 1/500: {r}"); return r == 0; });
                }
                if (ImGui.Button($"Generate random numbers 1/5000"))
                {
                    P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 5000); InternalLog.Verbose($"Gen 1/5000: {r}"); return r == 0; });
                }
                if (ImGui.Button($"Generate random numbers 1/100"))
                {
                    P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 100); InternalLog.Verbose($"Gen 1/100: {r}"); return r == 0; });
                }
            }
            ImGuiEx.Text($"Is in sanctuary: {GameMain.IsInSanctuary()}");
            ImGuiEx.Text($"Is in sanctuary ExcelTerritoryHelper: {ExcelTerritoryHelper.IsSanctuary(Svc.ClientState.TerritoryType)}");
            ImGui.Checkbox($"Bypass sanctuary check", ref P.config.BypassSanctuaryCheck);
            if (Svc.ClientState.LocalPlayer != null && Svc.Targets.Target != null)
            {
                ImGuiEx.Text($"Distance to target: {Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Svc.Targets.Target.Position)}");
                ImGuiEx.Text($"Target hitbox: {Svc.Targets.Target.HitboxRadius}");
                ImGuiEx.Text($"Distance to target's hitbox: {Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Svc.Targets.Target.Position) - Svc.Targets.Target.HitboxRadius}");
            }
            ImGuiEx.Text($"Free inventory slots: {Utils.GetInventoryFreeSlotCount()}");
            for (var i = 0; i < P.retainerManager.Count; i++)
            {
                var ret = P.retainerManager.Retainer(i);
                ImGuiEx.TextWrapped($"{ret.Name}\n           {ret.VentureID} {ret.VentureComplete} {ret.GetVentureSecondsRemaining()}/{ret.GetVentureSecondsRemaining()} | {ret.Gil} gil");
                if (SafeMemory.ReadBytes((IntPtr)(&ret), 0x48, out var buff))
                {
                    ImGuiEx.TextCopy(buff.Select(x => $"{x:X2}").Join(" "));
                }
            }
            ImGui.InputText("Retainer name", ref dbgRetName, 50);
            
            if (ImGui.Button("AtkStage get focus"))
            {
                var ptr = (IntPtr)AtkStage.GetSingleton()->GetFocus();
                Svc.Chat.Print($"Stage focus: {ptr}");
            }
            if (ImGui.Button("AtkStage clear focus"))
            {
                AtkStage.GetSingleton()->ClearFocus();
            }
            if (ImGui.Button("Try retrieve current retainer name"))
            {
                if (TryGetAddonByName<AddonSelectString>("SelectString", out var select) && IsAddonReady(&select->AtkUnitBase))
                {
                    var textNode = ((AtkTextNode*)select->AtkUnitBase.UldManager.NodeList[3]);
                    var text = MemoryHelper.ReadSeString(&textNode->NodeText);
                    foreach (var x in text.Payloads)
                    {
                        PluginLog.Information($"{x.Type}: {x.ToString()}");
                    }
                }
            }
            {
                if (ImGui.Button("Try close") && TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon))
                {
                    var v = stackalloc AtkValue[1]
                    {
                        new()
                        {
                            Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                            Int = -1
                        }
                    };
                    addon->FireCallback(1, v);
                    Notify.Info("Done");
                }
            }
            {
                if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon))
                {
                    if (ImGui.Button("test bank"))
                    {
                        var values = stackalloc AtkValue[2]
                        {
                            new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 3 },
                            new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt, Int = 50 },
                        };
                        addon->FireCallback(2, values);
                    }
                }
            }

            ImGuiEx.Text($"QSI status: {P.quickSellItems?.openInventoryContextHook?.IsEnabled}");
            ImGuiEx.Text($"QuickSellItems.IsReadyToUse: {QuickSellItems.IsReadyToUse()}");

            foreach (var x in StatisticsUI.CharTotal)
            {
                ImGuiEx.Text($"{x.Key} : {x.Value}");
            }
            foreach (var x in StatisticsUI.RetTotal)
            {
                ImGuiEx.Text($"{x.Key} : {x.Value}");
            }

            ImGui.Separator();
            ImGuiEx.Text($"GC Addon Life: {AutoGCHandin.GetAddonLife()}");
            {
                if (ImGui.Button("Fire") && TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && addon->UldManager.NodeList[5]->IsVisible)
                {
                    AutoGCHandin.InvokeHandin(addon);
                }
            }

            {
                if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon))
                {
                    ImGuiEx.Text($"IsSelectedFilterValid: {AutoGCHandin.IsSelectedFilterValid(addon)}");
                }
            }

            ImGui.Separator();
            ImGuiEx.Text("Throttle timers");
            foreach(var x in EzThrottler.ThrottleNames)
            {
                ImGuiEx.Text($"{x}: {EzThrottler.Check(x)} / {EzThrottler.GetRemainingTime(x)}");
            }
        });
    }
}
