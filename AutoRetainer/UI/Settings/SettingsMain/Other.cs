using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings.SettingsMain
{
    internal class Other
    {
        internal static void Draw()
        {

            if (ImGuiGroup.BeginGroupBox("Quick Retainer Action"))
            {
                SettingsMain.QRA("Sell Item", ref C.SellKey);
                SettingsMain.QRA("Entrust Item", ref C.EntrustKey);
                SettingsMain.QRA("Retrieve Item", ref C.RetrieveKey);
                SettingsMain.QRA("Put up For Sale", ref C.SellMarketKey);
                ImGuiGroup.EndGroupBox();
            };
            if (ImGuiGroup.BeginGroupBox("Statistics"))
            {
                ImGui.Checkbox($"Record Venture Statistics", ref C.RecordStats);
                ImGuiGroup.EndGroupBox();
            };
            if (ImGuiGroup.BeginGroupBox("Automatic Grand Company Expert Delivery"))
            {
                AutoGCHandinUI.Draw();
                ImGuiGroup.EndGroupBox();
            }

            if (ImGuiGroup.BeginGroupBox("Performance"))
            {
                if (Utils.IsBusy) ImGui.BeginDisabled();
                ImGui.Checkbox($"Remove minimized FPS restrictions while plugin is operating", ref C.UnlockFPS);
                ImGui.Checkbox($"- Also remove general FPS restriction", ref C.UnlockFPSUnlimited);
                ImGui.Checkbox($"- Also pause ChillFrames plugin", ref C.UnlockFPSChillFrames);
                ImGui.Checkbox($"Raise FFXIV process priority while plugin is operating", ref C.ManipulatePriority);
                ImGuiComponents.HelpMarker("May result other programs slowdown");
                if (Utils.IsBusy) ImGui.EndDisabled();
                ImGuiGroup.EndGroupBox();
            }
        }
    }
}
