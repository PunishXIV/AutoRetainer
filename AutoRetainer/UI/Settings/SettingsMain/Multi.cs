using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings.SettingsMain
{
    internal unsafe class Multi
    {
        internal static void Draw()
        {
            if (ImGuiGroup.BeginGroupBox("Common settings"))
            {
                ImGui.Checkbox($"Enforce Full Character Rotation", ref C.CharEqualize);
                ImGuiComponents.HelpMarker("Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.");
                ImGui.Checkbox($"Wait on login screen", ref C.MultiWaitOnLoginScreen);
                ImGuiComponents.HelpMarker($"If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.");
                ImGui.Checkbox("Synchronise Retainers (one time)", ref MultiMode.Synchronize);
                ImGuiComponents.HelpMarker("AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.");
                ImGuiGroup.EndGroupBox();
            }

            if (ImGuiGroup.BeginGroupBox("Retainers"))
            {
                ImGui.Checkbox("Wait For Venture Completion", ref C.MultiModeRetainerConfiguration.MultiWaitForAll);
                ImGuiComponents.HelpMarker("AutoRetainer will wait for all retainers to return before cycling to the next character in multi mode operation.");
                ImGui.SetNextItemWidth(60);
                ImGui.DragInt("Advance Relog Threshold", ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
                ImGui.SetNextItemWidth(100);
                ImGui.SliderInt("Minimum inventory slots to continue operation", ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30);
                ImGuiGroup.EndGroupBox();
            }

            if (ImGuiGroup.BeginGroupBox("Deployables"))
            {
                ImGui.PushID("workshop");
                ImGui.Checkbox("Wait For Voyage Completion", ref C.MultiModeWorkshopConfiguration.MultiWaitForAll);
                ImGuiComponents.HelpMarker("AutoRetainer will wait for all deployables to return before cycling to the next character in multi mode operation.");
                ImGui.SetNextItemWidth(60);
                ImGui.DragInt("Advance Relog Threshold", ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
                ImGui.Checkbox("Wait even when already logged in", ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn);
                ImGui.SetNextItemWidth(60);
                ImGui.DragInt("Retainer venture processing cutoff", ref C.DisableRetainerVesselReturn.ValidateRange(0, 60));
                ImGuiComponents.HelpMarker("The number of minutes remaining on deployable voyages to prevent processing of retainer tasks.");
                ImGui.PopID();
                ImGuiGroup.EndGroupBox();
            }

            if (ImGuiGroup.BeginGroupBox("Contingency"))
            {
                TabCont();
                ImGuiGroup.EndGroupBox();
            }

            if (ImGuiGroup.BeginGroupBox("FPS Limiter"))
            {
                ImGuiEx.Text($"FPS Limiter is only active when Multi Mode is enabled");
                ImGui.SetNextItemWidth(100f);
                SettingsMain.SliderIntFrameTimeAsFPS("Target frame rate when idling", ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
                ImGui.SetNextItemWidth(100f);
                SettingsMain.SliderIntFrameTimeAsFPS("Target frame rate when operating", ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
                ImGui.Checkbox("Release FPS lock when game is active", ref C.NoFPSLockWhenActive);
                ImGui.Checkbox($"Allow extra low FPS limiter values", ref C.ExtraFPSLockRange);
                ImGui.Checkbox($"Limiter active only when shutdown timer is set", ref C.FpsLockOnlyShutdownTimer);
                ImGuiComponents.HelpMarker("No support is provided if you enable this and run into ANY errors in Multi Mode");
                ImGuiGroup.EndGroupBox();
            }

            if(ImGuiGroup.BeginGroupBox("Bailout Module"))
            {
                ImGui.SetNextItemWidth(150f);
                ImGui.SliderInt("Timeout before AutoRetainer will attempt to unstuck, seconds", ref C.BailoutTimeout.ValidateRange(5, 60), 5, 30);
                ImGuiGroup.EndGroupBox();
            }
        }


        static readonly Dictionary<WorkshopFailAction, string> WorkshopFailActionNames = new() 
        {
            [WorkshopFailAction.StopPlugin] = "Halt all plugin operation",
            [WorkshopFailAction.ExcludeVessel] = "Exclude deployable from operation",
            [WorkshopFailAction.ExcludeChar] = "Exclude captain from multi mode rotation",
        };
        static void TabCont()
        {
            ImGuiEx.TextWrapped("Here you can apply various fallback actions to perform in the case of some common failure states or potential operation errors.");
            ImGuiEx.Text($"Ceruleum Tanks Expended");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of insufficient Ceruleum Tanks to deploy vessel on a new voyage.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Ceruleum Tanks Expended", ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames);

            ImGuiEx.Text($"Unable to Repair Deployable");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of insufficient Magitek Repair Materials to repair a vessel.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Unable to Repair Deployable", ref C.FailureNoRepair, WorkshopFailActionNames);

            ImGuiEx.Text($"Inventory at Capacity");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of the captain's inventory having insufficient space to receive voyage rewards.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Inventory at Capacity", ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames);

            ImGuiEx.Text($"Critical Operation Failure");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of any unknown or miscellaneous error.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Critical Operation Failure", ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames);

            ImGuiEx.Text($"Jailed by the GM");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case if you got jailed by the GM while plugin is running.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGui.BeginDisabled();
            if (ImGui.BeginCombo("##jailsel", "Terminate the game")) { ImGui.EndCombo(); }
            ImGui.EndDisabled();
        }
    }
}
