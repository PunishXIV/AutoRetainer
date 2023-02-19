using AutoRetainer.Multi;
using AutoRetainer.Offline;
using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoRetainer.UI;

internal unsafe static class MultiModeUI
{
    static Dictionary<string, (Vector2 start, Vector2 end)> bars = new();
    internal static void Draw()
    {
        if(MultiMode.Enabled && MultiMode.GetAutoAfkOpt() != 0)
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "Multi Mode requires Auto-afk option to be turned off");
        }
        if (ImGui.CollapsingHeader("Setup Guide"))
        {
            ImGuiEx.TextWrapped("1. Log into each of your characters, assign necessary ventures to your retainers and enable retainers that you want to resend on each character.");
            ImGuiEx.TextWrapped("2. For each character, configure character index. Character index is position of your character on a screen where you select characters. If you only have one character per world, it's usually just 1.");
            ImGuiEx.TextWrapped("3. Ensure that your characters are in their home worlds and close to retainer bells, preferably in not crowded areas. No housing retainer bells. The suggested place is the inn.");
            ImGuiEx.TextWrapped("4. Characters that ran out of ventures or inventory space will be automatically excluded from rotation. You will need to reenable them once you clean up inventory and restock ventures.");
            ImGuiEx.TextWrapped("5. You may set up one character to be preferred. When no retainers have upcoming ventures in next 15 minutes, you will be relogged back on that character.");
        }
        for(var index = 0; index < P.config.OfflineData.Count; index++)
        {
            var data = P.config.OfflineData[index];
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
                    Notify.Error("Set character index first");
                }
            }
            if (colen) ImGui.PopStyleColor();
            ImGui.SameLine(0, 3);
            if (ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen))
            {
                if (MultiMode.Enabled)
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
            ImGui.SameLine(0, 3);
            if (ImGuiEx.IconButton(FontAwesomeIcon.Cog))
            {
                ImGui.OpenPopup($"popup{data.CID}");
            }
            ImGui.SameLine(0, 3);

            if (ImGui.BeginPopup($"popup{data.CID}"))
            {
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
                    if (ImGui.BeginCombo("##sindex", $"Service account {data.ServiceAccount + 1}"))
                    {
                        for (var i = 1; i <= 10; i++)
                        {
                            if (ImGui.Selectable($"Service account {i}"))
                            {
                                data.ServiceAccount = i - 1;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }

                if (ImGui.Checkbox("Preferred character", ref data.Preferred))
                {
                    foreach (var z in P.config.OfflineData)
                    {
                        if (z.CID != data.CID)
                        {
                            z.Preferred = false;
                        }
                    }
                }

                ImGuiEx.Text($"Change character order:");
                
                if (ImGui.ArrowButton("##up", ImGuiDir.Up) && index > 0)
                {
                    try
                    {
                        (P.config.OfflineData[index - 1], P.config.OfflineData[index]) = (P.config.OfflineData[index], P.config.OfflineData[index - 1]);
                    }
                    catch (Exception e)
                    {
                        e.Log();
                    }
                }
                ImGui.SameLine();
                if (ImGui.ArrowButton("##down", ImGuiDir.Down) && index < P.config.OfflineData.Count - 1)
                {
                    try
                    {
                        (P.config.OfflineData[index + 1], P.config.OfflineData[index]) = (P.config.OfflineData[index], P.config.OfflineData[index + 1]);
                    }
                    catch (Exception e)
                    {
                        e.Log();
                    }
                }
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
                ImGui.BeginTable("##ertainertable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Venture");
                ImGui.TableSetupColumn("Interaction");
                ImGui.TableHeadersRow();
                var retainers = P.GetSelectedRetainers(data.CID);
                for (var i = 0; i < data.RetainerData.Count; i++)
                {
                    var ret = data.RetainerData[i];
                    if (ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                    var start = ImGui.GetCursorPos();
                    var selected = retainers.Contains(ret.Name.ToString());
                    if (ImGui.Checkbox($"Retainer {(P.config.NoNames ? (i + 1) : ret.Name)}", ref selected))
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
                    var end = ImGui.GetCursorPos();
                    bars[$"{data.CID}{data.RetainerData[i].Name}"] = (start, end);
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                    ImGuiEx.Text($"{(!ret.HasVenture ? "No Venture" : Utils.ToTimeString(ret.GetVentureSecondsRemaining(false)))}");
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                    ImGuiEx.Text($"{Utils.ToTimeString(Scheduler.GetRemainingBanTime(ret.Name.ToString()))}");
                }
                ImGui.EndTable();
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
        ImGuiEx.ImGuiLineCentered("AYSButtonClear Interaction Timeouts", delegate
        {
            if (ImGui.SmallButton("Clear Interaction Timeouts"))
            {
                Scheduler.Bans.Clear();
            }
        });


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
            ImGuiEx.Text($"LastLongin: {MultiMode.LastLongin:X16}");
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
