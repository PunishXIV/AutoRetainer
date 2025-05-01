using Dalamud.Utility;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Modules.Voyage.PartSwapper;
public static unsafe class PartSwapperTasks
{
    public static void Log(string t) => VoyageUtils.Log(t);
    public static bool? SelectChangeComponents()
    {
        return Utils.TrySelectSpecificEntry(Lang.ChangeSubmersibleComponents, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectManagement", 1000));
    }

    public static bool? SelectRegisterSub()
    {
        return Utils.TrySelectSpecificEntry(Lang.RegisterSub, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.RegisterSub", 1000));
    }

    public static bool? RegisterSub()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectYesno", out var _))
        {
            Log("Found yesno, register request success");
            return true;
        }
        if(GenericHelpers.TryGetAddonMaster<AddonMaster.CompanyCraftSupply>(out var addon) && addon.IsAddonReady)
        {
            if(Utils.GenericThrottle)
            {
                if(addon.CloseButton->IsEnabled)
                {
                    Log("Registering sub");
                    addon.Close();
                }
            }
        }
        return false;
    }

    public static bool? SetupNewSub()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon))
        {
            foreach(var plans in C.LevelAndPartsData)
            {
                if(plans.MinLevel == 1)
                {
                    if(Data.OfflineSubmarineData.Count != Data.NumSubSlots)
                    {
                        PluginLog.Warning($"OfflineSubmarineData has a size of {Data.OfflineSubmarineData.Count} but expected {Data.NumSubSlots}.");
                        return false;
                    }

                    var newSubName = Data.OfflineSubmarineData[Data.NumSubSlots - 1].Name;
                    Data.AdditionalSubmarineData[newSubName].VesselBehavior = Data.NumSubSlots == 1 && plans.FirstSubDifferent ? plans.FirstSubVesselBehavior : plans.VesselBehavior;
                    Data.AdditionalSubmarineData[newSubName].UnlockMode = Data.NumSubSlots == 1 && plans.FirstSubDifferent ? plans.FirstSubUnlockMode : plans.UnlockMode;
                    Data.AdditionalSubmarineData[newSubName].SelectedUnlockPlan = Data.NumSubSlots == 1 && plans.FirstSubDifferent ? plans.FirstSubSelectedUnlockPlan : plans.SelectedUnlockPlan;

                    Data.EnabledSubs.Add(newSubName);

                    return true;
                }
            }
        }

        return false;
    }

    public static bool? ChangeComponent(int slot, uint componentId, string name = "")
    {
        var t = $"VoyageScheduler.ChangeComponent{slot}";
        if(EzThrottler.Check(t))
        {
            if(!string.IsNullOrEmpty(name) && VoyageUtils.GetSubPart(name, slot) == componentId)
                return true;

            if(TryGetAddonByName<AddonContextIconMenu>("ContextIconMenu", out var addon) && IsAddonReady(&addon->AtkUnitBase))
            {
                var availablePartAmount = addon->AtkValuesSpan[4];
                if(availablePartAmount.Type != ValueType.UInt) return false;

                for(var i = 0; i < availablePartAmount.UInt; i++)
                {
                    var partData = Svc.Data.Excel.GetSheet<Item>().GetRow(componentId);
                    if((addon->AtkValuesSpan[13 + (8 * i)].Type == ValueType.ManagedString || addon->AtkValuesSpan[13 + (8 * i)].Type == ValueType.String) && addon->AtkValuesSpan[13 + (8 * i)].String.ExtractText().ToLower() == partData.Singular.ToString())
                    {
                        Callback.Fire(&addon->AtkUnitBase, true, Utils.ZeroAtkValue, i, componentId, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                        EzThrottler.Throttle(t, 1500, true);
                        Log($"Executing ContextIconMenu change request on slot {slot} ");

                        if(string.IsNullOrEmpty(name))
                            return true;
                    }
                }

                return false;
            }

            if(TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon2) && IsAddonReady(addon2))
            {
                Callback.Fire(addon2, true, (int)2, (int)1, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                EzThrottler.Throttle(t, 1500, true);
                Log($"Executing ContextIconMenu change request on slot {slot} ");
            }
            else
            {
                Utils.RethrottleGeneric();
            }
        }

        return false;
    }

    public static bool? CloseChangeComponents()
    {
        if(TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle)
            {
                Log("Closing components window (CompanyCraftSupply)");
                Callback.Fire(addon, true, 5);
                return true;
            }
        }
        return false;
    }
}
