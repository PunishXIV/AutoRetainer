global using OverlayTextData = (System.Numerics.Vector2 Curpos, (bool Warning, string Text)[] Texts);
using AutoRetainerAPI.Configuration;
using ECommons.Interop;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.UI;

internal static class UIUtils
{
    public static void DrawSearch()
    {
        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##search", "Search characters...", ref Ref<string>.Get("SearchChara"), 50);
    }

    public static void DrawDCV(this OfflineCharacterData data)
    {
        if(data.WorldOverride != null)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text("\uf0ac");
            ImGui.PopFont();
            if(ImGuiEx.HoveredAndClicked("Visiting another data center. Right click to clear this status.", ImGuiMouseButton.Right))
            {
                data.WorldOverride = null;
            }
            ImGui.SameLine();
        }
    }

    public static void DrawTeleportIcons(ulong cid)
    {
        var offlineData = C.OfflineData.FirstOrDefault(x => x.CID == cid);
        if(offlineData == null) return;
        var data = S.LifestreamIPC.GetHousePathData(cid);
        if(offlineData.GetAllowFcTeleportForSubs() || offlineData.GetAllowFcTeleportForRetainers())
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
            ImGuiEx.Tooltip(error ?? $"Free company house is registered in Lifestream and path is set. You will be teleported to Free company house for resending Deployables. If enabled, you will be teleported to Free company house for resending retainers as well.\nAddress: {Svc.Data.GetExcelSheet<Aetheryte>().GetRow((uint)data.FC.ResidentialDistrict)?.Territory.Value.PlaceNameRegion.Value.Name}, ward {data.FC.Ward + 1}, plot {data.FC.Plot + 1}");
            ImGui.SameLine(0, 3);
        }
        if(offlineData.GetAllowPrivateTeleportForRetainers())
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
            ImGuiEx.Text(error == null ? null : ImGuiColors.DalamudGrey3, "\ue1b0");
            ImGui.PopFont();
            ImGuiEx.Tooltip(error ?? $"Private house is registered in Lifestream and path is set. You will be teleported to Private house for resending Retainers.\nAddress: {Svc.Data.GetExcelSheet<Aetheryte>().GetRow((uint)data.Private.ResidentialDistrict)?.Territory.Value.PlaceNameRegion.Value.Name}, ward {data.Private.Ward+1}, plot {data.Private.Plot+1}");
            ImGui.SameLine(0, 3);
        }
    }

    public static void DrawOverlayTexts(List<OverlayTextData> overlayTexts, ref float statusTextWidth)
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
                    var width = maxSizes[i..].Sum() + (maxSizes[i..].Length - 1) * ImGui.CalcTextSize("      ").X;
                    ImGui.SetCursorPos(new(x.Curpos.X - width, x.Curpos.Y));
                    if(statusTextWidth < width) statusTextWidth = width;
                    ImGuiEx.Text(x.Texts[i].Warning ? ImGuiColors.DalamudOrange : null, x.Texts[i].Text);
                }
                ImGui.SetCursorPos(cur);
            }
        }
    }

    public static float CollapsingHeaderSpacingsWidth => ImGui.GetStyle().FramePadding.X * 2f + ImGui.GetStyle().ItemSpacing.X * 2 + ImGui.CalcTextSize("▲...").X;

    public static string GetCutCharaString(this OfflineCharacterData data, float statusTextWidth)
    {
        var chstr = Censor.Character(data.Name, data.World);
        var mod = false;
        while(ImGui.CalcTextSize(chstr).X > ImGui.GetContentRegionAvail().X - statusTextWidth - UIUtils.CollapsingHeaderSpacingsWidth && chstr.Length > 5)
        {
            mod = true;
            chstr = chstr[0..^1];
        }
        if(mod) chstr += "...";
        return chstr;
    }

    internal static void SliderIntFrameTimeAsFPS(string name, ref int frameTime, int min = 1)
    {
        var fps = 60;
        if(frameTime != 0)
        {
            fps = GetFPSFromMSPT(frameTime);
        }
        ImGuiEx.SliderInt(name, ref fps, min, 60, fps == 60 ? "Unlimited" : null, ImGuiSliderFlags.AlwaysClamp);
        frameTime = fps == 60 ? 0 : (int)(1000f / fps);
    }

    public static int GetFPSFromMSPT(int frameTime)
    {
        return frameTime == 0?60:(int)(1000f / frameTime);
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
