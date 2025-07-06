using AutoRetainerAPI.Configuration;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.Automation.UIInput;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Reflection.Metadata.Ecma335;

namespace AutoRetainer.Modules.GcHandin;

internal static unsafe class GCContinuation
{
    public static readonly GCInfo Maelstrom = new(1002387, 1002388, new(92.751045f, 40.27537f, 75.468185f));
    public static readonly GCInfo ImmortalFlames = new(1002390, 1002391, new(-141.44354f, 4.109951f, -106.125496f));
    public static readonly GCInfo TwinAdder = new(1002393, 1002394, new(-67.464386f, -0.5018193f, -8.161054f));

    public static readonly uint VentureItem = 21072;

    public static bool DebugMode = false;
    public static bool DebugConf = false;

    public static void EnqueueInitiation(bool redeliver)
    {
        P.TaskManager.Enqueue(GCContinuation.WaitUntilNotOccupied);
        P.TaskManager.Enqueue(GCContinuation.InteractWithShop);
        P.TaskManager.Enqueue(BeginNewPurchase);
        P.TaskManager.Enqueue(GCContinuation.WaitUntilNotOccupied);
        if(redeliver)
        {
            P.TaskManager.Enqueue(GCContinuation.InteractWithExchange);
            P.TaskManager.Enqueue(GCContinuation.SelectProvisioningMission);
            P.TaskManager.Enqueue(() => GCContinuation.SelectSupplyListTab(2), "SelectSupplyListTab(2)");
            P.TaskManager.Enqueue(GCContinuation.EnableDeliveringIfPossible);
        }
    }

    public static void EnqueueDeliveryClose()
    {
        P.TaskManager.Enqueue(GCContinuation.CloseSupplyList);
        P.TaskManager.Enqueue(GCContinuation.CloseSelectString);
        P.TaskManager.Enqueue(GCContinuation.WaitUntilNotOccupied);
    }

