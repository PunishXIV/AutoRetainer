using AutoRetainer.Scheduler.Handlers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Scheduler.Tasks;

public static unsafe class TaskAssignHuntingVenture
{
    public static void Enqueue(uint ventureId)
    {
        P.TaskManager.Enqueue(NewYesAlreadyManager.WaitForYesAlreadyDisabledTask);
        if(C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
        P.TaskManager.Enqueue(() => RetainerHandlers.GenericSelectByName(Lang.HuntingVentureNames), $"GenericSelectByName({Lang.HuntingVentureNames})");
        P.TaskManager.Enqueue(RetainerHandlers.WaitForVentureListUpdate);
        P.TaskManager.EnqueueDelay(Utils.FrameDelay, true);
        P.TaskManager.Enqueue(() => OpenAssignVentureWindow(ventureId));
        P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
    }

    public static bool OpenAssignVentureWindow(uint ventureId)
    {
        if(!FrameThrottler.Check("OpenAssignVentureWindow")) return false;
        if(TryGetAddonByName<AtkUnitBase>("RetainerTaskAsk", out _)) return true;
        if(TryGetAddonByName<AtkUnitBase>("RetainerTaskSupply", out var addon) && addon->IsReady())
        {
            for(var i = 0; i < addon->AtkValues[107].UInt; i++)
            {
                var ptr = (nint)addon->AtkValues[42 + i].Pointer;
                var id = *(uint*)ptr;
                if(id == ventureId && Utils.GenericThrottle)
                {
                    rethrottle();
                    Callback.Fire(addon, true, 5, i, Callback.ZeroAtkValue);
                    return false;
                }
            }
            if(Svc.Data.GetExcelSheet<RetainerTask>().TryGetRow(ventureId, out var task))
            {
                var level = task.RetainerLevel;
                var levelIndex = (level - 1) / 5;
                var listIndex = addon->AtkValues[40].Int - levelIndex - 1;
                if(listIndex >= 0 && Utils.GenericThrottle)
                {
                    rethrottle();
                    Callback.Fire(addon, true, 4, listIndex, Callback.ZeroAtkValue);
                    return false;
                }
            }
        }
        else
        {
            rethrottle();
        }

        return false;

        void rethrottle() => FrameThrottler.Throttle("OpenAssignVentureWindow", Utils.FrameDelay, true);
    }
}
