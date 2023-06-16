using Dalamud.Utility;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.UI.Dbg;

internal unsafe static class DebugMulti
{
    internal static void Draw()
    {

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
            if (ImGui.Button("Enter house"))
            {
                HouseEnterTask.EnqueueTask();
            }
        }
        if (ImGui.CollapsingHeader("Estate territories"))
        {
            ImGuiEx.Text(ResidentalAreas.List.Select(x => GenericHelpers.GetTerritoryName(x)).Join("\n"));
            ImGuiEx.Text($"In residental area: {ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType)}");
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
    }
}
