using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainer.UI;
using AutoRetainerAPI.Configuration;
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
using FFXIVClientStructs.FFXIV.Component.GUI;
using static AutoRetainer.Modules.OfflineDataManager;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Modules.Multi;

internal unsafe static class MultiMode
{
    internal static bool Active => Enabled && !IPC.Suppressed;

    internal static bool Enabled = false;

    internal static bool EnabledRetainers => C.MultiModeType.EqualsAny(MultiModeType.Retainers, MultiModeType.Everything);
    internal static bool EnabledSubmarines => C.MultiModeType.EqualsAny(MultiModeType.Submersibles, MultiModeType.Everything);

    internal static bool Synchronize = false;
    internal static long NextInteractionAt { get; private set; } = 0;
    internal static ulong LastLogin = 0;
    internal static bool IsAutoLogin = false;
    internal static CircularBuffer<long> Interactions = new(5);

    internal static Dictionary<ulong, int> CharaCnt = new();
    internal static bool CanHET => Active && C.MultiAllowHET && ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType);

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
            if (CanHET)
            {
                P.DebugLog($"ProperOnLogin: {Svc.ClientState.LocalPlayer}, residential area, scheduling HET");
                HouseEnterTask.EnqueueTask();
            }
            MultiModeUI.JustRelogged = true;
        });
        if (ProperOnLogin.PlayerPresent)
        {
            WriteOfflineData(true, true);
        }
    }

    internal static void OnMultiModeEnabled()
    {
        if (!Enabled)
        {
            return;
        }
        LastLogin = 0;
        if (C.MultiHETOnEnable && CanHET && Player.Available)
        {
            HouseEnterTask.EnqueueTask();
        }
    }

    internal static int GetAutoAfkOpt()
    {
        return (int)(Svc.GameConfig.System.TryGetUInt("AutoAfkSwitchingTime", out var val) ? val : 1);
    }

    internal static void Tick()
    {
        if (Active)
        {
            if(!Svc.ClientState.IsLoggedIn && TryGetAddonByName<AtkUnitBase>("Title", out _) && !AutoLogin.Instance.IsRunning)
            {
                LastLogin = 0;
            }
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
            if(C.MultiWaitOnLoginScreen)
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
                if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
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
                    if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
                    {
                        data.Enabled = false;
                    }
                }
            }
            if (ProperOnLogin.PlayerPresent && !AutoLogin.Instance.IsRunning && IsInteractionAllowed()
                && (!Synchronize || C.OfflineData.All(x => x.GetEnabledRetainers().All(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation))))
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
                        RestoreValidityInWorkshop();
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
                        if(C.MultiWaitOnLoginScreen)
                        {
                            P.DebugLog($"Enqueueing logoff");
                            BlockInteraction(20);
                            EnsureCharacterValidity();
                            RestoreValidityInWorkshop();
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
                else if (!IsOccupied() && !P.TaskManager.IsBusy)
                {
                    if (AnyRetainersAvailable() && EnabledRetainers)
                    {
                        //DuoLog.Information($"1234");
                        EnsureCharacterValidity();
                        RestoreValidityInWorkshop();
                        if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
                        {
                            P.DebugLog($"Enqueueing interaction with bell");
                            TaskInteractWithNearestBell.Enqueue();
                            P.TaskManager.Enqueue(() => { SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode); return true; });
                            BlockInteraction(10);
                            Interactions.PushBack(Environment.TickCount64);
                            P.DebugLog($"Added interaction because of interacting (state: {Interactions.Print()})");
                        }
                    }
                    else if(Data.AnyEnabledVesselsAvailable() && MultiMode.EnabledSubmarines)
                    {
                        P.DebugLog($"Enqueueing interaction with panel");
                        BlockInteraction(10);
                        TaskInteractWithNearestPanel.Enqueue();
                        P.TaskManager.Enqueue(() => { VoyageScheduler.Enabled = true; }) ;
                        Interactions.PushBack(Environment.TickCount64);
                        P.DebugLog($"Added interaction because of interacting (state: {Interactions.Print()})");
                    }
                }
            }
        }
    }

    internal static bool CheckInventoryValidity() => Svc.ClientState.LocalPlayer.HomeWorld.Id == Svc.ClientState.LocalPlayer.CurrentWorld.Id && Utils.GetVenturesAmount() >= Data.GetNeededVentureAmount() && Utils.IsInventoryFree();
    internal static void RestoreValidityInWorkshop()
    {
        return;
        if (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType))
        {
            if (!ProperOnLogin.PlayerPresent) return;
            if (Data != null)
            {
                if (CheckInventoryValidity())
                {
                    Data.Enabled = true;
                    return;
                }
            }
        }
        return;
    }

    internal static IEnumerable<OfflineCharacterData> GetEnabledOfflineData()
    {
        return C.OfflineData.Where(x => x.Enabled);
    }

    internal static bool AnyRetainersAvailable()
    {
        if (GetEnabledOfflineData().TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation);
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
        return C.OfflineData.FirstOrDefault(x => x.Preferred && x.CID != Svc.ClientState.LocalContentId);
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
        if (C.SelectedRetainers.TryGetValue(data.CID, out var enabledRetainers))
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
        /*else if (data != null && !data.Index.InRange(1, 9))
        {
            ErrorMessage = "Invalid character index";
        }*/
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
                        AutoLogin.Instance.SwapCharacter(data.WorldOverride ?? data.World, data.Name, data.ServiceAccount);
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
                    AutoLogin.Instance.Login(data.World, data.Name, data.ServiceAccount);
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
        var data = C.OfflineData;
        if (C.CharEqualize)
        {
            data = data.OrderBy(x => CharaCnt.GetOrDefault(x.CID)).ToList();
        }
        if (EnabledRetainers)
        {
            foreach (var x in data)
            {
                if (x.CID == Player.CID) continue;
                if (x.Enabled && C.SelectedRetainers.TryGetValue(x.CID, out var enabledRetainers))
                {
                    var selectedRetainers = x.GetEnabledRetainers().Where(z => z.HasVenture);
                    if (selectedRetainers.Any() &&
                        C.MultiWaitForAll ? selectedRetainers.All(z => z.GetVentureSecondsRemaining() <= C.AdvanceTimer) : selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= C.AdvanceTimer))
                    {
                        return x;
                    }
                }
            }
        }
        if (EnabledSubmarines)
        {
            foreach (var x in data)
            {
                if (x.CID == Player.CID) continue;
                if (x.WorkshopEnabled && x.GetEnabledVesselsData(VoyageType.Airship).Count + x.GetEnabledVesselsData(VoyageType.Submersible).Count > 0)
                {
                    if (x.AreAnyVesselsReturnInNext(2))
                    {
                        return x;
                    }
                }
            }
        }
        return null;
    }

    internal static bool IsCurrentCharacterDone() => IsCurrentCharacterRetainersDone() && IsCurrentCharacterCaptainDone();

    internal static bool IsCurrentCharacterRetainersDone()
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            if (!EnabledRetainers) return true;
            if (!data.Enabled) return true;
            if (Utils.GetVenturesAmount() < 2 || !Utils.IsInventoryFree()) return true;
            return !IsAnySelectedRetainerFinishesWithin(5 * 60);
        }
        return false;
    }

    internal static bool IsCurrentCharacterCaptainDone()
    {
        if (!EnabledSubmarines) return true;
        if(!Data.WorkshopEnabled) return true;
        return !Data.AreAnyVesselsReturnInNext(5);
    }

    internal static bool IsAnySelectedRetainerFinishesWithin(int seconds)
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (!EnabledRetainers) return false;
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
        if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            if (Svc.ClientState.LocalPlayer.HomeWorld.Id == Svc.ClientState.LocalPlayer.CurrentWorld.Id && Utils.GetVenturesAmount() >= data.GetNeededVentureAmount() && Utils.IsInventoryFree() && Utils.GetReachableRetainerBell(true) != null)
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

    /*internal static GameObject GetNearbyBell()
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
    }*/

    internal static int GetNeededVentureAmount(this OfflineCharacterData data)
    {
        return data.GetEnabledRetainers().Length * 2;
    }
}
