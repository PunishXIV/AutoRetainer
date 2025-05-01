using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Utility;
using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Modules.Voyage;

internal static unsafe class VoyageScheduler
{
    internal static void Log(string t) => VoyageUtils.Log(t);
    internal static bool Enabled = false;
    internal static bool? SelectQuitVesselMenu() => Utils.TrySelectSpecificEntry(Lang.VoyageQuitEntry);

    internal static bool? CloseRepair()
    {
        if(TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle)
            {
                Log("Closing repair window (CompanyCraftSupply)");
                Callback.Fire(addon, true, 5);
                return true;
            }
        }
        else if(TryGetAddonByName<AtkUnitBase>("AirShipPartsMenu", out var addon2) && IsAddonReady(addon2))
        {
            if(Utils.GenericThrottle)
            {
                Log("Closing repair window (AirShipPartsMenu)");
                Callback.Fire(addon2, true, 5);
                return true;
            }
        }
        return false;
    }

    internal static bool? TryRepair(int slot)
    {
        if(TaskRepairAll.Abort) return true;
        var t = $"VoyageScheduler.TryRepair{slot}";
        if(TryGetAddonByName<AtkUnitBase>("SelectYesno", out var _))
        {
            Log("Found yesno, repair request success");
            return true;
        }
        if(EzThrottler.Check(t))
        {
            if(TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon) && IsAddonReady(addon))
            {
                if(Utils.GenericThrottle)
                {
                    Callback.Fire(addon, true, (int)3, Utils.ZeroAtkValue, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                    EzThrottler.Throttle(t, 1000, true);
                    Log($"Executing CompanyCraftSupply repair request on slot {slot} ");
                    return false;
                }
            }
            else if(TryGetAddonByName<AtkUnitBase>("AirShipPartsMenu", out var addon2) && IsAddonReady(addon2))
            {
                if(Utils.GenericThrottle)
                {
                    Callback.Fire(addon2, true, (int)3, Utils.ZeroAtkValue, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                    EzThrottler.Throttle(t, 1000, true);
                    Log($"Executing AirShipPartsMenu repair request on slot {slot} ");
                    return false;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
        }
        return false;
    }

    internal static bool? WaitForYesNoDisappear()
    {
        return !TryGetAddonByName<AtkUnitBase>("SelectYesno", out _);
    }

    internal static bool? WaitForCutscene()
    {
        return Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || Svc.Condition[ConditionFlag.WatchingCutscene78];
    }

    internal static bool? WaitForNoCutscene()
    {
        return !(Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || Svc.Condition[ConditionFlag.WatchingCutscene78]);
    }


    internal static bool? Lockon()
    {
        if(VoyageUtils.TryGetNearestVoyagePanel(out var obj))
        {
            if(Svc.Targets.Target?.Address != obj.Address)
            {
                if(Utils.GenericThrottle)
                {
                    Log("Targeting workshop CP");
                    Svc.Targets.Target = obj;
                }
            }
            else
            {
                if(Utils.GenericThrottle)
                {
                    Log("Locking on workshop CP");
                    Chat.Instance.ExecuteCommand("/lockon");
                    return true;
                }
            }
        }
        return false;
    }

    internal static bool? Approach()
    {
        if(VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Svc.Targets.Target?.Address == obj.Address)
        {
            if(Utils.GenericThrottle)
            {
                Chat.Instance.ExecuteCommand("/automove on");
                Utils.RegenerateRandom();
                return true;
            }
        }
        return false;
    }

    internal static bool? AutomoveOffPanel()
    {
        if(VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Svc.Targets.Target?.Address == obj.Address)
        {
            if(Vector3.Distance(obj.Position, Player.Object.Position) < 4f + Utils.Random * 0.25f)
            {
                if(Utils.GenericThrottle)
                {
                    Chat.Instance.ExecuteCommand("/automove off");
                    return true;
                }
            }
        }
        return false;
    }

    internal static bool? InteractWithVoyagePanel()
    {
        if(VoyageUtils.TryGetNearestVoyagePanel(out var obj))
        {
            if(Svc.Targets.Target?.Address == obj.Address)
            {
                if(Player.IsAnimationLocked) return false;
                if(Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Interact", 2000))
                {
                    Log("Interacting with workshop CP");
                    TargetSystem.Instance()->InteractWithObject(obj.Struct(), false);
                    return true;
                }
            }
            else
            {
                if(obj.IsTargetable && Utils.GenericThrottle)
                {
                    Svc.Targets.Target = obj;
                }
            }
        }
        return false;
    }

    internal static bool? SelectAirshipManagement()
    {
        return Utils.TrySelectSpecificEntry(Lang.AirshipManagement, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectManagement", 1000));
    }

    internal static bool? SelectSubManagement()
    {
        return Utils.TrySelectSpecificEntry(Lang.SubmarineManagement, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectManagement", 1000));
    }

    internal static bool? SelectExitMainPanel()
    {
        return Utils.TrySelectSpecificEntry(Lang.CancelVoyage, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.ExitMainPanel", 1000));
    }

    internal static bool? SelectChangeComponents()
    {
        return Utils.TrySelectSpecificEntry(Lang.ChangeSubmersibleComponents, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectManagement", 1000));
    }

    internal static bool? SelectRegisterSub()
    {
        return Utils.TrySelectSpecificEntry(Lang.RegisterSub, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.RegisterSub", 1000));
    }

    internal static bool? RegisterSub()
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

    internal static bool? SetupNewSub()
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

    internal static bool? ChangeComponent(int slot, uint componentId, string name = "")
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

    internal static bool? CloseChangeComponents()
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

    internal static bool? SelectVesselByName(string name, VoyageType type)
    {
        var index = VoyageUtils.GetVesselIndex(name, type);
        if(index != null)
        {
            if(TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
            {
                var entries = Utils.GetEntries(addon);
                if(index.Value < entries.Count && entries[index.Value].Contains(name))
                {
                    if(index >= 0 && Utils.GenericThrottle && EzThrottler.Throttle("SelectVesselByName"))
                    {
                        DebugLog($"Selecting vessel {name}/{type}/{entries[index.Value]}/{index}");
                        new AddonMaster.SelectString(addon).Entries[index.Value].Select();
                        return true;
                    }
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
        }
        return false;
    }


    internal static bool? SelectViewPreviousLog()
    {
        return Utils.TrySelectSpecificEntry(Lang.ViewPrevVoyageLog, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectViewPreviousLog", 1000));
    }

    internal static bool WaitUntilFinalizeDeployAddonExists()
    {
        return TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon);
    }

    internal static bool? RedeployVessel()
    {
        if(!TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out _)) return true;
        if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out _)) return true;
        if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
        {
            var button = addon->UldManager.NodeList[3]->GetAsAtkComponentButton();
            if(!button->IsEnabled)
            {
                EzThrottler.Throttle("Voyage.Redeploy", 500, true);
                return false;
            }
            else
            {
                if(Utils.GenericThrottle && EzThrottler.Throttle($"Voyage.Redeploy_{(nint)addon}", 5000))
                {
                    Callback.Fire(addon, true, 1);
                    return false;
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? FinalizeVessel()
    {
        if(!TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out _)) return true;
        if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle($"Voyage.Redeploy_{(nint)addon}", 1000))
            {
                Callback.Fire(addon, true, 0);
                return false;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? DeployVessel()
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Deploy"))
            {
                Callback.Fire(addon, true, 0);
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? CancelDeployVessel()
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("Voyage.CancelDeploy"))
            {
                Callback.Fire(addon, true, -1);
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? SelectQuitVesselSelectorMenu()
    {
        return Utils.TrySelectSpecificEntry(Lang.NothingVoyage, () => EzThrottler.Throttle("Voyage.Quit", 1000));
    }

}
