using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;
using TerraFX.Interop.Windows;
using Cabinet = Lumina.Excel.Sheets.Cabinet;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

public unsafe class DebugCabinet : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("AutoMirage"))
        {
            if(ImGui.Button("MEnqueueGoToInnAndDeliverEverything"))
            {
                S.MirageManager.EnqueueGoToInnAndDeliverEverything();
            }
            foreach(var inv in Utils.PlayerEntireInventory)
            {
                if(ImGui.CollapsingHeader($"{inv}"))
                {
                    ImGui.Indent();
                    var invCont = InventoryManager.Instance()->GetInventoryContainer(inv);
                    for(int i = 0; i < invCont->Size; i++)
                    {
                        var item = invCont->Items[i];
                        if(item.ItemId != 0 && Svc.Data.GetExcelSheet<Item>().TryGetRow(item.ItemId, out var data) && Utils.CanItemBePartOfMirageSet(item.ItemId))
                        {
                            ImGuiEx.Text(Utils.IsItemAlreadyMiraged(item.ItemId) ? EColor.GreenBright : null, $"{data.RowId} {data.Name}");
                        }
                    }
                    ImGui.Unindent();
                }
            }
        }
        ImGui.Separator();
        ImGuiEx.Text($"CanDeliverCabinet: {S.CabinetManager.CanDeliverCabinet()}");
        if(ImGui.Button("Deliver items")) S.CabinetManager.EnqueueAllDeliverableItems();
        if(ImGui.Button("EnqueueGoToInnAndDeliverEverything")) S.CabinetManager.EnqueueGoToInnAndDeliverEverything();
        if(S.CabinetManager.TryGetStoredCabinetItems(out var cached, out var items))
        {
            ImGuiEx.Text($"Cached: {cached}");
        }
    }
}
