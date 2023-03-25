﻿using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings;

internal static class Expert
{
    internal static void Draw()
    {
        ImGuiHelpers.ScaledDummy(5f);
        InfoBox.DrawBox("Behavior##expert", delegate
        {
            ImGuiEx.Text($"Action on accessing retainer bell if no ventures available:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##OpenBellBehaviorNoVentures", ref P.config.OpenBellBehaviorNoVentures);

            ImGuiEx.Text($"Action on accessing retainer bell if any ventures available:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##OpenBellBehaviorWithVentures", ref P.config.OpenBellBehaviorWithVentures);

            ImGuiEx.Text($"Task completion behavior after accessing bell:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##TaskCompletedBehaviorAccess", ref P.config.TaskCompletedBehaviorAccess);

            ImGuiEx.Text($"Task completion behavior after manual enabling:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##TaskCompletedBehaviorManual", ref P.config.TaskCompletedBehaviorManual);

            ImGuiEx.Text($"Task completion behavior during plugin operation:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##TaskCompletedBehaviorAuto", ref P.config.TaskCompletedBehaviorAuto);

            ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "\"Close retainer list and disable plugin\" option for 3 previous settings is enforced during MultiMode operation.");

            ImGui.Checkbox($"Stay in retainer menu if there are retainers to finish ventures within 5 minutes or less", ref P.config.Stay5);
            ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "This option is enforced during MultiMode operation.");

            ImGui.Checkbox($"Auto-disable plugin when closing retainer list", ref P.config.AutoDisable);
            ImGuiEx.TextWrapped($"Only applies when you exit menu by yourself. Otherwise, settings above apply.");
            ImGui.Checkbox($"Do not show plugin status icons", ref P.config.HideOverlayIcons);
        });

        InfoBox.DrawBox("Settings##expert", delegate
        {
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.SliderIntAsFloat("Interaction Delay, seconds", ref P.config.Delay.ValidateRange(10, 1000), 20, 1000);
            ImGuiComponents.HelpMarker("The lower this value is the faster plugin will operate retainers. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it. ");
            ImGui.Checkbox($"Disable sorting and collapsing/expanding", ref P.config.NoCurrentCharaOnTop);
            ImGui.Checkbox($"Show MultiMode checkbox on plugin UI bar", ref P.config.MultiModeUIBar);
        });

        InfoBox.DrawBox("Server time", delegate
        {
            ImGui.Checkbox($"Use server time instead of PC time", ref P.config.UseServerTime);
        });
    }
}