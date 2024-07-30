using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using ECommons.EzIpcManager;
using ECommons.Reflection;
using System.Reflection;

namespace AutoRetainer.Modules;

internal static class IPC
{
    private static void Log(string s) => DebugLog($"[IPC] {s}");
    internal static bool Suppressed = false;

    internal static void Init()
    {
        Log("IPC init");
        Svc.PluginInterface.GetIpcProvider<object>("AutoRetainer.Init").RegisterAction(() => { });
        Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").RegisterFunc(GetSuppressed);
        Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").RegisterAction(SetSuppressed);
        Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetMultiModeEnabled").RegisterFunc(GetMultiModeEnabled);
        Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetMultiModeEnabled").RegisterAction(SetMultiModeEnabled);
        Svc.PluginInterface.GetIpcProvider<uint, object>("AutoRetainer.SetVenture").RegisterAction(SetVenture);
        Svc.PluginInterface.GetIpcProvider<ulong, OfflineCharacterData>("AutoRetainer.GetOfflineCharacterData").RegisterFunc(GetOCD);
        Svc.PluginInterface.GetIpcProvider<OfflineCharacterData, object>("AutoRetainer.WriteOfflineCharacterData").RegisterAction(SetOCD);
        Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData>("AutoRetainer.GetAdditionalRetainerData").RegisterFunc(GetARD);
        Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData, object>("AutoRetainer.WriteAdditionalRetainerData").RegisterAction(SetARD);
        Svc.PluginInterface.GetIpcProvider<List<ulong>>("AutoRetainer.GetRegisteredCIDs").RegisterFunc(GetRegisteredCIDs);
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.RequestRetainerPostProcess).RegisterAction(RequestRetainerPostprocess);
        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.FinishRetainerPostprocessRequest).RegisterAction(FinishRetainerPostprocessRequest);
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.RequestCharacterPostProcess).RegisterAction(RequestCharacterPostprocess);
        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.FinishCharacterPostprocessRequest).RegisterAction(FinishCharacterPostprocessRequest);
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.OnRetainerListCustomTask).RegisterAction(OnRetainerListCustomTask);
        EzIPC.Init(typeof(IPC));
    }

    private static void OnRetainerListCustomTask(string s)
    {
        P.RetainerListOverlay.PluginToProcess = s;
    }

    internal static void Shutdown()
    {
        Log("IPC Shutdown");
        Svc.PluginInterface.GetIpcProvider<object>("AutoRetainer.Init").UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetMultiModeEnabled").UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetMultiModeEnabled").UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<uint, object>("AutoRetainer.SetVenture").UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<ulong, OfflineCharacterData>("AutoRetainer.GetOfflineCharacterData").UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<OfflineCharacterData, object>("AutoRetainer.WriteOfflineCharacterData").UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData>("AutoRetainer.GetAdditionalRetainerData").UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData, object>("AutoRetainer.WriteAdditionalRetainerData").UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<List<ulong>>("AutoRetainer.GetRegisteredCIDs").UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.RequestRetainerPostProcess).UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.FinishRetainerPostprocessRequest).UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.RequestCharacterPostProcess).UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.FinishCharacterPostprocessRequest).UnregisterAction();
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.OnRetainerListCustomTask).UnregisterAction();
    }

    private static void FinishRetainerPostprocessRequest()
    {
        Log("Received retainer postprocess request finish");
        SchedulerMain.RetainerPostProcessLocked = false;
    }

    private static void FinishCharacterPostprocessRequest()
    {
        Log("Received character postprocess request finish");
        SchedulerMain.CharacterPostProcessLocked = false;
    }

    private static void RequestRetainerPostprocess(string pluginName)
    {
        if(SchedulerMain.RetainerPostprocess.Contains(pluginName))
        {
            throw new Exception($"Retainer Postprocess request from {pluginName} already exist");
        }
        SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Add(pluginName);
        Log($"Retainer Postprocess requested from {pluginName}");
    }

    private static void RequestCharacterPostprocess(string pluginName)
    {
        if(SchedulerMain.CharacterPostprocess.Contains(pluginName))
        {
            throw new Exception($"Character Postprocess request from {pluginName} already exist");
        }
        SchedulerMain.CharacterPostprocess = SchedulerMain.CharacterPostprocess.Add(pluginName);
        Log($"Character Postprocess requested from {pluginName}");
    }

    private static List<ulong> GetRegisteredCIDs()
    {
        return C.OfflineData.Where(x => !C.Blacklist.Any(z => z.CID == x.CID) && !x.Name.EqualsAny("Unknown", "")).Select(x => x.CID).ToList();
    }

    private static OfflineCharacterData GetOCD(ulong CID)
    {
        return C.OfflineData.FirstOrDefault(x => x.CID == CID);
    }

    private static void SetOCD(OfflineCharacterData OCD)
    {
        var index = C.OfflineData.IndexOf(x => x.CID == OCD.CID);
        if(index != -1)
        {
            //C.OfflineData[index] = OCD;
            var data = C.OfflineData[index];
            foreach(var field in OCD.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if(data.GetFoP(field.Name) != null)
                {
                    data.SetFoP(field.Name, field.GetValue(OCD));
                    PluginLog.Verbose($"Setting {field.Name} to {field.GetValue(data)}");
                }
            }
        }
        else
        {
            C.OfflineData.Add(OCD);
        }
    }

    private static AdditionalRetainerData GetARD(ulong cid, string name)
    {
        return Utils.GetAdditionalData(cid, name);
    }

    private static void SetARD(ulong cid, string name, AdditionalRetainerData data)
    {
        var x = C.AdditionalData[Utils.GetAdditionalDataKey(cid, name)];
        foreach(var field in data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if(x.GetFoP(field.Name) != null)
            {
                x.SetFoP(field.Name, field.GetValue(data));
                PluginLog.Verbose($"Setting {field.Name} to {field.GetValue(data)}");
            }
        }
    }

    private static void SetVenture(uint VentureID)
    {
        SchedulerMain.VentureOverride = VentureID;
        DebugLog($"Received venture override to {VentureID} / {VentureUtils.GetVentureName(VentureID)} via IPC");
    }

    private static bool GetSuppressed()
    {
        return Suppressed;
    }

    private static void SetSuppressed(bool s)
    {
        Suppressed = s;
    }

    private static bool GetMultiModeEnabled() => MultiMode.Enabled;
    private static void SetMultiModeEnabled(bool s)
    {
        MultiMode.Enabled = s;
        MultiMode.OnMultiModeEnabled();
    }

    internal static void FireSendRetainerToVentureEvent(string retainer)
    {
        Log($"Firing FireSendRetainerToVentureEvent for {retainer}");
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.OnSendRetainerToVenture).SendMessage(retainer);
    }

    internal static void FireRetainerPostprocessTaskRequestEvent(string retainer)
    {
        Log($"Firing FireRetainerPostprocessTaskRequestEvent for {retainer}");
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.OnRetainerAdditionalTask).SendMessage(retainer);
    }

    internal static void FireRetainerPostprocessEvent(string pluginName, string retainer)
    {
        Log($"Firing FireRetainerPostprocessEvent for {retainer} for plugin {pluginName}");
        Svc.PluginInterface.GetIpcProvider<string, string, object>(ApiConsts.OnRetainerReadyForPostprocess).SendMessage(pluginName, retainer);
    }

    internal static void FireCharacterPostprocessTaskRequestEvent()
    {
        Log($"Firing FireCharacterPostprocessTaskRequestEvent");
        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnCharacterAdditionalTask).SendMessage();
    }

    internal static void FireCharacterPostprocessEvent(string pluginName)
    {
        Log($"Firing FireCharacterPostprocessEvent for plugin {pluginName}");
        Svc.PluginInterface.GetIpcProvider<string, object>(ApiConsts.OnCharacterReadyForPostprocess).SendMessage(pluginName);
    }
}
