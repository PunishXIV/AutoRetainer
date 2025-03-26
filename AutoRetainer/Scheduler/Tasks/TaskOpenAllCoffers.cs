using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AutoRetainer.Scheduler.Tasks;

public static unsafe class TaskOpenAllCoffers
{
    public static void Enqueue()
    {
        P.TaskManager.Enqueue(RecursivelyOpenCoffers, new(timeLimitMS: 10 * 60 * 1000, abortOnTimeout: false));
        P.TaskManager.Enqueue(() => Utils.AnimationLock == 0);
    }

    public static bool? RecursivelyOpenCoffers()
    {
        var invManager = InventoryManager.Instance();
        if(invManager->GetInventoryItemCount(32161) == 0)
        {
            return true;
        }
        if(Utils.GetInventoryFreeSlotCount() < Math.Max(5, C.UIWarningRetSlotNum))
        {
            return true;
        }
        if(ActionManager.Instance()->GetActionStatus(ActionType.Item, 32161) == 0 && Utils.AnimationLock == 0)
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("AutoOpenCoffers", 1000))
            {
                OpenCoffer();
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static void OpenCoffer()
    {
        AgentInventoryContext.Instance()->UseItem(32161, (InventoryType)0x270F, 0, 0);
    }

}
