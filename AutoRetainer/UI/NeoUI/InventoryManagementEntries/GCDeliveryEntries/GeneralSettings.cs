using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Grand Company Delivery/General Settings";

    public override void Draw()
    {
        ImGui.Checkbox("Enable Expert Delivery continuation", ref C.AutoGCContinuation);
        ImGui.Indent();
        ImGuiEx.TextWrapped($"""
            When Expert Delivery Continuation is enabled:
            - The plugin will automatically spend available Grand Company Seals to purchase items from the configured Exchange List.
            - If the Exchange List is empty, only Ventures will be purchased.

            After seals have been spent:
            - Expert Delivery will resume automatically.
            - The process will repeat until there are no eligible items left to deliver or no seals remaining.
            """);
        ImGui.Unindent();
    }
}