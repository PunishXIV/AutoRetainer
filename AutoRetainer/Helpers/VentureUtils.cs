using AutoRetainer.Modules.Statistics;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI.Configuration;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Helpers
{
    internal static unsafe class VentureUtils
    {
        internal const uint QuickExplorationID = 395;

        private static bool IsNullOrEmpty(this string s) => GenericHelpers.IsNullOrEmpty(s);

        internal static void ProcessVenturePlanner(this SeRetainer ret, uint next)
        {
            DebugLog($"Not completed or restarting");
            if (ret.VentureID != 0)
            {
                DebugLog($"Venture id is not zero, next={next}, ventureID={ret.VentureID}");
                if (next == ret.VentureID)
                {
                    DebugLog($"Reassigning");
                    TaskReassignVenture.Enqueue();
                }
                else
                {
                    DebugLog($"Collecting");
                    TaskCollectVenture.Enqueue();
                    if (VentureUtils.GetVentureById(next).IsFieldExploration())
                    {
                        DebugLog($"Assigning field exploration: {next}");
                        TaskAssignFieldExploration.Enqueue(next);
                    }
                    else if (VentureUtils.GetVentureById(next).IsQuickExploration())
                    {
                        DebugLog($"Assigning quick: {next}");
                        TaskAssignQuickVenture.Enqueue();
                    }
                    else
                    {
                        DebugLog($"Assigning hunt: {next}");
                        TaskAssignHuntingVenture.Enqueue(next);
                    }
                }
            }
            else
            {
                DebugLog($"Venture not assigned");
                if (VentureUtils.GetVentureById(next).IsFieldExploration())
                {
                    DebugLog($"Assigning field exploration: {next}");
                    TaskAssignFieldExploration.Enqueue(next);
                }
                else if (VentureUtils.GetVentureById(next).IsQuickExploration())
                {
                    DebugLog($"Assigning quick: {next}");
                    TaskAssignQuickVenture.Enqueue();
                }
                else
                {
                    DebugLog($"Assigning hunt: {next}");
                    TaskAssignHuntingVenture.Enqueue(next);
                }
            }
        }

        internal static int GetVentureItemAmount(uint Task, OfflineCharacterData data, OfflineRetainerData retainer, out int index)
        {
            return GetVentureById(Task).GetVentureItemAmount(data, retainer, out index);
        }

        internal static int GetVentureItemAmount(this RetainerTask task, OfflineCharacterData data, OfflineRetainerData retainer, out int index)
        {
            index = 0;
            if (task.IsRandom)
            {
                return 0;
            }
            var adata = Utils.GetAdditionalData(data.CID, retainer.Name);

            var param = task.RetainerTaskParameter.Value;
            if (param == null) return 0;
            var normal = Svc.Data.GetExcelSheet<RetainerTaskNormal>().GetRow(task.Task);
            if (task.Task == 0 || normal == null) return 0;
            if (retainer.Job == (uint)Job.FSH)
            {
                for (int i = 0; i < param.PerceptionFSH.Length; i++)
                {
                    if(adata.Perception >= param.PerceptionFSH[i])
                    {
                        index = i+1;
                    }
                }
            }
            else if (IsDoL(retainer.Job))
            {
                for (int i = 0; i < param.PerceptionDoL.Length; i++)
                {
                    if (adata.Perception >= param.PerceptionDoL[i])
                    {
                        index = i+1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < param.ItemLevelDoW.Length; i++)
                {
                    if (adata.Ilvl >= param.ItemLevelDoW[i])
                    {
                        index = i+1;
                    }
                }
            }
            if (index >= normal.Quantity.Length) return 0;
            return normal.Quantity[index];
        }

        internal static int GetVentureRequitement(this RetainerTask task)
        {
            if (IsDoL(task.ClassJobCategory.Row))
            {
                return task.RequiredGathering;
            }
            else
            {
                return task.RequiredItemLevel;
            }
        }

        internal static (int[] Stat, int[] Amount) GetVentureAmounts(this RetainerTask task, OfflineRetainerData retainer)
        {
            var param = task.RetainerTaskParameter.Value;
            var normal = Svc.Data.GetExcelSheet<RetainerTaskNormal>().GetRow(task.Task);
            List<int> stat = new()
            {
                0
            };
            List<int> amount = new()
            {
                normal.Quantity[0]
            };
            if (retainer.Job == (uint)Job.FSH)
            {
                for (int i = 0; i < param.PerceptionFSH.Length; i++)
                {
                    amount.Add(normal.Quantity[i+1]);
                    stat.Add(param.PerceptionFSH[i]);
                }
            }
            else if (IsDoL(retainer.Job))
            {
                for (int i = 0; i < param.PerceptionDoL.Length; i++)
                {
                    amount.Add(normal.Quantity[i+1]);
                    stat.Add(param.PerceptionDoL[i]);
                }
            }
            else
            {
                for (int i = 0; i < param.ItemLevelDoW.Length; i++)
                {
                    amount.Add(normal.Quantity[i+1]);
                    stat.Add(param.ItemLevelDoW[i]);
                }
            }
            return (stat.ToArray(), amount.ToArray());
        }

        internal static string GetFancyVentureName(uint Task, OfflineCharacterData data, OfflineRetainerData retainer, out bool Available)
        {
            return GetVentureById(Task).GetFancyVentureName(data, retainer, out Available);
        }

        internal static string GetFancyVentureName(this RetainerTask Task, OfflineCharacterData data, OfflineRetainerData retainer, out bool Available) => GetFancyVentureName(Task, data, retainer, out Available, out _, out _);

        internal static string GetFancyVentureName(this RetainerTask Task, OfflineCharacterData data, OfflineRetainerData retainer, out bool Available, out string left, out string right)
        {
            var r = Task.GetFancyVentureNameParts(data, retainer, out Available);
            left = Available ? "" : Lang.CharDeny + r.UnavailabilitySymbols + " ";
            var lvls = r.Level == 0 ? "" : $"{Lang.CharLevel}{r.Level} ";
            right = r.Yield == 0 ? "" : $"x{r.Yield} {r.YieldStars}";
            left = $"{left}{lvls}{r.Name}";
            return (C.Verbose ? $"#{Task.RowId}/{Task.ClassJobCategory.Value.Name} " : "") + left + " " + right;
        }

        internal static (string UnavailabilitySymbols, int Level, string Name, int Yield, int YieldRate, string YieldStars) GetFancyVentureNameParts(this RetainerTask Task, OfflineCharacterData data, OfflineRetainerData retainer, out bool Available)
        {
            (string UnavailabilitySymbols, int Level, string Name, int Yield, int YieldRate, string YieldStars) retp = ("", 0, "", 0, 0, "");
            var adata = Utils.GetAdditionalData(data.CID, retainer.Name);
            var UnavailabilitySymbol = "";
            var canNotGather = Task.RequiredGathering > 0 && adata.Gathering < Task.RequiredGathering && adata.Gathering > -1;
            if (!Task.IsFieldExploration() && IsDoL(Task.ClassJobCategory.Row))
            {
                var gathered = data.UnlockedGatheringItems.Count == 0 || data.UnlockedGatheringItems.Contains(VentureUtils.GetGatheringItemByItemID(Task.GetVentureItemId()));
                if (gathered)
                {
                    if (canNotGather)
                    {
                        Available = false;
                        UnavailabilitySymbol = Lang.CharPlant;
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
                        UnavailabilitySymbol = Lang.CharQuestion + Lang.CharPlant;
                    }
                    else
                    {
                        UnavailabilitySymbol = Lang.CharQuestion;
                    }
                }
            }
            else
            {
                //PluginLog.Information($"{Task.GetVentureName()}, {Task.RequiredItemLevel} > {adata.Ilvl}, {Task.RequiredGathering} > {adata.Gathering}");
                if (Task.RequiredItemLevel > 0 && adata.Ilvl > -1)
                {
                    Available = Task.RequiredItemLevel <= adata.Ilvl;
                    if(!Available) UnavailabilitySymbol = Lang.CharItemLevel;
                }
                else if(Task.RequiredGathering > 0 && adata.Gathering > -1)
                {
                    Available = !canNotGather;
                    if (!Available) UnavailabilitySymbol = Lang.CharPlant;
                }
                else
                {
                    Available = true;
                }
            }
            retp.Name = Task.GetVentureName();
            if (Task.RetainerLevel == 0)
            {
                //
            }
            else
            {
                retp.Level = Task.RetainerLevel;
            }
            if(retainer.Level < Task.RetainerLevel)
            {
                Available = false;
                UnavailabilitySymbol += Lang.CharLevelSync;
            }
            if (!Available)
            {
                retp.UnavailabilitySymbols = UnavailabilitySymbol;

            }
            if (!Task.IsRandom)
            {
                var amount = Task.GetVentureItemAmount(data, retainer, out retp.YieldRate);
                retp.Yield = amount; 
                retp.YieldStars = $"{"★".Repeat(retp.YieldRate)}{"☆".Repeat(4 - retp.YieldRate)}";
            }
            return retp;
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
            if (ClassJob == (int)Job.BTN) return Lang.HuntingVentureNames[2][..^1];
            if (ClassJob == (int)Job.MIN) return Lang.HuntingVentureNames[1][..^1];
            if (ClassJob == (int)Job.FSH) return Lang.HuntingVentureNames[3][..^1];
            return Lang.HuntingVentureNames[0][..^1];
        }

        internal static string GetFieldExVentureName(uint ClassJob)
        {
            if (ClassJob == (int)Job.BTN) return Lang.FieldExplorationNames[2][..^1];
            if (ClassJob == (int)Job.MIN) return Lang.FieldExplorationNames[1][..^1];
            if (ClassJob == (int)Job.FSH) return Lang.FieldExplorationNames[3][..^1];
            return Lang.FieldExplorationNames[0][..^1];
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

        internal static IEnumerable<RetainerTask> GetFieldExplorations(uint ClassJob)
        {
            var cat = GetCategory(ClassJob);
            return Svc.Data.GetExcelSheet<RetainerTask>().Where(x => x.ClassJobCategory.Value.RowId == cat).Where(x => x.MaxTimemin == 1080 && !x.GetVentureName().IsNullOrEmpty()).OrderBy(x => x.RetainerLevel);
        }

        internal static IEnumerable<RetainerTask> GetHunts(uint ClassJob)
        {
            var cat = GetCategory(ClassJob);
            return Svc.Data.GetExcelSheet<RetainerTask>().Where(x => x.ClassJobCategory.Value.RowId == cat).Where(x => x.MaxTimemin == 60 && !x.GetVentureName().IsNullOrEmpty()).OrderBy(x => x.RetainerLevel);
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

        internal static Item GetVentureItem(this RetainerTask Task)
        {
            return Svc.Data.GetExcelSheet<RetainerTaskNormal>().GetRow(Task.Task)?.Item.Value;
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
