using Dalamud.Interface.Components;
using ECommons.Configuration;
using ECommons.MathHelpers;
using PInvoke;
using PunishLib.ImGuiMethods;
using System;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.UI.Settings;

internal static class SettingsMain
{
    internal static void Draw()
    {
        ImGuiHelpers.ScaledDummy(5f);
        InfoBox.DrawBox("Settings", delegate
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderInt("Time Desynchronization Compensation", ref P.config.UnsyncCompensation.ValidateRange(-60, 0), -10, 0);
            ImGuiComponents.HelpMarker("Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC. ");
            ImGui.Checkbox("Anonymise Retainers", ref P.config.NoNames);
            ImGuiComponents.HelpMarker("Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).");
            ImGui.Checkbox($"Do not use built-in theme", ref P.config.NoTheme);
            ImGui.Checkbox($"Display quick access overlay in retainer list", ref P.config.UIBar);
        });
        InfoBox.DrawBox("Operation", delegate
        {
            if (ImGui.RadioButton("Assign + Reassign", P.config.EnableAssigningQuickExploration && !P.config.DontReassign))
            {
                P.config.EnableAssigningQuickExploration = true;
                P.config.DontReassign = false;
            }
            ImGuiComponents.HelpMarker("Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.");
            if (ImGui.RadioButton("Collect", !P.config.EnableAssigningQuickExploration && P.config.DontReassign))
            {
                P.config.EnableAssigningQuickExploration = false;
                P.config.DontReassign = true;
            }
            ImGuiComponents.HelpMarker("Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.");
            if (ImGui.RadioButton("Reassign", !P.config.EnableAssigningQuickExploration && !P.config.DontReassign))
            {
                P.config.EnableAssigningQuickExploration = false;
                P.config.DontReassign = false;
            }
            ImGuiComponents.HelpMarker("Only reassign ventures that retainers are undertaking.");
        });

        InfoBox.DrawBox("Keybinds", delegate
        {
            DrawKeybind("Temporarily suppress enabling when opening retainer bell", ref P.config.Suppress);
            DrawKeybind("Temporarily set retainer action to collect venture rewards when enabling AutoRetainer", ref P.config.TempCollectB);
        });

        InfoBox.DrawBox("Multi Mode", delegate
    {
        ImGui.Checkbox("Wait for all retainers to be done before logging into character", ref P.config.MultiWaitForAll);
        ImGui.SetNextItemWidth(60);
        ImGui.DragInt("Relog in advance, seconds", ref P.config.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
        ImGui.Checkbox("Synchronize retainers (one time)", ref MultiMode.Synchronize);
        ImGuiComponents.HelpMarker("If this setting is on, plugin will wait until all enabled retainers have done their ventures. After that this setting will be disabled automatically and all characters will be processed.");
        ImGui.Separator();
        ImGuiEx.Text($"Character order:");
        for (int index = 0; index < P.config.OfflineData.Count; index++)
        {
            if (P.config.OfflineData[index].World.IsNullOrEmpty()) continue;
            ImGui.PushID($"c{index}");
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
            ImGui.SameLine();
            ImGuiEx.TextV(P.config.OfflineData[index].Name);
            ImGui.PopID();
        }
    });
        InfoBox.DrawBox("Quick Retainer Action", delegate
        {
            QRA("Sell Item", ref P.config.SellKey);
            QRA("Entrust Item", ref P.config.EntrustKey);
            QRA("Retrieve Item", ref P.config.RetrieveKey);
            QRA("Put up For Sale", ref P.config.SellMarketKey);
        });
        InfoBox.DrawBox("Statistics", delegate
        {
            ImGui.Checkbox($"Record venture statistics", ref P.config.RecordStats);
        });
        if (P.config.Blacklist.Any())
        {
            InfoBox.DrawBox("Excluded characters", delegate
            {
                for (int i = 0; i < P.config.Blacklist.Count; i++)
                {
                    var d = P.config.Blacklist[i];
                    ImGuiEx.TextV($"{d.Name} ({d.CID:X16})");
                    ImGui.SameLine();
                    if (ImGui.Button($"Delete##bl{i}"))
                    {
                        P.config.Blacklist.RemoveAt(i);
                        P.config.SelectedRetainers.Remove(d.CID);
                        break;
                    }
                }
            });
        }
    }

    static void QRA(string text, ref Keys key)
    {
        ImGui.PushID(text);
        ImGuiEx.TextV($"{text}:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f);
        if (ImGui.BeginCombo("##inputKey", $"{key}"))
        {
            var block = false;
            if (ImGui.Selectable("Cancel"))
            {
            }
            if (ImGui.IsItemHovered()) block = true;
            if (ImGui.Selectable("Clear"))
            {
                key = Keys.None;
            }
            if (ImGui.IsItemHovered()) block = true;
            if (!block)
            {
                ImGuiEx.Text(GradientColor.Get(ImGuiColors.ParsedGreen, ImGuiColors.DalamudRed), "Now press new key...");
                foreach (var x in Enum.GetValues<Keys>())
                {
                    if (Bitmask.IsBitSet(User32.GetKeyState((int)x), 15))
                    {
                        ImGui.CloseCurrentPopup();
                        key = x;
                        P.quickSellItems.Toggle();
                        break;
                    }
                }
            }
            ImGui.EndCombo();
        }
        if (key != Keys.None)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                key = Keys.None;
            }
            ImGui.SameLine();
            ImGuiEx.Text("+ right click");
        }
        ImGui.PopID();
    }

    static void DrawKeybind(string text, ref Keys key)
    {
        ImGui.PushID(text);
        ImGuiEx.Text($"{text}:");
        ImGui.Dummy(new(20, 1));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f);
        if (ImGui.BeginCombo("##inputKey", $"{key}"))
        {
            var block = false;
            if (ImGui.Selectable("Cancel"))
            {
            }
            if (ImGui.IsItemHovered()) block = true;
            if (ImGui.Selectable("Clear"))
            {
                key = Keys.None;
            }
            if (ImGui.IsItemHovered()) block = true;
            if (!block)
            {
                ImGuiEx.Text(GradientColor.Get(ImGuiColors.ParsedGreen, ImGuiColors.DalamudRed), "Now press new key...");
                foreach (var x in Enum.GetValues<Keys>())
                {
                    if (Bitmask.IsBitSet(User32.GetKeyState((int)x), 15))
                    {
                        ImGui.CloseCurrentPopup();
                        key = x;
                        break;
                    }
                }
            }
            ImGui.EndCombo();
        }
        if (key != Keys.None)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                key = Keys.None;
            }
        }
        ImGui.PopID();
    }
}
