using ECommons.Automation.LegacyTaskManager;
using ECommons.WindowsFormsReflector;

namespace AutoRetainer.Modules;

internal static class ApiTest
{
    internal static bool Enabled = false;
    internal static TaskManager TaskManager;

    internal static void Init()
    {
        P.API.OnRetainerPostprocessStep += API_OnRetainerPostprocessTask;
        P.API.OnRetainerReadyToPostprocess += API_OnRetainerReadyToPostprocess;
        TaskManager = new();
    }

    private static void API_OnRetainerPostprocessTask(string retainerName)
    {
        if(!Enabled) return;
        PluginLog.Information($"Now requesting postprocess for {retainerName}");
        P.API.RequestRetainerPostprocess();
    }

    private static void API_OnRetainerReadyToPostprocess(string retainerName)
    {
        PluginLog.Information($"Now postprocessing {retainerName}");
        TaskManager.Enqueue(() =>
        {
            if(GenericHelpers.IsKeyPressed(Keys.Back))
            {
                return true;
            }
            return false;
        }, int.MaxValue);
        TaskManager.Enqueue(P.API.FinishRetainerPostProcess);
    }
}
