using Dalamud.Memory;
using Dalamud;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AutoRetainer.QSI;
using AutoRetainer.Statistics;
using AutoRetainer.GcHandin;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ECommons.ExcelServices;

namespace AutoRetainer.UI;

internal unsafe static class Debug
{
    static string dbgRetName = string.Empty;
    internal static void Draw()
    {
        Safe(delegate
        {
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
            ImGuiEx.Text($"Random: {Scheduler.RandomAddition}");
            ImGuiEx.Text($"Free inventory slots: {Utils.GetInventoryFreeSlotCount()}");
            ImGuiEx.Text($"Last action: {Clicker.lastAction}");
            for (var i = 0; i < P.retainerManager.Count; i++)
            {
                var ret = P.retainerManager.Retainer(i);
                ImGuiEx.Text($"{ret.Name}\n           {ret.VentureID} {ret.VentureComplete} {ret.GetVentureSecondsRemaining()}/{ret.GetVentureSecondsRemaining()} Banned: {Scheduler.IsBanned(ret.Name.ToString())}");
                if (SafeMemory.ReadBytes((IntPtr)(&ret), 0x48, out var buff))
                {
                    ImGuiEx.TextCopy(buff.Select(x => $"{x:X2}").Join(" "));
                }
            }
            ImGui.InputText("Retainer name", ref dbgRetName, 50);
            if (ImGui.Button("SelectRetainerByName"))
            {
                Clicker.SelectRetainerByName(dbgRetName);
            }
            if (ImGui.Button("SelectVentureMenu"))
            {
                Clicker.SelectVentureMenu();
            }
            if (ImGui.Button("ClickReassign"))
            {
                Clicker.ClickReassign();
            }
            if (ImGui.Button("ClickRetainerTaskAsk"))
            {
                Clicker.ClickRetainerTaskAsk();
            }
            if (ImGui.Button("SelectQuit"))
            {
                Clicker.SelectQuit();
            }
            ImGuiEx.Text($"Next retainer: {Scheduler.GetNextRetainerName()}");
            if (ImGui.Button("Tick manually"))
            {
                Scheduler.Tick();
            }
            if (ImGui.Button("AtkStage get focus"))
            {
                var ptr = (IntPtr)AtkStage.GetSingleton()->GetFocus();
                Svc.Chat.Print($"Stage focus: {ptr}");
            }
            if (ImGui.Button("AtkStage clear focus"))
            {
                AtkStage.GetSingleton()->ClearFocus();
            }
            if (ImGui.Button("InteractWithNearestBell"))
            {
                Clicker.InteractWithNearestBell(out _);
            }
            if (ImGui.Button("Close retainer list"))
            {
                Clicker.ClickClose();
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
