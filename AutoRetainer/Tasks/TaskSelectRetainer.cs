using AutoRetainer.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Tasks
{
    internal static class TaskSelectRetainer
    {
        internal static void Enqueue(string name)
        {
            P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(name));
            P.TaskManager.Enqueue(() => Utils.TryGetCurrentRetainer(out _));
        }
    }
}
