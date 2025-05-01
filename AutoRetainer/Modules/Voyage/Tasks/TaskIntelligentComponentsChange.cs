using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.PartSwapper;
using AutoRetainerAPI.Configuration;
using ECommons.Automation.NeoTaskManager;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskIntelligentComponentsChange
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskIntelligentComponentsChange)}, name={name}, type={type}");
        P.TaskManager.Enqueue(() =>
        {
            var rep = PartSwapperUtils.GetIsVesselNeedsPartsSwap(name, type, out var log);
            if(rep.Count > 0)
            {
                TaskChangeComponents.EnqueueImmediate(rep, name, type);
            }
            PluginLog.Debug($"Change check log: {log.Join(", ")}");
        }, "IntelligentChangeTask");
    }
}
