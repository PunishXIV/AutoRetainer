using AutoRetainer.Modules.Voyage.Readers;
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
                if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var a) && IsAddonReady(a))
                {
                    var reader = new ReaderAirShipExploration(a);
                    ImGuiEx.Text($"Distance: {reader.Distance}");
                    ImGuiEx.Text($"Fuel: {reader.Fuel}");
                    foreach (var r in reader.Destinations)
                    {
                        ImGuiEx.Text($"Destination {r.NameFull}, rank={r.RequiredRank}, status={r.StatusFlag}, canBeSelected={r.CanBeSelected}");
                    }
                }
            }
            {
                if (TryGetAddonByName<AtkUnitBase>("SubmarineExplorationMapSelect", out var a) && IsAddonReady(a))
                {
                    var reader = new ReaderSubmarineExplorationMapSelect(a);
                    ImGuiEx.Text($"Current rank: {reader.SubmarineRank}");
                    foreach (var r in reader.Maps)
                    {
                        ImGuiEx.Text($"Map {r.Name}, rank={r.RequiredRank}");
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
