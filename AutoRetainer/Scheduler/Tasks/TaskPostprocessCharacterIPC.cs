namespace AutoRetainer.Scheduler.Tasks;

public class TaskPostprocessCharacterIPC
{
    internal static void Enqueue(string pluginToProcess = null)
    {
        P.TaskManager.Enqueue(() =>
        {
            SchedulerMain.CharacterPostprocess = SchedulerMain.CharacterPostprocess.Clear();
            IPC.FireCharacterPostprocessTaskRequestEvent();
        }, "TaskCharacterPostprocessIPCEnqueue");
        P.TaskManager.Enqueue(() =>
        {
            DebugLog($"SchedulerMain.CharacterPostprocess contains: {SchedulerMain.CharacterPostprocess.Print()}, pluginToProcess = {pluginToProcess}");
            foreach(var x in SchedulerMain.CharacterPostprocess.Where(x => pluginToProcess == null || x == pluginToProcess))
            {
                P.TaskManager.EnqueueImmediate(() =>
                    {
                        SchedulerMain.CharacterPostprocess = SchedulerMain.CharacterPostprocess.Remove(x);
                        SchedulerMain.CharacterPostProcessLocked = true;
                        IPC.FireCharacterPostprocessEvent(x);
                    }, $"Character Postprocess request from {x}");
                P.TaskManager.EnqueueImmediate(() => !SchedulerMain.CharacterPostProcessLocked, int.MaxValue, $"Character Postprocess task from {x}");
            }
        }, "TaskCharacterPostprocessProcessEntries");
    }
}
