using AutoRetainer.NewScheduler;
using AutoRetainer.NewScheduler.Tasks;
using AutoRetainer.Offline;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.CircularBuffers;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using static AutoRetainer.Offline.OfflineDataManager;

namespace AutoRetainer.Multi;

internal unsafe static class MultiMode
{
    internal static bool Enabled = false;

    internal static bool Synchronize = false;
    internal static long NextInteractionAt { get; private set; } = 0;
    internal static ulong LastLongin = 0;
    internal static CircularBuffer<long> Interactions = new(5);

    internal static void Init()
    {
        ProperOnLogin.Register(delegate
        {
            WriteVentureAndInventory();
            if(LastLongin == Svc.ClientState.LocalContentId && Enabled)
            {
                DuoLog.Error("Multi mode disabled as it have detected duplicate login.");
                Enabled = false;
            }
            LastLongin = Svc.ClientState.LocalContentId;
            Interactions.Clear();
            if (Enabled && P.config.MultiAllowHET && ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType))
            {
                PluginLog.Debug($"ProperOnLogin: {Svc.ClientState.LocalPlayer}, residental area, scheduling HET");
                HouseEnterTask.EnqueueTask();
            }
        });
        if(ProperOnLogin.PlayerPresent)
        {
            LastLongin = Svc.ClientState.LocalContentId;
            WriteVentureAndInventory();
        }
    }

    internal static int GetAutoAfkOpt()
    {
        return ConfigModule.Instance()->GetIntValue(145);
    }

    internal static void Tick()
    {
        if (Enabled)
        {
            if(GetAutoAfkOpt() != 0)
            {
                DuoLog.Warning("Using Multi Mode requires Auto-afk option to be turned off");
                Enabled = false;
                return;
            }
            if (GenericHelpers.IsOccupied() || !ProperOnLogin.PlayerPresent)
            {
                BlockInteraction(3);
            }
            if (P.TaskManager.IsBusy)
            {
                return;
            }
            if(Interactions.Count() == Interactions.Capacity && Interactions.All(x => Environment.TickCount64 - x < 180000))
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
            if(ProperOnLogin.PlayerPresent && !AutoLogin.Instance.IsRunning)
            {
                if (!Utils.IsInventoryFree())
                {
                    if(P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
                    {
                        data.Enabled = false;
                    }
                }
            }
            if(ProperOnLogin.PlayerPresent && !AutoLogin.Instance.IsRunning && IsInteractionAllowed()
                && (!Synchronize || P.config.OfflineData.All(x => x.GetEnabledRetainers().All(z => z.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation))))
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
                        BlockInteraction(20);
                        EnsureCharacterValidity();
                        if(!Relog(next, out var error))
                        {
                            DuoLog.Error(error);
                        }
                        Interactions.PushBack(Environment.TickCount64);
                    }
                }
                else if(!IsOccupied() && AnyRetainersAvailable())
                {
                    DuoLog.Information($"1234");
                    EnsureCharacterValidity();
                    if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data) && data.Enabled)
                    {
                        TaskInteractWithNearestBell.Enqueue();
                        P.TaskManager.Enqueue(() => SchedulerMain.Enabled = true);
                        BlockInteraction(10);
                        Interactions.PushBack(Environment.TickCount64);
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
        foreach(var x in GetEnabledOfflineData())
        {
            foreach(var z in x.GetEnabledRetainers())
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
        NextInteractionAt = Environment.TickCount64 + seconds * (new Random().Next(800, 1200));
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
        ErrorMessage = String.Empty;
        if (!ProperOnLogin.PlayerPresent)
        {
            ErrorMessage = "Local player is not present";
        }
        else if (GenericHelpers.IsOccupied())
        {
            ErrorMessage = "Player is occupied";
        }
        else if(data.CID == Svc.ClientState.LocalContentId)
        {
            ErrorMessage = "Targeted player is logged in";
        }
        else if (AutoLogin.Instance.IsRunning)
        {
            ErrorMessage = "AutoLogin is already running";
        }
        else if (!data.Index.InRange(1,9))
        {
            ErrorMessage = "Invalid character index";
        }
        else if (!GameMain.IsInSanctuary() && !ExcelTerritoryHelper.IsSanctuary(Svc.ClientState.TerritoryType) && !P.config.BypassSanctuaryCheck)
        {
            ErrorMessage = "You are not in the sanctuary";
        }
        else
        {
            AutoLogin.Instance.SwapCharacter(data.World, data.CharaIndex, data.ServiceAccount);
            return true;
        }
        return false;
    }

    internal static OfflineCharacterData GetCurrentTargetCharacter()
    {
        foreach (var x in P.config.OfflineData)
        {
            if (x.CID == Svc.ClientState.LocalContentId) continue;
            if (x.Enabled && P.config.SelectedRetainers.TryGetValue(x.CID, out var enabledRetainers))
            {
                var selectedRetainers = x.GetEnabledRetainers().Where(z => z.HasVenture);
                if(selectedRetainers.Any() && 
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
            if (Svc.ClientState.LocalPlayer.HomeWorld.Id != Svc.ClientState.LocalPlayer.CurrentWorld.Id) return false;
            if(Utils.GetVenturesAmount() >= data.GetNeededVentureAmount() && Utils.IsInventoryFree() && GetNearbyBell() != null)
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
            if ((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル"))
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
