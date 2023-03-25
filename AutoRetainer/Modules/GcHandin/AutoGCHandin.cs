using AutoRetainer.UI.Overlays;
using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Modules.GcHandin;

internal unsafe static class AutoGCHandin
{
    internal static AutoGCHandinOverlay Overlay;
    internal static bool Operation = false;
    internal static long AddonOpenedAt = 0;

    internal static bool IsEnabled()
    {
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var d))
        {
            return d.GCDeliveryType != GCDeliveryType.Disabled;
        }
        return false;
    }
    internal static bool IsArmoryChestEnabled()
    {
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var d))
        {
            return d.GCDeliveryType.EqualsAny(GCDeliveryType.Hide_Gear_Set_Items, GCDeliveryType.Show_All_Items);
        }
        return false;
    }

    internal static bool IsAllItemsEnabled()
    {
        Safety.Check();
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var d))
        {
            return d.GCDeliveryType == GCDeliveryType.Show_All_Items;
        }
        return false;
    }

    internal static void Init()
    {
        Overlay = new();
        P.ws.AddWindow(Overlay);
    }

    internal static void Tick()
    {
        if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent] && IsEnabled())
        {
            Safety.Check();
            if (TryGetAddonByName<AddonGrandCompanySupplyReward>("GrandCompanySupplyReward", out var addonGCSR) && IsAddonReady(&addonGCSR->AtkUnitBase) && Operation)
            {
                if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("CloseSupplyList", 200))
                {
                    UiHelper.Close(addon);
                    P.DebugLog($"Closing Supply List");
                }
                if (EzThrottler.Throttle("Handin", 200) && addonGCSR->DeliverButton->IsEnabled)
                {
                    ClickGrandCompanySupplyReward.Using((IntPtr)addonGCSR).Deliver();
                    P.DebugLog($"Delivering Item");
                }
            }
            else if (TryGetAddonByName<AddonSelectYesno>("SelectYesno", out var addonSS) && IsAddonReady(&addonSS->AtkUnitBase) && Operation)
            {
                if (EzThrottler.Throttle("Yesno", 200) && addonSS->YesButton->IsEnabled)
                {
                    var str = MemoryHelper.ReadSeString(&addonSS->PromptText->NodeText).ExtractText();
                    P.DebugLog($"SelectYesno encountered: {str}");
                    //102434	Do you really want to trade a high-quality item?
                    if (str.Equals(Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(102434).Text.ToDalamudString().ExtractText()))
                    {
                        ClickSelectYesNo.Using((IntPtr)addonSS).Yes();
                        P.DebugLog($"Selecting yes");
                    }
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsReadyToOperate(addon))
            {
                if (AddonOpenedAt == 0)
                {
                    AddonOpenedAt = Environment.TickCount64;
                }
                Overlay.Position = new(addon->X, addon->Y - Overlay.height);
                if (Operation)
                {
                    if (IsDone(addon))
                    {
                        var s = $"Automatic handin has been completed";
                        DuoLog.Information(s);
                        if (P.config.GCHandinNotify)
                        {
                            Utils.TryNotify(s);
                        }
                        Operation = false;
                    }
                    else
                    {
                        Overlay.IsOpen = true;
                        if (EzThrottler.Check("AutoGCHandin"))
                        {
                            try
                            {
                                var sealsCnt = MemoryHelper.ReadSeString(&addon->UldManager.NodeList[23]->GetAsAtkTextNode()->NodeText).ExtractText().Replace(",", "").Replace(".", "").Split("/");
                                if (sealsCnt.Length != 2)
                                {
                                    throw new FormatException();
                                }
                                var seals = sealsCnt[0].ParseInt();
                                var maxSeals = sealsCnt[1].ParseInt();

                                var step1 = addon->UldManager.NodeList[5];
                                var step2 = step1->GetAsAtkComponentNode()->Component->UldManager.NodeList[2];
                                var sealsForItem = MemoryHelper.ReadSeString(&step2->GetAsAtkComponentNode()->Component->UldManager.NodeList[4]->GetAsAtkTextNode()->NodeText).ExtractText().ParseInt();
                                if (seals == null || maxSeals == null || sealsForItem == null)
                                {
                                    throw new FormatException();
                                }
                                var step3 = step2->GetAsAtkComponentNode()->Component->UldManager.NodeList[5];
                                var text = MemoryHelper.ReadSeString(&step3->GetAsAtkTextNode()->NodeText).ExtractText();
                                var has = !text.IsNullOrWhitespace() && HasInInventory(text);
                                P.DebugLog($"Seals: {seals}/{maxSeals}, for item {sealsForItem} | {text}: {has}");
                                EzThrottler.Throttle("AutoGCHandin", 500, true);
                                if (!has)
                                {
                                    throw new GCHandinInterruptedException($"Item {text} was not found in inventory");
                                }
                                if (seals + sealsForItem > maxSeals)
                                {
                                    throw new GCHandinInterruptedException($"Too many seals, please spend them");
                                }
                                P.DebugLog($"Handing in item {text} for {sealsForItem} seals");
                                InvokeHandin(addon);
                            }
                            catch (FormatException e)
                            {
                                PluginLog.Verbose($"{e.Message}");
                            }
                            catch (GCHandinInterruptedException e)
                            {
                                Operation = false;
                                DuoLog.Information($"{e.Message}");
                                if (P.config.GCHandinNotify)
                                {
                                    Utils.TryNotify(e.Message);
                                }
                            }
                            catch (Exception e)
                            {
                                Operation = false;
                                e.Log();
                            }
                        }
                    }
                }
                else
                {
                    Overlay.IsOpen = IsReadyToOperate(addon);
                }
            }
            else
            {
                Overlay.IsOpen = Operation || IsReadyToOperate(addon);
                AddonOpenedAt = 0;
            }
        }
        else
        {
            if (Overlay.IsOpen)
            {
                Overlay.IsOpen = false;
            }
            if (Operation)
            {
                Operation = false;
            }
            if (AddonOpenedAt != 0)
            {
                AddonOpenedAt = 0;
            }
        }
    }

    static bool IsReadyToOperate(AtkUnitBase* GCSupplyListAddon)
    {
        try
        {
            return
                IsAddonReady(GCSupplyListAddon)
                && GCSupplyListAddon->UldManager.NodeListCount > 20
                && GCSupplyListAddon->UldManager.NodeList[5]->IsVisible
                && IsSelectedFilterValid(GCSupplyListAddon);
        }
        catch (Exception)
        {
            return false;
        }
    }
    internal static bool IsDone(AtkUnitBase* addon)
    {
        return addon->UldManager.NodeList[20]->IsVisible;
    }
    internal static bool IsSelectedFilterValid(AtkUnitBase* addon)
    {
        var step1 = addon->UldManager.NodeList[14];
        var step2 = step1->GetAsAtkComponentNode()->Component->UldManager.NodeList[1];
        var step3 = step2->GetAsAtkComponentNode()->Component->UldManager.NodeList[2];
        var text = MemoryHelper.ReadSeString(&step3->GetAsAtkTextNode()->NodeText).ExtractText();
        //4619	Hide Armoury Chest Items
        //4618	Hide Gear Set Items
        //4617	Show All Items
        var hideArmory = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(4619).Text.ToDalamudString().ExtractText();
        var hideGearSet = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(4618).Text.ToDalamudString().ExtractText();
        var showAll = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Addon>().GetRow(4617).Text.ToDalamudString().ExtractText();
        if (text.Equals(hideArmory))
        {
            return true;
        }
        else
        {
            if(P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
            {
                if(text.EqualsAny(hideGearSet))
                {
                    return IsArmoryChestEnabled() || IsAllItemsEnabled();
                }
                if(text.EqualsAny(showAll))
                {
                    return IsAllItemsEnabled();
                }
            }
        }
        return false;
    }

    internal static void InvokeHandin(AtkUnitBase* addon)
    {
        EzThrottler.Throttle("AutoGCHandin", 500, true);
        var values = stackalloc AtkValue[]
        {
            new() { Type = ValueType.Int, Int = 1 },
            new() { Type = ValueType.Int, Int = 0 },
            new() { Type = 0, Int = 0 }
        };
        addon->FireCallback(3, values);
    }

    internal static long GetAddonLife()
    {
        return AddonOpenedAt == 0 ? 0 : Environment.TickCount64 - AddonOpenedAt;
    }

    internal static bool HasInInventory(string item)
    {
        var id = Svc.Data.GetExcelSheet<Item>().FirstOrDefault(x => x.Name.ExtractText() == item);
        //InternalLog.Information($"{id}");
        return id != null && InventoryManager.Instance()->GetInventoryItemCount(id.RowId) + InventoryManager.Instance()->GetInventoryItemCount(id.RowId, true) > 0;
    }
}
