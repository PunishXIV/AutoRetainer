using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System.Numerics;
using GrandCompany = ECommons.ExcelServices.GrandCompany;
using GrandCompanyRank = Lumina.Excel.Sheets.GrandCompanyRank;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public unsafe sealed class ExchangeLists : InventoryManagemenrBase
{
    ImGuiEx.RealtimeDragDrop<ItemWithQuantity> DragDrop = new("GCELDD", x => x.ID);
    public override string Name { get; } = "Grand Company Delivery/Exchange Lists";
    GCExchangeCategoryTab? SelectedCategory = null;
    GCExchangeRankTab? SelectedRank = null;
    Guid SelectedPlanGuid = Guid.Empty;

    public override void Draw()
    {
        C.AdditionalGCExchangePlans.Where(x => x.GUID == Guid.Empty).Each(x => x.GUID = Guid.NewGuid());
        ImGuiEx.TextWrapped($"""
            Select which items you'd like to buy during automatic Grand Company expert delivery missions.
            First available item for purchase will be bought until it's amount in your inventory reaches the target. If there is nothing else to be purchased or items don't fit into your inventory, ventures will be purchased until you reach 65000 of them, at which point expert delivery will discard excess seals.
            """);

        var selectedPlan = C.AdditionalGCExchangePlans.FirstOrDefault(x => x.GUID ==  SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selplan", selectedPlan?.DisplayName ?? "Default Plan"))
            {
                if(ImGui.Selectable("Default Plan", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalGCExchangePlans)
                {
                    ImGui.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new GCExchangePlan();
                C.AdditionalGCExchangePlans.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("Add new plan");
            ImGui.SameLine(0,1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultGCExchangePlan).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("Copy");
            ImGui.SameLine(0,1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<GCExchangePlan>(Paste()) ?? throw new NullReferenceException();
                    C.AdditionalGCExchangePlans.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("Paste");
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultGCExchangePlan = selectedPlan;
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("Make this plan default. Current default plan will be overwritten. Hold CTRL and click.");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("Delete this plan. Hold CTRL and click.");
            }
        });

        if(SelectedPlanGuid == Guid.Empty)
        {
            DrawGCEchangeList(C.DefaultGCExchangePlan);
        }
        else
        {
            var planIndex = C.AdditionalGCExchangePlans.IndexOf(x => x.GUID == SelectedPlanGuid);
            if(planIndex == -1)
            {
                SelectedPlanGuid = Guid.Empty;
            }
            else
            {
                DrawGCEchangeList(C.AdditionalGCExchangePlans[planIndex]);
            }
        }
    }

    public void DrawGCEchangeList(GCExchangePlan plan)
    {
        ImGui.PushID(plan.ID);  
        plan.Validate();

        ImGuiEx.TextV($"Plan settings:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);
        ImGui.SliderInt("Seals to keep", ref plan.RemainingSeals.ValidateRange(0, 15000), 0, 15000);
        ImGuiEx.HelpMarker("This amount of seals will be kept after purchase list is executed");
        ImGui.SameLine();
        ImGui.Checkbox("Finish by purchasing items", ref plan.FinalizeByPurchasing);
        ImGuiEx.HelpMarker("If selected, after final exchange items will be purchased, otherwise - purchase will not be made until seals are capped again.");

        ref string getFilter() => ref Ref<string>.Get($"{plan.ID}filter");
        ref bool onlySelected() => ref Ref<bool>.Get($"{plan.ID}onlySel");
        if(ImGui.BeginPopup("Ex"))
        {
            if(ImGui.Selectable("Fill weapons and armor purchases optimally for extra FC points"))
            {
                List<ItemWithQuantity> items = [];
                for(int i = plan.Items.Count-1; i >= 0; i--)
                {
                    var item = plan.Items[i];
                    var meta = Utils.SharedGCExchangeListings[item.ItemID];
                    if((meta.Category == GCExchangeCategoryTab.Weapons || meta.Category == GCExchangeCategoryTab.Armor) && meta.Data.GetRarity() == ItemRarity.Green)
                    {
                        items.Add(item);
                        plan.Items.RemoveAt(i);
                    } 
                }
                items = items.OrderByDescending(x => (double)Svc.Data.GetExcelSheet<GCSupplyDutyReward>().GetRow(x.Data.Value.LevelItem.RowId).SealsExpertDelivery / (double)Utils.SharedGCExchangeListings[x.ItemID].Seals).ToList();
                foreach(var x in items)
                {
                    plan.Items.Add(x);
                    x.Quantity = Utils.SharedGCExchangeListings[x.ItemID].Data.IsUnique ? 1 : 999;
                }
            }
            ImGuiEx.Tooltip("Select this option to fill in your plan with all purchaseable weapons and gear items. By doing so, weapons and items will be purchased and handed right back to the Grand Company, maximizing amount of generated Free Company points. All these items will be placed at the end of the list and only purchased if nothing else is available.");
            if(ImGui.Selectable("Reset quantities"))
            {
                plan.Items.Each(x => x.Quantity = 0);
            }
            if(ImGui.Selectable("Full reset"))
            {
                plan.Items.Clear();
            }
            ImGui.EndPopup();
        }
        if(ImGuiEx.IconButton(FontAwesomeIcon.AngleDoubleDown))
        {
            ImGui.OpenPopup("Ex");
        }
        ImGui.SameLine();
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            ImGui.InputTextWithHint("##filter", "Search...", ref getFilter(), 100);
        }, () =>
        {
            ImGui.Checkbox("Only Selected", ref onlySelected());
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.EnumCombo("##cat", ref SelectedCategory, nullName:"All Categories");
            ImGuiEx.Tooltip("Category");
        });
        
        DragDrop.Begin();
        if(ImGuiEx.BeginDefaultTable("GCDeliveryList", ["##dragDrop", "~Item", "GC", "Lv", "Price", "Category", "Amount", "##controls"]))
        {
            for(var i = 0; i < plan.Items.Count; i++)
            {
                var x = plan.Items[i];
                var meta = Utils.SharedGCExchangeListings[x.ItemID];
                if(onlySelected() && x.Quantity == 0) continue;
                if(getFilter().Length > 0 
                    && !meta.Data.Name.GetText().Contains(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !meta.Category.ToString().Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[meta.MinPurchaseRank].Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory != null && meta.Category != SelectedCategory.Value) continue;
                ImGui.PushID(x.ID);
                ImGui.TableNextRow();
                DragDrop.SetRowColor(x);
                ImGui.TableNextColumn();
                DragDrop.NextRow();
                DragDrop.DrawButtonDummy(x, plan.Items, i);
                ImGui.TableNextColumn();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(meta.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.ImGuiHandle, new(ImGui.GetFrameHeight()));
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Data.Name.GetText()}");
                ImGui.TableNextColumn();
                foreach(var c in Enum.GetValues<GrandCompany>().Where(x => x != GrandCompany.Unemployed))
                {
                    if(ThreadLoadImageHandler.TryGetIconTextureWrap(60870 + (int)c, false, out var ctex))
                    {
                        var trans = !meta.Companies.Contains(c);
                        if(trans) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                        ImGui.Image(ctex.ImGuiHandle, new(ImGui.GetFrameHeight()));
                        if(trans) ImGui.PopStyleVar();
                        ImGuiEx.Tooltip($"{c}" + (trans?" (unavailable)":""));
                        ImGui.SameLine(0, 1);
                    }
                }
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Data.LevelItem.RowId}");
                ImGui.TableNextColumn();
                if(Svc.Data.GetExcelSheet<GrandCompanyRank>().TryGetRow(meta.MinPurchaseRank, out var rank) && ThreadLoadImageHandler.TryGetIconTextureWrap(rank.IconFlames, false, out var tex))
                {
                    ImGui.Image(tex.ImGuiHandle, new(ImGui.GetFrameHeight()));
                    var rankName = Utils.GCRanks[meta.MinPurchaseRank];
                    ImGuiEx.Tooltip(rankName);
                    if(ImGuiEx.HoveredAndClicked()) getFilter() = rankName;
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Seals}");
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Category}");
                if(ImGuiEx.HoveredAndClicked()) getFilter() = meta.Category.ToString();
                ImGui.TableNextColumn();
                if(x.Data.Value.IsUnique)
                {
                    ImGuiEx.Checkbox("Unique", ref x.Quantity);
                }
                else
                {
                    ImGui.SetNextItemWidth(150f.Scale());
                    ImGui.InputInt("##qty", ref x.Quantity.ValidateRange(0, int.MaxValue), 1, 10);
                }
                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Clone))
                {
                    plan.Items.Insert(i+1, x.JSONClone());
                }
                ImGuiEx.Tooltip("Clone this listing. This allows you to set multiple tresholds for purchasing it.");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    if(plan.Items.Count(s => s.ItemID == x.ItemID) > 1)
                    {
                        new TickScheduler(() => plan.Items.Remove(x));
                    }
                    else
                    {
                        x.Quantity = 0;
                    }
                }
                ImGuiEx.Tooltip($"Deletes item from the list if there are multiple copies of it or sets it's amount to 0 if there is only one copy");
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();
        ImGui.PopID();
    }
}