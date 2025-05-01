using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;
using System.Xml.Linq;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskChangeComponents
{
    internal static volatile bool Abort = false;
    internal static string Name = "";
    internal static VoyageType Type = 0;
    internal static void EnqueueImmediate(List<(int, uint)> indexes, string vesselName, VoyageType type)
    {
        P.TaskManager.BeginStack();
        try
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskChangeComponents)}");
            Name = vesselName;
            Type = type;
            Abort = false;
            P.TaskManager.Enqueue(VoyageScheduler.SelectChangeComponents, "SelectChangeComponents");
            foreach(var index in indexes)
            {
                if(index.Item1 < 0 || index.Item1 > 3) throw new ArgumentOutOfRangeException(nameof(index));
                P.TaskManager.Enqueue(() => VoyageScheduler.ChangeComponent(index.Item1, index.Item2, Name), $"Change {index}");
                P.TaskManager.EnqueueDelay(Utils.FrameDelay * 2, true);
            }
            P.TaskManager.Enqueue(VoyageScheduler.CloseChangeComponents, "CloseChangeComponents");
        }
        catch(Exception e) { e.Log(); }
        P.TaskManager.InsertStack();
    }
}
