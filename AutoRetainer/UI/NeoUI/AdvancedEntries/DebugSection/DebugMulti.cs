using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Utility;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugMulti : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("NeoHET"))
        {
            if(ImGui.Button("Enqueue HET")) TaskNeoHET.Enqueue(null);
            if(ImGui.Button("Enqueue workshop")) TaskNeoHET.TryEnterWorkshop(() => DuoLog.Error("Fail"));
            ImGuiEx.Text($"""
                Can enter workshop: {S.LifestreamIPC.CanMoveToWorkshop()}
                """);
        }
        if(ImGui.CollapsingHeader("Tasks"))
        {
            if(ImGui.Button("TestAutomoveTask")) P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(() => Svc.Targets.FocusTarget));
            if(ImGui.Button("TestInteractTask")) P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(() => Svc.Targets.FocusTarget));
            if(ImGui.Button("TestBoth"))
            {
                P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(() => Svc.Targets.FocusTarget));
                P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(() => Svc.Targets.FocusTarget));
            }
        }
        ImGui.Checkbox("Don't logout", ref C.DontLogout);
        ImGui.Checkbox("Enabled", ref MultiMode.Enabled);
        ImGuiEx.Text($"Expected: {TaskChangeCharacter.Expected}");
        if(ImGui.Button("Force mismatch")) TaskChangeCharacter.Expected = ("AAAAAAAA", "BBBBBBB");
        if(ImGui.Button("Simulate nothing left"))
        {
            MultiMode.Relog(null, out var error, RelogReason.MultiMode);
        }
        if(ImGui.Button($"Simulate autostart"))
        {
            MultiMode.PerformAutoStart();
        }
        if(ImGui.Button("Delete was loaded data"))
        {
            DalamudReflector.DeleteSharedData("AutoRetainer.WasLoaded");
        }
        ImGuiEx.Text($"Moving: {AgentMap.Instance()->IsPlayerMoving}");
        ImGuiEx.Text($"Occupied: {IsOccupied()}");
        ImGuiEx.Text($"Casting: {Player.Object?.IsCasting}");
        ImGuiEx.TextCopy($"CID: {Player.CID}");
        ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Addon>()?.GetRow(115).Text.ToDalamudString().ExtractText()}");
        ImGuiEx.Text($"Server time: {CSFramework.GetServerTime()}");
        ImGuiEx.Text($"PC time: {DateTimeOffset.Now.ToUnixTimeSeconds()}");
        if(ImGui.CollapsingHeader("HET"))
        {
            ImGuiEx.Text($"Nearest entrance: {Utils.GetNearestEntrance(out var d)}, d={d}");
            if(ImGui.Button("Enter house"))
            {
                TaskNeoHET.Enqueue(null);
            }
        }
        if(ImGui.CollapsingHeader("Estate territories"))
        {
            ImGuiEx.Text(ResidentalAreas.List.Select(x => GenericHelpers.GetTerritoryName(x)).Join("\n"));
            ImGuiEx.Text($"In residental area: {ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType)}");
        }
        ImGuiEx.Text($"Is in sanctuary: {TerritoryInfo.Instance()->InSanctuary}");
        ImGuiEx.Text($"Is in sanctuary ExcelTerritoryHelper: {ExcelTerritoryHelper.IsSanctuary(Svc.ClientState.TerritoryType)}");
        ImGui.Checkbox($"Bypass sanctuary check", ref C.BypassSanctuaryCheck);
        if(Svc.ClientState.LocalPlayer != null && Svc.Targets.Target != null)
        {
            ImGuiEx.Text($"Distance to target: {Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Svc.Targets.Target.Position)}");
            ImGuiEx.Text($"Target hitbox: {Svc.Targets.Target.HitboxRadius}");
            ImGuiEx.Text($"Distance to target's hitbox: {Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Svc.Targets.Target.Position) - Svc.Targets.Target.HitboxRadius}");
        }
        if(ImGui.CollapsingHeader("CharaSelect"))
        {
            foreach(var x in Utils.GetCharacterNames())
            {
                ImGuiEx.Text($"{x.Name}@{x.World}");
            }
        }
    }
}
