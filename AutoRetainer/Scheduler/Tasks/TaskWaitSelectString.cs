using AutoRetainer.Scheduler.Handlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks
{
    internal unsafe static class TaskWaitSelectString
    {
        internal static void Enqueue(int ms)
        {
            P.TaskManager.Enqueue(() => { return TryGetAddonByName<AtkUnitBase>("SelectString", out _); });
            P.TaskManager.Enqueue(() => GenericHandlers.Throttle(ms));
            P.TaskManager.Enqueue(() => GenericHandlers.WaitFor(ms));
        }
    }
}
