using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using ECommons.Automation.UIInput;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Services;
public unsafe class CharaListDebugger : IDisposable
{
    private CharaListDebugger()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "_CharaSelectListMenu", Handler);
    }

    private void Handler(AddonEvent type, AddonArgs args)
    {
        var evt = (AddonReceiveEventArgs)args;
        if(evt.AtkEventType.EqualsAny<byte>(33, 34)) return;
        PluginLog.Information($"""
            Event:
            Param: {evt.EventParam:X16}
            Num_: {Enumerable.Range(0, 40).Select(x => (byte)x).ToHexString()}
            Data: {MemoryHelper.ReadRaw(evt.Data, 40).ToHexString()}
            AtkType: {evt.AtkEventType}
            Flags: {((AtkEvent*)evt.AtkEvent)->Param}
            
            """);
    }

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PreReceiveEvent, "_CharaSelectListMenu", Handler);
    }
}
