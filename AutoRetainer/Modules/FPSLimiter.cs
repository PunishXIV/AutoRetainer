using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal unsafe static class FPSLimiter
    {
        readonly static Stopwatch Stopwatch = new();
        internal static void FPSLimit()
        {
            if (MultiMode.Active)
            {
                if (!C.NoFPSLockWhenActive || CSFramework.Instance()->WindowInactive)
                {
                    if (Utils.IsBusy)
                    {
                        if (C.TargetMSPTIdle > 0)
                        {
                            int ms = (int)(C.TargetMSPTRunning - Stopwatch.ElapsedMilliseconds);
                            if (ms > 0 && ms <= C.TargetMSPTRunning)
                            {
                                Thread.Sleep(ms);
                            }
                        }
                    }
                    else
                    {
                        if (C.TargetMSPTIdle > 0)
                        {
                            int ms = (int)(C.TargetMSPTIdle - Stopwatch.ElapsedMilliseconds);
                            if (ms > 0 && ms <= C.TargetMSPTIdle)
                            {
                                Thread.Sleep(ms);
                            }
                        }
                    }
                }
                Stopwatch.Restart();
            }
        }
    }
}
