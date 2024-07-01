
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugIPC : DebugUIEntry
{
    public override void Draw()
    {
        ImGui.Checkbox($"API Test", ref ApiTest.Enabled);
        ImGuiEx.Text($"IPC suppressed: {Svc.PluginInterface.GetIpcSubscriber<bool>("AutoRetainer.GetSuppressed").InvokeFunc()}");
        if (ImGui.Button($"Suppress = true"))
        {
            Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoRetainer.SetSuppressed").InvokeAction(true);
        }
        if (ImGui.Button($"Suppress = false"))
        {
            Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoRetainer.SetSuppressed").InvokeAction(false);
        }
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var sel))
        {
            var entries = Utils.GetEntries(sel);
            foreach (var x in entries)
            {
                var index = entries.IndexOf(x);
                if (ImGui.SmallButton($"{x} / {index}") && index >= 0)
                {
                    new AddonMaster.SelectString(sel).Entries[index].Select();
                }
            }
        }
    }
}
