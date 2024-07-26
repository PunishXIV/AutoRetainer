using AutoRetainer.Modules.Voyage;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules;

internal static unsafe class BailoutManager
{
    internal static bool SimulateStuckOnQuit = false;
    internal static bool SimulateStuckOnVoyagePanel = false;
    internal static long NoSelectString = long.MaxValue;
    internal static long CharaSelectStuck = long.MaxValue;
    internal static bool IsLogOnTitleEnabled = false;

    internal static void Tick()
    {
        if(C.EnableBailout)
        {
            if(SchedulerMain.PluginEnabled || (MultiMode.Enabled && VoyageUtils.IsInVoyagePanel()))
            {
                if(!Utils.IsBusy && !VoyageScheduler.Enabled && TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon))
                {
                    if(Environment.TickCount64 - NoSelectString > C.BailoutTimeout * 1000)
                    {
                        if(Utils.GenericThrottle)
                        {
                            DuoLog.Warning($"[Bailout] Closing stuck SelectString window");
                            Callback.Fire(addon, true, -1);
                            NoSelectString = Environment.TickCount64;
                        }
                    }
                }
                else
                {
                    NoSelectString = Environment.TickCount64;
                }
            }

            if(!Svc.ClientState.IsLoggedIn && C.EnableCharaSelectBailout)
            {
                if(MultiMode.Enabled)
                {
                    var lobby = AgentLobby.Instance();
                    if(!Utils.IsBusy && !TryGetAddonByName<AtkUnitBase>("SelectOk", out _) && TryGetAddonByName<AtkUnitBase>("_CharaSelectReturn", out var addon) && IsAddonReady(addon) && (!lobby->AgentInterface.IsAgentActive() || !lobby->TemporaryLocked))
                    {
                        if(Environment.TickCount64 - CharaSelectStuck > 10 * 1000)
                        {
                            if(Utils.GenericThrottle)
                            {
                                DuoLog.Warning($"[Bailout] Backing out of CharaSelect");
                                Callback.Fire(addon, true, 4);
                                CharaSelectStuck = Environment.TickCount64;
                                EzThrottler.Throttle("MultiModeAfkOnTitleLogin", 60000, true);
                                IsLogOnTitleEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        CharaSelectStuck = Environment.TickCount64;
                    }
                }
                else
                {
                    IsLogOnTitleEnabled = false;
                    CharaSelectStuck = long.MaxValue;
                }
            }
        }
    }
}
