using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Internal;

internal unsafe class Memory : IDisposable
{
    internal int LastSearchItem = -1;

    delegate ulong InteractWithObjectDelegate(TargetSystem* system, GameObject* obj, bool los);
    Hook<InteractWithObjectDelegate> InteractWithObjectHook;

    delegate byte GetIsGatheringItemGatheredDelegate(ushort item);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 8B D9 8B F9")]
    GetIsGatheringItemGatheredDelegate GetIsGatheringItemGathered;

    internal delegate byte AtkUnitBase_FireCallbackDelegate(AtkUnitBase* a1, int valueCount, AtkValue* values, byte updateState);
    [Signature("E8 ?? ?? ?? ?? 8B 4C 24 20 0F B6 D8", DetourName = nameof(FireCallbackDetour))]
    internal Hook<AtkUnitBase_FireCallbackDelegate> FireCallbackHook;

    internal bool IsGatheringItemGathered(uint item) => GetIsGatheringItemGathered((ushort)item) != 0;

    internal Memory()
    {
        SignatureHelper.Initialise(this, true);
        //FireCallbackHook?.Enable();
    }

    internal byte FireCallbackDetour(AtkUnitBase* a1, int valueCount, AtkValue* values, byte updateState)
    {
        if(a1->ID == 118 && valueCount == 2 && values[0].Int == 5)
        {
            PluginLog.Verbose($"Last search item: {values[1].Int}");
            LastSearchItem = values[1].Int;
        }
        return FireCallbackHook.Original(a1, valueCount, values, updateState);
    }

    internal ulong InteractWithObjectDetour(TargetSystem* system, GameObject* obj, bool los)
    {
        DuoLog.Information($"Interacted with {MemoryHelper.ReadSeStringNullTerminated((nint)obj->Name)}, los={los}");
        return InteractWithObjectHook.Original(system, obj, los);
    }

    internal void InstallInteractHook()
    {
        if (InteractWithObjectHook == null)
        {
            InteractWithObjectHook = Hook<InteractWithObjectDelegate>.FromAddress((nint)TargetSystem.Addresses.InteractWithObject.Value, InteractWithObjectDetour);
            InteractWithObjectHook.Enable();
        }
    }

    public void Dispose()
    {
        InteractWithObjectHook?.Disable();
        InteractWithObjectHook?.Dispose();
        FireCallbackHook?.Disable();
        FireCallbackHook?.Dispose();
    }
}
