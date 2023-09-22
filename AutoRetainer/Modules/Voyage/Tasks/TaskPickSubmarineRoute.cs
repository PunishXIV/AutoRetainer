using AutoRetainer.Modules.Voyage.Readers;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using Dalamud.Utility;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static unsafe class TaskPickSubmarineRoute
    {
        internal static void Enqueue(uint map, params uint[] points)
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskPickSubmarineRoute)}, map={map}, points={points.Print()}");
            if (Svc.Data.GetExcelSheet<SubmarineMap>().GetRow(map).Name.ExtractText() == "") throw new ArgumentOutOfRangeException(nameof(map));
            if (points.Length < 1 || points.Length > 5) throw new ArgumentOutOfRangeException(nameof(points));
            P.TaskManager.Enqueue(() => PickMap(map), $"PickMap({map})");
            foreach (var point in points)
            {
                var name = C.SimpleTweaksCompat ? Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(point).Location.ToDalamudString().ExtractText().Trim() : Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(point).Destination.ToDalamudString().ExtractText().Trim();
                P.TaskManager.Enqueue(() => PickPoint(name), $"PickPoint({name})");
            }
        }

        internal static void EnqueueImmediate(uint map, params uint[] points)
        {
            VoyageUtils.Log($"Task enqueued (immediate): {nameof(TaskPickSubmarineRoute)}, map={map}, points={points.Print()}");
            if(Svc.Data.GetExcelSheet<SubmarineMap>().GetRow(map).Name.ExtractText() == "") throw new ArgumentOutOfRangeException(nameof(map));
            if (points.Length < 1 || points.Length > 5) throw new ArgumentOutOfRangeException(nameof(points));
            P.TaskManager.EnqueueImmediate(() => PickMap(map), $"PickMap({map})");
            foreach( var point in points )
            {
                var name = C.SimpleTweaksCompat? Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(point).Location.ToDalamudString().ExtractText().Trim() : Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(point).Destination.ToDalamudString().ExtractText().Trim();
                P.TaskManager.EnqueueImmediate(() => PickPoint(name), $"PickPoint({name})");
            }
        }

        internal static bool? PickMap(uint which)
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out _)) return true;
            if (TryGetAddonByName<AtkUnitBase>("SubmarineExplorationMapSelect", out var addon) && IsAddonReady(addon))
            {
                var cnt = new ReaderSubmarineExplorationMapSelect(addon).Maps.Count;
                if (which < 1 || which > cnt)
                {
                    PluginLog.Error($"Invalid map index specified (specified {which}, max {cnt})");
                    return false;
                }
                if(Utils.GenericThrottle && EzThrottler.Throttle("PickMapVoyage", 2000))
                {
                    Callback.Fire(addon, true, 2, Utils.ZeroAtkValue, which);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? PickPoint(string name)
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle)
                {
                    VoyageUtils.SelectRoutePointSafe(name);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }
    }
}
