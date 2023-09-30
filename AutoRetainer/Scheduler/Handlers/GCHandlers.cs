using ClickLib.Clicks;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using Dalamud.Memory;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Scheduler.Handlers
{
    internal unsafe static class GCHandlers
    {
        internal static bool? SetMaxVenturesExchange()
        {
            if(TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon) && IsAddonReady(addon) && TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var gca) && IsAddonReady(gca))
            {
                var num = MemoryHelper.ReadSeString(&gca->UldManager.NodeList[52]->GetAsAtkTextNode()->NodeText).ExtractText().Replace(" ", "").Replace(",", "").Replace(".", "").ParseInt();
                if (num != null && EzThrottler.Throttle("GC SetMaxVenturesExchange"))
                {
                    var numeric = (AtkComponentNumericInput*)addon->UldManager.NodeList[8]->GetComponent();
                    var set = num.Value / 200;
                    PluginLog.Debug($"Setting {set} ventures");
                    numeric->SetValue(set);
                    return true;
                }
            }
            return false;
        }

        internal static bool? SelectExchange()
        {
            if (TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectExchange"))
            {
                Callback.Fire(addon, true, (int)0, (int)1);
                return true;
            }
            return false;
        }

        internal static bool? ConfirmExchange()
        {
            var x = Utils.GetSpecificYesno(x => x.Contains("Exchange") && x.Contains("seals for"));
            if(x != null && EzThrottler.Throttle("GC ConfirmExchange"))
            {
                ClickSelectYesNo.Using((nint)x).Yes();
                return true;
            }
            return false;
        }

        internal static bool? SelectGCExchangeVerticalTab()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectGCExchangeVerticalTab"))
            {
                var values = stackalloc AtkValue[]
            {
                    new() { Type = ValueType.Int, Int = 1 },
                    new() { Type = ValueType.Int, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                };
                Callback.FireRaw(addon, 9, values, 1);
                return true;
            }
            return false;
        }

        internal static bool? SelectGCExchangeHorizontalTab()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectGCExchangeHorizontalTab"))
            {
                var values = stackalloc AtkValue[]
            {
                    new() { Type = ValueType.Int, Int = 2 },
                    new() { Type = ValueType.Int, Int = 1 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = 0, Int = 0 },
                };
                Callback.FireRaw(addon, 9, values, 1);
                return true;
            }
            return false;
        }

        internal static bool? TargetShop()
        {
            foreach (var x in Svc.Objects)
            {
                if (x.Name.ToString().EqualsAny("Flame Quartermaster") && x.ObjectKind == ObjectKind.EventNpc && x.IsTargetable() && Vector3.Distance(Player.Object.Position, x.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("GCSetTarget"))
                {
                    if (Svc.Targets.Target?.Address != x.Address)
                    {
                        Svc.Targets.SetTarget(x);
                    }
                    return true;
                }
            }
            return false;
        }

        internal static bool? InteractWithShop()
        {
            var x = Svc.Targets.Target;
            if (x == null) return false;
            if (x.Name.ToString().EqualsAny("Flame Quartermaster") && x.ObjectKind == ObjectKind.EventNpc && x.IsTargetable() && Vector3.Distance(Player.Object.Position, x.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("GCInteractWithShop"))
            {
                TargetSystem.Instance()->InteractWithObject(x.Struct(), false);
                return true;
            }
            return false;
        }

        internal static bool? TargetExchange()
        {
            foreach (var x in Svc.Objects)
            {
                if (x.Name.ToString().EqualsAny("Flame Personnel Officer") && x.ObjectKind == ObjectKind.EventNpc && x.IsTargetable() && Vector3.Distance(Player.Object.Position, x.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("GCSetTarget"))
                {
                    if (Svc.Targets.Target?.Address != x.Address)
                    {
                        Svc.Targets.SetTarget(x);
                    }
                    return true;
                }
            }
            return false;
        }

        internal static bool? InteractExchange()
        {
            var x = Svc.Targets.Target;
            if (x == null) return false;
            if (x.Name.ToString().EqualsAny("Flame Personnel Officer") && x.ObjectKind == ObjectKind.EventNpc && x.IsTargetable() && Vector3.Distance(Player.Object.Position, x.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("GCInteractWithExchange"))
            {
                TargetSystem.Instance()->InteractWithObject(x.Struct(), false);
                return true;
            }
            return false;
        }

        internal static bool? WaitUntilNotOccupied => !IsOccupied();

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

        internal static bool? SelectGCExpertDelivery()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC SelectGCExpertDelivery"))
            {
                var val = stackalloc AtkValue[]
                {
                    new AtkValue() { Type = ValueType.Int, Int = 0 },
                    new AtkValue() { Type = ValueType.Int, Int = 2 },
                    new AtkValue() { Type = 0, Int = 0 },
                };
                Callback.FireRaw(addon, 3, val, 1);
                return true;
            }
            return false;
        }

        internal static bool? EnableDeliveringIfPossible()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC EnableDeliveringIfPossible"))
            {
                if (AutoGCHandin.Overlay.DrawConditions() && AutoGCHandin.Overlay.Allowed)
                {
                    AutoGCHandin.Operation = true;
                    return true;
                }
            }
            return false;
        }

        internal static bool? CloseSupplyList()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC CloseSupplyList"))
            {
                Callback.Fire(addon, true, (int)-1);
                return true;
            }
            return false;
        }

        internal static bool? CloseSelectString()
        {
            if (TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC CloseSelectString"))
            {
                Callback.Fire(addon, true, (int)-1);
                return true;
            }
            return false;
        }

        internal static bool? CloseExchange()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC GrandCompanyExchange"))
            {
                Callback.Fire(addon, true, (int)-1);
                return true;
            }
            return false;
        }

        internal static bool? OpenCurrency()
        {
            if (TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon) && EzThrottler.Throttle("GC GrandCompanyExchange") && EzThrottler.Throttle("GC OpenCurrency", 3000))
            {
                var values = stackalloc AtkValue[]
            {
                    new() { Type = ValueType.Int, Int = 0 },
                    new() { Type = ValueType.Int, Int = 0 },
                    new() { Type = ValueType.Int, Int = 1 },
                    new() { Type = 0, Int = 0 },
                    new() { Type = ValueType.Bool, Byte = 1 },
                    new() { Type = ValueType.Bool, Byte = 1 },
                    new() { Type = ValueType.UInt, UInt = 21072 },
                    new() { Type = ValueType.UInt, UInt = 60179 },
                    new() { Type = ValueType.UInt, UInt = 200 },
                };
                Callback.FireRaw(addon, 9, values, 1);
                return true;
            }
            return false;
        }
    }
}
