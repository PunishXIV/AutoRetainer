using AutoRetainer.UI.Statistics;
using ECommons.Automation;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PInvoke;
using ItemLevel = AutoRetainer.Helpers.ItemLevel;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugMisc : DebugSectionBase
{
    public override void Draw()
    {
        if (ImGui.CollapsingHeader("pfinder"))
        {
            if (ImGui.Button("Callback"))
            {
                if (!TryGetAddonByName<AtkUnitBase>("LookingForGroup", out var _))
                {
                    P.TaskManager.Enqueue(() => Chat.Instance.ExecuteCommand("/partyfinder"));
                    P.TaskManager.DelayNext(500);
                }
                P.TaskManager.Enqueue(() =>
                {
                    if (TryGetAddonByName<AtkUnitBase>("LookingForGroup", out var addon))
                    {
                        var btn = addon->UldManager.NodeList[35];
                        var enabled = btn->GetAsAtkComponentNode()->Component->UldManager.NodeList[2]->Alpha_2 == 255;
                        var selected = btn->GetAsAtkComponentNode()->Component->UldManager.NodeList[4]->GetAsAtkImageNode()->PartId == 0;
                        if (enabled)
                        {
                            if (!selected)
                            {
                                PluginLog.Debug($"Selecting hunts");
                                Callback.Fire(addon, true, 21, 11, Callback.ZeroAtkValue);
                            }
                            return true;
                        }
                    }
                    return false;
                });
            }
        }
        ImGuiEx.Text($"FC points: {Utils.FCPoints}");
        if (ImGui.CollapsingHeader("Housing"))
        {
            ImGuiEx.Text($"FC aetheryte: {ExcelTerritoryHelper.GetName(Utils.GetFCHouseTerritory())} / {Utils.IsSureNotInFcTerritory()}");
            ImGuiEx.Text($"Private aetheryte: {ExcelTerritoryHelper.GetName(Utils.GetPrivateHouseTerritory())} / {Utils.IsSureNotInPrivateTerritory()}");
            var h = HousingManager.Instance();
            ImGuiEx.Text($"GetCurrentDivision {h->GetCurrentDivision()}");
            ImGuiEx.Text($"GetCurrentHouseId {h->GetCurrentHouseId()}");
            ImGuiEx.Text($"GetCurrentPlot {h->GetCurrentPlot()}");
            ImGuiEx.Text($"GetCurrentRoom {h->GetCurrentRoom()}");
            ImGuiEx.Text($"GetCurrentWard {h->GetCurrentWard()}");
            if (ImGui.Button("Simulate login"))
            {
                ProperOnLogin.FireArtificially();
            }
            if (h->OutdoorTerritory != null)
            {
                for (var i = 0; i < 30; i++)
                {
                    ImGuiEx.Text($"IsEstateResident {i}: {P.Memory.OutdoorTerritory_IsEstateResident((nint)h->OutdoorTerritory, (byte)i)}");
                }
            }
        }
        if (ImGui.Button("Install callback hook")) Callback.InstallHook();
        if (ImGui.Button("Disable callback hook")) Callback.UninstallHook();
        ImGuiEx.TextCopy($"{(nint)(&TargetSystem.Instance()->Target):X16}");
        ImGui.Checkbox($"Log opcodes", ref P.LogOpcodes);
        ImGuiEx.Text($"CSFramework.Instance()->FrameCounter: {CSFramework.Instance()->FrameCounter}");
        if (ImGui.Button("Test entrust dup"))
        {
            if (TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon))
            {
                Callback.Fire(addon, true, 0, (uint)29);
            }
        }
        ImGuiEx.Text($"Lockon: {*(byte*)((nint)TargetSystem.Instance() + 309)}");
        if (ImGui.Button("Chill frames lock"))
        {
            FPSManager.LockChillFrames();
        }
        if (ImGui.Button("Unlock frames lock"))
        {
            FPSManager.UnlockChillFrames();
        }
        ImGui.Separator();
        ImGuiEx.Text($"CSFramework.Instance()->WindowInactive: {CSFramework.Instance()->WindowInactive}");
        ImGuiEx.Text($"IsKeyPressed(C.TempCollectB): {IsKeyPressed(C.TempCollectB)}");
        ImGuiEx.Text($"Bitmask.IsBitSet(User32.GetKeyState((int)C.TempCollectB), 15): {Bitmask.IsBitSet(User32.GetKeyState((int)C.TempCollectB), 15)}");
        ImGuiEx.Text($"DontReassign: {C.DontReassign}, key {C.TempCollectB}/{(int)C.TempCollectB}");
        foreach (var x in C.OfflineData)
        {
            ImGuiEx.Text($"{x.Name}@{x.World}: {x.Gil + x.RetainerData.Sum(z => z.Gil)}");
        }
        var ocd = Data;
        if (ocd != null)
        {
            ImGuiEx.Text($"Level array:");
            ImGuiEx.Text(ocd.ClassJobLevelArray.Print());
        }

        ImGuiEx.Text($"{Utils.TryGetCurrentRetainer(out var n)}/{n}");
        ImGuiEx.Text($"{ItemLevel.Calculate(out var g, out var p)}/{g}/{p}");
        if (ImGui.Button("Regenerate censor seed"))
        {
            C.CensorSeed = Guid.NewGuid().ToString();
        }
        var inv = Utils.GetActiveRetainerInventoryName();
        ImGuiEx.Text($"Utils.GetActiveRetainerInventoryName(): {inv.Name} {inv.EntrustDuplicatesIndex}");
        ImGuiEx.Text($"ConditionWasEnabled={P.ConditionWasEnabled}");
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
        ImGuiEx.Text($"QSI status: {P.quickSellItems?.openInventoryContextHook?.IsEnabled}");
        ImGuiEx.Text($"QuickSellItems.IsReadyToUse: {QuickSellItems.IsReadyToUse()}");

        foreach (var x in S.VentureStats.CharTotal)
        {
            ImGuiEx.Text($"{x.Key} : {x.Value}");
        }
        foreach (var x in S.VentureStats.RetTotal)
        {
            ImGuiEx.Text($"{x.Key} : {x.Value}");
        }

        ImGui.Separator();
        {
            if (ImGui.Button("Fire") && TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && addon->UldManager.NodeList[5]->IsVisible())
            {
                AutoGCHandin.InvokeHandin(addon, 0);
            }
        }

        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon))
            {
                ImGuiEx.Text($"IsSelectedFilterValid: {AutoGCHandin.IsSelectedFilterValid(addon)}");
            }
        }

    }
}
