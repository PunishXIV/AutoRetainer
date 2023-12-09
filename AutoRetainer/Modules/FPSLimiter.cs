using FFXIVClientStructs.FFXIV.Client.Game;
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
                if (
                    (!C.NoFPSLockWhenActive || CSFramework.Instance()->WindowInactive)
                    && (!C.FpsLockOnlyShutdownTimer || Shutdown.Active || (P.NightMode && C.NightModeFPSLimit))
                    )
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
                            var targetMSPT = C.TargetMSPTIdle;
                            if (P.NightMode && Utils.CanAutoLogin() && MultiMode.Active)
                            {
                                targetMSPT = CSFramework.Instance()->WindowInactive? 5000:100;
                            }
                            int ms = (int)(targetMSPT - Stopwatch.ElapsedMilliseconds);
                            if (ms > 0 && ms <= targetMSPT)
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
