using ECommons.Configuration;
using ECommons.Reflection;
using ECommons.Throttlers;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class EntrustManager : InventoryManagemenrBase
{
    public override string Name { get; } = "Entrust Manager";
    private Guid SelectedGuid = Guid.Empty;
    private string Filter = "";

    public override void Draw()
    {
        ImGuiEx.TextWrapped("Use advanced entrust manager to entrust specific items to specific retainers. In this window you can configure specific plans; then, you can assign entrust plans to your retainers in retainer configuration window.");
        var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == SelectedGuid);

        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo($"##select", selectedPlan?.Name ?? "Select plan...", ImGuiComboFlags.HeightLarge))
            {
                for(int i = 0; i < C.EntrustPlans.Count; i++)
                {
                    var plan = C.EntrustPlans[i];
                    ImGui.PushID(plan.Guid.ToString());
                    if(ImGui.Selectable(plan.Name, plan == selectedPlan))
                    {
                        SelectedGuid = plan.Guid;
                    }
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var plan = new EntrustPlan();
                C.EntrustPlans.Add(plan);
                SelectedGuid = plan.Guid;
                plan.Name = $"Entrust plan {C.EntrustPlans.Count}";
            }
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled:selectedPlan != null && ImGuiEx.Ctrl))
            {
                C.EntrustPlans.Remove(selectedPlan);
            }
            ImGuiEx.Tooltip("Hold CTRL and click");
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy, enabled: selectedPlan != null))
            {
                Copy(EzConfig.DefaultSerializationFactory.Serialize(selectedPlan, false));
            }
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste, enabled:EzThrottler.Check("ImportPlan")))
            {
                try
                {
                    var plan = EzConfig.DefaultSerializationFactory.Deserialize<EntrustPlan>(Paste()) ?? throw new NullReferenceException();
                    plan.Guid = Guid.NewGuid();
                    if(plan.GetType().GetFieldPropertyUnions(ReflectionHelper.AllFlags).Any(x => x.GetValue(plan) == null)) throw new NullReferenceException();
                    C.EntrustPlans.Add(plan);
                    SelectedGuid = plan.Guid;
                    Notify.Success("Imported plan from clipboard");
                    EzThrottler.Throttle("ImportPlan", 2000, true);
                }
                catch(Exception e)
                {
                    DuoLog.Error(e.Message);
                }
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint($"##name", "Plan name", ref selectedPlan.Name, 100);
            ImGui.Checkbox("Entrust Duplicates", ref selectedPlan.Duplicates);
            ImGuiEx.HelpMarker("Mimics vanilla entrust duplicates option: entrusts any items that already present in retainer's inventory up until your retainer fills up it's stack of items. Does not affects crystals. Items and categories that are explicitly added into the list below will be excluded from being processed by this option.");
            ImGui.Indent();
            ImGui.Checkbox("Allow going over stack", ref selectedPlan.DuplicatesMultiStack);
            ImGuiEx.HelpMarker("Allows entrust duplicates to create new stacks of items that already exist in the selected retainer.");
            ImGui.Unindent();
            ImGui.Checkbox("Allow entrusting from Armory Chest", ref selectedPlan.AllowEntrustFromArmory);
            ImGui.Checkbox("Manual execution only", ref selectedPlan.ManualPlan);
            ImGuiEx.HelpMarker("Mark this plan for manual execution only. This plan will only be processed upon manual \"Entrust Items\" button click and never automatically.");
            ImGui.Separator();
            ImGuiEx.TreeNodeCollapsingHeader($"Entrust categories ({selectedPlan.EntrustCategories.Count} selected)###ecats", () =>
            {
                ImGuiEx.TextWrapped($"Here you can select item categories that will be entrusted as a whole. Individual items that are selected below will be excluded from these rules.");
                if(ImGui.BeginTable("EntrustTable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.BordersInner))
                {
                    ImGui.TableSetupColumn("##1");
                    ImGui.TableSetupColumn("Item name", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Amount to keep");
                    ImGui.TableHeadersRow();
                    foreach(var x in Svc.Data.GetExcelSheet<ItemUICategory>())
                    {
                        if(x.Name == "" || x.RowId == 39) continue;
                        var contains = selectedPlan.EntrustCategories.Any(s => s.ID == x.RowId);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Icon, true, out var icon))
                        {
                            ImGui.Image(icon.ImGuiHandle, new(ImGui.GetFrameHeight()));
                        }
                        ImGui.TableNextColumn();
                        if(ImGui.Checkbox(x.Name, ref contains))
                        {
                            if(contains)
                            {
                                selectedPlan.EntrustCategories.Add(new() { ID = x.RowId });
                            }
                            else
                            {
                                selectedPlan.EntrustCategories.RemoveAll(s => s.ID == x.RowId);
                            }
                        }
                        ImGui.TableNextColumn();
                        if(selectedPlan.EntrustCategories.TryGetFirst(s => s.ID == x.RowId, out var result))
                        {
                            ImGui.SetNextItemWidth(130f);
                            ImGui.InputInt($"##amtkeep{result.ID}", ref result.AmountToKeep);
                        }
                    }
                    ImGui.EndTable();
                }
            });
            ImGuiEx.TreeNodeCollapsingHeader($"Entrust individual items ({selectedPlan.EntrustItems.Count} selected)###eitems", () =>
            {
                InventoryManagementCommon.DrawListNew(selectedPlan.EntrustItems, (x) =>
                {
                    var amount = selectedPlan.EntrustItemsAmountToKeep.SafeSelect(x);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(130f);
                    if(ImGui.InputInt($"##amtkeepitem{x}", ref amount))
                    {
                        selectedPlan.EntrustItemsAmountToKeep[x] = amount;
                    }
                    ImGuiEx.Tooltip("Amount to keep in your inventory");
                });
            });
        }
    }
}
