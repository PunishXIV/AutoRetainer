using ECommons.UIHelpers;
using ECommons.UIHelpers.Implementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal unsafe static class DebugReader
    {
        internal static void Draw()
        {
            {
                if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var a) && IsAddonReady(a))
                {
                    var reader = new ReaderRetainerList(a);
                    foreach (var r in reader.Retainers)
                    {
                        ImGuiEx.Text($"Retainer {r.Name}, gil={r.Gil}, lvl={r.Level}, active={r.IsActive}");
                    }
                }
            }
            {
                if (TryGetAddonByName<AtkUnitBase>("SelectString", out var a) && IsAddonReady(a))
                {
                    var reader = new ReaderSelectString(a);
                    foreach (var r in reader.Entries)
                    {
                        ImGuiEx.Text($"{r.Text}");
                    }
                }
            }
            {
                if (TryGetAddonByName<AtkUnitBase>("RetainerSellList", out var a) && IsAddonReady(a))
                {
                    var reader = new ReaderRetainerSellList(a);
                    foreach (var r in reader.Entries)
                    {
                        ImGuiEx.Text($"{r.Name} / {r.Id} / {r.Amount} / {r.PricePerUnit}");
                    }
                }
            }
        }
    }
}
