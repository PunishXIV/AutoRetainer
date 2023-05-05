using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using PInvoke;
using PunishLib.ImGuiMethods;
using System.Windows.Forms;

namespace AutoRetainer.UI.Settings;

internal static class SettingsMain
{
    internal static void Draw()
    {
        ImGuiEx.EzTabBar("GeneralSettings",
            ("General", TabGeneral, null, true),
            ("Multi Mode", TabMulti, null, true),
            ("Other", TabOther, null, true)
            );
    }

    static void TabGeneral()
    {
        ImGuiHelpers.ScaledDummy(5f);
        InfoBox.DrawBox("Settings", delegate
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderInt("Time Desynchronization Compensation", ref P.config.UnsyncCompensation.ValidateRange(-60, 0), -10, 0);
            ImGuiComponents.HelpMarker("Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC. ");
            ImGui.Checkbox("Anonymise Retainers", ref P.config.NoNames);
            ImGuiComponents.HelpMarker("Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).");
            ImGui.Checkbox($"Display Quick Menu in Retainer UI", ref P.config.UIBar);
            ImGui.Checkbox($"Opt out of custom Dalamud theme", ref P.config.NoTheme);
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.SliderIntAsFloat("Interaction Delay, seconds", ref P.config.Delay.ValidateRange(10, 1000), 20, 1000);
            ImGuiComponents.HelpMarker("The lower this value is the faster plugin will use actions. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it. ");
            ImGui.Checkbox($"Display Extended Retainer Info", ref P.config.ShowAdditionalInfo);
            ImGuiComponents.HelpMarker("Displays retainer item level/gathering/perception and the name of their current venture in the main UI.");
        });
        InfoBox.DrawBox("Operation", delegate
        {
            if (ImGui.RadioButton("Assign + Reassign", P.config.EnableAssigningQuickExploration && !P.config._dontReassign))
            {
                P.config.EnableAssigningQuickExploration = true;
                P.config.DontReassign = false;
            }
            ImGuiComponents.HelpMarker("Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.");
            if (ImGui.RadioButton("Collect", !P.config.EnableAssigningQuickExploration && P.config._dontReassign))
            {
                P.config.EnableAssigningQuickExploration = false;
                P.config.DontReassign = true;
            }
            ImGuiComponents.HelpMarker("Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.");
            if (ImGui.RadioButton("Reassign", !P.config.EnableAssigningQuickExploration && !P.config._dontReassign))
            {
                P.config.EnableAssigningQuickExploration = false;
                P.config.DontReassign = false;
            }
            ImGuiComponents.HelpMarker("Only reassign ventures that retainers are undertaking.");

            var d = MultiMode.GetAutoAfkOpt() != 0;
            if (d) ImGui.BeginDisabled();
            ImGui.Checkbox("RetainerSense", ref P.config.RetainerSense);
            ImGuiComponents.HelpMarker($"AutoRetainer will automatically enable itself when the player is within interaction range of a Summoning Bell. You must remain stationary or the activation will be cancelled.");
            if (d)
            {
                ImGui.EndDisabled();
                ImGuiComponents.HelpMarker("Using RetainerSense requires Auto-afk option to be turned off.");
            }
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.SliderIntAsFloat("Activation Time", ref P.config.RetainerSenseThreshold, 1000, 100000);
        });
    }

    static void TabMulti()
    {
        ImGuiHelpers.ScaledDummy(5f);
        ImGui.Checkbox("Wait For Venture Completion", ref P.config.MultiWaitForAll);
        ImGuiComponents.HelpMarker("AutoRetainer will wait for all ventures to return before cycling to the next character in multi mode operation.");
        ImGui.SetNextItemWidth(60);
        ImGui.DragInt("Advance Relog Threshold", ref P.config.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
        ImGui.Checkbox($"Housing Bell Support", ref P.config.MultiAllowHET);
        ImGuiEx.TextWrapped(ImGuiColors.DalamudOrange, $"A Summoning Bell must be within range of the spawn point once the home is entered.");
        ImGui.Checkbox($"Upon activating Multi Mode, attempt to enter nearby house", ref P.config.MultiHETOnEnable);
        ImGui.Checkbox($"Display Login Overlay", ref P.config.LoginOverlay);
        ImGui.Checkbox($"Enforce Full Character Rotation", ref P.config.CharEqualize);
        ImGuiComponents.HelpMarker("Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.");
        ImGui.Checkbox($"Wait on login screen", ref P.config.MultiWaitOnLoginScreen);
        ImGuiComponents.HelpMarker($"If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.");
        ImGui.Checkbox("Synchronise Retainers (one time)", ref MultiMode.Synchronize);
        ImGuiComponents.HelpMarker("AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.");
        ImGui.Separator();
        ImGuiEx.Text($"Character Order:");
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
            ImGuiEx.TextV(Censor.Character(P.config.OfflineData[index].Name));
            ImGui.PopID();
        }

        if (P.config.Blacklist.Any())
        {
            InfoBox.DrawBox("Excluded Characters", delegate
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

    static void TabOther()
    {
        ImGuiHelpers.ScaledDummy(5f);
        InfoBox.DrawBox("Keybinds", delegate
        {
            DrawKeybind("Temporarily prevents AutoRetainer from being automatically enabled when using a Summoning Bell", ref P.config.Suppress);
            DrawKeybind("Temporarily set the Collect Operation mode, preventing ventures from being assigned for the current cycle", ref P.config.TempCollectB);
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
            ImGui.Checkbox($"Record Venture Statistics", ref P.config.RecordStats);
        });
        InfoBox.DrawBox("Automatic Grand Company Expert Delivery", AutoGCHandinUI.Draw);
    }

    static void QRA(string text, ref Keys key)
    {
        if(DrawKeybind(text, ref key))
        {
            P.quickSellItems.Toggle();
        }
        ImGui.SameLine();
        ImGuiEx.Text("+ right click");
    }

    static string KeyInputActive = null;
    static bool DrawKeybind(string text, ref Keys key)
    {
        bool ret = false;
        ImGui.PushID(text);
        ImGuiEx.Text($"{text}:");
        ImGui.Dummy(new(20, 1));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f);
        if (ImGui.BeginCombo("##inputKey", $"{key}"))
        {
            if (text == KeyInputActive)
            {
                ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...");
                foreach (var x in Enum.GetValues<Keys>())
                {
                    if (Utils.IsKeyPressed(x))
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
            if(text == KeyInputActive)
            {
                KeyInputActive = null;
            }
        }
        if (key != Keys.None)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                key = Keys.None;
                ret = true;
            }
        }
        ImGui.PopID();
        return ret;
    }
}
