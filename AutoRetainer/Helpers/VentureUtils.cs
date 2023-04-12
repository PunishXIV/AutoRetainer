using AutoRetainer.Modules.Statistics;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AutoRetainer.Helpers
{
    internal static unsafe class VentureUtils
    {
        internal const uint QuickExplorationID = 395;

        private static bool IsNullOrEmpty(this string s) => GenericHelpers.IsNullOrEmpty(s);

        internal static void ProcessVenturePlanner(this SeRetainer ret, uint next)
        {
            P.DebugLog($"Not completed or restarting");
            if (ret.VentureID != 0)
            {
                P.DebugLog($"Venture id is not zero, next={next}, ventureID={ret.VentureID}");
                if (next == ret.VentureID)
                {
                    P.DebugLog($"Reassigning");
                    TaskReassignVenture.Enqueue();
                }
                else
                {
                    P.DebugLog($"Collecting");
                    TaskCollectVenture.Enqueue();
                    if (VentureUtils.GetVentureById(next).IsFieldExploration())
                    {
                        P.DebugLog($"Assigning field exploration: {next}");
                        TaskAssignFieldExploration.Enqueue(next);
                    }
                    else if (VentureUtils.GetVentureById(next).IsQuickExploration())
                    {
                        P.DebugLog($"Assigning quick: {next}");
                        TaskAssignQuickVenture.Enqueue();
                    }
                    else
                    {
                        P.DebugLog($"Assigning hunt: {next}");
                        TaskAssignHuntingVenture.Enqueue(next);
                    }
                }
            }
            else
            {
                P.DebugLog($"Venture not assigned");
                if (VentureUtils.GetVentureById(next).IsFieldExploration())
                {
                    P.DebugLog($"Assigning field exploration: {next}");
                    TaskAssignFieldExploration.Enqueue(next);
                }
                else if (VentureUtils.GetVentureById(next).IsQuickExploration())
                {
                    P.DebugLog($"Assigning quick: {next}");
                    TaskAssignQuickVenture.Enqueue();
                }
                else
                {
                    P.DebugLog($"Assigning hunt: {next}");
                    TaskAssignHuntingVenture.Enqueue(next);
                }
            }
        }

        internal static int GetCategory(uint ClassJob)
        {
            if (ClassJob == (int)Job.BTN) return 18;
            if (ClassJob == (int)Job.MIN) return 17;
            if (ClassJob == (int)Job.FSH) return 19;
            return 34;
        }

        internal static bool IsDoL(uint ClassJob)
        {
            if (ClassJob == (int)Job.BTN) return true;
            if (ClassJob == (int)Job.MIN) return true;
            if (ClassJob == (int)Job.FSH) return true;
            return false;
        }

        internal static uint GetGatheringItemByItemID(uint itemID)
        {
            return Svc.Data.GetExcelSheet<GatheringItem>().FirstOrDefault(x => x.Item == itemID)?.RowId ?? 0;
        }

        internal static RetainerTask GetVentureById(uint id)
        {
            return Svc.Data.GetExcelSheet<RetainerTask>().GetRow(id);
        }

        internal static IEnumerable<RetainerTask> GetFieldExplorations(uint ClassJob, int level)
        {
            var cat = GetCategory(ClassJob);
            return Svc.Data.GetExcelSheet<RetainerTask>().Where(x => x.ClassJobCategory.Value.RowId == cat).Where(x => x.MaxTimemin == 1080 && x.RetainerLevel <= level && !x.GetVentureName().IsNullOrEmpty()).OrderBy(x => x.RetainerLevel);
        }

        internal static IEnumerable<RetainerTask> GetHunts(uint ClassJob, int level)
        {
            var cat = GetCategory(ClassJob);
            return Svc.Data.GetExcelSheet<RetainerTask>().Where(x => x.ClassJobCategory.Value.RowId == cat).Where(x => x.MaxTimemin == 60 && x.RetainerLevel <= level && !x.GetVentureName().IsNullOrEmpty()).OrderBy(x => x.RetainerLevel);
        }

        internal static RetainerTask QuickExploration => Svc.Data.GetExcelSheet<RetainerTask>().GetRow(QuickExplorationID);

        internal static bool IsFieldExploration(this RetainerTask task) => task.MaxTimemin == 1080;

        internal static bool IsQuickExploration(this RetainerTask task) => task.RowId == QuickExplorationID;

        internal static IEnumerable<RetainerTask> GetAvailableVentures(this IEnumerable<RetainerTask> tasks, OfflineRetainerData data)
        {
            return tasks.Where(x => x.RetainerLevel <= data.Level);
        }

        internal static string GetVentureName(uint id) => GetVentureName(Svc.Data.GetExcelSheet<RetainerTask>().GetRow(id));

        internal static string GetVentureName(this RetainerTask Task)
        {
            if (Task == null) return null;
            if (Task.IsRandom)
            {
                return $"{Svc.Data.GetExcelSheet<RetainerTaskRandom>().GetRow(Task.Task)?.Name.ToDalamudString().ExtractText()}";
            }
            else
            {
                return $"{Svc.Data.GetExcelSheet<RetainerTaskNormal>().GetRow(Task.Task)?.Item.Value?.Name.ToDalamudString().ExtractText()}";
            }
        }

        internal static uint GetVentureItemId(this RetainerTask Task)
        {
            return Svc.Data.GetExcelSheet<RetainerTaskNormal>().GetRow(Task.Task)?.Item.Value.RowId ?? 0;
        }

        internal static List<string> GetAvailableVentureNames()
        {
            List<string> ret = new();
            var data = CSFramework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.GetStringArrayData(95);
            if (data != null)
            {
                for (int i = 0; i < data->AtkArrayData.Size; i++)
                {
                    if (data->StringArray[i] == null) break;
                    if (i % 4 != 1) continue;
                    var item = data->StringArray[i];
                    if (item != null)
                    {
                        var str = MemoryHelper.ReadSeStringNullTerminated((nint)item);
                        ret.Add(str.ExtractText());
                    }
                }
            }
            return ret;
        }

        internal static string[] GetVentureLevelCategory(uint id)
        {
            return Svc.Data.GetExcelSheet<RetainerTask>().GetRow(id).GetVentureLevelCategory();
        }

        internal static string[] GetVentureLevelCategory(this RetainerTask Task)
        {
            foreach(var x in Svc.Data.GetExcelSheet<RetainerTaskLvRange>())
            {
                if(Task.RetainerLevel >= x.Min && Task.RetainerLevel <= x.Max)
                {
                    return new string[] { $" {x.Min}-{x.Max}.", $"  {x.Min}～{x.Max}", $" {x.Min} - {x.Max}", $" {x.Min} à {x.Max}" };
                }
            }
            return null;
        }
    }
}
