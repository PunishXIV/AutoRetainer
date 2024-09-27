using AutoRetainerAPI.Configuration;
using NotificationMasterAPI;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data)
    {
        SharedUI.DrawMultiModeHeader(data);
        new NuiBuilder()

        .Section("General Character Specific Settings")
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
            ImGui.Checkbox("List & Process Retainers in Display Order", ref data.ShowRetainersInDisplayOrder);

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
            ImGuiGroup.EndGroupBox();
        })
        .Section("Teleport overrides")
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
    }
}
