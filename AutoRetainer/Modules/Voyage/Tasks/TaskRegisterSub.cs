using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using ECommons.Throttlers;
using System;
using AutoRetainerAPI.Configuration;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskRegisterSub
{
    internal static string Name = "";
    internal static VoyageType Type = 0;
    internal static void Enqueue()
    {
        P.TaskManager.BeginStack();
        try
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskChangeComponents)}");

            P.TaskManager.Enqueue(VoyageScheduler.SelectRegisterSub, "SelectRegisterSub");
            var sharkParts = new[] { (uint)Hull.Shark, (uint)Stern.Shark, (uint)Bow.Shark, (uint)Bridge.Shark };

            for (int i = 0; i < sharkParts.Length; i++)
            {
                int index = i;
                P.TaskManager.Enqueue(() => VoyageScheduler.ChangeComponent(index, (uint)sharkParts[index]), $"Set {index}");
                P.TaskManager.EnqueueDelay(Utils.FrameDelay * 2, true);
            }

            P.TaskManager.Enqueue(VoyageScheduler.RegisterSub);
        }
        catch(Exception e) { e.Log(); }
        P.TaskManager.InsertStack();
    }
}
