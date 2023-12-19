using ECommons.Events;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PInvoke;
using ItemLevel = AutoRetainer.Helpers.ItemLevel;

namespace AutoRetainer.UI.Dbg;

internal static unsafe class DebugMisc
{

    internal static void Draw()
    {
        if (ImGui.CollapsingHeader("Housing"))
        {
            var h = HousingManager.Instance();
            ImGuiEx.Text($"GetCurrentDivision {h->GetCurrentDivision()}");
            ImGuiEx.Text($"GetCurrentHouseId {h->GetCurrentHouseId()}");
            ImGuiEx.Text($"GetCurrentPlot {h->GetCurrentPlot()}");
            ImGuiEx.Text($"GetCurrentRoom {h->GetCurrentRoom()}");
            ImGuiEx.Text($"GetCurrentWard {h->GetCurrentWard()}");
            if(ImGui.Button("Simulate login"))
            {
                ProperOnLogin.FireArtificially();
            }
        }
        if (ImGui.Button("Install callback hook")) Callback.InstallHook();
        if (ImGui.Button("Disable callback hook")) Callback.UninstallHook();
        ImGuiEx.TextCopy($"{(nint)(&TargetSystem.Instance()->Target):X16}");
        ImGui.Checkbox($"Log opcodes", ref P.LogOpcodes);
        ImGuiEx.Text($"CSFramework.Instance()->FrameCounter: {CSFramework.Instance()->FrameCounter}");
        if(ImGui.Button("Test entrust dup"))
        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon))
            {
                Callback.Fire(addon, true, 0, (uint)29);
            }
        }
        ImGuiEx.Text($"Lockon: {*(byte*)(((nint)TargetSystem.Instance()) + 309)}");
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
        foreach(var x in C.OfflineData)
        {
            ImGuiEx.Text($"{x.Name}@{x.World}: {(x.Gil + x.RetainerData.Sum(z => z.Gil))}");
        }
        var ocd = Data;
        if(ocd != null)
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

    }
}
