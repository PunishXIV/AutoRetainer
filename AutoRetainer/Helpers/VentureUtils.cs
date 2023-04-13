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

        internal static string GetFancyVentureName(uint Task, OfflineCharacterData data, OfflineRetainerData retainer, out bool Available)
        {
            return GetVentureById(Task).GetFancyVentureName(data, retainer, out Available);
        }

        internal static string GetFancyVentureName(this RetainerTask Task, OfflineCharacterData data, OfflineRetainerData retainer, out bool Available)
        {
            var adata = Utils.GetAdditionalData(data.CID, retainer.Name);
            var UnavailabilitySymbol = "";
            var canNotGather = Task.RequiredGathering > 0 && adata.Gathering < Task.RequiredGathering && adata.Gathering > 0;
            if (!Task.IsFieldExploration() && IsDoL(Task.ClassJobCategory.Row))
            {
                var gathered = data.UnlockedGatheringItems.Count == 0 || data.UnlockedGatheringItems.Contains(VentureUtils.GetGatheringItemByItemID(Task.GetVentureItemId()));
                if (gathered)
                {
                    if (canNotGather)
                    {
                        Available = false;
                        UnavailabilitySymbol = Consts.CharPlant;
                    }
                    else
                    {
                        Available = true;
                    }
                }
                else
                {
                    Available = false;
                    if(canNotGather)
                    {
                        UnavailabilitySymbol = Consts.CharQuestion + Consts.CharPlant;
                    }
                    else
                    {
                        UnavailabilitySymbol = Consts.CharQuestion;
                    }
                }
            }
            else
            {
                //PluginLog.Information($"{Task.GetVentureName()}, {Task.RequiredItemLevel} > {adata.Ilvl}, {Task.RequiredGathering} > {adata.Gathering}");
                if (Task.RequiredItemLevel > 0 && adata.Ilvl > 0)
                {
                    Available = Task.RequiredItemLevel <= adata.Ilvl;
                    UnavailabilitySymbol = Consts.CharItemLevel;
                }
                else if(Task.RequiredGathering > 0 && adata.Gathering > 0)
                {
                    Available = !canNotGather;
                    UnavailabilitySymbol = Consts.CharPlant;
                }
                else
                {
                    Available = true;
                }
            }
            string ret = "";
            if(Task.RetainerLevel == 0)
            {
                ret = $"{Task.GetVentureName()}";
            }
            else
            {
                ret = $"{Task.RetainerLevel} {Task.GetVentureName()}";
            }
            if (!Available) ret = $"{UnavailabilitySymbol}{ret}";
            return ret;
        }

        internal static int GetCategory(uint ClassJob)
        {
            if (ClassJob == (int)Job.BTN) return 18;
            if (ClassJob == (int)Job.MIN) return 17;
            if (ClassJob == (int)Job.FSH) return 19;
            return 34;
        }

        internal static string GetHuntingVentureName(uint ClassJob)
        {
            if (ClassJob == (int)Job.BTN) return Consts.HuntingVentureNames[2][..^1];
            if (ClassJob == (int)Job.MIN) return Consts.HuntingVentureNames[1][..^1];
            if (ClassJob == (int)Job.FSH) return Consts.HuntingVentureNames[3][..^1];
            return Consts.HuntingVentureNames[0][..^1];
        }

        internal static string GetFieldExVentureName(uint ClassJob)
        {
            if (ClassJob == (int)Job.BTN) return Consts.FieldExplorationNames[2][..^1];
            if (ClassJob == (int)Job.MIN) return Consts.FieldExplorationNames[1][..^1];
            if (ClassJob == (int)Job.FSH) return Consts.FieldExplorationNames[3][..^1];
            return Consts.FieldExplorationNames[0][..^1];
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
