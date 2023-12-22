using AutoRetainer.Internal.InventoryManagement;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Experiments.Inventory
{
    public static class InventoryManagement
    {
        public static void Draw()
        {
            ImGui.Checkbox($"Display tab", ref C.IMDisplayTab);
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
            if(ImGui.CollapsingHeader("Fast addition/removal"))
            {
                ImGuiEx.TextWrapped($"While this text is visible, hover over items while holding:\nShift - add to soft list;\nCtrl - add to hard list;\nAlt - delete from either list");
                if(Svc.GameGui.HoveredItem > 0)
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
            }
        }

        static void DrawListOfItems(List<uint> ItemList)
        {
            foreach(var x in ItemList)
            {
                ImGuiEx.Text($"{ExcelItemHelper.GetName(x)}");
                if (ImGuiEx.HoveredAndClicked())
                {
                    new TickScheduler(() => ItemList.Remove(x));
                }
            }
        }
    }
}
