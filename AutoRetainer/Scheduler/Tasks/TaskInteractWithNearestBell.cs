using AutoRetainer.Modules.Voyage;
using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal unsafe static class TaskInteractWithNearestBell
{
    internal static void Enqueue(bool interact = true)
    {
        P.TaskManager.Enqueue(NewYesAlreadyManager.WaitForYesAlreadyDisabledTask);
        P.TaskManager.Enqueue(() => 
        {
            if (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) && Utils.GetReachableRetainerBell(false) == null)
            {
                var bell = Utils.GetNearestRetainerBell(out var distance);
                if(distance < 20f)
                {
                    P.TaskManager.EnqueueImmediate(HouseEnterTask.LockonBell);
                    P.TaskManager.EnqueueImmediate(HouseEnterTask.Approach);
                    P.TaskManager.EnqueueImmediate(HouseEnterTask.AutorunOffBell);
                }
            }
        }, "ApproachWorkshopBell");
        P.TaskManager.Enqueue(PlayerWorldHandlers.SelectNearestBell);
        if (interact)
        {
            P.TaskManager.Enqueue(() => { P.IsInteractionAutomatic = true; }, "Mark interaction as automatic");
            P.TaskManager.Enqueue(PlayerWorldHandlers.InteractWithTargetedBell);
        }
    }
}
