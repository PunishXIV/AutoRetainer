using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugGCAuto : DebugSectionBase
{
    public override void Draw()
    {
        if (ImGui.CollapsingHeader("Expert items"))
        {
            foreach (var x in AutoGCHandin.GetHandinItems())
            {
                ImGuiEx.Text(x.ToString() + "/" + ExcelItemHelper.GetName(x.ItemID));
            }
        }
        if (ImGui.Button("EnqueueInitiation")) GCContinuation.EnqueueInitiation();
        if (ImGui.Button("EnqueueExchangeClose")) GCContinuation.EnqueueDeliveryClose();
        if (ImGui.Button("EnqueueExchangeVentures")) GCContinuation.EnqueueExchangeVentures();
        if (ImGui.Button("Step on")) P.TaskManager.SetStepMode(true);
        ImGui.SameLine();
        if (ImGui.Button("Step off")) P.TaskManager.SetStepMode(false);
        ImGui.SameLine();
        if (ImGui.Button("Step")) P.TaskManager.Step();
        if (ImGui.CollapsingHeader("GrandCompanySupplyList"))
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon))
            {
                var reader = new ReaderGrandCompanySupplyList(addon);
                if (reader.IsLoaded)
                {
                    var ptr = (GCExpectEntry*)*(nint*)((nint)addon + 624);
                    for (var i = 0; i < reader.NumItems; i++)
                    {
                        var entry = ptr[i];
                        ImGuiEx.Text($"{entry.Unk112}/{entry.Unk116}/{entry.Seals}/{entry.ItemID} {ExcelItemHelper.GetName(entry.ItemID)}/{entry.Unk136}/{entry.Unk145}");
                    }
                }
            }
        }
        if (ImGui.CollapsingHeader("GrandCompanyExchange"))
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon))
            {
                var reader = new ReaderGrandCompanyExchange(addon);
                List<ImGuiEx.EzTableEntry> entries = [];
                foreach (var x in reader.Items)
                {
                    entries.Add(new("Item", () => ImGuiEx.TextCopy($"{x.Name}")));
                    entries.Add(new("ID", () => ImGuiEx.TextCopy($"{x.ItemID}")));
                    entries.Add(new("Bag", () => ImGuiEx.TextCopy($"{x.Bag}")));
                    entries.Add(new("IconID", () => ImGuiEx.TextCopy($"{x.IconID}")));
                    entries.Add(new("RankReq", () => ImGuiEx.TextCopy($"{x.RankReq}")));
                    entries.Add(new("Seals", () => ImGuiEx.TextCopy($"{x.Seals}")));
                    entries.Add(new("Unk250", () => ImGuiEx.TextCopy($"{x.Unk250}")));
                    entries.Add(new("Unk350", () => ImGuiEx.TextCopy($"{x.Unk350}")));
                    entries.Add(new("Unk450", () => ImGuiEx.TextCopy($"{x.OpenCurrencyExchange}")));
                }
                ImGuiEx.EzTable(entries);
            }
        }
        ImGuiEx.Text($"GetGCSealMultiplier: {Utils.GetGCSealMultiplier()}");
        if (ImGui.Button(nameof(GCContinuation.SetMaxVenturesExchange))) DuoLog.Information($"{GCContinuation.SetMaxVenturesExchange()}");
        if (ImGui.Button(nameof(GCContinuation.SelectExchange))) DuoLog.Information($"{GCContinuation.SelectExchange()}");
        if (ImGui.Button(nameof(GCContinuation.ConfirmExchange))) DuoLog.Information($"{GCContinuation.ConfirmExchange()}");
        if (ImGui.Button(nameof(GCContinuation.SelectGCExchangeVerticalTab))) DuoLog.Information($"{GCContinuation.SelectGCExchangeVerticalTab(0)}");
        if (ImGui.Button(nameof(GCContinuation.SelectGCExchangeHorizontalTab))) DuoLog.Information($"{GCContinuation.SelectGCExchangeHorizontalTab(2)}");
        if (ImGui.Button(nameof(GCContinuation.InteractWithShop))) DuoLog.Information($"{GCContinuation.InteractWithShop()}");
        if (ImGui.Button(nameof(GCContinuation.InteractWithExchange))) DuoLog.Information($"{GCContinuation.InteractWithExchange()}");
        if (ImGui.Button(nameof(GCContinuation.SelectProvisioningMission))) DuoLog.Information($"{GCContinuation.SelectProvisioningMission()}");
        if (ImGui.Button(nameof(GCContinuation.SelectSupplyListTab))) DuoLog.Information($"{GCContinuation.SelectSupplyListTab(2)}");
        if (ImGui.Button(nameof(GCContinuation.EnableDeliveringIfPossible))) DuoLog.Information($"{GCContinuation.EnableDeliveringIfPossible()}");
        if (ImGui.Button(nameof(GCContinuation.CloseSupplyList))) DuoLog.Information($"{GCContinuation.CloseSupplyList()}");
        if (ImGui.Button(nameof(GCContinuation.CloseSelectString))) DuoLog.Information($"{GCContinuation.CloseSelectString()}");
        if (ImGui.Button(nameof(GCContinuation.CloseExchange))) DuoLog.Information($"{GCContinuation.CloseExchange()}");
        if (ImGui.Button(nameof(GCContinuation.OpenSeals))) DuoLog.Information($"{GCContinuation.OpenSeals()}");
    }
}
