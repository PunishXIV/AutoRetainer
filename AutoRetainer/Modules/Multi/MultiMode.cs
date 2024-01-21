using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainer.UI;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Config;
using Dalamud.Interface.Internal.Notifications;
using ECommons.CircularBuffers;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.EzSharedDataManager;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Data.Files.Excel;
using System.Xml.Linq;
using static AutoRetainer.Modules.OfflineDataManager;

namespace AutoRetainer.Modules.Multi;

internal unsafe static class MultiMode
{
    internal static bool Active => Enabled && !IPC.Suppressed;

    internal static bool Enabled = false;

    internal static bool WaitOnLoginScreen => C.MultiWaitOnLoginScreen || BailoutManager.IsLogOnTitleEnabled || C.NightMode;

    internal static bool EnabledRetainers => C.MultiModeType.EqualsAny(MultiModeType.Retainers, MultiModeType.Everything) && !VoyageUtils.IsRetainerBlockedByVoyage() && (!C.NightMode || C.NightModeRetainers);
    internal static bool EnabledSubmarines => C.MultiModeType.EqualsAny(MultiModeType.Submersibles, MultiModeType.Everything) && (!C.NightMode || C.NightModeDeployables);

    internal static bool Synchronize = false;
    internal static long NextInteractionAt { get; private set; } = 0;
    internal static ulong LastLogin = 0;
    internal static bool IsAutoLogin = false;
    internal static CircularBuffer<long> Interactions = new(5);

