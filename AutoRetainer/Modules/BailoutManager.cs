using AutoRetainer.Modules.Voyage;
using AutoRetainer.Scheduler.Tasks;
using ECommons.Automation;
using ECommons.Automation.UIInput;
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
    public static MonitoredAddon[] MonitoredAddons => field ??= 
        [
        new("SelectYesno", TimeSpan.FromSeconds(30)),
        new("SelectString", TimeSpan.FromMinutes(7)),
        new("SelectIconString", TimeSpan.FromSeconds(30)),
        new("AirShipExplorationDetail", TimeSpan.FromSeconds(30)),
        new("AirShipExplorationResult", TimeSpan.FromSeconds(30)),
        new("SubmarineExplorationMapSelect", TimeSpan.FromSeconds(30)),
        new("AirShipExploration", TimeSpan.FromMinutes(5)),
        new("CompanyCraftSupply", TimeSpan.FromMinutes(5)),
        new("FreeCompanyCreditShop", TimeSpan.FromMinutes(5)),
        new("FreeCompanyChest", TimeSpan.FromMinutes(10)),
        new("RetainerTaskAsk", TimeSpan.FromSeconds(30)),
        new("RetainerHistory", TimeSpan.FromSeconds(30)),
        new("RetainerTaskResult", TimeSpan.FromSeconds(30)),
        new("RetainerTaskSupply", TimeSpan.FromSeconds(30)),
        new("GrandCompanySupplyList", TimeSpan.FromSeconds(30)),
        new("GrandCompanySupplyReward", TimeSpan.FromSeconds(30)),
        new("GrandCompanyExchange", TimeSpan.FromMinutes(15)),
        new("RetainerList", TimeSpan.FromMinutes(7)),
        new("MiragePrismPrismBox", TimeSpan.FromMinutes(11)),
        ];
    public static Dictionary<string, long> AddonOpenedAt = [];

    internal static void Tick()
    {
        if(MultiMode.Active && !SchedulerMain.CharacterPostProcessLocked && !SchedulerMain.RetainerPostProcessLocked)
        {
            RunStuckDetection();
            RunPluginStateDetection();
            foreach(var x in MonitoredAddons)
            {
                if(!AddonOpenedAt.ContainsKey(x.Name)) AddonOpenedAt[x.Name] = Environment.TickCount64;
                if(!TryGetAddonByName<AtkUnitBase>(x.Name, out _))
                {
                    AddonOpenedAt[x.Name] = Environment.TickCount64;
                }
                if(AddonOpenedAt.TryGetValue(x.Name, out var value) && value != 0 && Environment.TickCount64 - value > x.Timeout.TotalMilliseconds)
                {
                    Utils.CleanupOperations();
                    S.AnomalyWindow.Add($"UI stuck during multi mode ({x}). Cleaning up operations and trying to resume.");
                    AddonOpenedAt.Clear();
                    break;
                }
            }
        }
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
                                addon->GetComponentButtonById(4)->ClickAddonButton(addon);
                                CharaSelectStuck = Environment.TickCount64;
                                EzThrottler.Throttle("MultiModeAfkOnTitleLogin", 60000, true);
                                IsLogOnTitleEnabled = true;
                                S.AnomalyWindow.Add("Stuck character select encountered. Retrying login...");
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

            if(!Svc.ClientState.IsLoggedIn && C.ResolveConnectionErrors && MultiMode.Enabled)
            {
                if(Utils.GetRemainingSessionMiliSeconds() > 10 * 60 * 1000)
                {
                    if(TryGetAddonByName<AtkUnitBase>("Dialogue", out var addon) && IsAddonReady(addon))
                    {
                        if(EzThrottler.Throttle("ClickDialogueOk", 10000))
                        {
                            addon->GetComponentButtonById(4)->ClickAddonButton(addon);
                            EzThrottler.Throttle("MultiModeAfkOnTitleLogin", 60000, true);
                            IsLogOnTitleEnabled = true;
                            S.AnomalyWindow.Add("Disconnected. Trying to log back in...");
                        }
                    }
                }
                else if(EzThrottler.Throttle("NotifyRestart", TimeSpan.FromDays(10)))
                {
                    S.AnomalyWindow.Add("You need to restart your game to continue playing.");
                }
            }
        }
    }

    private static void RunPluginStateDetection()
    {
        if(EzThrottler.Throttle("CheckPluginStates"))
        {
            if(WrathCombo.Available && WrathCombo.GetAutoRotationState())
            {
                Svc.Commands.ProcessCommand("/wrath auto off");
                S.AnomalyWindow.Add("Wrath Combo autorotation is on. Autoretainer has turned it off.");
            }
            if(BossMod.Available && BossMod.Presets_GetActiveList().Count > 0)
            {
                BossMod.Presets_ClearActive();
                Svc.Commands.ProcessCommand("/vbm ai off");
                S.AnomalyWindow.Add("Boss mod was active. Autoretainer has turned it off.");
            }
            if(Questionable.Available && Questionable.IsRunning())
            {
                Questionable.Stop(Svc.PluginInterface.InternalName);
                S.AnomalyWindow.Add("Questionable was active. Autoretainer has turned it off.");
            }
        }
    }

    public static Vector3? LastPosition = null;
    public static long LastRefreshTime = 0;
    public static void RunStuckDetection()
    {
        if(!EzThrottler.Check("NoMoveCheck")) return;
        var isMoving = AgentMap.Instance()->IsPlayerMoving;
        if(isMoving) EzThrottler.Throttle("ExtendIsMoving", 1000, true);
        if((!isMoving && EzThrottler.Check("ExtendIsMoving")) || !IsScreenReady())
        {
            LastRefreshTime = Environment.TickCount64;
            LastPosition = null;
        }
        else
        {
            if(LastPosition != null)
            {
                if(Vector3.Distance(Player.Position, LastPosition.Value) > 1)
                {
                    LastPosition = Player.Position;
                    LastRefreshTime = Environment.TickCount64;
                }
                else
                {
                    if(Environment.TickCount64 - LastRefreshTime > TimeSpan.FromSeconds(15).TotalMilliseconds)
                    {
                        EzThrottler.Throttle("NoMoveCheck", 1000);
                        Data.WorkshopEnabled = false;
                        Data.Enabled = false;
                        Vnavmesh.Stop();
                        Vnavmesh.Reload();
                        Lifestream.Abort();
                        Chat.ExecuteCommand("/automove off");
                        Utils.CleanupOperations();
                        S.AnomalyWindow.Add($"Movement stuck. Character excluded from submarines and retainers. Check your house registration.");
                    }
                }
            }
            else
            {
                LastPosition = Player.Position;
                LastRefreshTime = Environment.TickCount64;
            }
        }
    }
}

public readonly record struct MonitoredAddon(string Name, TimeSpan Timeout);