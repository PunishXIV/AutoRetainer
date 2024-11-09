using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainer.UI.MainWindow.MultiModeTab;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Config;
using Dalamud.Interface.ImGuiNotification;
using ECommons.CircularBuffers;
using ECommons.Events;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.EzSharedDataManager;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static AutoRetainer.Modules.OfflineDataManager;

namespace AutoRetainer.Modules.Multi;

internal static unsafe class MultiMode
{
    internal static bool Active => Enabled && !IPC.Suppressed;

    internal static bool Enabled = false;

    internal static bool WaitOnLoginScreen => C.MultiWaitOnLoginScreen || BailoutManager.IsLogOnTitleEnabled || C.NightMode;

    internal static bool EnabledRetainers => C.MultiModeType.EqualsAny(MultiModeType.Retainers, MultiModeType.Everything) && !VoyageUtils.IsRetainerBlockedByVoyage() && (!C.NightMode || C.NightModeRetainers);
    internal static bool EnabledSubmarines => C.MultiModeType.EqualsAny(MultiModeType.Submersibles, MultiModeType.Everything) && (!C.NightMode || C.NightModeDeployables);

    internal static bool Synchronize = false;
    internal static long NextInteractionAt { get; private set; } = 0;
    internal static ulong LastLogin = 0;
    internal static CircularBuffer<long> Interactions = new(5);

