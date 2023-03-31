using AutoRetainer.Internal.Clicks;
using AutoRetainer.Scheduler.Tasks;
using ClickLib.Clicks;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using static System.Net.Mime.MediaTypeNames;

namespace AutoRetainer.Scheduler.Handlers;

internal unsafe static class RetainerHandlers
{
    internal static bool? SelectAssignVenture()
    {
        var text = new string[] { Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2386).Text.ToDalamudString().ExtractText(), Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2387).Text.ToDalamudString().ExtractText() };
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? SelectQuit()
    {
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2383).Text.ToDalamudString().ExtractText();
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? SelectViewVentureReport()
    {
        //2385	View venture report. (Complete)
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2385).Text.ToDalamudString().ExtractText();
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? ClickResultReassign()
    {
        if (TryGetAddonByName<AddonRetainerTaskResult>("RetainerTaskResult", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if (addon->ReassignButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskResult.Using((IntPtr)addon).Reassign();
                P.DebugLog($"Clicked reassign");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? ClickResultConfirm()
    {
        if (TryGetAddonByName<AddonRetainerTaskResult>("RetainerTaskResult", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if (addon->ConfirmButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskResult.Using((IntPtr)addon).Confirm();
                P.DebugLog($"Clicked confirm");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? ClickAskAssign()
    {
        if (TryGetAddonByName<AddonRetainerTaskAsk>("RetainerTaskAsk", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if (addon->AssignButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskAsk.Using((IntPtr)addon).Assign();
                P.DebugLog("Clicked assign");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? ClickAskReturn()
    {
        if (TryGetAddonByName<AddonRetainerTaskAsk>("RetainerTaskAsk", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if (addon->AssignButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskAsk.Using((IntPtr)addon).Return();
                P.DebugLog("Clicked return");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? SelectQuickExploration()
    {
        return Utils.TrySelectSpecificEntry(Consts.QuickExploration);
    }

    internal static bool? SelectEntrustItems()
    {
        //2378	Entrust or withdraw items.
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2378).Text.ToDalamudString().ExtractText();
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? SelectEntrustGil()
    {
        //2379	Entrust or withdraw gil.
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2379).Text.ToDalamudString().ExtractText();
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? ClickEntrustDuplicates()
    {
        var invName = Utils.GetActiveRetainerInventoryName();
        if (TryGetAddonByName<AtkUnitBase>(invName.Name, out var addon) && IsAddonReady(addon))
        {
            var button = (AtkComponentButton*)addon->UldManager.NodeList[invName.EntrustDuplicatesIndex]->GetComponent();
            if (addon->UldManager.NodeList[invName.EntrustDuplicatesIndex]->IsVisible && button->IsEnabled && Utils.GenericThrottle)
            {
                //new ClickButtonGeneric(addon, invName.Name).Click(button);
                Callback(addon, (int)0);
                P.DebugLog($"Clicked entrust duplicates {invName.Name} {invName.EntrustDuplicatesIndex}");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? ClickEntrustDuplicatesConfirm()
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon) && IsAddonReady(addon))
        {
            var button = (AtkComponentButton*)addon->UldManager.NodeList[3]->GetComponent();
            if (addon->UldManager.NodeList[3]->IsVisible && button->IsEnabled && Utils.GenericThrottle)
            {
                new ClickButtonGeneric(addon, "RetainerItemTransferList").Click(button);
                P.DebugLog($"Clicked duplicates confirm");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? ClickCloseEntrustWindow()
    {
        //13530	Close Window
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(13530).Text.ToDalamudString().ExtractText();
        if (TryGetAddonByName<AtkUnitBase>("RetainerItemTransferProgress", out var addon) && IsAddonReady(addon))
        {
            var button = (AtkComponentButton*)addon->UldManager.NodeList[2]->GetComponent();
            var nodetext = MemoryHelper.ReadSeString(&addon->UldManager.NodeList[2]->GetComponent()->UldManager.NodeList[2]->GetAsAtkTextNode()->NodeText).ExtractText();
            if (nodetext == text && addon->UldManager.NodeList[2]->IsVisible && button->IsEnabled && Utils.GenericThrottle)
            {
                new ClickButtonGeneric(addon, "RetainerItemTransferProgress").Click(button);
                P.DebugLog($"Clicked transfer progress close");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? CloseAgentRetainer()
    {
        var a = Framework.Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.Retainer);
        if (a->IsAgentActive())
        {
            a->Hide();
            return true;
        }
        return false;
    }

    internal static bool? SetWithdrawGilAmount(int percent)
    {
        if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon) && Utils.TryGetCurrentRetainer(out var name) && Utils.TryGetRetainerByName(name, out var retainer))
        {
            if (percent < 1 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent), percent, "Percent must be between 1 and 100");
            if (uint.TryParse(MemoryHelper.ReadSeString(&addon->UldManager.NodeList[27]->GetAsAtkTextNode()->NodeText).ExtractText().RemoveOtherChars("0123456789"), out var numGil))
            {
                P.DebugLog($"Gil: {numGil}");
                var gilToWithdraw = (uint)(percent == 100 ? numGil : numGil / 100f * percent);
                if (gilToWithdraw > 0 && gilToWithdraw <= numGil)
                {
                    if (Utils.GenericThrottle)
                    {
                        var v = stackalloc AtkValue[]
                        {
                            new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 3 },
                            new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt, UInt = gilToWithdraw }
                        };
                        addon->FireCallback(2, v);
                        P.DebugLog($"Set gil to withdraw {gilToWithdraw} (total: {numGil})");
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? SetDepositGilAmount(int percent)
    {
        if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon))
        {
            if (percent < 1 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent), percent, "Percent must be between 1 and 100");
            var numGil = TaskDepositGil.Gil;
            P.DebugLog($"Gil: {numGil}");
            var gilToDeposit = (uint)(percent == 100 ? numGil : numGil / 100f * percent);
            if (gilToDeposit > 0 && gilToDeposit <= numGil)
            {
                if (Utils.GenericThrottle)
                {
                    var v = stackalloc AtkValue[]
                    {
                        new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 3 },
                        new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt, UInt = gilToDeposit }
                    };
                    addon->FireCallback(2, v);
                    P.DebugLog($"Set gil to deposit {gilToDeposit} (total: {numGil})");
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? SwapBankMode()
    {
        if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon))
        {
            if (Utils.GenericThrottle)
            {
                var v = stackalloc AtkValue[]
                {
                    new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 2 },
                    new() { Type = 0, UInt = 0 }
                };
                addon->FireCallback(2, v);
                P.DebugLog($"Swapping withdraw mode");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? ProcessBankOrCancel()
    {
        return ProcessBankOrCancel(false);
    }

    internal static bool? ProcessBankOrCancel(bool forceCancel = false)
    {
        if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon))
        {
            var withdraw = (AtkComponentButton*)addon->UldManager.NodeList[3]->GetComponent();
            if (addon->UldManager.NodeList[3]->IsVisible && withdraw->IsEnabled && !forceCancel)
            {
                if (Utils.GenericThrottle)
                {
                    var v = stackalloc AtkValue[]
                    {
                        new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 0 },
                        new() { Type = 0, Int = 0 }
                    };
                    addon->FireCallback(2, v);
                    UiHelper.Close(addon, true);
                    P.DebugLog($"Clicked withdraw");
                    //new ClickButtonGeneric(addon, "Bank").Click(withdraw);
                    return true;
                }
            }
            else
            {
                var cancel = (AtkComponentButton*)addon->UldManager.NodeList[2]->GetComponent();
                if (addon->UldManager.NodeList[2]->IsVisible && cancel->IsEnabled)
                {
                    if (Utils.GenericThrottle)
                    {
                        var v = stackalloc AtkValue[]
                    {
                            new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 1 },
                            new() { Type = 0, Int = 0 }
                        };
                        addon->FireCallback(2, v);
                        UiHelper.Close(addon, true);
                        P.DebugLog($"Clicked cancel");
                        //new ClickButtonGeneric(addon, "Bank").Click(cancel);
                        return true;
                    }
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? GenericSelectByName(params string[] text)
    {
        return Utils.TrySelectSpecificEntry(text);
    }

    public static bool? SelectSpecificVenture(uint VentureID)
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerTaskList", out var addon) && IsAddonReady(addon))
        {
            var ventureData = Svc.Data.GetExcelSheet<RetainerTask>().GetRow(VentureID);
            var ventureName = ventureData.GetVentureName();
            if (Utils.GenericThrottle && EzThrottler.Throttle("AssignSpecificVenture", 1000))
            {
                if (VentureUtils.GetAvailableVentureNames().Contains(ventureName))
                {
                    Callback(addon, (int)11, (int)VentureID);
                    return true;
                }
                else
                {
                    PluginLog.Error($"Can not find venture id {VentureID} [{ventureName}] in list {VentureUtils.GetAvailableVentureNames().Print()}");
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }
}
