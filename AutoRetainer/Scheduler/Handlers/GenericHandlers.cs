using ECommons.Throttlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Handlers
{
    internal static class GenericHandlers
    {
        internal static bool? Throttle(int ms)
        {
            return EzThrottler.Throttle("AutoRetainerWait", ms);
        }

        internal static bool? WaitFor(int ms)
        {
            return EzThrottler.Check("AutoRetainerWait");
        }
    }
}
