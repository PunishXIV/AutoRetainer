using AutoRetainer.Scheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks
{
    internal static class TaskWait
    {
        internal static void Enqueue(int ms)
        {
            P.TaskManager.Enqueue(() => GenericHandlers.Throttle(ms));
            P.TaskManager.Enqueue(() => GenericHandlers.WaitFor(ms));
        }
    }
}
