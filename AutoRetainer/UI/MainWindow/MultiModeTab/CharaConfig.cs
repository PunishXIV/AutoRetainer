using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data, bool isRetainer)
    {
        ImGui.PushID(data.CID.ToString());
        SharedUI.DrawMultiModeHeader(data);
        var b = new NuiBuilder()

        .Section("General Character Specific Settings")
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
        });
        if(isRetainer)
        {
            b = b.Section("Retainers").Widget(() =>
            {
                ImGuiEx.Text($"Automatic Grand Company Expert Delivery:");
                if(!AutoGCHandin.Operation)
                {
                    ImGuiEx.SetNextItemWidthScaled(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);
                }
                else
                {
                    ImGuiEx.Text($"Can't change this now");
                }
            });
        }
        else
        {
            b = b.Section("Deployables").Widget(() =>
            {
                ImGui.Checkbox($"Wait For All Pending Deployables", ref data.MultiWaitForAllDeployables);
                ImGuiComponents.HelpMarker("Prevent processing this character until all enabled deployables have returned from their voyages.");
            });
        }
        b = b.Section("Teleport overrides", data.GetAreTeleportSettingsOverriden() ? ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] with { X = 1f} :null, true)
        .Widget(() =>
        {
            ImGuiEx.Text($"You can override teleport settings per character.");
            bool? demo = null;
            ImGuiEx.Checkbox("Options marked with this marker will use values from global configuration", ref demo);
            ImGuiEx.Checkbox("Enabled", ref data.TeleportOptionsOverride.Enabled);
            ImGui.Indent();
            ImGuiEx.Checkbox("Teleport for retainers...", ref data.TeleportOptionsOverride.Retainers);
            ImGui.Indent();
            ImGuiEx.Checkbox("...to private house", ref data.TeleportOptionsOverride.RetainersPrivate);
            ImGuiEx.Checkbox("...to free company house", ref data.TeleportOptionsOverride.RetainersFC);
            ImGuiEx.Checkbox("...to apartment", ref data.TeleportOptionsOverride.RetainersApartment);
            ImGui.Text("If all above are disabled or fail, will be teleported to inn.");
            ImGui.Unindent();
            ImGuiEx.Checkbox("Teleport to free company house for deployables", ref data.TeleportOptionsOverride.Deployables);
            ImGui.Unindent();
            ImGuiGroup.EndGroupBox();
        }).Draw();
        SharedUI.DrawExcludeReset(data);
        ImGui.PopID();
    }
}
