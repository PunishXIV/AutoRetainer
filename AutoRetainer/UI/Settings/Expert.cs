using Dalamud.Interface.Components;
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
            ImGuiEx.EnumCombo("##OpenBellBehaviorNoVentures", ref C.OpenBellBehaviorNoVentures);

            ImGuiEx.Text($"Action on accessing retainer bell if any ventures available:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##OpenBellBehaviorWithVentures", ref C.OpenBellBehaviorWithVentures);

            ImGuiEx.Text($"Task completion behavior after accessing bell:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##TaskCompletedBehaviorAccess", ref C.TaskCompletedBehaviorAccess);

            ImGuiEx.Text($"Task completion behavior after manual enabling:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##TaskCompletedBehaviorManual", ref C.TaskCompletedBehaviorManual);

            ImGuiEx.Text($"Task completion behavior during plugin operation:");
            ImGui.SetNextItemWidth(400);
            ImGuiEx.EnumCombo("##TaskCompletedBehaviorAuto", ref C.TaskCompletedBehaviorAuto);

            ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "\"Close retainer list and disable plugin\" option for 3 previous settings is enforced during MultiMode operation.");

            ImGui.Checkbox($"Stay in retainer menu if there are retainers to finish ventures within 5 minutes or less", ref C.Stay5);
            ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "This option is enforced during MultiMode operation.");

            ImGui.Checkbox($"Auto-disable plugin when closing retainer list", ref C.AutoDisable);
            ImGuiEx.TextWrapped($"Only applies when you exit menu by yourself. Otherwise, settings above apply.");
            ImGui.Checkbox($"Do not show plugin status icons", ref C.HideOverlayIcons);
        });

        InfoBox.DrawBox("Settings##expert", delegate
        {
            ImGui.Checkbox($"Disable sorting and collapsing/expanding", ref C.NoCurrentCharaOnTop);
            ImGui.Checkbox($"Show MultiMode checkbox on plugin UI bar", ref C.MultiModeUIBar);
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.SliderIntAsFloat("Retainer menu delay, seconds", ref C.RetainerMenuDelay.ValidateRange(0, 2000), 0, 2000);
            ImGui.Checkbox($"Allow venture timer to display negative values", ref C.TimerAllowNegative);
        });

        InfoBox.DrawBox("Server time", delegate
        {
            ImGui.Checkbox($"Use server time instead of PC time", ref C.UseServerTime);
        });
    }
}
