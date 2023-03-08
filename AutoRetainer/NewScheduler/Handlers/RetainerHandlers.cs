using ClickLib.Clicks;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler.Handlers
{
    internal unsafe static class RetainerHandlers
    {
        internal static bool? SelectAssignVenture()
        {
            var text = new string[] { Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2386).Text.ToDalamudString().ExtractText(), Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(2387).Text.ToDalamudString().ExtractText() };
            return Utils.TrySelectSpecificEntry(text);
        }

        internal static bool? SelectQuit()
        {
            var text = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(917).Text.ToDalamudString().ExtractText();
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
                    PluginLog.Debug("Clicking assign...");
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
                    PluginLog.Debug("Clicking return...");
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
            if (TryGetAddonByName<AtkUnitBase>("InventoryRetainerLarge", out var addon) && IsAddonReady(addon))
            {
                var button = (AtkComponentButton*)addon->UldManager.NodeList[8]->GetComponent();
                if (addon->UldManager.NodeList[8]->IsVisible && button->IsEnabled && Utils.GenericThrottle)
                {
                    new ClickButtonGeneric(addon, "InventoryRetainerLarge").Click(button);
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
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? CloseRetainerInventory()
        {
            if (TryGetAddonByName<AtkUnitBase>("InventoryRetainerLarge", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle)
                {
                    addon->Hide(true);
                    if (TryGetAddonByName<AtkUnitBase>("InventoryLarge", out var iaddon) && IsAddonReady(iaddon))
                    {
                        iaddon->Hide(true);
                    }
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? SetWithdrawGilAmount(int percent)
        {
            if (TryGetAddonByName<AtkUnitBase>("Bank", out var addon) && IsAddonReady(addon) && Utils.TryGetCurrentRetainer(out var name) && Utils.TryGetRetainerByName(name, out var retainer))
            {
                if (percent < 1 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent), percent, "Percent must be between 1 and 100");
                var gilToWithdraw = (uint)(percent == 100 ? retainer.Gil : retainer.Gil / 100f * percent);
                if (gilToWithdraw > 0 && gilToWithdraw <= retainer.Gil && Utils.GenericThrottle)
                {
                    var v = stackalloc AtkValue[]
                    {
                        new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int, Int = 3 },
                        new() { Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt, UInt = gilToWithdraw }
                    };
                    addon->FireCallback(2, v);
                    return true;
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

        internal static bool? WithdrawGilOrCancel()
        {
            return WithdrawGilOrCancel(false);
        }

        internal static bool? WithdrawGilOrCancel(bool forceCancel = false)
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
                        addon->Hide(true);
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
                            addon->Hide(true);
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
            return true;
        }
    }
}
