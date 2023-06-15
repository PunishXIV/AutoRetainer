using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler
{
    internal static class Artisan
    {
        internal static void CheckIfArtisanShouldBeEnabled()
        {
            if (C.ArtisanIntegration)
            {
                if (IsCurrentlyOperating() && MultiMode.EnsureCharacterValidity(true))
                {
                    if(!SchedulerMain.PluginEnabled || SchedulerMain.Reason != PluginEnableReason.Artisan)
                    {
                        SchedulerMain.EnablePlugin(PluginEnableReason.Artisan);
                        P.DebugLog($"Enabling AutoRetainer because of Artisan integration");
                    }
                }
            }
        }

        internal static bool IsCurrentlyOperating()
        {

            return true;
        }

        
    }
}
