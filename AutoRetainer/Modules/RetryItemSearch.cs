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
                        if(t.EqualsAny(Utils.GetAddonText(1997), Utils.GetAddonText(1998)))
                        {
                            if (!P.TaskManager.IsBusy && EzThrottler.Throttle("RetrySearch", 1000))
                            {
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
        }
    }
}
