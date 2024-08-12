global using OverlayTextData = (System.Numerics.Vector2 Curpos, (bool Warning, string Text)[] Texts);
using ECommons.Interop;

namespace AutoRetainer.UI;

internal static class UIUtils
{
    public static void DrawTeleportIcons(ulong cid)
    {
        var data = S.LifestreamIPC.GetHousePathData(cid);
        if(C.AllowFcTeleport)
        {
            string error = null;
            if(data.FC == null)
            {
                error = "Free company house is not registered in Lifestream";
            }
            else if(data.FC.PathToEntrance.Count == 0)
            {
                error = "Free company house is registered in Lifestream but path to entrance is not set";
            }
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(error == null ? null : ImGuiColors.DalamudGrey3, "\uf1ad");
            ImGui.PopFont();
            ImGuiEx.Tooltip(error ?? "Free company house is registered in Lifestream and path is set. You will be teleported to Free company house for resending Deployables. If Private house is not registered, you will be teleported to Free company house for resending retainers as well.");
            ImGui.SameLine(0,3);
        }
        if(C.AllowPrivateTeleport)
        {
            string error = null;
            if(data.Private == null)
            {
                error = "Private house is not registered in Lifestream.";
            }
            else if(data.Private.PathToEntrance.Count == 0)
            {
                error = "Private house is registered in Lifestream but path to entrance is not set.";
            }
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(error == null ? null:ImGuiColors.DalamudGrey3, "\ue1b0");
            ImGui.PopFont();
            ImGuiEx.Tooltip(error ?? "Private is registered in Lifestream and path is set. You will be teleported to Private house for resending Retainers.");
            ImGui.SameLine(0, 3);
        }
    }

    public static void DrawOverlayTexts(List<OverlayTextData> overlayTexts)
    {
        if(overlayTexts.Count > 0)
        {
            var maxSizes = new float[overlayTexts[0].Texts.Length];
            for(var i = 0; i < maxSizes.Length; i++)
            {
                maxSizes[i] = overlayTexts.Select(x => ImGui.CalcTextSize(x.Texts[i].Text).X).Max();
            }
            foreach(var x in overlayTexts)
            {
                var cur = ImGui.GetCursorPos();
                for(var i = x.Texts.Length - 1; i >= 0; i--)
                {
                    ImGui.SetCursorPos(new(x.Curpos.X - maxSizes[i..].Sum() - (maxSizes[i..].Length - 1) * ImGui.CalcTextSize("      ").X, x.Curpos.Y));
                    ImGuiEx.Text(x.Texts[i].Warning ? ImGuiColors.DalamudOrange : null, x.Texts[i].Text);
                }
                ImGui.SetCursorPos(cur);
            }
        }
    }

    internal static void SliderIntFrameTimeAsFPS(string name, ref int frameTime, int min = 1)
    {
        var fps = 60;
        if(frameTime != 0)
        {
            fps = (int)(1000f / frameTime);
        }
        ImGuiEx.SliderInt(name, ref fps, min, 60, fps == 60 ? "Unlimited" : null, ImGuiSliderFlags.AlwaysClamp);
        frameTime = fps == 60 ? 0 : (int)(1000f / fps);
    }

    internal static void QRA(string text, ref LimitedKeys key)
    {
        if(DrawKeybind(text, ref key))
        {
            P.quickSellItems.Toggle();
        }
        ImGui.SameLine();
        ImGuiEx.Text("+ right click");
    }

    private static string KeyInputActive = null;
    internal static bool DrawKeybind(string text, ref LimitedKeys key)
    {
        var ret = false;
        ImGui.PushID(text);
        ImGuiEx.Text($"{text}:");
        ImGui.Dummy(new(20, 1));
        ImGui.SameLine();
        ImGuiEx.SetNextItemWidthScaled(200f);
        if(ImGui.BeginCombo("##inputKey", $"{key}", ImGuiComboFlags.HeightLarge))
        {
            if(text == KeyInputActive)
            {
                ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...");
                foreach(var x in Enum.GetValues<LimitedKeys>())
                {
                    if(IsKeyPressed(x))
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
                if(ImGui.Selectable("Auto-detect new key", false, ImGuiSelectableFlags.DontClosePopups))
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
            if(text == KeyInputActive)
            {
                KeyInputActive = null;
            }
        }
        if(key != LimitedKeys.None)
        {
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                key = LimitedKeys.None;
                ret = true;
            }
        }
        ImGui.PopID();
        return ret;
    }
}
