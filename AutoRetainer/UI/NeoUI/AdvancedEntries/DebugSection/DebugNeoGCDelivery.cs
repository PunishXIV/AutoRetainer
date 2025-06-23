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
        foreach(var x in Utils.SharedGCExchangeListings)
        {
            ImGuiEx.Text($"{x.Data.Name} / {x.ItemID} / {x.Category} / Min rank {x.MinPurchaseRank} {x.Rank} / {x.Seals} seals");
        }
    }
}