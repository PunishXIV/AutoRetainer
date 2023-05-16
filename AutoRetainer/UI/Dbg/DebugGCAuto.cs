using AutoRetainer.Scheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal static unsafe class DebugGCAuto
    {
        internal static void Draw()
        {
            ImGuiEx.Text($"GetGCSealMultiplier: {Utils.GetGCSealMultiplier()}");
            if (ImGui.Button(nameof(GCHandlers.SetMaxVenturesExchange))) DuoLog.Information($"{GCHandlers.SetMaxVenturesExchange()}");
            if (ImGui.Button(nameof(GCHandlers.SelectExchange))) DuoLog.Information($"{GCHandlers.SelectExchange()}");
            if (ImGui.Button(nameof(GCHandlers.ConfirmExchange))) DuoLog.Information($"{GCHandlers.ConfirmExchange()}");
            if (ImGui.Button(nameof(GCHandlers.SelectGCExchangeVerticalTab))) DuoLog.Information($"{GCHandlers.SelectGCExchangeVerticalTab()}");
            if (ImGui.Button(nameof(GCHandlers.SelectGCExchangeHorizontalTab))) DuoLog.Information($"{GCHandlers.SelectGCExchangeHorizontalTab()}");
            if (ImGui.Button(nameof(GCHandlers.TargetShop))) DuoLog.Information($"{GCHandlers.TargetShop()}");
            if (ImGui.Button(nameof(GCHandlers.InteractWithShop))) DuoLog.Information($"{GCHandlers.InteractWithShop()}");
            if (ImGui.Button(nameof(GCHandlers.TargetExchange))) DuoLog.Information($"{GCHandlers.TargetExchange()}");
            if (ImGui.Button(nameof(GCHandlers.InteractExchange))) DuoLog.Information($"{GCHandlers.InteractExchange()}");
            if (ImGui.Button(nameof(GCHandlers.SelectProvisioningMission))) DuoLog.Information($"{GCHandlers.SelectProvisioningMission()}");
            if (ImGui.Button(nameof(GCHandlers.SelectGCExpertDelivery))) DuoLog.Information($"{GCHandlers.SelectGCExpertDelivery()}");
            if (ImGui.Button(nameof(GCHandlers.EnableDeliveringIfPossible))) DuoLog.Information($"{GCHandlers.EnableDeliveringIfPossible()}");
            if (ImGui.Button(nameof(GCHandlers.CloseSupplyList))) DuoLog.Information($"{GCHandlers.CloseSupplyList()}");
            if (ImGui.Button(nameof(GCHandlers.CloseSelectString))) DuoLog.Information($"{GCHandlers.CloseSelectString()}");
            if (ImGui.Button(nameof(GCHandlers.CloseExchange))) DuoLog.Information($"{GCHandlers.CloseExchange()}");
            if (ImGui.Button(nameof(GCHandlers.OpenCurrency))) DuoLog.Information($"{GCHandlers.OpenCurrency()}");
        }
    }
}
