using AutoRetainer.Multi;
using AutoRetainer.Offline;
using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoRetainer.UI;

internal unsafe static class MultiModeUI
{
    internal static void Draw()
    {
        if(MultiMode.GetAutoAfkOpt() != 0)
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
        if (ImGui.CollapsingHeader("Configuration")) 
        {
            ImGui.Checkbox($"I have multiple service accounts", ref P.config.MultipleServiceAccounts);
            ImGui.Checkbox("Wait for all retainers to be done before logging into character", ref P.config.MultiWaitForAll);
            ImGui.SetNextItemWidth(60);
            ImGui.DragInt("Relog in advance, seconds", ref P.config.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
            ImGui.Checkbox("Synchronize retainers (one time)", ref MultiMode.Synchronize);
            ImGuiComponents.HelpMarker("If this setting is on, plugin will wait until all enabled retainers have done their ventures. After that this setting will be disabled automatically and all characters will be processed.");
        }
        for(var index = 0; index < P.config.OfflineData.Count; index++)
        {
            var x = P.config.OfflineData[index];
            if (x.World.IsNullOrEmpty()) continue;
            ImGui.PushID(x.CID.ToString());
            var rCurPos = ImGui.GetCursorPos();
            if (ImGui.Checkbox("##enable", ref x.Enabled))
            {
                if(x.Enabled && !x.Index.InRange(1, 9))
                {
                    x.Enabled = false;
                    Notify.Error("Set character index first");
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Relog"))
            {
                if (MultiMode.Enabled)
                {
                    foreach(var z in P.config.OfflineData)
                    {
                        z.Preferred = false;
                    }
                    Notify.Warning("Preferred character has been reset");
                }
                if(MultiMode.Relog(x, out var error))
                {
                    Notify.Success("Relogging...");
                }
                else
                {
                    Notify.Error(error);
                }
            }
            ImGui.SameLine();
            var initCurpos = ImGui.GetCursorPos();
            var lowestRetainer = P.config.MultiWaitForAll? x.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).LastOrDefault() : x.GetEnabledRetainers().OrderBy(z => z.GetVentureSecondsRemaining()).FirstOrDefault();
            if (lowestRetainer != default)
            {
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                ImGui.ProgressBar(Math.Max(0, (float)(3600 - lowestRetainer.GetVentureSecondsRemaining(false)) / 3600f), new(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("A").Y + ImGui.GetStyle().FramePadding.Y*2), "");
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(initCurpos);
            }
            float pad = 0;
            var col = x.Preferred;
            if (col)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, GradientColor.Get(ImGui.GetStyle().Colors[(int)ImGuiCol.Text], ImGuiColors.ParsedGreen));
            }
            if (ImGui.CollapsingHeader((P.config.NoNames?$"Character {index+1}":$"{x}")+$"###chara{x.CID}"))
            {
                SetAsPreferred(x);
                if (col)
                {
                    ImGui.PopStyleColor();
                    col = false;
                }
                pad = ImGui.GetStyle().FramePadding.Y;
                var enabledRetainers = x.GetEnabledRetainers();

                ImGuiEx.TextV("Character index:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.BeginCombo("##index", x.Index == 0?"n/a":x.Index.ToString()))
                {
                    for(var i = 1; i <= 8; i++)
                    {
                        if (ImGui.Selectable($"{i}"))
                        {
                            x.Index = i;
                        }
                    }
                    ImGui.EndCombo();
                }

                if (P.config.MultipleServiceAccounts)
                {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    if (ImGui.BeginCombo("##sindex", $"Service account {x.ServiceAccount+1}"))
                    {
                        for (var i = 1; i <= 10; i++)
                        {
                            if (ImGui.Selectable($"Service account {i}"))
                            {
                                x.ServiceAccount = i-1;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }

                ImGui.SameLine();
                if(ImGui.Checkbox("Preferred character", ref x.Preferred))
                {
                    foreach(var z in P.config.OfflineData)
                    {
                        if(z.CID != x.CID)
                        {
                            z.Preferred = false;
                        }
                    }
                }
                ImGui.SameLine();
                if (ImGui.ArrowButton("##up", ImGuiDir.Up) && index > 0)
                {
                    try
                    {
                        (P.config.OfflineData[index - 1], P.config.OfflineData[index]) = (P.config.OfflineData[index], P.config.OfflineData[index - 1]);
                    }
                    catch(Exception e)
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
                    catch(Exception e)
                    {
                        e.Log();
                    }
                }
                ImGuiEx.Text($"Ventures: {x.Ventures}");
                ImGui.SameLine();
                ImGuiEx.Text($"Inventory slots: {x.InventorySpace}");
                //ImGui.SameLine();
                //ImGuiEx.Text($"Venture coffers: {x.VentureCoffers}");
                for (var n=0;n < x.RetainerData.Count;n++)
                {
                    var r = x.RetainerData[n];
                    if (r.Name.IsNullOrEmpty()) continue;
                    ImGui.PushID(r.Name);
                    ImGuiEx.Text(enabledRetainers.Contains(r) ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudGrey, $"{(P.config.NoNames?"Retainer "+(n+1):r.Name)}");
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(150f);
                    ImGuiEx.Text(r.HasVenture ? Utils.ToTimeString(r.GetVentureSecondsRemaining()) : "No venture");
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(250f);
                    ImGuiEx.Text("Level "+r.Level);
                    ImGui.PopID();
                }
            }
            else
            {
                SetAsPreferred(x);
                if (col)
                {
                    ImGui.PopStyleColor();
                    col = false;
                }
            }
            var rightText = $"V: {x.Ventures} | I: {x.InventorySpace}";
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
