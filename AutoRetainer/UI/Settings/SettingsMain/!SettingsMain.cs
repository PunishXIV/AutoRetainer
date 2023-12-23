using AutoRetainer.UI.Experiments;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.Interop;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings.SettingsMain;

internal static class SettingsMain
{
    internal static void Draw()
    {
        ImGuiEx.EzTabBar("GeneralSettings", [
            ("General", General.Draw, null, true),
            ("Multi Mode", Multi.Draw, null, true),
            ("Deployables", Deployables.Draw, null, true),
            ("Character Order", CharaOrder.Draw, null, true),
            ("Exclusions", Exclusions.Draw, null, true),
            ("Other", Other.Draw, null, true),
            ("Experiments", ExperimentsMain.Draw, null, true),
            ]);
    }

    internal static void SliderIntFrameTimeAsFPS(string name, ref int frameTime, int min = 1)
    {
        int fps = 60;
        if (frameTime != 0)
        {
            fps = (int)(1000f / frameTime);
        }
        ImGuiEx.SliderInt(name, ref fps, min, 60, fps == 60 ? "Unlimited" : null, ImGuiSliderFlags.AlwaysClamp);
        frameTime = fps == 60 ? 0 : (int)(1000f / fps);
    }

    internal static void QRA(string text, ref LimitedKeys key)
    {
        if (DrawKeybind(text, ref key))
        {
            P.quickSellItems.Toggle();
        }
        ImGui.SameLine();
        ImGuiEx.Text("+ right click");
    }

    static string KeyInputActive = null;
    internal static bool DrawKeybind(string text, ref LimitedKeys key)
    {
        bool ret = false;
        ImGui.PushID(text);
        ImGuiEx.Text($"{text}:");
        ImGui.Dummy(new(20, 1));
        ImGui.SameLine();
        ImGuiEx.SetNextItemWidthScaled(200f);
        if (ImGui.BeginCombo("##inputKey", $"{key}"))
        {
            if (text == KeyInputActive)
            {
                ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...");
                foreach (var x in Enum.GetValues<LimitedKeys>())
                {
                    if (IsKeyPressed(x))
                    {
                        KeyInputActive = null;
                        key = x;
                        ret = true;
                        break;
                    }
                }
            }
            else
            {
                if (ImGui.Selectable("Auto-detect new key", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    KeyInputActive = text;
                }
                ImGuiEx.Text($"Select key manually:");
                ImGuiEx.SetNextItemFullWidth();
                ImGuiEx.EnumCombo("##selkeyman", ref key);
            }
            ImGui.EndCombo();
        }
        else
        {
            if (text == KeyInputActive)
            {
                KeyInputActive = null;
            }
        }
        if (key != LimitedKeys.None)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                key = LimitedKeys.None;
                ret = true;
            }
        }
        ImGui.PopID();
        return ret;
    }
}
