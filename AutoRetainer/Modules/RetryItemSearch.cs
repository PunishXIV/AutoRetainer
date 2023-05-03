using Dalamud.Memory;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal unsafe static class RetryItemSearch
    {
        internal static void Tick()
        {
            if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedSummoningBell])
            {
                if(TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out var addon) && IsAddonReady(addon))
                {
                    if (addon->UldManager.NodeList[25]->IsVisible)
                    {
                        var t = MemoryHelper.ReadSeString(&addon->UldManager.NodeList[25]->GetAsAtkTextNode()->NodeText).ExtractText();
                        if (t.EqualsAny(Utils.GetAddonText(1997), Utils.GetAddonText(1998)))
                        {
                            if (!P.TaskManager.IsBusy && EzThrottler.Throttle("RetrySearch", 1000))
                            {
                                P.DebugLog("Enqueueing ItemSearchResult retry (retainer)");
                                P.TaskManager.Enqueue(delegate
                                {
                                    if (TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out var a))
                                    {
                                        Utils.Callback(a, true, (int)-1);
                                    }
                                });
                                P.TaskManager.Enqueue(() => !TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out _));
                                P.TaskManager.Enqueue(delegate
                                {
                                    if (TryGetAddonByName<AtkUnitBase>("RetainerSell", out var a))
                                    {
                                        Utils.Callback(a, true, (int)4);
                                    }
                                });
                            }
                        }
                    }
                }
            }



            if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedInEvent] 
                && Svc.Targets.Target?.Name.ToString().EqualsIgnoreCaseAny(Utils.GetEObjNames(2000073, 2000402, 2000440, 2000442, 2010285)) == true)
            {
                if (!P.Memory.FireCallbackHook.IsEnabled)
                {
                    P.Memory.FireCallbackHook.Enable();
                }
                if (TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out var addon) && IsAddonReady(addon))
                {
                    if (addon->UldManager.NodeList[25]->IsVisible)
                    {
                        var t = MemoryHelper.ReadSeString(&addon->UldManager.NodeList[25]->GetAsAtkTextNode()->NodeText).ExtractText();
                        if (t.EqualsAny(Utils.GetAddonText(1997), Utils.GetAddonText(1998)))
                        {
                            if (!P.TaskManager.IsBusy && P.Memory.LastSearchItem != -1 && EzThrottler.Throttle("RetrySearch", 1500))
                            {
                                P.DebugLog("Enqueueing ItemSearchResult retry (marketboard)");
                                P.TaskManager.Enqueue(delegate
                                {
                                    if (TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out var a))
                                    {
                                        Utils.Callback(a, true, (int)-1);
                                    }
                                });
                                P.TaskManager.Enqueue(() => !TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out _));
                                P.TaskManager.Enqueue(delegate
                                {
                                    if (TryGetAddonByName<AtkUnitBase>("ItemSearch", out var a))
                                    {
                                        Utils.Callback(a, true, (int)5, (int)P.Memory.LastSearchItem);
                                    }
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                P.Memory.LastSearchItem = -1;
                if (P.Memory.FireCallbackHook.IsEnabled)
                {
                    P.Memory.FireCallbackHook.Disable();
                }
            }
        }
    }
}