    internal static Dictionary<ulong, int> CharaCnt = new();
    internal static bool CanHET => Active && C.ExpertMultiAllowHET && (ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType) || Data.TeleportToFCHouse || Data.TeleportToRetainerHouse);

    internal static void Init()
    {
        ProperOnLogin.RegisterInteractable(delegate
        {
            BailoutManager.IsLogOnTitleEnabled = false;
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
                DebugLog($"ProperOnLogin: {Svc.ClientState.LocalPlayer}, residential area, scheduling HET");
                if (Data.EnforceTeleportsOnLogin)
                {
                    HouseEnterTask.EnqueueForcedTeleport();
                }
                HouseEnterTask.EnqueueTask();
            }
            MultiModeUI.JustRelogged = true;
        });
        if (ProperOnLogin.PlayerPresent)
        {
            WriteOfflineData(true, true);
        }
    }

    internal static void BailoutNightMode()
    {
        if (!C.NightMode && !C.MultiWaitOnLoginScreen && C.EnableBailout)
        {
            BailoutManager.IsLogOnTitleEnabled = true;
        }
        if (C.NightMode)
        {
            MultiMode.Enabled = true;
        }
    }

    internal static void OnMultiModeEnabled()
    {
        if (!Enabled)
        {
            return;
        }
        LastLogin = 0;
        if (C.MultiHETOnEnable && Player.Available && CanHET)
        {
            HouseEnterTask.EnqueueTask(noTeleport:true);
        }
        if(Utils.GetNearestWorkshopEntrance(out _) && Utils.GetReachableRetainerBell(false) == null)
        {
            TaskEnterWorkshop.Enqueue();
        }
    }

    internal static void ValidateAutoAfkSettings()
    {
        {
            if (Svc.GameConfig.TryGet(SystemConfigOption.AutoAfkSwitchingTime, out uint val))
            {
                if (val != 0)
                {
                    Svc.GameConfig.Set(SystemConfigOption.AutoAfkSwitchingTime, 0u);
                    DuoLog.Warning($"Your Auto Afk Switching Time option was incompatible with current AutoRetainer configuration and was set to (Never). This is not an error.");
                }
            }
        }
        {
            if (Svc.GameConfig.TryGet(SystemConfigOption.IdlingCameraAFK, out uint val))
            {
                if (val != 0)
                {
                    Svc.GameConfig.Set(SystemConfigOption.IdlingCameraAFK, 0u);
                    DuoLog.Warning($"Your Idling Camera AFK option was incompatible with current AutoRetainer configuration and was set to (Disabled). This is not an error.");
                }
            }
        }
    }

    internal static void Tick()
    {
        if (Active)
        {
            ValidateAutoAfkSettings();
            if(!C.MultiModeWorkshopConfiguration.MultiWaitForAll && C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
            {
                DuoLog.Warning($"Invalid configuration: {nameof(C.MultiModeWorkshopConfiguration.MultiWaitForAll)} was not activated but {nameof(C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)} was. The configuration was fixed.");
                C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn = false;
            }
            if(!Svc.ClientState.IsLoggedIn && TryGetAddonByName<AtkUnitBase>("Title", out _) && !AutoLogin.Instance.IsRunning)
            {
                LastLogin = 0;
            }
            if (IsOccupied() || !ProperOnLogin.PlayerPresent)
            {
                BlockInteraction(3);
            }
            if (P.TaskManager.IsBusy)
            {
                return;
            }
            if(MultiMode.WaitOnLoginScreen)
            {
                if(!Player.Available && Utils.CanAutoLogin())
                {
                    AgentLobby.Instance()->IdleTime = 0;
                    var next = GetCurrentTargetCharacter();
                    if(next != null)
                    {
                        if(EzThrottler.Throttle("MultiModeAfkOnTitleLogin", 20000))
                        {
                            if(!Relog(next, out var error, RelogReason.MultiMode))
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
                    data.WorkshopEnabled = false;
                    DuoLog.Warning("Too many errors, current character is excluded.");
                    Interactions.Clear();
                    return;
                }
                else
                {
                    Enabled = false;
                    data.WorkshopEnabled = false;
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
                        DebugLog($"Enqueueing relog");
                        BlockInteraction(20);
                        EnsureCharacterValidity();
                        RestoreValidityInWorkshop();
                        if (!Relog(next, out var error, RelogReason.MultiMode))
                        {
                            DuoLog.Error(error);
                        }
                        else
                        {
                            DebugLog($"Relog command success");
                        }
                        Interactions.PushBack(Environment.TickCount64);
                        DebugLog($"Added interaction because of relogging (state: {Interactions.Print()})");
                    }
                    else
                    {
                        if(MultiMode.WaitOnLoginScreen)
                        {
                            DebugLog($"Enqueueing logoff");
                            BlockInteraction(20);
                            EnsureCharacterValidity();
                            RestoreValidityInWorkshop();
                            if (!Relog(null, out var error, RelogReason.MultiMode))
                            {
                                DuoLog.Error(error);
                            }
                            else
                            {
                                DebugLog($"Logoff command success");
                            }
                            Interactions.PushBack(Environment.TickCount64);
                            DebugLog($"Added interaction because of logging off (state: {Interactions.Print()})");
                        }
                    }
                }
                else if (!IsOccupied() && !P.TaskManager.IsBusy)
                {
                    if (AnyRetainersAvailable() && EnabledRetainers)
                    {
                        //DuoLog.Information($"1234");
                        if (CanHET && Data.TeleportToRetainerHouse && Utils.GetReachableRetainerBell(true) == null)
                        {
                            HouseEnterTask.EnqueueTask();
                            BlockInteraction(10);
                            Interactions.PushBack(Environment.TickCount64);
                            DebugLog($"Added interaction because of HET teleport (state: {Interactions.Print()})");
                        }
                        else
                        {
                            EnsureCharacterValidity();
                            RestoreValidityInWorkshop();
                            if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
                            {
                                DebugLog($"Enqueueing interaction with bell");
                                TaskInteractWithNearestBell.Enqueue();
                                P.TaskManager.Enqueue(() => { SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode); return true; });
                                BlockInteraction(10);
                                Interactions.PushBack(Environment.TickCount64);
                                DebugLog($"Added interaction because of interacting (state: {Interactions.Print()})");
                            }
                        }
                    }
                    else if(Data.WorkshopEnabled && Data.AnyEnabledVesselsAvailable() && MultiMode.EnabledSubmarines)
                    {
                        if (!C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn || Data.AreAnyEnabledVesselsReturnInNext(0, true))
                        {
                            DebugLog($"Enqueueing interaction with panel");
                            BlockInteraction(10);
                            TaskInteractWithNearestPanel.Enqueue();
                            P.TaskManager.Enqueue(() => { VoyageScheduler.Enabled = true; });
                            Interactions.PushBack(Environment.TickCount64);
                            DebugLog($"Added interaction because of interacting (state: {Interactions.Print()})");
                        }
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

    internal static bool Relog(OfflineCharacterData data, out string ErrorMessage, RelogReason reason)
    {
        if(reason.EqualsAny(RelogReason.Overlay, RelogReason.Command, RelogReason.ConfigGUI))
        {
            if(C.MultiDisableOnRelog)
            {
                MultiMode.Enabled = false;
            }
            if (MultiMode.Active && !C.MultiNoPreferredReset)
            {
                foreach (var z in C.OfflineData)
                {
                    z.Preferred = false;
                }
                Notify.Warning("Preferred character has been reset");
            }
        }
        ErrorMessage = string.Empty;
        if (AutoLogin.Instance.IsRunning)
        {
            ErrorMessage = "AutoLogin is already running";
        }
        else if (SchedulerMain.CharacterPostProcessLocked)
        {
            ErrorMessage = "Currently in post-processing of character";
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

                    TaskPostprocessCharacterIPC.Enqueue();
                    if (data != null)
                    {
                        P.TaskManager.Enqueue(() => AutoLogin.Instance.SwapCharacter(data.CurrentWorld, data.Name, ExcelWorldHelper.GetWorldByName(data.World).RowId, data.ServiceAccount));
                    }
                    else
                    {
                        P.TaskManager.Enqueue(() => AutoLogin.Instance.Logoff());
                    }
                    return true;
                }
            }
            else
            {
                if (Utils.CanAutoLogin())
                {
                    AutoLogin.Instance.Login(data.CurrentWorld, data.Name, ExcelWorldHelper.GetWorldByName(data.World).RowId, data.ServiceAccount);
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
        var data = C.OfflineData.ToList();
        if (C.CharEqualize)
        {
            data = [.. data.OrderBy(x => CharaCnt.GetOrDefault(x.CID))];
        }
        if (C.MultiPreferredCharLast)
        {
            var pref = data.FirstOrDefault(x => x.Preferred);
            if(pref != null)
            {
                data.Remove(pref);
                data.Add(pref);
            }
        }
        if (EnabledSubmarines)
        {
            foreach (var x in data)
            {
                if (x.CID == Player.CID) continue;
                if (x.WorkshopEnabled && x.GetEnabledVesselsData(VoyageType.Airship).Count + x.GetEnabledVesselsData(VoyageType.Submersible).Count > 0)
                {
                    if (x.AreAnyEnabledVesselsReturnInNext(C.MultiModeWorkshopConfiguration.AdvanceTimer, C.MultiModeWorkshopConfiguration.MultiWaitForAll))
                    {
                        return x;
                    }
                }
            }
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
                        C.MultiModeRetainerConfiguration.MultiWaitForAll ? selectedRetainers.All(z => z.GetVentureSecondsRemaining() <= C.MultiModeRetainerConfiguration.AdvanceTimer) : selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= C.MultiModeRetainerConfiguration.AdvanceTimer))
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
        return !Data.AreAnyEnabledVesselsReturnInNext(5 * 60, C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn);
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

    internal static void PerformAutoStart()
    {
        EzSharedData.TryGet<object>("AutoRetainer.WasLoaded", out _, CreationMode.CreateAndKeep, new());
        for (int i = 0; i < 10; i++)
        {
            var seconds = 10 - i;
            P.TaskManager.Enqueue(() => Svc.PluginInterface.UiBuilder.AddNotification($"MultiMode autostarts in {seconds} seconds!", P.Name, NotificationType.Warning, 0));
            P.TaskManager.DelayNext(1000);
        }
        P.TaskManager.Enqueue(() =>
        {
            MultiMode.Enabled = true;
            BailoutManager.IsLogOnTitleEnabled = true;
        });
    }
}
