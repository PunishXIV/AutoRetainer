using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
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
        ImGuiEx.TextCopy($"{((nint)(&UIState.Instance()->Cabinet) + 4):X}");
        ImGuiEx.Text($"""
            
            State 1: {*(int*)((nint)(&UIState.Instance()->Cabinet))}
            State 2: {*(int*)((nint)(&UIState.Instance()->Cabinet) + 4)}
            """);
        if(ImGuiEx.Button("Execute command 423", ImGuiEx.Ctrl))
        {
            GameMain.ExecuteCommand(423);
        }
        ref var onlyIn = ref Ref<bool>.Get("OnlyIn");
        if(ImGui.CollapsingHeader("Cabinet items"))
        {
            ImGui.Checkbox("Show all##1", ref onlyIn);
            var state = UIState.Instance()->Cabinet.State;
            if(state == FFXIVClientStructs.FFXIV.Client.Game.UI.Cabinet.CabinetState.Loaded)
            {
                if(ImGuiEx.BeginDefaultTable("CabItems", ["~Item", "Cab. ID", "In cabinet"]))
                {
                    foreach(var x in Cabinet.Values)
                    {
                        var cab = UIState.Instance()->Cabinet.IsItemInCabinet(x.RowId);
                        if(!onlyIn && !cab) continue;
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGuiEx.Text($"{ExcelItemHelper.GetName(x.Item.RowId, true)}");
                        ImGui.TableNextColumn();
                        ImGuiEx.Text($"{x.RowId}");
                        ImGui.TableNextColumn();
                        ImGuiEx.Text(cab ? EColor.GreenBright : null, cab.ToString());
                    }
                    ImGui.EndTable();
                }
            }
            else
            {
                ImGuiEx.Text("Not loaded");
            }
        }
        if(ImGui.CollapsingHeader("Cabinet items (with cache)"))
        {
            ImGui.Checkbox("Only show stored##2", ref onlyIn);
            if(S.CabinetManager.TryGetStoredCabinetItems(out bool itemsCached, out var cabItems))
            {
                ImGuiEx.Text($"Cached: {itemsCached}");
                if(ImGuiEx.BeginDefaultTable("CabItems", ["~Item", "Cab. ID", "AR verdict"]))
                {
                    foreach(var x in Cabinet.Values)
                    {
                        var cab = cabItems.Contains(x.Item.RowId);
                        if(!onlyIn && !cab) continue;
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGuiEx.Text($"{ExcelItemHelper.GetName(x.Item.RowId, true)}");
                        ImGui.TableNextColumn();
                        ImGuiEx.Text($"{x.RowId}");
                        ImGui.TableNextColumn();
                        ImGuiEx.Text(cab ? EColor.GreenBright : null, cab.ToString());
                    }
                    ImGui.EndTable();
                }
            }
        }
        if(ImGui.CollapsingHeader("Cabinet items (from offline data only)"))
        {
            ImGui.Checkbox("Only show stored", ref onlyIn);
            if(ImGui.Button("Write offline data")) OfflineDataManager.WriteOfflineData(true, true);
            if(ImGuiEx.BeginDefaultTable("CabItems", ["~Item", "Cab. ID", "AR verdict"]))
            {
                foreach(var x in Cabinet.Values)
                {
                    var cab = Data.StoredCabinetItems.Contains(x.Item.RowId);
                    if(!onlyIn && !cab) continue;
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGuiEx.Text($"{ExcelItemHelper.GetName(x.Item.RowId, true)}");
                    ImGui.TableNextColumn();
                    ImGuiEx.Text($"{x.RowId}");
                    ImGui.TableNextColumn();
                    ImGuiEx.Text(cab ? EColor.GreenBright : null, cab.ToString());
                }
                ImGui.EndTable();
            }
        }
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
            if(ImGui.CollapsingHeader("Items"))
            {
                foreach(var x in items)
                {
                    ImGuiEx.Text($"{ExcelItemHelper.GetName(x)} / fmic w/o armory: {Utils.FindMinimumItemCondition(x, false)} / fmic w/ armory: {Utils.FindMinimumItemCondition(x, true)}");
                }
            }
        }
        if(ImGui.CollapsingHeader("Protected by cabinet"))
        {
            foreach(var x in Utils.PlayerEntireInventory)
            {
                var cnt = InventoryManager.Instance()->GetInventoryContainer(x);
                List<uint> prot = [];
                for(var i = 0; i < cnt->Size; i++)
                {
                    var slot = cnt->GetInventorySlot(i);
                    if(slot->ItemId != 0)
                    {
                        if(S.CabinetManager.ShouldExcludeItemFromProcessing(slot->ItemId, slot->Condition))
                        {
                            prot.Add(slot->ItemId);
                        }
                    }
                }
                if(prot.Count > 0)
                {
                    if(ImGui.CollapsingHeader($"{x}##cab"))
                    {
                        ImGuiEx.Text(prot.Select(z => ExcelItemHelper.GetName(z, true)).Print("\n"));
                    }
                }
            }
        }
        if(ImGui.CollapsingHeader("Protected by mirage"))
        {
            foreach(var x in Utils.PlayerEntireInventory)
            {
                var cnt = InventoryManager.Instance()->GetInventoryContainer(x);
                List<uint> prot = [];
                for(var i = 0; i < cnt->Size; i++)
                {
                    var slot = cnt->GetInventorySlot(i);
                    if(slot->ItemId != 0)
                    {
                        if(S.MirageManager.ShouldExcludeItemFromProcessing(slot->ItemId, slot->Condition))
                        {
                            prot.Add(slot->ItemId);
                        }
                    }
                }
                if(prot.Count > 0)
                {
                    if(ImGui.CollapsingHeader($"{x}##cab2"))
                    {
                        ImGuiEx.Text(prot.Select(z => ExcelItemHelper.GetName(z, true)).Print("\n"));
                    }
                }
            }
        }
    }
}
