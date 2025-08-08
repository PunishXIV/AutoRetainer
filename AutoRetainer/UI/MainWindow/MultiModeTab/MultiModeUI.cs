using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;

internal static unsafe class MultiModeUI
{
    internal static bool JustRelogged = false;
    private static Dictionary<string, (Vector2 start, Vector2 end)> bars = [];
    private static float StatusTextWidth = 0f;
    internal static void Draw()
    {
        List<OverlayTextData> overlayTexts = [];
        C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
        var sortedData = new List<OfflineCharacterData>();
        var shouldExpand = false;
        var doExpand = JustRelogged && !C.NoCurrentCharaOnTop;
        JustRelogged = false;
        if(C.NoCurrentCharaOnTop)
        {
            sortedData = C.OfflineData.ApplyOrder(C.RetainersVisualOrders);
        }
        else
        {
            if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
            {
                sortedData.Add(cdata);
                shouldExpand = true;
            }
            foreach(var x in C.OfflineData.ApplyOrder(C.RetainersVisualOrders))
            {
                if(x.CID != Svc.ClientState.LocalContentId)
                {
                    sortedData.Add(x);
                }
            }
        }
        UIUtils.DrawSearch();

        for(var index = 0; index < sortedData.Count; index++)
        {
            var data = sortedData[index];
            if(data.World.IsNullOrEmpty() || data.ExcludeRetainer) continue;
            var search = Ref<string>.Get("SearchChara");
            if(search != "" && !$"{data.Name}@{data.World}".Contains(search, StringComparison.OrdinalIgnoreCase)) continue;
            ImGuiEx.PushID(data.CID.ToString());
            var rCurPos = ImGui.GetCursorPos();
            var colen = false;
            if(data.Enabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);
                colen = true;
            }
            if(ImGuiEx.IconButton(Lang.IconMultiMode))
            {
                data.Enabled = !data.Enabled;
            }
            if(colen) ImGui.PopStyleColor();
            ImGuiEx.Tooltip($"Enable multi-mode for this character");
            ImGui.SameLine(0, 3);
            if(ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen))
            {
                if(MultiMode.Relog(data, out var error, RelogReason.ConfigGUI))
                {
                    Notify.Success("Relogging...");
                }
                else
                {
                    Notify.Error(error);
                }
            }
            if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                Copy($"/ays relog {data.Name}@{data.World}");
            }
            ImGuiEx.Tooltip($"Left click - relog to this character\nRight click - copy relog command into clipboard");
            ImGui.SameLine(0, 3);
            if(ImGuiEx.IconButton(FontAwesomeIcon.UserCog))
            {
                ImGui.OpenPopup($"popup{data.CID}");
            }
            ImGuiEx.Tooltip($"Configure Character");
            ImGui.SameLine(0, 3);

            if(ImGui.BeginPopup($"popup{data.CID}"))
            {
                CharaConfig.Draw(data, true);
                ImGui.EndPopup();
            }
            data.DrawDCV();
            UIUtils.DrawTeleportIcons(data.CID);
            SharedUI.DrawLockout(data);

            var initCurpos = ImGui.GetCursorPos();
            var lowestRetainer = C.MultiModeRetainerConfiguration.MultiWaitForAll ? data.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).LastOrDefault() : data.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).FirstOrDefault();
            if(lowestRetainer != default)
            {
                var prog = Math.Max(0, (3600 - lowestRetainer.GetVentureSecondsRemaining(false)) / 3600f);
                var pcol = prog == 1f ? (C.NoGradient ? 0xbb005000.ToVector4() : GradientColor.Get(0xbb500000.ToVector4(), 0xbb005000.ToVector4())) : 0xbb500000.ToVector4();
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, pcol);
                ImGui.ProgressBar(prog, new(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y * 2), "");
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(initCurpos);
            }
            var colpref = UIUtils.PushColIfPreferredCurrent(data);

            if(shouldExpand && doExpand)
            {
                ImGui.SetNextItemOpen(index == 0);
            }
            if(ImGui.CollapsingHeader(data.GetCutCharaString(StatusTextWidth) + $"###workshop{data.CID}" + $"###chara{data.CID}"))
            {
                SetAsPreferred(data);
                if(colpref)
                {
                    ImGui.PopStyleColor();
                    colpref = false;
                }
                var enabledRetainers = data.GetEnabledRetainers();
                ImGuiEx.PushID(data.CID.ToString());

                var storePos = ImGui.GetCursorPos();
                var retainerData = data.RetainerData;
                foreach(var ret in retainerData)
                {
                    if(bars.TryGetValue($"{data.CID}{ret.Name}", out var v))
                    {
                        if(!ret.HasVenture || ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                        ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
                        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                        ImGui.ProgressBar(1f - Math.Min(1f, ret.GetVentureSecondsRemaining(false) / (60f * 60f)),
                            new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
                        ImGui.PopStyleColor(2);
                    }
                }
                ImGui.SetCursorPos(storePos);
                RetainerTable.Draw(data, retainerData, bars);
                ImGui.Dummy(new(2, 2));
                ImGui.PopID();
            }
            else
            {
                SetAsPreferred(data);
                if(colpref)
                {
                    ImGui.PopStyleColor();
                    colpref = false;
                }
            }
            ImGui.SameLine(0, 0);
            List<(bool, string)> texts = [(data.Ventures < C.UIWarningRetVentureNum, $"V: {data.Ventures}"), (data.InventorySpace < C.UIWarningRetSlotNum, $"I: {data.InventorySpace}")];
            if(C.CharEqualize && MultiMode.Enabled)
            {
                texts.Insert(0, (false, $"C: {MultiMode.CharaCnt.GetOrDefault(data.CID)}"));
            }
            overlayTexts.Add((new Vector2(ImGui.GetContentRegionMax().X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + ImGui.GetStyle().FramePadding.Y), [.. texts]));
            ImGui.NewLine();
            ImGui.PopID();
        }
        
        StatusTextWidth = 0f;
        UIUtils.DrawOverlayTexts(overlayTexts, ref StatusTextWidth);

        if(C.Verbose && ImGui.CollapsingHeader("Debug"))
        {
            ImGuiEx.Text($"GetCurrentTargetCharacter: {MultiMode.GetCurrentTargetCharacter()}");
            //ImGuiEx.Text($"Yes Already: {YesAlready.IsEnabled()}");
            ImGuiEx.Text($"IsCurrentCharacterDone: {MultiMode.IsCurrentCharacterDone()}");
            ImGuiEx.Text($"NextInteraction: {Math.Max(0, MultiMode.NextInteractionAt - Environment.TickCount64)}");
            ImGuiEx.Text($"EnsureCharacterValidity: {MultiMode.EnsureCharacterValidity(true)}");
            ImGuiEx.Text($"IsInteractionAllowed: {MultiMode.IsInteractionAllowed()}");
            ImGuiEx.Text($"GetPreferredCharacter: {MultiMode.GetPreferredCharacter()}");
            ImGuiEx.Text($"IsAllRetainersHaveMoreThan15Mins: {MultiMode.IsAllRetainersHaveMoreThan15Mins()}");
            ImGuiEx.Text($"Target ?? Preferred: {MultiMode.GetCurrentTargetCharacter() ?? MultiMode.GetPreferredCharacter()}");
            //ImGuiEx.Text($"GetAutoAfkOpt: {MultiMode.GetAutoAfkOpt()}");
            //ImGuiEx.Text($"AutoAfkValue: {ConfigModule.Instance()->GetIntValue(145)}");
            ImGuiEx.Text($"LastLongin: {MultiMode.LastLogin:X16}");
            ImGuiEx.Text($"AnyRetainersAvailable: {MultiMode.AnyRetainersAvailable()}");
            ImGuiEx.Text($"IsAnySelectedRetainerFinishesWithin, 60: {MultiMode.IsAnySelectedRetainerFinishesWithin(60)}");
            ImGuiEx.Text($"IsAnySelectedRetainerFinishesWithin, 5*60: {MultiMode.IsAnySelectedRetainerFinishesWithin(5 * 60)}");
            foreach(var data in C.OfflineData)
            {
                ImGuiEx.Text($"Chatacter {data}\n  GetNeededVentureAmount: {data.GetNeededVentureAmount()}");
            }
        }
    }

    internal static void SetAsPreferred(OfflineCharacterData x)
    {
        if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            if(x.Preferred)
            {
                x.Preferred = false;
            }
            else
            {
                C.OfflineData.Each(x => x.Preferred = false);
                x.Preferred = true;
            }
        }
    }
}