    internal static Dictionary<ulong, int> CharaCnt = [];
    internal static bool CanHET => Active && CanHETRaw;
    internal static bool CanHETRaw => ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType) && (TaskNeoHET.GetFcOrPrivateEntranceFromMarkers() != null || TaskNeoHET.GetApartmentEntrance() != null) && (!C.NoTeleportHetWhenNextToBell || Utils.GetReachableRetainerBell(false) == null);

    internal static void Init()
    {
        ProperOnLogin.RegisterInteractable(delegate
        {
            if(Data != null)
            {
                C.LastLoggedInChara = Data.CID;
            }
            if(TaskChangeCharacter.Expected != null)
            {
                if(MultiMode.Enabled)
                {
                    if(TaskChangeCharacter.Expected.Value.Name != Player.Name || TaskChangeCharacter.Expected.Value.World != Player.HomeWorld)
                    {
                        DuoLog.Warning($"[ARERRCMM] Character mismatch, expected {TaskChangeCharacter.Expected}, but logged in on {Player.NameWithWorld}. Please report this to developer unless you have manually interfered with login process");
                    }
                }
                TaskChangeCharacter.Expected = null;
            }
            BailoutManager.IsLogOnTitleEnabled = false;
            WriteOfflineData(true, true);
            if(LastLogin == Svc.ClientState.LocalContentId && Active)
            {
                DuoLog.Error("Multi mode disabled as it have detected duplicate login.");
                Enabled = false;
            }
            LastLogin = MultiMode.Enabled && !C.MultiWaitOnLoginScreen ? Svc.ClientState.LocalContentId : 0;
            Interactions.Clear();
            if(CanHET)
            {
                DebugLog($"ProperOnLogin: {Svc.ClientState.LocalPlayer}, residential area, scheduling HET");
                if(!TaskTeleportToProperty.ShouldVoidHET()) TaskNeoHET.Enqueue(null);
            }
            MultiModeUI.JustRelogged = true;
            if(!MultiMode.Enabled && C.HETWhenDisabled && CanHETRaw)
            {
                TaskNeoHET.Enqueue(null);
            }
        });
        if(ProperOnLogin.PlayerPresent)
        {
            WriteOfflineData(true, true);
            if(Data != null)
            {
                C.LastLoggedInChara = Data.CID;
            }
        }
    }

    internal static void BailoutNightMode()
    {
        if(!C.NightMode && !C.MultiWaitOnLoginScreen && C.EnableBailout)
        {
            BailoutManager.IsLogOnTitleEnabled = true;
        }
        if(C.NightMode)
        {
            MultiMode.Enabled = true;
        }
    }

    internal static void OnMultiModeEnabled()
    {
        if(!Enabled)
        {
            return;
        }
        LastLogin = 0;
        if(!TaskTeleportToProperty.ShouldVoidHET())
        {
            if(C.MultiHETOnEnable && Player.Available && CanHET)
            {
                TaskNeoHET.Enqueue(null);
            }
        }
    }

    internal static void ValidateAutoAfkSettings()
    {
        {
            if(Svc.GameConfig.TryGet(SystemConfigOption.AutoAfkSwitchingTime, out uint val))
            {
                if(val != 0)
                {
                    Svc.GameConfig.Set(SystemConfigOption.AutoAfkSwitchingTime, 0u);
                    DuoLog.Warning($"Your Auto Afk Switching Time option was incompatible with current AutoRetainer configuration and was set to (Never). This is not an error.");
                }
            }
        }
        {
            if(Svc.GameConfig.TryGet(SystemConfigOption.IdlingCameraAFK, out uint val))
            {
                if(val != 0)
                {
                    Svc.GameConfig.Set(SystemConfigOption.IdlingCameraAFK, 0u);
                    DuoLog.Warning($"Your Idling Camera AFK option was incompatible with current AutoRetainer configuration and was set to (Disabled). This is not an error.");
                }
            }
        }
    }

    internal static void Tick()
    {
        if(Active)
        {
            ValidateAutoAfkSettings();
            if(!Svc.ClientState.IsLoggedIn && TryGetAddonByName<AtkUnitBase>("Title", out _) && !P.TaskManager.IsBusy)
            {
                LastLogin = 0;
            }
            if(IsOccupied() || !IsScreenReady() || !ProperOnLogin.PlayerPresent)
            {
                BlockInteraction(1);
            }
            if(P.TaskManager.IsBusy)
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
            if(Interactions.Count() == Interactions.Capacity && Interactions.All(x => Environment.TickCount64 - x < 60000))
            {
                if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
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
            if(ProperOnLogin.PlayerPresent && !P.TaskManager.IsBusy)
            {
                if(!Utils.IsInventoryFree())
                {
                    if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
                    {
                        data.Enabled = false;
                    }
                }
            }
            if(ProperOnLogin.PlayerPresent && !P.TaskManager.IsBusy && IsInteractionAllowed()
                && (!Synchronize || C.OfflineData.All(x => x.GetEnabledRetainers().All(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation))))
            {
                Synchronize = false;
                if(IsCurrentCharacterDone() && !IsOccupied())
                {
                    var next = GetCurrentTargetCharacter();
                    if(next == null && IsAllRetainersHaveMoreThan15Mins())
                    {
                        next = GetPreferredCharacter();
                    }
                    if(next != null)
                    {
                        DebugLog($"Enqueueing relog");
                        BlockInteraction(20);
                        if(!Relog(next, out var error, RelogReason.MultiMode))
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
                            if(!Relog(null, out var error, RelogReason.MultiMode))
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
                else if(!IsOccupied() && !Utils.IsBusy)
                {
                    if(Data.WorkshopEnabled && Data.AnyEnabledVesselsAvailable() && MultiMode.EnabledSubmarines)
                    {
                        if(!Data.ShouldWaitForAllWhenLoggedIn() || Data.AreAnyEnabledVesselsReturnInNext(0, true))
                        {
                            if(!TaskTeleportToProperty.EnqueueIfNeededAndPossible(true))
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
                    else if(AnyRetainersAvailable() && EnabledRetainers)
                    {
                        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
                        {
                            if(!TaskTeleportToProperty.EnqueueIfNeededAndPossible(false))
                            {
                                EnterWorkshopForRetainers();
                                EnsureCharacterValidity();
                                if(data.Enabled)
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
                    }
                }
            }
        }
    }

    internal static void EnterWorkshopForRetainers()
    {
        if(Utils.GetReachableRetainerBell(true) == null && Houses.List.Contains((ushort)Player.Territory))
        {
            TaskNeoHET.TryEnterWorkshop(() =>
            {
                Data.Enabled = false;
                DuoLog.Error($"Due to absence of retainer bell and failure to find workshop, character is excluded from processing retainers");
                P.TaskManager.Abort();
            });
        }
    }

    internal static bool CheckInventoryValidity() => Svc.ClientState.LocalPlayer.HomeWorld.Id == Svc.ClientState.LocalPlayer.CurrentWorld.Id && Utils.GetVenturesAmount() >= Data.GetNeededVentureAmount() && Utils.IsInventoryFree();

    internal static IEnumerable<OfflineCharacterData> GetEnabledOfflineData()
    {
        return C.OfflineData.Where(x => x.Enabled);
    }

    internal static bool AnyRetainersAvailable(int advanceSeconds = 0)
    {
        if(GetEnabledOfflineData().TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation + advanceSeconds);
        }
        return false;
    }

    internal static bool IsAllRetainersHaveMoreThan15Mins()
    {
        foreach(var x in GetEnabledOfflineData())
        {
            foreach(var z in x.GetEnabledRetainers())
            {
                if(z.GetVentureSecondsRemaining() < 15 * 60) return false;
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
        if(C.SelectedRetainers.TryGetValue(data.CID, out var enabledRetainers))
        {
            return data.RetainerData.Where(z => enabledRetainers.Contains(z.Name) && z.HasVenture).ToArray();
        }
        return Array.Empty<OfflineRetainerData>();
    }

    internal static bool Relog(OfflineCharacterData data, out string ErrorMessage, RelogReason reason, bool allowFromTaskManager = false)
    {
        if(reason.EqualsAny(RelogReason.Overlay, RelogReason.Command, RelogReason.ConfigGUI))
        {
            if(C.MultiDisableOnRelog)
            {
                MultiMode.Enabled = false;
            }
            if(MultiMode.Active && !C.MultiNoPreferredReset)
            {
                foreach(var z in C.OfflineData)
                {
                    z.Preferred = false;
                }
                Notify.Warning("Preferred character has been reset");
            }
        }
        ErrorMessage = string.Empty;
        if(P.TaskManager.IsBusy && !allowFromTaskManager)
        {
            ErrorMessage = "AutoRetainer is processing tasks";
        }
        else if(SchedulerMain.CharacterPostProcessLocked)
        {
            ErrorMessage = "Currently in post-processing of character";
        }
        /*else if (data != null && !data.Index.InRange(1, 9))
        {
            ErrorMessage = "Invalid character index";
        }*/
        else
        {
            if(Player.Available)
            {
                if(IsOccupied())
                {
                    ErrorMessage = "Player is occupied";
                }
                else if(data != null && data.CID == Svc.ClientState.LocalContentId)
                {
                    ErrorMessage = "Targeted player is logged in";
                }
                else
                {
                    if(reason == RelogReason.MultiMode || C.AllowManualPostprocess)
                    {
                        TaskPostprocessCharacterIPC.Enqueue();
                    }
                    if(MultiMode.Enabled)
                    {
                        CharaCnt.IncrementOrSet(Svc.ClientState.LocalContentId);
                    }
                    else
                    {
                        CharaCnt.Clear();
                    }
                    if(data != null)
                    {
                        TaskChangeCharacter.Enqueue(data.CurrentWorld, data.Name, data.World, data.ServiceAccount);
                    }
                    else
                    {
                        TaskChangeCharacter.EnqueueLogout();
                    }
                    return true;
                }
            }
            else
            {
                if(Utils.CanAutoLogin() || (allowFromTaskManager && Utils.CanAutoLoginFromTaskManager()))
                {
                    TaskChangeCharacter.EnqueueLogin(data.CurrentWorld, data.Name, data.World, data.ServiceAccount);
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
        if(C.CharEqualize)
        {
            data = [.. data.OrderBy(x => CharaCnt.GetOrDefault(x.CID))];
        }
        if(C.MultiPreferredCharLast)
        {
            var pref = data.FirstOrDefault(x => x.Preferred);
            if(pref != null)
            {
                data.Remove(pref);
                data.Add(pref);
            }
        }
        if(EnabledSubmarines)
        {
            foreach(var x in data)
            {
                if(x.CID == Player.CID) continue;
                if(x.WorkshopEnabled && x.GetEnabledVesselsData(VoyageType.Airship).Count + x.GetEnabledVesselsData(VoyageType.Submersible).Count > 0)
                {
                    if(x.AreAnyEnabledVesselsReturnInNext(0, C.MultiModeWorkshopConfiguration.MultiWaitForAll))
                    {
                        return x;
                    }
                }
            }
            foreach(var x in data)
            {
                if(x.CID == Player.CID) continue;
                if(x.WorkshopEnabled && x.GetEnabledVesselsData(VoyageType.Airship).Count + x.GetEnabledVesselsData(VoyageType.Submersible).Count > 0)
                {
                    if(x.AreAnyEnabledVesselsReturnInNext(C.MultiModeWorkshopConfiguration.AdvanceTimer, C.MultiModeWorkshopConfiguration.MultiWaitForAll))
                    {
                        return x;
                    }
                }
            }
        }
        if(EnabledRetainers)
        {
            foreach(var x in data)
            {
                if(x.CID == Player.CID) continue;
                if(x.Enabled && C.SelectedRetainers.TryGetValue(x.CID, out var enabledRetainers))
                {
                    var selectedRetainers = x.GetEnabledRetainers().Where(z => z.HasVenture);
                    if(selectedRetainers.Any() &&
                        C.MultiModeRetainerConfiguration.MultiWaitForAll ? selectedRetainers.All(z => z.GetVentureSecondsRemaining() <= 0) : selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= 0))
                    {
                        return x;
                    }
                }
            }
            foreach(var x in data)
            {
                if(x.CID == Player.CID) continue;
                if(x.Enabled && C.SelectedRetainers.TryGetValue(x.CID, out var enabledRetainers))
                {
                    var selectedRetainers = x.GetEnabledRetainers().Where(z => z.HasVenture);
                    if(selectedRetainers.Any() &&
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
        if(!ProperOnLogin.PlayerPresent) return false;
        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            if(!EnabledRetainers) return true;
            if(!data.Enabled) return true;
            if(Utils.GetVenturesAmount() < 2 || !Utils.IsInventoryFree()) return true;
            return !IsAnySelectedRetainerFinishesWithin(5 * 60);
        }
        return false;
    }

    internal static bool IsCurrentCharacterCaptainDone()
    {
        if(!EnabledSubmarines) return true;
        if(!Data.WorkshopEnabled) return true;
        return !Data.AreAnyEnabledVesselsReturnInNext(5 * 60, Data.ShouldWaitForAllWhenLoggedIn());
    }

    internal static bool IsAnySelectedRetainerFinishesWithin(int seconds)
    {
        if(!ProperOnLogin.PlayerPresent) return false;
        if(!EnabledRetainers) return false;
        if(GetEnabledOfflineData().TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            var selectedRetainers = data.GetEnabledRetainers().Where(z => z.HasVenture);
            return selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= seconds);
        }
        return false;
    }

    internal static bool EnsureCharacterValidity(bool ro = false)
    {
        if(!ProperOnLogin.PlayerPresent) return false;
        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            if(Svc.ClientState.LocalPlayer.HomeWorld.Id == Svc.ClientState.LocalPlayer.CurrentWorld.Id && Utils.GetVenturesAmount() >= data.GetNeededVentureAmount() && Utils.IsInventoryFree() && Utils.GetReachableRetainerBell(true) != null)
            {
                return true;
            }
            if(!ro)
            {
                data.Enabled = false;
            }
        }
        return false;
    }
    internal static int GetNeededVentureAmount(this OfflineCharacterData data)
    {
        return data.GetEnabledRetainers().Length * 2;
    }

    internal static void PerformAutoStart()
    {
        EzSharedData.TryGet<object>("AutoRetainer.WasLoaded", out _, CreationMode.CreateAndKeep, new());
        for(var i = 0; i < C.AutoLoginDelay; i++)
        {
            var seconds = C.AutoLoginDelay - i;
            P.TaskManager.Enqueue(() => Svc.NotificationManager.AddNotification(new()
            {
                Content = $"Autostart in {seconds}!",
                InitialDuration = TimeSpan.FromSeconds(1),
                HardExpiry = DateTime.Now.AddSeconds(1),
                Type = NotificationType.Warning,
            }));
            P.TaskManager.EnqueueDelay(1000);
        }
        P.TaskManager.Enqueue(() =>
        {
            if(C.AutoLogin != "")
            {
                OfflineCharacterData data;
                if(C.AutoLogin == "~")
                {
                    data = C.OfflineData.FirstOrDefault(s => s.CID == C.LastLoggedInChara);
                }
                else
                {
                    data = C.OfflineData.First(s => $"{s.Name}@{s.World}" == C.AutoLogin);
                }
                if(data == null) return true;
                if(Utils.CanAutoLoginFromTaskManager())
                {
                    MultiMode.Relog(data, out var error, RelogReason.Command, true);
                    if(error == "")
                    {
                        return true;
                    }
                    else
                    {
                        DuoLog.Error($"Error during auto login: {error}");
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        });
        P.TaskManager.Enqueue(() =>
        {
            if(C.MultiAutoStart)
            {
                MultiMode.Enabled = true;
                BailoutManager.IsLogOnTitleEnabled = true;
            }
        });
    }
}
