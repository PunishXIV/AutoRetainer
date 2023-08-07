using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks.Voyage
{
    internal static class TaskQuitMenu
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(SchedulerVoyage.Quit);
        }
    }
}
