using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Readers;
using ECommons.UIHelpers.AtkReaderImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugReader : DebugUIEntry
{
		public override void Draw()
		{
				{
						if (TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var a) && IsAddonReady(a))
						{
								var reader = new ReaderRetainerItemTransferList(a);
								foreach (var r in reader.Items)
								{
										ImGuiEx.Text($"Item {r.ItemID}, isHQ = {r.IsHQ}");
								}
						}
				}
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
		}
}
