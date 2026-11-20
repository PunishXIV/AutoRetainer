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
            P.TaskManager.BeginStack();
            try
            {
                DebugLog($"SchedulerMain.CharacterPostprocess contains: {SchedulerMain.CharacterPostprocess.Print()}, pluginToProcess = {pluginToProcess}");
                foreach(var pname in SchedulerMain.CharacterPostprocess.Where(x => pluginToProcess == null || x == pluginToProcess))
                {
                    var x = pname;
                    P.TaskManager.Enqueue(() =>
                        {
                            SchedulerMain.CharacterPostprocess = SchedulerMain.CharacterPostprocess.Remove(x);
                            SchedulerMain.CharacterPostProcessLocked = x;
                            IPC.FireCharacterPostprocessEvent(x);
                        }, $"Character Postprocess request from {x}");
                    P.TaskManager.Enqueue(() => SchedulerMain.CharacterPostProcessLocked == null, $"Character Postprocess task from {x}", new(timeLimitMS: int.MaxValue));
                }
            }
            catch(Exception e) { e.Log(); }
            P.TaskManager.InsertStack();
        }, "TaskCharacterPostprocessProcessEntries");
    }
}
