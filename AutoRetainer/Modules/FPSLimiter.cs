using System.Diagnostics;
using System.Threading;

namespace AutoRetainer.Modules;

internal static unsafe class FPSLimiter
{
    private static readonly Stopwatch Stopwatch = new();
    internal static void FPSLimit()
    {
        if (MultiMode.Active)
        {
            if (
                (!C.NoFPSLockWhenActive || CSFramework.Instance()->WindowInactive)
                && (!C.FpsLockOnlyShutdownTimer || Shutdown.Active || (C.NightMode && C.NightModeFPSLimit))
                )
            {
                if (Utils.IsBusy)
                {
                    if (C.TargetMSPTIdle > 0)
                    {
                        var ms = (int)(C.TargetMSPTRunning - Stopwatch.ElapsedMilliseconds);
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
                        if (C.NightMode && Utils.CanAutoLogin() && MultiMode.Active)
                        {
                            targetMSPT = CSFramework.Instance()->WindowInactive ? 5000 : 100;
                        }
                        var ms = (int)(targetMSPT - Stopwatch.ElapsedMilliseconds);
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
