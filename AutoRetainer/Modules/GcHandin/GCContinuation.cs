
using Dalamud.Memory;
using ECommons.Automation.UIInput;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.GcHandin;

internal static unsafe class GCContinuation
{
    public static readonly GCInfo Maelstrom = new(1002387, 1002388, new(92.751045f, 40.27537f, 75.468185f));
    public static readonly GCInfo ImmortalFlames = new(1002390, 1002391, new(-141.44354f, 4.109951f, -106.125496f));
    public static readonly GCInfo TwinAdder = new(1002393, 1002394, new(-67.464386f, -0.5018193f, -8.161054f));

    public static void EnqueueInitiation()
    {
        EnqueueExchangeVentures();
        P.TaskManager.Enqueue(GCContinuation.WaitUntilNotOccupied);
        P.TaskManager.Enqueue(GCContinuation.InteractWithExchange);
        P.TaskManager.Enqueue(GCContinuation.SelectProvisioningMission);
        P.TaskManager.Enqueue(() => GCContinuation.SelectSupplyListTab(2), "SelectSupplyListTab(2)");
        P.TaskManager.Enqueue(GCContinuation.EnableDeliveringIfPossible);
    }

    public static async void EnqueueExchangeVentures()
    {
        if(AutoGCHandin.GetSeals() > 1000 && Utils.GetVenturesAmount() < 65000)
        {
            P.TaskManager.Enqueue(GCContinuation.WaitUntilNotOccupied);
            P.TaskManager.Enqueue(GCContinuation.InteractWithShop);
            P.TaskManager.Enqueue(() => GCContinuation.SelectGCExchangeVerticalTab(0), "SelectGCExchangeVerticalTab(0)");
            P.TaskManager.Enqueue(() => GCContinuation.SelectGCExchangeHorizontalTab(2), "SelectGCExchangeHorizontalTab(2)");
            P.TaskManager.Enqueue(GCContinuation.OpenSeals);
            P.TaskManager.Enqueue(GCContinuation.SetMaxVenturesExchange);
            P.TaskManager.Enqueue(GCContinuation.SelectExchange);
            P.TaskManager.Enqueue(GCContinuation.ConfirmExchange);
            P.TaskManager.Enqueue(GCContinuation.CloseExchange);
        }
    }

    public static void EnqueueDeliveryClose()
    {
        P.TaskManager.Enqueue(GCContinuation.CloseSupplyList);
        P.TaskManager.Enqueue(GCContinuation.CloseSelectString);
        P.TaskManager.Enqueue(GCContinuation.WaitUntilNotOccupied);
    }

    internal static bool? SetMaxVenturesExchange()
    {
        if(TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon) && IsAddonReady(addon) && TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var gca) && IsAddonReady(gca))
        {
            var num = MemoryHelper.ReadSeString(&gca->UldManager.NodeList[52]->GetAsAtkTextNode()->NodeText).ExtractText().Replace(" ", "").Replace(",", "").Replace(".", "").ParseInt();
            if(num != null && EzThrottler.Throttle("GC SetMaxVenturesExchange"))
            {
                var numeric = (AtkComponentNumericInput*)addon->UldManager.NodeList[8]->GetComponent();
                var set = Math.Min(65000 - Utils.GetVenturesAmount(), (int)(num.Value / 200));
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
        var x = Utils.GetSpecificYesno(x => x.Contains("Exchange") && x.Contains("seals for"));
        if(x != null && EzThrottler.Throttle("GC ConfirmExchange"))
        {
            new AddonMaster.SelectYesno((nint)x).Yes();
            return true;
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

    internal static bool? InteractWithExchange() => InteractWithDataID(GetGCInfo().Value.ExchangeDataID);

    internal static bool? InteractWithShop() => InteractWithDataID(GetGCInfo().Value.ShopDataID);

    private static bool? InteractWithDataID(uint dataID)
    {
        if(Svc.Targets.Target != null)
        {
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

    internal static bool? WaitUntilNotOccupied() => !IsOccupied();

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
                    if(currentRank >= itemInfo.RankReq && AutoGCHandin.GetSeals() >= itemInfo.Seals)
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

    internal static bool? SelectGCPurchaseItem(int which)
    {
        if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && AutoGCHandin.IsValidGCTerritory())
        {
            var reader = new ReaderGrandCompanyExchange(addon);
            if(which < reader.ItemCount)
            {
                for(var i = 0; i < reader.Items.Count; i++)
                {
                    var itemInfo = reader.Items[i];
                    if(itemInfo.ItemID == 21072)
                    {
                        var currentRank = AutoGCHandin.GetRank();
                        if(currentRank >= itemInfo.RankReq && AutoGCHandin.GetSeals() >= itemInfo.Seals)
                        {
                            if(FrameThrottler.Throttle("GCCont.SelectGCPurchaseItem", 20))
                            {

                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
}
