using AutoRetainer.Internal.InventoryManagement;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.MathHelpers;
using Lumina.Excel.GeneratedSheets;
using Action = System.Action;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public static unsafe class InventoryManagementCommon
{
    private static HashSet<uint> SelectedCategories = [];
    private static bool? Tradeable = null;
    private static HashSet<ItemRarity> Rarities = [];
    private static int ItemLevelMin = 0;
    private static int ItemLevelMax = 999;
    private static List<Item> SelectedItems = [];
    private static string ItemSearch = "";
    private static bool Modified = false;
    public static void DrawListNew(List<uint> list, Action<uint> additionalButtons = null)
    {
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "Copy to Clipboard"))
        {
            Copy(EzConfig.DefaultSerializationFactory.Serialize(list, false));
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Paste, "Merge with Clipboard", ImGuiEx.Ctrl))
        {
            try
            {
                var result = EzConfig.DefaultSerializationFactory.Deserialize<List<uint>>(Paste());
                if(result != null)
                {
                    foreach(var x in result)
                    {
                        if(ExcelItemHelper.Get(x) != null && !list.Contains(x))
                        {
                            list.Add(x);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Notify.Error(e.Message);
                e.Log();
            }
        }
        ImGuiEx.Tooltip("Hold CTRL and click");
        ImGuiEx.TreeNodeCollapsingHeader("Mass addition/removal", () =>
        {
            ImGui.SetNextItemWidth(200f);
            if(ImGui.BeginCombo("Select Categories", SelectedCategories.Count != 0 ? $"{SelectedCategories.Count} selected" : "None selected", ImGuiComboFlags.HeightLarge))
            {
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "All"))
                {
                    SelectedCategories.Clear();
                    SelectedCategories.UnionWith(Svc.Data.GetExcelSheet<ItemUICategory>().Where(x => x.Name != "").Select(x => x.RowId));
                    Modified = true;
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Minus, "None"))
                {
                    SelectedCategories.Clear();
                    Modified = true;
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Hammer, "+Main/offhand"))
                {
                    SelectedCategories.Add(Range(1u, 33));
                    SelectedCategories.Add(Range(105u, 111));
                    SelectedCategories.Add((uint[])[84, 87, 88, 89, 96, 97, 98, 99]);
                    Modified = true;
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.User, "+Armor"))
                {
                    SelectedCategories.Add(Range(34u, 38));
                    SelectedCategories.Add(Range(40u, 43));
                    Modified = true;
                }
                ImGui.Separator();
                foreach(var x in Svc.Data.GetExcelSheet<ItemUICategory>())
                {
                    if(x.Name == "") continue;
                    Modified |= ImGuiEx.CollectionCheckbox($"{x.Name}##{x.RowId}", x.RowId, SelectedCategories);
                }
                ImGui.EndCombo();
            }

            if(SelectedCategories.Count > 0)
            {
                ImGui.SetNextItemWidth(200f);
                Modified |= ImGui.InputText($"Filter by name", ref ItemSearch, 100);
                ImGui.SetNextItemWidth(200f);
                if(ImGui.BeginCombo("Select rarity", Rarities.Any() ? $"{Rarities.Print()}" : "Any rarity", ImGuiComboFlags.HeightLarge))
                {
                    foreach(var r in Enum.GetValues<ItemRarity>())
                    {
                        Modified |= ImGuiEx.CollectionCheckbox(r.ToString(), r, Rarities);
                    }
                    ImGui.EndCombo();
                }
                ImGui.SetNextItemWidth(200f);
                Modified |= ImGui.InputInt("Minimum item level", ref ItemLevelMin);
                ImGui.SetNextItemWidth(200f);
                Modified |= ImGui.InputInt("Maximum item level", ref ItemLevelMax);
                Modified |= ImGuiEx.Checkbox("Tradeable", ref Tradeable);

                if(Modified)
                {
                    Modified = false;
                    SelectedItems = Svc.Data.GetExcelSheet<Item>().Where(x =>
                    x.LevelItem.Row.InRange((uint)ItemLevelMin, (uint)ItemLevelMax, true)
                    && (Tradeable == null || x.IsUntradable == !Tradeable)
                    && x.ItemUICategory.Row.EqualsAny(SelectedCategories)
                    && (Rarities.Count == 0 || ((ItemRarity)x.Rarity).EqualsAny(Rarities))
                    && (ItemSearch == "" || x.Name.ToString().Contains(ItemSearch, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                if(ImGuiEx.CollapsingHeader($"Selected {SelectedItems.Count} items, among which {SelectedItems.Count(x => list.Contains(x.RowId))} already present in list###counter"))
                {
                    var actions = new List<Action>();
                    foreach(var x in SelectedItems)
                    {
                        actions.Add(() =>
                        {
                            if(ThreadLoadImageHandler.TryGetIconTextureWrap(x?.Icon ?? 0, false, out var tex))
                            {
                                ImGui.Image(tex.ImGuiHandle, new(ImGuiHelpers.GetButtonSize("X").Y));
                                Tooltip();
                                ImGui.SameLine();
                            }
                            ImGuiEx.Text(list.Contains(x.RowId) ? ImGuiColors.DalamudGrey3 : null, x.Name);
                            Tooltip();

                            void Tooltip()
                            {
                                if(!list.Contains(x.RowId))
                                {
                                    if(ImGuiEx.HoveredAndClicked("Click to add this single item to list immediately"))
                                    {
                                        list.Add(x.RowId);
                                    }
                                }
                                else
                                {
                                    if(ImGuiEx.HoveredAndClicked("Right click to add this single item to list immediately", ImGuiMouseButton.Right))
                                    {
                                        list.Remove(x.RowId);
                                    }
                                }
                            }
                        });
                    }
                    var draw = ImGuiEx.Pagination([.. actions], 100, 10);
                    ImGuiEx.EzTableColumns("cols", draw, Math.Max(1, (int)ImGui.GetContentRegionAvail().X / 150));
                }
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.PlusSquare, "Add these items to list", ImGuiEx.Ctrl))
                {
                    foreach(var x in SelectedItems)
                    {
                        if(!list.Contains(x.RowId)) list.Add(x.RowId);
                    }
                }
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.MinusSquare, "Remove these items to list", ImGuiEx.Ctrl))
                {
                    foreach(var x in SelectedItems)
                    {
                        list.Remove(x.RowId);
                    }
                }
            }
        });

        Dictionary<uint, List<uint>> Categories = [];
        Dictionary<uint, List<Item>> ItemsByCategories = [];
        foreach(var x in list)
        {
            var data = ExcelItemHelper.Get(x);
            if(data != null)
            {
                if(!ItemsByCategories.TryGetValue(data.ItemUICategory.Row, out var lst))
                {
                    ItemsByCategories[data.ItemUICategory.Row] = [];
                    lst = ItemsByCategories[data.ItemUICategory.Row];
                }
                lst.Add(data);
            }
        }
        foreach(var cat in ItemsByCategories)
        {
            var dataList = list.Select(ExcelItemHelper.Get);
            var items = cat.Value;
            if(ImGui.BeginTable("IMList", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                var postTableAction = new List<Action>();
                ImGui.TableSetupColumn($"###1");
                ImGui.TableSetupColumn($"{Svc.Data.GetExcelSheet<ItemUICategory>().GetRow(cat.Key).Name}###name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"###2");
                ImGui.TableHeadersRow();

                var actions = new List<Action>();
                foreach(var item in items)
                {
                    actions.Add(() =>
                    {
                        ImGui.PushID(item.RowId.ToString());
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if(ThreadLoadImageHandler.TryGetIconTextureWrap(item?.Icon ?? 0, false, out var tex))
                        {
                            ImGui.Image(tex.ImGuiHandle, new(ImGuiHelpers.GetButtonSize("X").Y));
                        }
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV($"{item?.Name}");
                        ImGui.TableNextColumn();
                        if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                        {
                            new TickScheduler(() => list.Remove(item.RowId));
                            C.IMAutoVendorHardIgnoreStack.Remove(item.RowId);
                        }
                        additionalButtons?.Invoke(item.RowId);
                        ImGui.PopID();
                    });
                }
                var pages = ImGuiEx.Pagination($"IMList.{cat.Key}", [.. actions], out var paginator, 300, 10);
                if(paginator != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var curpos = ImGui.GetCursorPos();
                    ImGuiEx.TextV("");
                    postTableAction.Add(() =>
                    {
                        var cur = ImGui.GetCursorPos();
                        ImGui.SetCursorPos(curpos);
                        paginator();
                        ImGui.SetCursorPos(cur);
                    });
                }
                foreach(var x in pages) x();
                ImGui.EndTable();
                foreach(var x in postTableAction) x();
            }
        }
    }

    public static void ImportFromArDiscard(List<uint> target)
    {
        if(ImGuiEx.Button("Import discard entries from Discard Helper", ImGuiEx.Ctrl))
        {
            try
            {
                foreach(var x in EzConfig.LoadConfiguration<ARDiscardMiniConfig>(System.IO.Path.Combine(Svc.PluginInterface.ConfigDirectory.Parent.FullName, "ARDiscard.json")).DiscardingItems)
                {
                    if(!target.Contains(x) && !C.IMProtectList.Contains(x))
                    {
                        target.Add(x);
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        ImGuiEx.HelpMarker("If you're using Discard Helper plugin, you may import entries from it using this button. They will be merged with your existing entries. Hold CTRL and click.");
    }

    public static void ImportBlacklistFromArDiscard()
    {
        if(ImGuiEx.Button("Import blacklisted entries from Discard Helper", ImGuiEx.Ctrl))
        {
            try
            {
                foreach(var x in EzConfig.LoadConfiguration<ARDiscardMiniConfig>(System.IO.Path.Combine(Svc.PluginInterface.ConfigDirectory.Parent.FullName, "ARDiscard.json")).DiscardingItems)
                {
                    if(!C.IMProtectList.Contains(x))
                    {
                        C.IMProtectList.Add(x);
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        ImGuiEx.HelpMarker("If you're using Discard Helper plugin, you may import entries from it using this button. They will be merged with your existing entries. Hold CTRL and click.");
    }

    private static void DrawListOfItems(List<uint> ItemList)
    {
        Dictionary<uint, List<Item>> ListByCategories = [];
        foreach(var x in ItemList)
        {
            var data = Svc.Data.GetExcelSheet<Item>().GetRow(x);
            if(data != null)
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
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(data.Icon, false, out var tex))
                {
                    ImGui.Image(tex.ImGuiHandle, new(ImGuiHelpers.GetButtonSize("X").Y));
                    ImGui.SameLine();
                }
                ImGuiEx.Text($"{data.GetName()}");
                if(ImGuiEx.HoveredAndClicked())
                {
                    new TickScheduler(() => ItemList.Remove(x.Key));
                }
            }
        }
    }
}
