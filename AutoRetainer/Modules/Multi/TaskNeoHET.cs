using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.StaticData;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Multi;
public static unsafe class TaskNeoHET
{
    public static readonly uint[] PrivateMarkers = Enum.GetValues<PrivateHousingMarker>().Select(x => (uint)x).ToArray();
    public static readonly uint[] SharedMarkers = Enum.GetValues<SharedHousingMarker>().Select(x => (uint)x).ToArray();
    public static readonly uint[] FcMarkers = Enum.GetValues<FCHousingMarker>().Select(x => (uint)x).ToArray();
    public static readonly uint[] ApartmentMarkers = Enum.GetValues<ApartmentHousingMarker>().Select(x => (uint)x).ToArray();
    public static readonly float ValidPlayerToApartmentDistance = 24f;

    public static void Enqueue(Action onFailure)
    {
        PluginLog.Debug($"Enqueued HouseEnterTask from {new StackTrace().GetFrames().Select(x => x.GetMethod()?.Name).Prepend("      ").Print("\n")}");
        P.TaskManager.EnqueueTask(NeoTasks.WaitForNotOccupied(new(timeLimitMS: 10 * 60 * 1000)));
        P.TaskManager.Enqueue(() =>
        {
            var entrance = GetFcOrPrivateEntranceFromMarkers();
            if(entrance != null && Player.DistanceTo(entrance) < 40f)
            {
                P.TaskManager.InsertMulti(
                    NeoTasks.WaitForNotOccupied(),
                    NeoTasks.ApproachObjectViaAutomove(GetFcOrPrivateEntranceFromMarkers, 4f),
                    NeoTasks.InteractWithObject(GetFcOrPrivateEntranceFromMarkers),
                    new(HouseEnterTask.SelectYesno),
                    new(HouseEnterTask.WaitUntilLeavingZone),
                    NeoTasks.WaitForScreenAndPlayer(),
                    new(NpcSaleManager.EnqueueIfItemsPresent),
                    new(() =>
                    {
                        if(GetWorkshopEntrance() != null && Utils.GetReachableRetainerBell(false) == null)
                        {
                            P.TaskManager.BeginStack();
                            try
                            {
                                TryEnterWorkshop(null);
                            }
                            catch(Exception e) { e.Log(); }
                            P.TaskManager.InsertStack();
                        }
                    })
                    );
            }
            else if(GetApartmentEntrance() != null && Player.DistanceTo(GetApartmentEntrance()) < 40f)
            {
                P.TaskManager.InsertMulti(
                NeoTasks.WaitForNotOccupied(),
                    NeoTasks.ApproachObjectViaAutomove(GetApartmentEntrance, 4f),
                    NeoTasks.InteractWithObject(GetApartmentEntrance),
                    new(HouseEnterTask.SelectYesno),
                    new(HouseEnterTask.WaitUntilLeavingZone),
                    NeoTasks.WaitForScreenAndPlayer()
                    );
            }
            else
            {
                onFailure?.Invoke();
            }
        }, "TaskNeoHET");
    }

    public static bool HasEntranceNearby() => GetApartmentEntrance() != null || GetFcOrPrivateEntranceFromMarkers() != null;

    public static IGameObject GetApartmentEntrance() => GetHouseEntranceFromMarkers(ApartmentMarkers);

    public static void TryEnterWorkshop(Action onFailure)
    {
        P.TaskManager.Enqueue(NpcSaleManager.EnqueueIfItemsPresent);
        P.TaskManager.Enqueue(() =>
        {
            if(GetWorkshopEntrance() != null)
            {
                P.TaskManager.InsertMulti(
                    NeoTasks.ApproachObjectViaAutomove(GetWorkshopEntrance, 4f),
                    NeoTasks.InteractWithObject(GetWorkshopEntrance),
                    new(TaskContinueHET.SelectEnterWorkshop),
                    new(() => VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType), "Wait Until entered workshop"),
                    NeoTasks.WaitForScreenAndPlayer()
                    );
            }
            else
            {
                onFailure?.Invoke();
            }
        }, "TryEnterWorkshop");
    }

    public static IGameObject GetWorkshopEntrance() => Svc.Objects.FirstOrDefault(x => x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(Lang.AdditionalChambersEntrance));

    public static IGameObject GetFcOrPrivateEntranceFromMarkers() => GetHouseEntranceFromMarkers([.. PrivateMarkers, .. FcMarkers, .. (C.SharedHET ? TaskNeoHET.SharedMarkers : [])]);

    public static IGameObject GetHouseEntranceFromMarkers(IEnumerable<uint> markers)
    {
        var hud = AgentHUD.Instance();
        if(hud->MapMarkers.Where(x => x.IconId.EqualsAny(markers)).OrderBy(x => Player.DistanceTo(new Vector2(x.X, x.Z))).TryGetFirst(out var marker))
        {
            var mpos = new Vector2(marker.X, marker.Z);
            var entrance = Svc.Objects.Where(x => x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny([.. Lang.Entrance, Lang.ApartmentEntrance])).OrderBy(x => Vector2.Distance(x.Position.ToVector2(), mpos)).FirstOrDefault(x => Vector2.Distance(mpos, x.Position.ToVector2()) < ValidPlayerToApartmentDistance);
            return entrance;
        }
        return null;
    }

    public static bool IsInMarkerHousingPlot(IEnumerable<uint> markers)
    {
        if(HousingManager.Instance()->GetCurrentPlot() < 0) return false;
        var hud = AgentHUD.Instance();
        if(hud->MapMarkers.Where(x => x.IconId.EqualsAny(markers)).TryGetFirst(x => Player.DistanceTo(new Vector2(x.X, x.Z)) < ValidPlayerToApartmentDistance, out var marker))
        {
            return true;
        }
        return false;
    }
}
