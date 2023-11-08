using Dalamud.Interface.Components;
using ECommons.Interop;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings.SettingsMain
{
    internal unsafe class General
    {
        internal static void Draw()
        {
            if (ImGuiGroup.BeginGroupBox("Settings"))
            {
                ImGui.SetNextItemWidth(100f);
                ImGui.SliderInt("Time Desynchronization Compensation", ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0);
                ImGuiComponents.HelpMarker("Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC. ");
                ImGui.Checkbox($"Enable frame delay", ref C.UseFrameDelay);
                ImGui.SetNextItemWidth(100f);
                if (!C.UseFrameDelay)
                {
                    ImGuiEx.SliderIntAsFloat("Interaction Delay, seconds", ref C.Delay.ValidateRange(10, 1000), 20, 1000);
                }
                else
                {
                    ImGui.SliderInt("Interaction Delay, frames", ref C.FrameDelay.ValidateRange(2, 500), 2, 12);
                }
                ImGuiComponents.HelpMarker("The lower this value is the faster plugin will use actions. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it. ");
                ImGui.Checkbox("Extra logging", ref C.ExtraDebug);
                ImGuiComponents.HelpMarker("This option enables excessive logging for debugging purposes. It will spam your log and cause performance issues while enabled. This option will disable itself upon plugin reload or game restart.");
                ImGuiGroup.EndGroupBox();
            };
            if (ImGuiGroup.BeginGroupBox("Operation"))
            {
                if (ImGui.RadioButton("Assign + Reassign", C.EnableAssigningQuickExploration && !C._dontReassign))
                {
                    C.EnableAssigningQuickExploration = true;
                    C.DontReassign = false;
                }
                ImGuiComponents.HelpMarker("Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.");
                if (ImGui.RadioButton("Collect", !C.EnableAssigningQuickExploration && C._dontReassign))
                {
                    C.EnableAssigningQuickExploration = false;
                    C.DontReassign = true;
                }
                ImGuiComponents.HelpMarker("Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.");
                if (ImGui.RadioButton("Reassign", !C.EnableAssigningQuickExploration && !C._dontReassign))
                {
                    C.EnableAssigningQuickExploration = false;
                    C.DontReassign = false;
                }
                ImGuiComponents.HelpMarker("Only reassign ventures that retainers are undertaking.");

                var d = MultiMode.GetAutoAfkOpt() != 0;
                if (d) ImGui.BeginDisabled();
                ImGui.Checkbox("RetainerSense", ref C.RetainerSense);
                ImGuiComponents.HelpMarker($"AutoRetainer will automatically enable itself when the player is within interaction range of a Summoning Bell. You must remain stationary or the activation will be cancelled.");
                if (d)
                {
                    ImGui.EndDisabled();
                    ImGuiComponents.HelpMarker("Using RetainerSense requires Auto-afk option to be turned off.");
                }
                ImGui.SetNextItemWidth(200f);
                ImGuiEx.SliderIntAsFloat("Activation Time", ref C.RetainerSenseThreshold, 1000, 100000);
                ImGuiGroup.EndGroupBox();
            };
            if (ImGuiGroup.BeginGroupBox("User Interface"))
            {
                ImGui.Checkbox("Anonymise Retainers", ref C.NoNames);
                ImGuiComponents.HelpMarker("Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).");
                ImGui.Checkbox($"Display Quick Menu in Retainer UI", ref C.UIBar);
                ImGui.Checkbox($"Opt out of custom Dalamud theme", ref C.NoTheme);
                ImGui.Checkbox($"Display Extended Retainer Info", ref C.ShowAdditionalInfo);
                ImGuiComponents.HelpMarker("Displays retainer item level/gathering/perception and the name of their current venture in the main UI.");
                if (ImGui.Checkbox("Do not close AutoRetainer windows on ESC key press", ref C.IgnoreEsc))
                {
                    Utils.ResetEscIgnoreByWindows();
                }
                ImGui.Separator();
                TabLoginOverlay();
                ImGui.Separator();
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt($"Retainer list: remaining inventory slots warning", ref C.UIWarningRetSlotNum.ValidateRange(2, 1000));
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt($"Retainer list: remaining ventures warning", ref C.UIWarningRetVentureNum.ValidateRange(2, 1000));
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt($"Deployables list: remaining inventory slots warning", ref C.UIWarningDepSlotNum.ValidateRange(2, 1000));
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt($"Deployables list: remaining fuel warning", ref C.UIWarningDepTanksNum.ValidateRange(20, 1000));
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt($"Deployables list: remaining repair kit warning", ref C.UIWarningDepRepairNum.ValidateRange(5, 1000));
                ImGuiGroup.EndGroupBox();
            }
            if (ImGuiGroup.BeginGroupBox("Keybinds"))
            {
                SettingsMain.DrawKeybind("Temporarily prevents AutoRetainer from being automatically enabled when using a Summoning Bell", ref C.Suppress);
                SettingsMain.DrawKeybind("Temporarily set the Collect Operation mode, preventing ventures from being assigned for the current cycle", ref C.TempCollectB);
                ImGuiGroup.EndGroupBox();
            };
        }

        static void TabLoginOverlay()
        {
            ImGui.Checkbox($"Display Login Overlay", ref C.LoginOverlay);
            ImGui.SetNextItemWidth(150f);
            if (ImGui.SliderFloat($"Login overlay scale multiplier", ref C.LoginOverlayScale.ValidateRange(0.1f, 5f), 0.2f, 2f)) P.LoginOverlay.bWidth = 0;
            ImGui.SetNextItemWidth(150f);
            if (ImGui.SliderFloat($"Login overlay button padding", ref C.LoginOverlayBPadding.ValidateRange(0.5f, 5f), 1f, 1.5f)) P.LoginOverlay.bWidth = 0;
        }

        
    }
}