    internal static bool SetVenturesExchangeAmount(int amount)
    {
        if(TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon) && IsAddonReady(addon) && TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var gca) && IsAddonReady(gca))
        {
            var num = GenericHelpers.ReadSeString(&gca->UldManager.NodeList[52]->GetAsAtkTextNode()->NodeText).GetText().Replace(" ", "").Replace(",", "").Replace(".", "").ParseInt();
            if(num != null && EzThrottler.Throttle("GC SetMaxVenturesExchange"))
            {
                var numeric = (AtkComponentNumericInput*)addon->UldManager.NodeList[8]->GetComponent();
                var set = Math.Min(amount, (int)(num.Value / 200));
                if(set < 1) throw new Exception($"Venture amount is too low, is {set}, expected 1 or more");
                PluginLog.Debug($"Setting {set} ventures");
                numeric->SetValue((int)set);
                return true;
            }
        }
        return false;
    }

    internal static bool? SelectExchange()
    {
        if(TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectExchange"))
        {
            var button = addon->GetButtonNodeById(17);
            if(button->IsEnabled)
            {
                (*button).ClickAddonButton(addon);
            }
            return true;
        }
        return false;
    }

    internal static bool? ConfirmExchange()
    {
        {
            var x = Utils.GetSpecificYesno(x => x.Contains("You cannot currently equip this item"));
            if(x != null && FrameThrottler.Throttle("ConfirmCannotEquip", 4))
            {
                new AddonMaster.SelectYesno((nint)x).Yes();
                return false;
            }
        }
        {
            var x = Utils.GetSpecificYesno(x => x.Contains("Exchange"));
            if(x != null && EzThrottler.Throttle("GC ConfirmExchange"))
            {
                new AddonMaster.SelectYesno((nint)x).Yes();
                return true;
            }
        }
        return false;
    }

    internal static bool? SelectGCExchangeVerticalTab(int which)
    {
        if(!which.InRange(0, 3, false)) throw new ArgumentOutOfRangeException(nameof(which));
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectGCExchangeVerticalTab"))
        {
            var button = addon->GetNodeById((uint)(37 + which))->GetAsAtkComponentRadioButton();
            (*button).ClickRadioButton(addon);
            return true;
        }
        return false;
    }

    internal static bool? SelectGCExchangeHorizontalTab(int which)
    {
        if(!which.InRange(0, 4, false)) throw new ArgumentOutOfRangeException(nameof(which));
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectGCExchangeHorizontalTab"))
        {
            var button = addon->GetNodeById((uint)(44 + which))->GetAsAtkComponentRadioButton();
            (*button).ClickRadioButton(addon);
            return true;
        }
        return false;
    }

    internal static GCInfo? GetGCInfo()
    {
        if(PlayerState.Instance()->GrandCompany == 1) return Maelstrom;
        if(PlayerState.Instance()->GrandCompany == 2) return TwinAdder;
        if(PlayerState.Instance()->GrandCompany == 3) return ImmortalFlames;
        return null;
    }

    internal static bool? InteractWithExchange()
    {
        return InteractWithDataID(GetGCInfo().Value.ExchangeDataID);
    }

    internal static bool? InteractWithShop()
    {
        return InteractWithDataID(GetGCInfo().Value.ShopDataID);
    }

    private static bool? InteractWithDataID(uint dataID)
    {
        if(Svc.Targets.Target != null)
        {
            if(Player.IsAnimationLocked) return false;
            var t = Svc.Targets.Target;
            if(t.IsTargetable && t.DataId == dataID && Vector3.Distance(Player.Object.Position, t.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("GCInteract"))
            {
                TargetSystem.Instance()->InteractWithObject(Svc.Targets.Target.Struct(), false);
                return true;
            }
        }
        else
        {
            foreach(var t in Svc.Objects)
            {
                if(t.IsTargetable && t.DataId == dataID && Vector3.Distance(Player.Object.Position, t.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("GCSetTarget"))
                {
                    Svc.Targets.Target = t;
                    return false;
                }
            }
        }
        return false;
    }

    internal static bool? WaitUntilNotOccupied()
    {
        return !IsOccupied();
    }

    internal static bool? SelectProvisioningMission()
    {
        if(TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if(EzThrottler.Throttle("SelectProvisioningMission") && Utils.TrySelectSpecificEntry("Undertake supply and provisioning missions."))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool? SelectSupplyListTab(int which)
    {
        if(!which.InRange(0, 3, false)) throw new ArgumentOutOfRangeException(nameof(which));
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectGCExpertDelivery"))
        {
            var button = addon->GetNodeById((uint)(11 + which))->GetAsAtkComponentRadioButton();
            button->ClickRadioButton(addon);
            return true;
        }
        return false;
    }

    internal static bool? EnableDeliveringIfPossible()
    {
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC EnableDeliveringIfPossible"))
        {
            if(AutoGCHandin.Overlay.DrawConditions() && AutoGCHandin.Overlay.Allowed)
            {
                AutoGCHandin.Operation = true;
                return true;
            }
        }
        return false;
    }

    public static int GetTab(this GCExchangeCategoryTab cat)
    {
        return cat switch
        {
            GCExchangeCategoryTab.Materiel => 2,
            GCExchangeCategoryTab.Weapons => 0,
            GCExchangeCategoryTab.Armor => 1,
            GCExchangeCategoryTab.Materials => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    internal static bool? CloseSupplyList()
    {
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC CloseSupplyList"))
        {
            Callback.Fire(addon, true, -1);
            return true;
        }
        return false;
    }

    internal static bool? CloseSelectString()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC CloseSelectString"))
        {
            Callback.Fire(addon, true, -1);
            return true;
        }
        return false;
    }

    internal static bool? CloseExchange()
    {
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC GrandCompanyExchange"))
        {
            Callback.Fire(addon, true, -1);
            return true;
        }
        return false;
    }

    public static uint GetAdjustedSeals()
    {
        var plan = Utils.GetGCExchangePlanWithOverrides();
        return (uint)Math.Max(0, AutoGCHandin.GetSeals() - plan.RemainingSeals.ValidateRange(0, 15000));
    }

    internal static bool? OpenSeals()
    {
        
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && AutoGCHandin.IsValidGCTerritory())
        {
            var reader = new ReaderGrandCompanyExchange(addon);
            for(var i = 0; i < reader.Items.Count; i++)
            {
                var itemInfo = reader.Items[i];
                if(itemInfo.ItemID == 21072)
                {
                    var currentRank = AutoGCHandin.GetRank();
                    if(currentRank >= itemInfo.RankReq && GetAdjustedSeals() >= itemInfo.Seals)
                    {
                        if(FrameThrottler.Throttle("GCCont.OpenItem", 20))
                        {
                            Callback.Fire(addon, true, 0, i, 1, Callback.ZeroAtkValue, currentRank >= itemInfo.RankReq, itemInfo.OpenCurrencyExchange, itemInfo.ItemID, itemInfo.IconID, itemInfo.Seals);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public static uint GetAmountThatCanBePurchased(this ItemWithQuantity item)
    {
        var meta = Utils.GetCurrentlyAvailableSharedExchangeListings().SafeSelect(item.ItemID);
        if(meta == null) return 0;
        if(AutoGCHandin.GetRank() < meta.MinPurchaseRank) return 0;
        if(GetAdjustedSeals() < meta.Seals) return 0;
        var cnt = InventoryManager.Instance()->GetInventoryItemCount(meta.ItemID);
        var targetQuantity = item.Quantity - cnt;
        if(targetQuantity <= 0) return 0;
        if(meta.ItemID == VentureItem)
        {
            var canBuy = (uint)(65000 - InventoryManager.Instance()->GetInventoryItemCount(VentureItem));
            return (uint)Math.Min(canBuy, targetQuantity);
        }
        var canFit = Utils.GetAmountThatCanFit(Utils.PlayerInvetories, meta.ItemID, false, out _);
        if(canFit == 0) return 0;
        canFit = Math.Min(canFit, (uint)targetQuantity);
        canFit = Math.Min(canFit, 99);
        canFit = Math.Min(canFit, meta.Data.StackSize);
        canFit = Math.Min(canFit, GetAdjustedSeals() / meta.Seals);
        if(meta.Data.IsUnique)
        {
            canFit = Math.Min(canFit, 1);
            if(cnt > 0) return 0;
        }
        return canFit;
    }

    public static bool PurchaseItem(this ItemWithQuantity item)
    {
        var meta = Utils.GetCurrentlyAvailableSharedExchangeListings()[item.ItemID];
        var amount = item.GetAmountThatCanBePurchased();
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && AutoGCHandin.IsValidGCTerritory())
        {
            var reader = new ReaderGrandCompanyExchange(addon);
            if(reader.RankTab != meta.Rank)
            {
                if(Utils.GenericThrottle)
                {
                    if(CleanupUI()) return false;
                    SelectGCExchangeVerticalTab((int)meta.Rank);
                }
                return false;
            }
            else
            {
                for(var i = 0; i < reader.Items.Count; i++)
                {
                    var itemInfo = reader.Items[i];
                    if(itemInfo.ItemID == meta.ItemID)
                    {
                        var canPurchase = AutoGCHandin.GetRank() >= itemInfo.RankReq;
                        var adjustedAmount = itemInfo.Stackable ? amount : 1;
                        var currentSealsCount = AutoGCHandin.GetSeals();
                        if(itemInfo.ItemID == VentureItem)
                        {
                            if(Utils.GenericThrottle && EzThrottler.Throttle("GCBuy"))
                            {
                                if(CleanupUI()) return false;
                                if(!DebugConf)
                                {
                                    Callback.Fire(addon, true, 0, i, 1, Callback.ZeroAtkValue, canPurchase, itemInfo.OpenCurrencyExchange, itemInfo.ItemID, itemInfo.IconID, itemInfo.Seals);
                                }
                                else
                                {
                                    DuoLog.Information($"Purchasing {i}'th item {itemInfo.Name} (venture)");
                                }
                                ContinuePurchase(meta, amount, currentSealsCount);
                                return true;
                            }
                        }
                        else
                        {
                            if(Utils.GenericThrottle && EzThrottler.Throttle("GCBuy"))
                            {
                                if(CleanupUI()) return false;
                                if(!DebugConf)
                                {
                                    Callback.Fire(addon, true, 0, i, adjustedAmount, Callback.ZeroAtkValue, canPurchase, itemInfo.OpenCurrencyExchange, Callback.ZeroAtkValue, Callback.ZeroAtkValue, Callback.ZeroAtkValue);
                                }
                                else
                                {
                                    DuoLog.Information($"Purchasing {i}'th item {itemInfo.Name}");
                                }
                                ContinuePurchase(meta, amount, currentSealsCount);
                                return true;
                            }
                        }
                        return false;
                    }
                }
                if(Utils.GenericThrottle)
                {
                    if(CleanupUI()) return false;
                    SelectGCExchangeHorizontalTab(meta.Category.GetTab());
                }
            }
        }
        return false;
    }

    public static void ContinuePurchase(this GCExchangeListingMetadata listing, uint itemCount, uint sealsCount)
    {
        TaskManagerConfiguration conf = new(abortOnTimeout: false, timeLimitMS: 5000);
        List<TaskManagerTask> tasks = [];
        if(listing.ItemID == VentureItem)
        {
            tasks.Add(new(() => SetVenturesExchangeAmount((int)itemCount), conf));
            tasks.Add(new(SelectExchange, conf));
        }
        tasks.Add(new(ConfirmExchange, conf));
        tasks.Add(new(() => AutoGCHandin.GetSeals() < sealsCount, conf));
        tasks.Add(new FrameDelayTask(4));
        tasks.Add(new(BeginNewPurchase));
        P.TaskManager.InsertMulti([.. tasks]);
    }

    public static void BeginNewPurchase()
    {
        var next = GetNextPurchaseListing();
        if(next != null)
        {
            P.TaskManager.Insert(next.PurchaseItem);
        }
        else
        {
            P.TaskManager.Insert(CloseExchange);
        }
    }

    public static ItemWithQuantity GetNextPurchaseListing()
    {
        List<ItemWithQuantity> items = [.. Utils.GetGCExchangePlanWithOverrides().Items];
        if((double)GetAdjustedSeals() / (double)AutoGCHandin.GetMaxSeals() > 0.5f)
        {
            items.Add(new(VentureItem, 65000));
        }
        foreach(var l in items)
        {
            var amt = l.GetAmountThatCanBePurchased();
            if(amt != 0)
            {
                return l;
            }
        }
        return null;
    }

    public static bool CleanupUI()
    {
        {
            if(TryGetAddonByName<AtkUnitBase>("SelectYesno", out var addon))
            {
                if(addon->IsReady())
                {
                    Callback.Fire(addon, true, -1);
                    return true;
                }
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon))
            {
                if(addon->IsReady())
                {
                    Callback.Fire(addon, true, -1);
                    return true;
                }
            }
        }
        return false;
    }
}
