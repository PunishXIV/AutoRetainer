using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Grand Company Delivery/General Settings";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("General Settings")
        .Checkbox("Enable Expert Delivery continuation", () => ref C.AutoGCContinuation)
        .TextWrapped($"""
            When Expert Delivery Continuation is enabled:
            - The plugin will automatically spend available Grand Company Seals to purchase items from the configured Exchange List.
            - If the Exchange List is empty, only Ventures will be purchased.
            - Make sure that "Delivery Mode" is not set to "Disabled" in "Character Configuration" section

            After seals have been spent:
            - Expert Delivery will resume automatically.
            - The process will repeat until there are no eligible items left to deliver or no seals remaining.
            """)

        .Section("Multi Mode Expert Delivery")
        .TextWrapped($"""
        When enabled:
        - Characters with teleportation enabled will automatically deliver items for expert delivery and buy items according to exchange plan, if their rank is sufficient, during multi mode.
        """)
        .Checkbox("Enable Multi Mode Expert Delivery", () => ref C.FullAutoGCDelivery)
        .Checkbox("Only when workstation is not locked", () => ref C.FullAutoGCDeliveryOnlyWsUnlocked)
        .InputInt(150f, "Inventory slots remaining to trigger delivery, less or equal", () => ref C.FullAutoGCDeliveryInventory, "Only primary inventory is accounted for, not armory")
        .Checkbox("Trigger on venture exhaustion", () => ref C.FullAutoGCDeliveryDeliverOnVentureExhaust, "This may cause situation where you will just go to GC exchange every login. Make sure you have a purchase plan to buy enough ventures set. ")
        .Indent()
        .InputInt(150f, "Ventures remaining to trigger delivery, less or equal", () => ref C.FullAutoGCDeliveryDeliverOnVentureLessThan)
        .Unindent()
        .Checkbox("Use Priority seal allowance, if possible", () => ref C.FullAutoGCDeliveryUseBuffItem)
        .Checkbox("Use Free Company seal buff, if possible", () => ref C.FullAutoGCDeliveryUseBuffFCAction)
        .Checkbox("Teleport back to house/inn after delivery", () => ref C.TeleportAfterGCExchange)
        .Indent()
        .Checkbox("Only when Multi Mode is active", () => ref C.TeleportAfterGCExchangeMulti)
        .Unindent()
        ;
}