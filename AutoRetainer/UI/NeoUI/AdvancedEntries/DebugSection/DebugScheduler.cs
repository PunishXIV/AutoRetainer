using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugScheduler : DebugSectionBase
{
    private string dbgRetName = string.Empty;
    public override void Draw()
    {
        ImGuiEx.Text($"Gil: {TaskDepositGil.Gil}");
        ImGui.Checkbox($"TaskWithdrawGil.forceCheck", ref TaskWithdrawGil.forceCheck);
        ImGuiEx.Text($"{Svc.Data.GetExcelSheet<LogMessage>().GetRow(4578).Text.ToDalamudString().ExtractText(true)}");
        if(ImGui.Button("Close retainer"))
        {
            DuoLog.Information($"{RetainerHandlers.CloseAgentRetainer()}");
        }
        ImGuiEx.Text($"Utils.AnyRetainersAvailableCurrentChara(): {Utils.AnyRetainersAvailableCurrentChara()}");
        if(ImGui.Button($"SelectAssignVenture"))
        {
            DuoLog.Information($"{RetainerHandlers.SelectAssignVenture()}");
        }
        if(ImGui.Button($"SelectQuit"))
        {
            DuoLog.Information($"{RetainerHandlers.SelectQuit()}");
        }
        if(ImGui.Button($"SelectViewVentureReport"))
        {
            DuoLog.Information($"{RetainerHandlers.SelectViewVentureReport()}");
        }
        if(ImGui.Button($"ClickResultReassign"))
        {
            DuoLog.Information($"{RetainerHandlers.ClickResultReassign()}");
        }
        if(ImGui.Button($"ClickResultConfirm"))
        {
            DuoLog.Information($"{RetainerHandlers.ClickResultConfirm()}");
        }
        if(ImGui.Button($"ClickAskAssign"))
        {
            DuoLog.Information($"{RetainerHandlers.ClickAskAssign()}");
        }
        if(ImGui.Button($"SelectQuickExploration"))
        {
            DuoLog.Information($"{RetainerHandlers.SelectQuickExploration()}");
        }
        if(ImGui.Button($"SelectEntrustItems"))
        {
            DuoLog.Information($"{RetainerHandlers.SelectEntrustItems()}");
        }
        if(ImGui.Button($"SelectEntrustGil"))
        {
            DuoLog.Information($"{RetainerHandlers.SelectEntrustGil()}");
        }
        if(ImGui.Button($"ClickEntrustDuplicates"))
        {
            DuoLog.Information($"{RetainerHandlers.ClickEntrustDuplicates()}");
        }
        if(ImGui.Button($"ClickEntrustDuplicatesConfirm"))
        {
            DuoLog.Information($"{RetainerHandlers.ClickEntrustDuplicatesConfirm()}");
        }
        if(ImGui.Button($"ClickCloseEntrustWindow"))
        {
            DuoLog.Information($"{RetainerHandlers.ClickCloseEntrustWindow()}");
        }
        if(ImGui.Button($"CloseRetainerInventory"))
        {
            DuoLog.Information($"{RetainerHandlers.CloseAgentRetainer()}");
        }
        if(ImGui.Button($"CloseRetainerInventory"))
        {
            DuoLog.Information($"{RetainerHandlers.CloseAgentRetainer()}");
        }
        if(ImGui.Button($"SetWithdrawGilAmount (1%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(1)}");
        }
        if(ImGui.Button($"SetWithdrawGilAmount (50%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(50)}");
        }
        if(ImGui.Button($"SetWithdrawGilAmount (99%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(99)}");
        }
        if(ImGui.Button($"SetWithdrawGilAmount (100%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetWithdrawGilAmount(100)}");
        }
        if(ImGui.Button($"WithdrawGilOrCancel"))
        {
            DuoLog.Information($"{RetainerHandlers.ProcessBankOrCancel()}");
        }
        if(ImGui.Button($"WithdrawGilOrCancel (force cancel)"))
        {
            DuoLog.Information($"{RetainerHandlers.ProcessBankOrCancel(true)}");
        }
        if(ImGui.Button($"SwapBankMode"))
        {
            DuoLog.Information($"{RetainerHandlers.SwapBankMode()}");
        }
        if(ImGui.Button($"SetDepositGilAmount (1%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetDepositGilAmount(1)}");
        }
        if(ImGui.Button($"SetDepositGilAmount (50%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetDepositGilAmount(50)}");
        }
        if(ImGui.Button($"SetDepositGilAmount (99%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetDepositGilAmount(99)}");
        }
        if(ImGui.Button($"SetDepositGilAmount (100%)"))
        {
            DuoLog.Information($"{RetainerHandlers.SetDepositGilAmount(100)}");
        }

        ImGui.Separator();

        if(ImGui.Button($"TaskEntrustDuplicates"))
        {
            TaskEntrustDuplicates.Enqueue();
        }
        if(ImGui.Button($"TaskAssignQuickVenture"))
        {
            TaskAssignQuickVenture.Enqueue();
        }
        if(ImGui.Button($"TaskReassignVenture"))
        {
            TaskReassignVenture.Enqueue();
        }
        if(ImGui.Button($"TaskWithdrawGil (50%)"))
        {
            TaskWithdrawGil.Enqueue(50);
        }

        ImGuiEx.Text($"Free inventory slots: {Utils.GetInventoryFreeSlotCount()}");
        ImGui.InputText("Retainer name", ref dbgRetName, 50);
        if(ImGui.Button("Select retainer by name"))
        {
            DuoLog.Information($"{RetainerListHandlers.SelectRetainerByName(dbgRetName)}");
        }

        if(ImGui.Button("AtkStage get focus"))
        {
            var ptr = (nint)AtkStage.Instance()->GetFocus();
            Svc.Chat.Print($"Stage focus: {ptr}");
        }
        if(ImGui.Button("AtkStage clear focus"))
        {
            AtkStage.Instance()->ClearFocus();
        }
        if(ImGui.Button("Try retrieve current retainer name"))
        {
            if(TryGetAddonByName<AddonSelectString>("SelectString", out var select) && IsAddonReady(&select->AtkUnitBase))
            {
                var textNode = (AtkTextNode*)select->AtkUnitBase.UldManager.NodeList[3];
                var text = MemoryHelper.ReadSeString(&textNode->NodeText);
                foreach(var x in text.Payloads)
                {
                    PluginLog.Information($"{x.Type}: {x.ToString()}");
                }
            }
        }
        {
            if(ImGui.Button("Try close") && TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon))
            {
                var v = stackalloc AtkValue[1]
                {
                                        new()
                                        {
                                                Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                                                Int = -1
                                        }
                                };
                addon->FireCallback(1, v);
                Notify.Info("Done");
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon))
            {
                if(ImGui.Button("test bank"))
                {
                    var values = stackalloc AtkValue[2]
                    {
                                                new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 3 },
                                                new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt, Int = 50 },
                                        };
                    addon->FireCallback(2, values);
                }
            }
        }

        ImGui.Separator();

        if(ImGui.Button("TaskDesynthItems"))
            TaskDesynthItems.Enqueue();
    }
}
