using AutoRetainer.QSI;
using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Throttlers;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.GcHandin
{
    internal unsafe static class AutoGCHandin
    {
        internal static AutoGCHandinOverlay Overlay;
        internal static bool Operation = false;
        internal static long AddonOpenedAt = 0;

        internal static void Init()
        {
            Overlay = new();
            P.ws.AddWindow(Overlay);
        }

        internal static void Tick()
        {
            if (P.config.EnableAutoGCHandin && Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
            {
                if (TryGetAddonByName<AddonGrandCompanySupplyReward>("GrandCompanySupplyReward", out var addonGCSR) && IsAddonReady(&addonGCSR->AtkUnitBase) && Operation)
                {
                    if (YesAlready.IsEnabled())
                    {
                        YesAlready.DisableIfNeeded();
                    }
                    if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("CloseSupplyList", 200))
                    {
                        UiHelper.Close(addon);
                        PluginLog.Information($"Closing Supply List");
                    }
                    if (EzThrottler.Throttle("Handin", 200) && addonGCSR->DeliverButton->IsEnabled)
                    {
                        ClickGrandCompanySupplyReward.Using((IntPtr)addonGCSR).Deliver();
                        PluginLog.Information($"Delivering Item");
                    }
                }
                else if (TryGetAddonByName<AddonSelectYesno>("SelectYesno", out var addonSS) && IsAddonReady(&addonSS->AtkUnitBase) && Operation)
                {
                    if (YesAlready.IsEnabled())
                    {
                        YesAlready.DisableIfNeeded();
                    }
                    if (EzThrottler.Throttle("Yesno", 200) && addonSS->YesButton->IsEnabled)
                    {
                        var str = MemoryHelper.ReadSeString(&addonSS->PromptText->NodeText).ToString();
                        PluginLog.Information($"SelectYesno encountered: {str}");
                        if (str.EqualsAny("Do you really want to trade a high-quality item?"))
                        {
                            ClickSelectYesNo.Using((IntPtr)addonSS).Yes();
                            PluginLog.Information($"Selecting yes");
                        }
                    }
                }
                else if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsReadyToOperate(addon))
                {
                    if (AddonOpenedAt == 0)
                    {
                        AddonOpenedAt = Environment.TickCount64;
                    }
                    Overlay.Position = new(addon->X, addon->Y);
                    if (Operation)
                    {
                        if (YesAlready.IsEnabled())
                        {
                            YesAlready.DisableIfNeeded();
                        }
                        if (IsDone(addon))
                        {
                            DuoLog.Information($"Automatic handin has been completed");
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
                                    PluginLog.Information($"Seals: {seals}/{maxSeals}, for item {sealsForItem} | {text}: {has}");
                                    EzThrottler.Throttle("AutoGCHandin", 500, true);
                                    if (!has)
                                    {
                                        throw new GCHandinInterruptedException($"Item {text} was not found in inventory");
                                    }
                                    if (seals + sealsForItem > maxSeals)
                                    {
                                        throw new GCHandinInterruptedException($"Too many seals, please spend them");
                                    }
                                    DuoLog.Information($"Handing in item {text} for {sealsForItem} seals");
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
                        if (YesAlready.Reenable)
                        {
                            YesAlready.EnableIfNeeded();
                        }
                    }
                }
                else
                {
                    Overlay.IsOpen = Operation || IsReadyToOperate(addon);
                    AddonOpenedAt = 0;
                    if (!Operation && YesAlready.Reenable)
                    {
                        YesAlready.EnableIfNeeded();
                    }
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
                if(AddonOpenedAt != 0)
                {
                    AddonOpenedAt = 0;
                }
                if (YesAlready.Reenable)
                {
                    YesAlready.EnableIfNeeded();
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
            catch(Exception)
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
            var ret = text.EqualsAny("Hide Armoury Chest Items");
            //if (!ret) PluginLog.Verbose($"Selected filter is not valid");
            return ret;
        }

        internal static void InvokeHandin(AtkUnitBase* addon)
        {
            EzThrottler.Throttle("AutoGCHandin", 500, true);
            var values = stackalloc AtkValue[]
            {
                new() { Type = ValueType.Int, Int = 1 },
                new() { Type = ValueType.Int, Int = 0 },
                new() { Type = 0, Int = 0}
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
}
