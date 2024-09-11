using ECommons.GameHelpers;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskInteractWithNearestPanel
{
    internal static void Enqueue(bool interact = true)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskInteractWithNearestPanel)} interact={interact}");
        if(!VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType))
        {
            TaskNeoHET.TryEnterWorkshop(() =>
            {
                Data.WorkshopEnabled = false;
                DuoLog.Error($"Due to failure to find workshop, character is excluded from processing deployables");
                P.TaskManager.Abort();
            });
        }
        P.TaskManager.Enqueue(() =>
        {
            if(VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Vector3.Distance(Player.Object.Position, obj.Position) > 4.25f)
            {
                P.TaskManager.BeginStack();
                P.TaskManager.Enqueue(VoyageScheduler.Lockon);
                P.TaskManager.Enqueue(VoyageScheduler.Approach);
                P.TaskManager.Enqueue(VoyageScheduler.AutomoveOffPanel);
                P.TaskManager.InsertStack();
            }
        }, "ApproachPanelIfNeeded");
        if(interact) P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
    }
}
