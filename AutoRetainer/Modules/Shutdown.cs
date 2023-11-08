using AutoRetainer.Modules.Voyage;
using ClickLib.Clicks;
using ECommons.Automation;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoRetainer.Modules
{
    internal unsafe static class Shutdown
    {
        internal static bool Active => ShutdownAt > 0 || ForceShutdownAt > 0;
        internal static long ShutdownAt = 0;
        internal static long ForceShutdownAt = 0;
        
        internal static void Tick()
        {
            if(ShutdownAt != 0)
            {
                if(Environment.TickCount64 > ShutdownAt && Player.Available)
                {
                    if(MultiMode.Enabled)
                    {
                        MultiMode.Enabled = false;
                    }

                    if(!VoyageScheduler.Enabled && !SchedulerMain.PluginEnabled && !P.TaskManager.IsBusy)
                    {
                        ShutdownAt = 0;
                        P.TaskManager.Enqueue(() =>
                        {
                            if (EzThrottler.Throttle("SendChat"))
                            {
                                Chat.Instance.SendMessage("/shutdown");
                                return true;
                            }
                            return false;
                        });
                        P.TaskManager.Enqueue(() =>
                        {
                            var yesno = Utils.GetSpecificYesno(Lang.LogOutAndExitGame);
                            if(yesno != null)
                            {
                                if (EzThrottler.Throttle("ClickExit"))
                                {
                                    ClickSelectYesNo.Using((nint)yesno).Yes();
                                    return true;
                                }
                            }
                            return false;
                        });
                    }

                    if(ForceShutdownAt  != 0)
                    {
                        if(Environment.TickCount64 > ForceShutdownAt)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }
    }
}
