using AutoRetainer.UI.Experiments.Inventory;
using AutoRetainer.UI.NeoUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Experiments
{
    internal static class ExperimentsMain
    {
        internal static void Draw()
        {
            ImGuiEx.EzTabBar("Experiments", [
                ("Night mode", Night.Draw, null, true),
                ("Inventory management", InventoryManagement.Draw, null, true),
                //("GC Auto Delivery", GCAutoDelivery.Draw, null, true),
                ]);
        }
    }
}
