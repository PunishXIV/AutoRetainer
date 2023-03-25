using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoRetainer.UI;

internal unsafe static class MultiModeUI
{
    internal static bool JustRelogged = false;
    static Dictionary<string, (Vector2 start, Vector2 end)> bars = new();
    internal static void Draw()
    {
        if(MultiMode.Enabled && MultiMode.GetAutoAfkOpt() != 0)
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "Multi Mode requires Auto-afk option to be turned off");
        }
        /*if (ImGui.CollapsingHeader("Setup Guide"))
        {
            ImGuiEx.TextWrapped("1. Log into each of your characters, assign necessary ventures to your retainers and enable retainers that you want to resend on each character.");
            ImGuiEx.TextWrapped("2. For each character, configure character index. Character index is position of your character on a screen where you select characters. If you only have one character per world, it's usually just 1.");
            ImGuiEx.TextWrapped("3. Ensure that your characters are in their home worlds and close to retainer bells, preferably in not crowded areas. No housing retainer bells. The suggested place is the inn.");
            ImGuiEx.TextWrapped("4. Characters that ran out of ventures or inventory space will be automatically excluded from rotation. You will need to reenable them once you clean up inventory and restock ventures.");
            ImGuiEx.TextWrapped("5. You may set up one character to be preferred. When no retainers have upcoming ventures in next 15 minutes, you will be relogged back on that character.");
        }*/
        P.config.OfflineData.RemoveAll(x => P.config.Blacklist.Any(z => z.CID == x.CID));
        var sortedData = new List<OfflineCharacterData>();
        var shouldExpand = false;
        var doExpand = JustRelogged && !P.config.NoCurrentCharaOnTop;
        JustRelogged = false;
        if (P.config.NoCurrentCharaOnTop)
        {
            sortedData = P.config.OfflineData;
        }
        else
        {
            if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
            {
                sortedData.Add(cdata);
                shouldExpand = true;
            }
            foreach (var x in P.config.OfflineData)
            {
                if (x.CID != Svc.ClientState.LocalContentId)
                {
                    sortedData.Add(x);
                }
            }
        }
        for(var index = 0; index < sortedData.Count; index++)
        {
            var data = sortedData[index];
            if (data.World.IsNullOrEmpty()) continue;
            ImGui.PushID(data.CID.ToString());
            var rCurPos = ImGui.GetCursorPos();
            var colen = false;
            if (data.Enabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);
                colen = true;
            }
            if (ImGuiEx.IconButton("\uf021"))
            {
                data.Enabled = !data.Enabled;
                if (data.Enabled && !data.Index.InRange(1, 9))
                {
                    data.Enabled = false;
                    Svc.Chat.PrintError("[AutoRetainer] Error: Please set the character index and service account for this character before enabling multi mode.");
                }
            }
            if (colen) ImGui.PopStyleColor();
            ImGuiEx.Tooltip($"Enable multi-mode for this character");
            ImGui.SameLine(0, 3);
            if (ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen))
            {
                if (MultiMode.Active)
                {
                    foreach(var z in P.config.OfflineData)
                    {
                        z.Preferred = false;
                    }
                    Notify.Warning("Preferred character has been reset");
                }
                if(MultiMode.Relog(data, out var error))
                {
                    Notify.Success("Relogging...");
                }
                else
                {
                    Notify.Error(error);
                }
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.SetClipboardText($"/ays relog {data.Name}@{data.World}");
            }
            ImGuiEx.Tooltip($"Left click - relog to this character\nRight click - copy relog command into clipboard");
            ImGui.SameLine(0, 3);
            if (ImGuiEx.IconButton(FontAwesomeIcon.Cog))
            {
                ImGui.OpenPopup($"popup{data.CID}");
            }
            ImGuiEx.Tooltip($"Configure Character");
            ImGui.SameLine(0, 3);

            if (ImGui.BeginPopup($"popup{data.CID}"))
            {
                var b = true;
                ImGui.CollapsingHeader($"{data.Name} Configuration##conf", ref b, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
                if(b == false)
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGuiEx.TextV("Character index:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.BeginCombo("##index", data.Index == 0 ? "n/a" : data.Index.ToString()))
                {
                    for (var i = 1; i <= 8; i++)
                    {
                        if (ImGui.Selectable($"{i}"))
                        {
                            data.Index = i;
                        }
                    }
                    ImGui.EndCombo();
                }

                //if (P.config.MultipleServiceAccounts)
                {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    if (ImGui.BeginCombo("##sindex", $"Service Account {data.ServiceAccount + 1}"))
                    {
                        for (var i = 1; i <= 10; i++)
                        {
                            if (ImGui.Selectable($"Service Account {i}"))
                            {
                                data.ServiceAccount = i - 1;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }

                if (ImGui.Checkbox("Preferred Character", ref data.Preferred))
                {
                    foreach (var z in P.config.OfflineData)
                    {
                        if (z.CID != data.CID)
                        {
                            z.Preferred = false;
                        }
                    }
                }
                ImGuiComponents.HelpMarker("When operating in multi mode, if there are no other characters with imminent ventures to collect, it will relog back to your preferred character.");

                ImGuiEx.Text($"GC handin configuration:");
                if (!AutoGCHandin.Operation)
                {
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);
                }
                else
                {
                    ImGuiEx.Text($"Can't change this now");
                }
                ImGui.Separator();
                if(ImGui.Button("Exclude Character"))
                {
                    P.config.Blacklist.Add((data.CID, data.Name));
                }
                ImGuiComponents.HelpMarker("Excluding this character will immediately reset it's settings, remove it from this list and exclude all retainers from being processed. You can still run manual tasks on it's retainers. You can cancel this action in settings.");
                ImGui.EndPopup();
            }

            var initCurpos = ImGui.GetCursorPos();
            var lowestRetainer = P.config.MultiWaitForAll? data.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).LastOrDefault() : data.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).FirstOrDefault();
            if (lowestRetainer != default)
            {
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                ImGui.ProgressBar(Math.Max(0, (float)(3600 - lowestRetainer.GetVentureSecondsRemaining(false)) / 3600f), new(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y*2), "");
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(initCurpos);
            }
            float pad = 0;
            var col = data.Preferred;
            if (col)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, GradientColor.Get(ImGui.GetStyle().Colors[(int)ImGuiCol.Text], ImGuiColors.ParsedGreen));
            }
            if (shouldExpand && doExpand)
            {
                ImGui.SetNextItemOpen(index == 0);
            }
            if (ImGui.CollapsingHeader((P.config.NoNames?$"Character {index+1}":$"{data}")+$"###chara{data.CID}"))
            {
                SetAsPreferred(data);
                if (col)
                {
                    ImGui.PopStyleColor();
                    col = false;
                }
                pad = ImGui.GetStyle().FramePadding.Y;
                var enabledRetainers = data.GetEnabledRetainers();
                ImGui.PushID(data.CID.ToString());

                var storePos = ImGui.GetCursorPos();
                for (var i = 0; i < data.RetainerData.Count; i++)
                {
                    if (bars.TryGetValue($"{data.CID}{data.RetainerData[i].Name}", out var v))
                    {
                        var ret = data.RetainerData[i];
                        if (!ret.HasVenture || ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                        ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
                        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                        ImGui.ProgressBar(1f - Math.Min(1f, (float)ret.GetVentureSecondsRemaining(false) / (60f * 60f)),
                            new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
                        ImGui.PopStyleColor(2);
                    }
                }
                ImGui.SetCursorPos(storePos);
                if (ImGui.BeginTable("##retainertable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Job");
                    ImGui.TableSetupColumn("Venture");
                    ImGui.TableSetupColumn("");
                    ImGui.TableHeadersRow();
                    var retainers = P.GetSelectedRetainers(data.CID);
                    for (var i = 0; i < data.RetainerData.Count; i++)
                    {
                        var ret = data.RetainerData[i];
                        if (ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                        var adata = Utils.GetAdditionalData(data.CID, ret.Name);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                        var start = ImGui.GetCursorPos();
                        var selected = retainers.Contains(ret.Name.ToString());
                        if (ImGui.Checkbox($"{(P.config.NoNames ? $"Retainer {(i + 1)}" : ret.Name)}", ref selected))
                        {
                            if (selected)
                            {
                                retainers.Add(ret.Name.ToString());
                            }
                            else
                            {
                                retainers.Remove(ret.Name.ToString());
                            }
                        }
                        if (adata.EntrustDuplicates)
                        {
                            ImGui.SameLine();
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGuiEx.Text($"\uf24d");
                            ImGui.PopFont();
                        }
                        if (adata.WithdrawGil)
                        {
                            ImGui.SameLine();
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGuiEx.Text($"\uf51e");
                            ImGui.PopFont();
                        }
                        var end = ImGui.GetCursorPos();
                        bars[$"{data.CID}{data.RetainerData[i].Name}"] = (start, end);
                        ImGui.TableNextColumn();
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);

                        if (ThreadLoadImageHandler.TryGetIconTextureWrap(ret.Job == 0 ? 62143 : (062100 + ret.Job), true, out var t))
                        {
                            ImGui.Image(t.ImGuiHandle, new(24, 24));
                        }
                        else
                        {
                            ImGui.Dummy(new(24, 24));
                        }
                        if (ret.Level > 0)
                        {
                            ImGui.SameLine(0, 2);
                            ImGuiEx.TextV($"{ret.Level}".ReplaceByChar("0123456789", ""));
                        }
                        ImGui.TableNextColumn();
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                        ImGuiEx.Text($"{(!ret.HasVenture ? "No Venture" : Utils.ToTimeString(ret.GetVentureSecondsRemaining(false)))}");
                        ImGui.TableNextColumn();
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                        var n = $"{data.CID} {ret.Name} settings";
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Cogs, $"{data.CID} {ret.Name}"))
                        {
                            ImGui.OpenPopup(n);
                        }
                        if (ImGuiEx.BeginPopupNextToElement(n))
                        {
                            ImGui.CollapsingHeader($"{ret.Name} - {data.Name} configuration  ##conf", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
                            ImGuiEx.Text($"Additional post-venture tasks:");
                            ImGui.Checkbox($"Entrust duplicates", ref adata.EntrustDuplicates);
                            ImGui.Checkbox($"Withdraw or Deposit gil", ref adata.WithdrawGil);
                            if (adata.WithdrawGil)
                            {
                                if (ImGui.RadioButton("Withdraw", !adata.Deposit)) adata.Deposit = false;
                                if (ImGui.RadioButton("Deposit", adata.Deposit)) adata.Deposit = true;
                                ImGui.SetNextItemWidth(200f);
                                ImGui.InputInt($"Amount, %", ref adata.WithdrawGilPercent.ValidateRange(1, 100), 1, 10);
                            }
                            ImGui.EndPopup();
                        }
                    }
                    ImGui.EndTable();
                }
                ImGui.Dummy(new(2, 2));
                ImGui.PopID();
            }
            else
            {
                SetAsPreferred(data);
                if (col)
                {
                    ImGui.PopStyleColor();
                    col = false;
                }
            }
            var rightText = $"V: {data.Ventures} | I: {data.InventorySpace}";
            var cur = ImGui.GetCursorPos();
            ImGui.SameLine();
            ImGui.SetCursorPos(new(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + pad));
            ImGuiEx.Text(rightText);
            ImGui.PopID();
        }


        if (P.config.Verbose && ImGui.CollapsingHeader("Debug"))
        {
            ImGuiEx.Text($"GetCurrentTargetCharacter: {MultiMode.GetCurrentTargetCharacter()}");
            ImGuiEx.Text($"Yes Already: {YesAlready.IsEnabled()}");
            ImGuiEx.Text($"IsCurrentCharacterDone: {MultiMode.IsCurrentCharacterDone()}");
            ImGuiEx.Text($"NextInteraction: {Math.Max(0, MultiMode.NextInteractionAt - Environment.TickCount64)}");
            ImGuiEx.Text($"EnsureCharacterValidity: {MultiMode.EnsureCharacterValidity(true)}");
            ImGuiEx.Text($"GetNearbyBell: {MultiMode.GetNearbyBell()}");
            ImGuiEx.Text($"IsInteractionAllowed: {MultiMode.IsInteractionAllowed()}");
            ImGuiEx.Text($"GetPreferredCharacter: {MultiMode.GetPreferredCharacter()}");
            ImGuiEx.Text($"IsAllRetainersHaveMoreThan15Mins: {MultiMode.IsAllRetainersHaveMoreThan15Mins()}");
            ImGuiEx.Text($"Target ?? Preferred: {MultiMode.GetCurrentTargetCharacter() ?? MultiMode.GetPreferredCharacter()}");
            ImGuiEx.Text($"GetAutoAfkOpt: {MultiMode.GetAutoAfkOpt()}");
            ImGuiEx.Text($"AutoAfkValue: {ConfigModule.Instance()->GetIntValue(145)}");
            ImGuiEx.Text($"LastLongin: {MultiMode.LastLogin:X16}");
            ImGuiEx.Text($"AnyRetainersAvailable: {MultiMode.AnyRetainersAvailable()}");
            ImGuiEx.Text($"IsAnySelectedRetainerFinishesWithin, 60: {MultiMode.IsAnySelectedRetainerFinishesWithin(60)}");
            ImGuiEx.Text($"IsAnySelectedRetainerFinishesWithin, 5*60: {MultiMode.IsAnySelectedRetainerFinishesWithin(5*60)}");
            foreach(var data in P.config.OfflineData)
            {
                ImGuiEx.Text($"Chatacter {data}\n  GetNeededVentureAmount: {data.GetNeededVentureAmount()}");
            }
        }
    }

    static void SetAsPreferred(OfflineCharacterData x)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            if (x.Preferred)
            {
                x.Preferred = false;
            }
            else
            {
                P.config.OfflineData.Each(x => x.Preferred = false);
                x.Preferred = true;
            }
        }
    }
}
