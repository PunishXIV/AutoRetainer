using AutoRetainer.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Tasks
{
    internal static class TaskAssignQuickVenture
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
            P.TaskManager.Enqueue(RetainerHandlers.SelectQuickExploration);
            P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
        }
    }
}
