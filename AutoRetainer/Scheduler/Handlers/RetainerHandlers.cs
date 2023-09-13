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
    internal static bool? WaitForVentureListUpdate()
    {
        if (P.ListUpdateFrame > CSFramework.Instance()->FrameCounter - 10) return true;
        return false;
    }

    internal static bool? SelectAssignVenture()
    {
        var text = new string[] { Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2386).Text.ToDalamudString().ExtractText(), Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2387).Text.ToDalamudString().ExtractText() };
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? SelectQuit()
    {
        if(TryGetAddonByName<AtkUnitBase>("RetainerTaskSupply", out var addon))
        {
            if (Utils.GenericThrottle)
            {
                addon->Close(true);
            }
            return false;
        }
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2383).Text.ToDalamudString().ExtractText();
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static void EnforceSelectStringThrottle() => EzThrottler.Throttle("EnforceSelectString", 3000, true);
    internal static bool? SelectViewVentureReport()
    {
        EnforceSelectStringThrottle();
        //2385	View venture report. (Complete)
        var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2385).Text.ToDalamudString().ExtractText();
        return Utils.TrySelectSpecificEntry(text);
    }

    internal static bool? EnforceSelectString(Func<bool?> Action)
    {
        if(!(TryGetAddonByName<AtkUnitBase>("SelectString", out var a) && a->IsVisible))
        {
            return true;
        }
        if(EzThrottler.Throttle("EnforceSelectString", 3000))
        {
            PluginLog.Warning($"Enforcing {Action.GetType().FullName} ");
            Action();
        }
        return false;
    }

    internal static bool? ClickResultReassign()
    {
        if (TryGetAddonByName<AddonRetainerTaskResult>("RetainerTaskResult", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            const string thrName = "ClickResultReassign.WaitForButtonEnabled";
            if (!addon->ReassignButton->IsEnabled)
            {
                FrameThrottler.Throttle(thrName, 5, true);
            }
            if (FrameThrottler.Check(thrName) && addon->ReassignButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskResult.Using((IntPtr)addon).Reassign();
                DebugLog($"Clicked reassign");
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
            const string thrName = "RetainerTaskResult.WaitForButtonEnabled";
            if (!addon->ConfirmButton->IsEnabled)
            {
                FrameThrottler.Throttle(thrName, 5, true);
            }
            if (FrameThrottler.Check(thrName) && addon->ConfirmButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskResult.Using((IntPtr)addon).Confirm();
                DebugLog($"Clicked confirm");
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
            const string thrName = "ClickAskAssign.WaitForButtonEnabled";
            if (!addon->AssignButton->IsEnabled)
            {
                FrameThrottler.Throttle(thrName, 5, true);
            }
            if (FrameThrottler.Check(thrName) && addon->AssignButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskAsk.Using((IntPtr)addon).Assign();
                DebugLog("Clicked assign");
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
            const string thrName = "ClickAskReturn.WaitForButtonEnabled";
            if (!addon->ReturnButton->IsEnabled)
            {
                FrameThrottler.Throttle(thrName, 5, true);
            }
            if (FrameThrottler.Check(thrName) && addon->ReturnButton->IsEnabled && Utils.GenericThrottle)
            {
                ClickRetainerTaskAsk.Using((IntPtr)addon).Return();
                DebugLog("Clicked return");
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
        return Utils.TrySelectSpecificEntry(Lang.QuickExploration);
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
                Callback.Fire(addon,false, (int)0);
                DebugLog($"Clicked entrust duplicates {invName.Name} {invName.EntrustDuplicatesIndex}");
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
                DebugLog($"Clicked duplicates confirm");
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
                DebugLog($"Clicked transfer progress close");
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
                DebugLog($"Gil: {numGil}");
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
                        DebugLog($"Set gil to withdraw {gilToWithdraw} (total: {numGil})");
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
            DebugLog($"Gil: {numGil}");
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
                    DebugLog($"Set gil to deposit {gilToDeposit} (total: {numGil})");
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

    internal static bool? SetDepositGilAmountExact(int amount)
    {
        if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon))
        {
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be 1 or higher");
            var numGil = TaskDepositGil.Gil;
            DebugLog($"Gil: {numGil}");
            var gilToDeposit = (uint)numGil;
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
                    DebugLog($"Set gil to deposit {gilToDeposit} (total: {numGil})");
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
                DebugLog($"Swapping withdraw mode");
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
                    DebugLog($"Clicked withdraw");
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
                        DebugLog($"Clicked cancel");
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
                    Callback.Fire(addon, false,(int)11, (int)VentureID);
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

    internal static bool? CheckForErrorAssignedVenture(uint ventureID)
    {
        if (TryGetAddonByName<AddonRetainerTaskAsk>("RetainerTaskAsk", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if (addon->AtkUnitBase.UldManager.NodeList[6]->IsVisible)
            {
                //An Error is on screen.
                ClickRetainerTaskAsk.Using((IntPtr)addon).Return();
                DebugLog($"Clicked cancel");
                P.TaskManager.EnqueueImmediate(() => SelectSpecificVentureByName(ventureID), "SelectSpecificVenture");
                P.TaskManager.DelayNextImmediate(10, false);
                P.TaskManager.EnqueueImmediate(() => CheckForErrorAssignedVenture(ventureID), 500, false, "RedoErrorCheck");
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }


    /*public static bool? SearchVentureByName(uint id) => SearchVentureByName(VentureUtils.GetVentureName(id));

    public static bool? SearchVentureByName(string name)
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerTaskSupply", out var addon) && IsAddonReady(addon))
        {
            if (Utils.GenericThrottle) 
            {
                Callback.Fire(addon, true, 2, new AtkValue() { Type = 0, Int = 0}, name);
                return true;
            }
        }
        return false;
    }*/

    public static bool? SelectSpecificVentureByName(uint id) => SelectSpecificVentureByName(VentureUtils.GetVentureName(id));

    public static bool? SelectSpecificVentureByName(string name)
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerTaskSupply", out var addon) && IsAddonReady(addon) && addon->AtkValuesCount > 2)
        {
            var state = addon->AtkValues[3];
            if (state.Type == 0)
            {
                FrameThrottler.Throttle("RetainerTaskSupply.InitWait", 10, true);
                PluginLog.Debug($"RetainerTaskSupply waiting (2)...");
                return false;
            }
            
            if (FrameThrottler.Check("RetainerTaskSupply.InitWait") && Utils.GenericThrottle)
            {
                if (addon->UldManager.NodeList[3]->IsVisible)
                {
                    var list = addon->UldManager.NodeList[3]->GetAsAtkComponentList();
                    PluginLog.Debug($"Cnt: {list->ListLength}");
                    for (int i = 0; i < Math.Min(list->ListLength, 16); i++)
                    {
                        var el = list->AtkComponentBase.UldManager.NodeList[2 + i];
                        var text = MemoryHelper.ReadSeString(&el->GetAsAtkComponentNode()->Component->UldManager.NodeList[9]->GetAsAtkTextNode()->NodeText).ExtractText();
                        PluginLog.Debug($"Text: {text}, name: {name}");
                        if (text == name)
                        {
                            PluginLog.Debug($"Match");
                            Callback.Fire(addon, true, 5, i, new AtkValue() { Type = 0, Int = 0 });
                            return true;
                        }
                    }

                    Callback.Fire(addon, true, 1);
                    return false;
                }
                else
                {
                    Callback.Fire(addon, true, 2, new AtkValue() { Type = 0, Int = 0 }, name);
                    Utils.RethrottleGeneric();
                    return false;
                }
            }
        }
        else
        {
            FrameThrottler.Throttle("RetainerTaskSupply.InitWait", 10, true);
            PluginLog.Debug($"RetainerTaskSupply waiting...");
        }
        return false;
    }

    internal static bool? ClearTaskSupplylist()
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerTaskSupply", out var addon) && IsAddonReady(addon))
        {
            if (Utils.GenericThrottle)
            {
                Callback.Fire(addon, true, 1);
                return true;
            }
        }
        return false;
    }
}
