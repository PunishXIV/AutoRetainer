using AutoRetainerAPI.Configuration;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskInteractWithNearestPanel
{
    internal static void Enqueue(bool interact = true)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskInteractWithNearestPanel)} interact={interact}");
        if (!VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType))
        {
            if ((!Houses.List.Contains(Svc.ClientState.TerritoryType) || Utils.GetNearestWorkshopEntrance(out _) == null) && (Data.TeleportToFCHouse || Data.TeleportToRetainerHouse))
            {
                HouseEnterTask.EnqueueTask(true);
            }
            else
            {
                TaskEnterWorkshop.EnqueueEnterWorkshop();
            }
        }
        P.TaskManager.Enqueue(() =>
        {
            if(VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Vector3.Distance(Player.Object.Position, obj.Position) > 4.25f)
            {
                P.TaskManager.EnqueueImmediate(VoyageScheduler.Lockon);
                P.TaskManager.EnqueueImmediate(VoyageScheduler.Approach);
                P.TaskManager.EnqueueImmediate(VoyageScheduler.AutomoveOffPanel);
            }
        }, "ApproachPanelIfNeeded");
        if(interact) P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
    }
}
