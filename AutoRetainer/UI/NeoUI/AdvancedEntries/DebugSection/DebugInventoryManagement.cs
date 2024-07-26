using AutoRetainer.Internal.InventoryManagement;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public unsafe class DebugInventoryManagement : DebugSectionBase
{
    private int slot;
    private InventoryType Type;
    private HashSet<uint> Whitelist = [];

    public override void Draw()
    {
        ImGuiEx.EnumCombo($"type", ref Type);
        ImGui.InputInt("Slot", ref slot);
        ImGuiEx.Text(ExcelItemHelper.GetName(InventoryManager.Instance()->GetInventoryContainer(Type)->GetInventorySlot(slot)->ItemId));
        if(ImGui.Button("Sell"))
        {
            P.Memory.SellItemToShop(Type, slot);
        }
        if(ImGui.Button("Enqueue if present"))
        {
            NpcSaleManager.EnqueueIfItemsPresent();
        }
        ImGuiEx.Text($"Valid npc: {NpcSaleManager.GetValidNPC()}");
        if(ImGui.Button("Interact with target")) TargetSystem.Instance()->InteractWithObject(Svc.Targets.Target.Struct(), false);
        if(TryGetAddonMaster<AddonMaster.SelectIconString>(out var m))
        {
            foreach(var x in m.Entries)
            {
                if(ImGui.Selectable(x.Text))
                {
                    x.Select();
                }
            }
        }

        foreach(var x in Vendors)
        {
            ImGuiEx.Text(Whitelist.Contains(x) ? EColor.GreenBright : null, $"{x}: {Svc.Data.GetExcelSheet<ENpcResident>().GetRow(x)?.Plural}");
            if(ImGui.IsItemHovered())
            {
                if(ImGuiEx.Ctrl)
                {
                    Whitelist.Add(x);
                }
                if(ImGuiEx.Shift) Whitelist.Remove(x);
            }
        }
        if(ImGui.Button("Copy")) Copy(Whitelist.Print());
    }

    public IEnumerable<uint> Vendors
    {
        get
        {
            foreach(var x in Svc.Data.GetExcelSheet<HousingEmploymentNpcList>())
            {
                for(var i = 0; i < x.ENpcBase.Length; i++)
                {
                    var ret = x.ENpcBase[i];
                    if(ret.Row != 0) yield return ret.Row;
                }
            }
        }
    }
}
