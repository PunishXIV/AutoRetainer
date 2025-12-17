using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Services;
public unsafe class EventLogger : IDisposable
{
    private string Addon = "FreeCompanyCreditShop";
    private EventLogger()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, Addon, Handler);
    }

    private void Handler(AddonEvent type, AddonArgs args)
    {
        var evt = (AddonReceiveEventArgs)args;
        if(evt.AtkEventType.EqualsAny<byte>(33, 34)) return;
        PluginLog.Information($"""
            Event:
            Param: {evt.EventParam:X16}
            Num_: {Enumerable.Range(0, 40).Select(x => (byte)x).ToHexString()}
            Data: {MemoryHelper.ReadRaw(evt.AtkEventData, 40).ToHexString()}
            AtkType: {evt.AtkEventType}
            Flags: {((AtkEvent*)evt.AtkEvent)->Param}
            
            """);
    }

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PreReceiveEvent, Addon, Handler);
    }
}
