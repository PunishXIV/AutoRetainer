using AutoRetainer.Scheduler.Tasks;
using AutoRetainer.UI;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.CircularBuffers;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using static AutoRetainer.Modules.OfflineDataManager;

namespace AutoRetainer.Modules.Multi;

internal unsafe static class MultiMode
{
    internal static bool Active => Enabled && !IPC.Suppressed;

    internal static bool Enabled = false;

    internal static bool Synchronize = false;
    internal static long NextInteractionAt { get; private set; } = 0;
    internal static ulong LastLogin = 0;
    internal static CircularBuffer<long> Interactions = new(5);

    internal static Dictionary<ulong, int> CharaCnt = new();

    internal static void Init()
    {
        ProperOnLogin.Register(delegate
        {
            WriteOfflineData(true, true);
            if (LastLogin == Svc.ClientState.LocalContentId && Active)
            {
                DuoLog.Error("Multi mode disabled as it have detected duplicate login.");
                Enabled = false;
            }
            LastLogin = Svc.ClientState.LocalContentId;
            Interactions.Clear();
            if (Active && P.config.MultiAllowHET && ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType))
            {
                P.DebugLog($"ProperOnLogin: {Svc.ClientState.LocalPlayer}, residential area, scheduling HET");
                HouseEnterTask.EnqueueTask();
            }
            MultiModeUI.JustRelogged = true;
        });
        if (ProperOnLogin.PlayerPresent)
        {
            LastLogin = Svc.ClientState.LocalContentId;
            WriteOfflineData(true, true);
        }
    }

    internal static int GetAutoAfkOpt()
    {
        return ConfigModule.Instance()->GetIntValue(145);
    }

    internal static void Tick()
    {
        if (Active)
        {
            if (GetAutoAfkOpt() != 0)
            {
                DuoLog.Warning("Using Multi Mode requires Auto-afk option to be turned off");
                Enabled = false;
                return;
            }
            if (IsOccupied() || !ProperOnLogin.PlayerPresent)
            {
                BlockInteraction(3);
            }
            if (P.TaskManager.IsBusy)
            {
                return;
            }
            if(P.config.MultiWaitOnLoginScreen)
            {
                if(!Player.Available && Utils.CanAutoLogin())
                {
                    AgentLobby.Instance()->IdleTime = 0;
                    var next = GetCurrentTargetCharacter();
                    if(next != null)
                    {
                        if(EzThrottler.Throttle("MultiModeAfkOnTitleLogin", 20000))
                        {
                            if(!Relog(next, out var error))
                            {
                                PluginLog.Error($"Error while automatically logging in: {error}");
                                Notify.Error($"{error}");
                            }
                        }
                    }
                }
            }
            if (Interactions.Count() == Interactions.Capacity && Interactions.All(x => Environment.TickCount64 - x < 60000))
            {
                if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
                {
                    data.Enabled = false;
                    DuoLog.Warning("Too many errors, current character is excluded.");
                    Interactions.Clear();
                    return;
                }
                else
                {
                    Enabled = false;
                    DuoLog.Error("Fatal error. Please report this with logs.");
                    Interactions.Clear();
                    return;
                }
            }
            if (ProperOnLogin.PlayerPresent && !AutoLogin.Instance.IsRunning)
            {
                if (!Utils.IsInventoryFree())
                {
                    if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
                    {
                        data.Enabled = false;
                    }
                }
            }
            if (ProperOnLogin.PlayerPresent && !AutoLogin.Instance.IsRunning && IsInteractionAllowed()
                && (!Synchronize || P.config.OfflineData.All(x => x.GetEnabledRetainers().All(z => z.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation))))
            {
                Synchronize = false;
                if (IsCurrentCharacterDone() && !IsOccupied())
                {
                    var next = GetCurrentTargetCharacter();
                    if (next == null && IsAllRetainersHaveMoreThan15Mins())
                    {
                        next = GetPreferredCharacter();
                    }
                    if (next != null)
                    {
                        P.DebugLog($"Enqueueing relog");
                        BlockInteraction(20);
                        EnsureCharacterValidity();
                        if (!Relog(next, out var error))
                        {
                            DuoLog.Error(error);
                        }
                        else
                        {
                            P.DebugLog($"Relog command success");
                        }
                        Interactions.PushBack(Environment.TickCount64);
                        P.DebugLog($"Added interaction because of relogging (state: {Interactions.Print()})");
                    }
                    else
                    {
                        if(P.config.MultiWaitOnLoginScreen)
                        {
                            P.DebugLog($"Enqueueing logoff");
                            BlockInteraction(20);
                            EnsureCharacterValidity();
                            if (!Relog(null, out var error))
                            {
                                DuoLog.Error(error);
                            }
                            else
                            {
                                P.DebugLog($"Logoff command success");
                            }
                            Interactions.PushBack(Environment.TickCount64);
                            P.DebugLog($"Added interaction because of logging off (state: {Interactions.Print()})");
                        }
                    }
                }
                else if (!IsOccupied() && AnyRetainersAvailable())
                {
                    //DuoLog.Information($"1234");
                    EnsureCharacterValidity();
                    if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
                    {
                        P.DebugLog($"Enqueueing interaction with bell");
                        TaskInteractWithNearestBell.Enqueue();
                        P.TaskManager.Enqueue(() => { SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode); return true; });
                        BlockInteraction(10);
                        Interactions.PushBack(Environment.TickCount64);
                        P.DebugLog($"Added interaction because of interacting (state: {Interactions.Print()})");
                    }
                }
            }
        }
    }

    internal static IEnumerable<OfflineCharacterData> GetEnabledOfflineData()
    {
        return P.config.OfflineData.Where(x => x.Enabled);
    }

    internal static bool AnyRetainersAvailable()
    {
        if (GetEnabledOfflineData().TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation);
        }
        return false;
    }

    internal static bool IsAllRetainersHaveMoreThan15Mins()
    {
        foreach (var x in GetEnabledOfflineData())
        {
            foreach (var z in x.GetEnabledRetainers())
            {
                if (z.GetVentureSecondsRemaining() < 15 * 60) return false;
            }
        }
        return true;
    }

    internal static OfflineCharacterData GetPreferredCharacter()
    {
        return P.config.OfflineData.FirstOrDefault(x => x.Preferred && x.CID != Svc.ClientState.LocalContentId);
    }

    internal static void BlockInteraction(int seconds)
    {
        NextInteractionAt = Environment.TickCount64 + seconds * new Random().Next(800, 1200);
    }

    internal static bool IsInteractionAllowed()
    {
        return Environment.TickCount64 > NextInteractionAt;
    }

    internal static OfflineRetainerData[] GetEnabledRetainers(this OfflineCharacterData data)
    {
        if (P.config.SelectedRetainers.TryGetValue(data.CID, out var enabledRetainers))
        {
            return data.RetainerData.Where(z => enabledRetainers.Contains(z.Name) && z.HasVenture).ToArray();
        }
        return Array.Empty<OfflineRetainerData>();
    }

    internal static bool Relog(OfflineCharacterData data, out string ErrorMessage)
    {
        ErrorMessage = string.Empty;
        if (AutoLogin.Instance.IsRunning)
        {
            ErrorMessage = "AutoLogin is already running";
        }
        else if (data != null && !data.Index.InRange(1, 9))
        {
            ErrorMessage = "Invalid character index";
        }
        else 
        {
            if (Player.Available)
            {
                if (IsOccupied())
                {
                    ErrorMessage = "Player is occupied";
                }
                else if (data != null && data.CID == Svc.ClientState.LocalContentId)
                {
                    ErrorMessage = "Targeted player is logged in";
                }
                else
                {
                    if (MultiMode.Enabled)
                    {
                        CharaCnt.IncrementOrSet(Svc.ClientState.LocalContentId);
                    }
                    else
                    {
                        CharaCnt.Clear();
                    }
                    if (data != null)
                    {
                        AutoLogin.Instance.SwapCharacter(data.World, data.CharaIndex, data.ServiceAccount);
                    }
                    else
                    {
                        AutoLogin.Instance.Logoff();
                    }
                    return true;
                }
            }
            else
            {
                if (Utils.CanAutoLogin())
                {
                    AutoLogin.Instance.Login(data.World, data.CharaIndex, data.ServiceAccount);
                    return true;
                }
                else
                {
                    ErrorMessage = "Can not log in now";
                }
            }
        }
        return false;
    }

    internal static OfflineCharacterData GetCurrentTargetCharacter()
    {
        var data = P.config.OfflineData;
        if (P.config.CharEqualize)
        {
            data = data.OrderBy(x => CharaCnt.GetOrDefault(x.CID)).ToList();
        }
        foreach (var x in data)
        {
            if (x.CID == Player.CID) continue;
            if (x.Enabled && P.config.SelectedRetainers.TryGetValue(x.CID, out var enabledRetainers))
            {
                var selectedRetainers = x.GetEnabledRetainers().Where(z => z.HasVenture);
                if (selectedRetainers.Any() &&
                    P.config.MultiWaitForAll ? selectedRetainers.All(z => z.GetVentureSecondsRemaining() <= P.config.AdvanceTimer) : selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= P.config.AdvanceTimer))
                {
                    return x;
                }
            }
        }
        return null;
    }

    internal static bool IsCurrentCharacterDone()
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            if (!data.Enabled) return true;
            if (Utils.GetVenturesAmount() < 2 || !Utils.IsInventoryFree()) return true;
            return !IsAnySelectedRetainerFinishesWithin(5 * 60);
        }
        return false;
    }

    internal static bool IsAnySelectedRetainerFinishesWithin(int seconds)
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (GetEnabledOfflineData().TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            var selectedRetainers = data.GetEnabledRetainers().Where(z => z.HasVenture);
            return selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= seconds);
        }
        return false;
    }

    internal static bool EnsureCharacterValidity(bool ro = false)
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            if (Svc.ClientState.LocalPlayer.HomeWorld.Id == Svc.ClientState.LocalPlayer.CurrentWorld.Id && Utils.GetVenturesAmount() >= data.GetNeededVentureAmount() && Utils.IsInventoryFree() && GetNearbyBell() != null)
            {
                return true;
            }
            if (!ro)
            {
                data.Enabled = false;
            }
        }
        return false;
    }

    internal static GameObject GetNearbyBell()
    {
        if (!ProperOnLogin.PlayerPresent) return null;
        foreach (var x in Svc.Objects)
        {
            if ((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName, "リテイナーベル"))
            {
                if (Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < Utils.GetValidInteractionDistance(x) && x.Struct()->GetIsTargetable())
                {
                    return x;
                }
            }
        }
        return null;
    }

    internal static int GetNeededVentureAmount(this OfflineCharacterData data)
    {
        return data.GetEnabledRetainers().Length * 2;
    }

}
