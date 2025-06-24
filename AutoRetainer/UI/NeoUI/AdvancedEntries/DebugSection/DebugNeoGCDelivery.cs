using AutoRetainerAPI.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public unsafe sealed class DebugNeoGCDelivery : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.Button("BeginNewPurchase")) GCContinuation.BeginNewPurchase();
        foreach(var x in Utils.SharedGCExchangeListings.Values)
        {
            ImGuiEx.Text($"{x.Data.Name} / {x.ItemID} / {x.Category} / Min rank {x.MinPurchaseRank} {x.Rank} / {x.Seals} seals | can purchase: x{new ItemWithQuantity(x.ItemID, int.MaxValue).GetAmountThatCanBePurchased()}");
        }
    }
}