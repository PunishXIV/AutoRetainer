using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings;

internal static class Beta
{
    internal static void Draw()
    {
        ImGuiEx.Text(ImGuiColors.DalamudOrange, $"These features might be incomplete, cause problems, or simply not work.");
        ImGui.Separator();
        foreach (var x in Utils.GetCurrentCharacterData().OfflineAirshipData)
        {
            if (x.ReturnTime != 0)
            {
                ImGuiEx.Text($"Airship {x.Name} returns in {x.GetRemainingSeconds()} seconds");
            }
            else
            {
                ImGuiEx.Text($"Airship {x.Name} is not occupied");
            }
        }
        foreach (var x in Utils.GetCurrentCharacterData().OfflineSubmarineData)
        {
            if (x.ReturnTime != 0)
            {
                ImGuiEx.Text($"Submarine {x.Name} returns in {x.GetRemainingSeconds()} seconds");
            }
            else
            {
                ImGuiEx.Text($"Submarine {x.Name} is not occupied");
            }
        }
        ImGui.Checkbox($"Auto-resend airships and submersibles", ref C.AutoResendSubs);
    }
}
