using AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.MathHelpers;
using Lumina.Excel.Sheets;
using Action = System.Action;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public unsafe class InventoryManagementCommon
{
    private HashSet<uint> SelectedCategories = [];
    private bool? Tradeable = null;
    private HashSet<ItemRarity> Rarities = [];
    private int ItemLevelMin = 0;
    private int ItemLevelMax = 999;
    private List<Item> SelectedItems = [];
    private string ItemSearch = "";
    private bool Modified = false;
    public void DrawListNew(Action<uint> addAction, Action<uint> removeAction, IReadOnlyList<uint> itemList, Action<uint> additionalButtons = null, Predicate<Item> filter = null)
    {
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "Copy to Clipboard"))
        {
            Copy(EzConfig.DefaultSerializationFactory.Serialize(itemList, false));
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
                        if(ExcelItemHelper.Get(x) != null && !itemList.Contains(x))
                        {
                            addAction(x);
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
                    SelectedCategories.Add(Utils.WeaponsUICategories);
                    Modified = true;
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.User, "+Armor"))
                {
                    SelectedCategories.Add(Utils.ArmorsUICategories);
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
                    x.LevelItem.RowId.InRange((uint)ItemLevelMin, (uint)ItemLevelMax, true)
                    && (Tradeable == null || x.IsUntradable == !Tradeable)
                    && x.ItemUICategory.RowId.EqualsAny(SelectedCategories)
                    && (Rarities.Count == 0 || ((ItemRarity)x.Rarity).EqualsAny(Rarities))
                    && (ItemSearch == "" || x.Name.ToString().Contains(ItemSearch, StringComparison.OrdinalIgnoreCase))
                    && (filter == null || filter(x))
                    ).ToList();
                }
                if(ImGuiEx.CollapsingHeader($"Selected {SelectedItems.Count} items, among which {SelectedItems.Count(x => itemList.Contains(x.RowId))} already present in list###counter"))
                {
                    var actions = new List<Action>();
                    foreach(var x in SelectedItems)
                    {
                        actions.Add(() =>
                        {
                            if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Icon, false, out var tex))
                            {
                                ImGui.Image(tex.Handle, new(ImGuiHelpers.GetButtonSize("X").Y));
                                Tooltip();
                                ImGui.SameLine();
                            }
                            ImGuiEx.Text(itemList.Contains(x.RowId) ? ImGuiColors.DalamudGrey3 : null, x.Name.ToString());
                            Tooltip();

                            void Tooltip()
                            {
                                if(!itemList.Contains(x.RowId))
                                {
                                    if(ImGuiEx.HoveredAndClicked("Click to add this single item to list immediately"))
                                    {
                                        addAction(x.RowId);
                                    }
                                }
                                else
                                {
                                    if(ImGuiEx.HoveredAndClicked("Right click to remove this single item from list immediately", ImGuiMouseButton.Right))
                                    {
                                        removeAction(x.RowId);
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
                        if(!itemList.Contains(x.RowId)) addAction(x.RowId);
                    }
                }
                ImGuiEx.Tooltip("Hold CTRL and click");
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.MinusSquare, "Remove these items to list", ImGuiEx.Ctrl))
                {
                    foreach(var x in SelectedItems)
                    {
                        removeAction(x.RowId);
                    }
                }
                ImGuiEx.Tooltip("Hold CTRL and click");
            }
        });

        Dictionary<uint, List<uint>> Categories = [];
        Dictionary<uint, List<Item>> ItemsByCategories = [];
        foreach(var x in itemList)
        {
            var data = ExcelItemHelper.Get(x);
            if(data != null)
            {
                if(!ItemsByCategories.TryGetValue(data.Value.ItemUICategory.RowId, out var lst))
                {
                    ItemsByCategories[data.Value.ItemUICategory.RowId] = [];
                    lst = ItemsByCategories[data.Value.ItemUICategory.RowId];
                }
                lst.Add(data.Value);
            }
        }
        foreach(var cat in ItemsByCategories)
        {
            var dataList = itemList.Select(ExcelItemHelper.Get);
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
                        ImGuiEx.PushID(item.RowId.ToString());
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if(ThreadLoadImageHandler.TryGetIconTextureWrap(item.Icon, false, out var tex))
                        {
                            ImGui.Image(tex.Handle, new(ImGuiHelpers.GetButtonSize("X").Y));
                        }
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV($"{item.Name}");
                        ImGui.TableNextColumn();
                        if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                        {
                            new TickScheduler(() => removeAction(item.RowId));
                            InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardIgnoreStack.Remove(item.RowId);
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

    public void ImportFromArDiscard(List<uint> target)
    {
        if(ImGuiEx.Button("Import discard entries from Discard Helper", ImGuiEx.Ctrl))
        {
            try
            {
                foreach(var x in EzConfig.LoadConfiguration<ARDiscardMiniConfig>(System.IO.Path.Combine(Svc.PluginInterface.ConfigDirectory.Parent.FullName, "ARDiscard.json")).DiscardingItems)
                {
                    if(!target.Contains(x) && !InventoryCleanupCommon.SelectedPlan.IMProtectList.Contains(x))
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

    public void ImportBlacklistFromArDiscard()
    {
        var s = InventoryCleanupCommon.SelectedPlan;
        if(ImGuiEx.Button("Import blacklisted entries from Discard Helper", ImGuiEx.Ctrl))
        {
            try
            {
                foreach(var x in EzConfig.LoadConfiguration<ARDiscardMiniConfig>(System.IO.Path.Combine(Svc.PluginInterface.ConfigDirectory.Parent.FullName, "ARDiscard.json")).DiscardingItems)
                {
                    if(!s.IMProtectList.Contains(x))
                    {
                        s.IMProtectList.Add(x);
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
            var data = Svc.Data.GetExcelSheet<Item>().GetRowOrDefault(x);
            if(data != null)
            {
                if(!ListByCategories.TryGetValue(data.Value.ItemUICategory.RowId, out var list))
                {
                    list = [];
                    ListByCategories[data.Value.ItemUICategory.RowId] = list;
                }
                list.Add(data.Value);
            }
        }
        foreach(var x in ListByCategories)
        {
            ImGui.Selectable($"{Svc.Data.GetExcelSheet<ItemUICategory>().GetRowOrDefault(x.Key)?.Name.GetText() ?? x.Key.ToString()}", true);
            foreach(var data in x.Value)
            {
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(data.Icon, false, out var tex))
                {
                    ImGui.Image(tex.Handle, new(ImGuiHelpers.GetButtonSize("X").Y));
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
