using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Interface.Style;
using ECommons;
using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoRetainer.UI.Experiments.Inventory
{
    public unsafe static class InventoryManagement
    {
        public static void Draw()
        {
            ImGui.Checkbox($"Auto-open coffers", ref C.IMEnableCofferAutoOpen);
            ImGui.Checkbox($"Auto-vendor items", ref C.IMEnableAutoVendor);
            ImGui.Checkbox($"Enable context menu integration", ref C.IMEnableContextMenu);
            ImGui.InputInt($"Hard list max stack size", ref C.IMAutoVendorHardStackLimit);
            ImGui.Checkbox($"Dry mode", ref C.IMDry);
            ImGui.Checkbox($"Treat soft list as hard list", ref C.TreatSoftAsHard);
            if (ImGui.CollapsingHeader("Hard list"))
            {
                DrawListOfItems(C.IMAutoVendorHard);
            }
            if (ImGui.CollapsingHeader("Soft list"))
            {
                DrawListOfItems(C.IMAutoVendorSoft);
            }
            if (ImGui.CollapsingHeader("Protection list"))
            {
                DrawListOfItems(C.IMProtectList);
            }
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudOrange);
            if(ImGui.CollapsingHeader("Fast addition/removal"))
            {
                ImGuiEx.TextWrapped($"While this text is visible, hover over items while holding:");
                ImGuiEx.Text(!ImGui.GetIO().KeyShift?ImGuiColors.DalamudGrey:ImGuiColors.DalamudRed, $"Shift - add to soft list");
                ImGuiEx.Text($"* Items that already in hard list WILL NOT BE ADDED to soft list");
                ImGuiEx.Text(!ImGui.GetIO().KeyCtrl ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Ctrl - add to hard list");
                ImGuiEx.Text($"* Items that already in soft list WILL BE MOVED to hard list");
                ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Alt - delete from either list");
                ImGuiEx.Text("\nItems that are protected are unaffected by these actions");
                if (Svc.GameGui.HoveredItem > 0)
                {
                    var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
                    if (ImGui.GetIO().KeyShift)
                    {
                        if (!C.IMProtectList.Contains(id) && !C.IMAutoVendorSoft.Contains(id) && !C.IMAutoVendorHard.Contains(id))
                        {
                            C.IMAutoVendorSoft.Add(id);
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to soft list");
                            C.IMAutoVendorHard.Remove(id);
                        }
                    }
                    if (ImGui.GetIO().KeyCtrl)
                    {
                        if (!C.IMProtectList.Contains(id) && !C.IMAutoVendorHard.Contains(id) && !C.IMAutoVendorSoft.Contains(id))
                        {
                            C.IMAutoVendorHard.Add(id);
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to HARD list");
                            C.IMAutoVendorSoft.Remove(id);
                        }
                    }
                    if (ImGui.GetIO().KeyAlt)
                    {
                        if (C.IMAutoVendorSoft.Remove(id)) Notify.Info($"Removed {ExcelItemHelper.GetName(id)} from soft list");
                        if (C.IMAutoVendorHard.Remove(id)) Notify.Info($"Removed {ExcelItemHelper.GetName(id)} from hard list");
                    }
                }
            }
            ImGui.PopStyleColor();

            ImGui.Separator();
            if (ImGui.CollapsingHeader("Debug"))
            {
                if (ImGui.CollapsingHeader("Queue"))
                {
                    ImGuiEx.Text(InventorySpaceManager.SellSlotTasks.Print("\n"));
                }
                if(ImGui.CollapsingHeader("Sell log"))
                {
                    ImGuiEx.TextWrappedCopy(InventorySpaceManager.Log.Print("\n"));
                }
                if(ImGui.CollapsingHeader("Hotbar slot"))
                {
                    var slot = RaptureHotbarModule.Instance()->GetSlotById(0, 0);
                    ImGuiEx.Text($"CommandId {slot->CommandId}");
                    ImGuiEx.Text($"CommandType {slot->CommandType}");
                    ImGuiEx.Text($"UNK_0xC4 {slot->UNK_0xC4}");
                    ImGuiEx.Text($"UNK_0xD4 {slot->UNK_0xD4}");
                    ImGuiEx.Text($"UNK_0xD8 {slot->UNK_0xD8}");
                    ImGuiEx.Text($"UNK_0xDC {slot->UNK_0xDC}");
                    ImGuiEx.Text($"UNK_0xDD {slot->UNK_0xDD}");
                    ImGuiEx.Text($"UNK_0xDE {slot->UNK_0xDE}");
                    if (ImGui.Button("OpenCoffer")) TaskOpenAllCoffers.OpenCoffer();
                    ImGuiEx.Text($"{ActionManager.Instance()->GetActionStatus(ActionType.Item, 32161)}");
                    ImGuiEx.Text($"Animation lock: {Utils.AnimationLock}");
                    if (ImGui.Button("Open all coffers")) TaskOpenAllCoffers.Enqueue();
                }
            }
        }

        static void DrawListOfItems(List<uint> ItemList)
        {
            Dictionary<uint, List<Item>> ListByCategories = [];
            foreach(var x in ItemList)
            {
                var data = Svc.Data.GetExcelSheet<Item>().GetRow(x);
                if (data != null)
                {
                    if(!ListByCategories.TryGetValue(data.ItemUICategory.Row, out var list))
                    {
                        list = [];
                        ListByCategories[data.ItemUICategory.Row] = list;
                    }
                    list.Add(data);
                }
            }
            foreach(var x in ListByCategories)
            {
                ImGui.Selectable($"{Svc.Data.GetExcelSheet<ItemUICategory>().GetRow(x.Key).Name?.ExtractText() ?? x.Key.ToString()}", true);
                foreach(var data in x.Value)
                {
                    if (ThreadLoadImageHandler.TryGetIconTextureWrap(data.Icon, false, out var tex))
                    {
                        ImGui.Image(tex.ImGuiHandle, new(ImGuiHelpers.GetButtonSize("X").Y));
                        ImGui.SameLine();
                    }
                    ImGuiEx.Text($"{data.GetName()}");
                    if (ImGuiEx.HoveredAndClicked())
                    {
                        new TickScheduler(() => ItemList.Remove(x.Key));
                    }
                }
            }
        }
    }
}
