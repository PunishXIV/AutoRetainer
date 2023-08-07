using ClickLib.Structures;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Net.NetworkInformation;

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
        Hook?.Enable();
        Hook2?.Enable();
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
        Hook?.Disable();
        Hook?.Dispose();
        Hook2?.Disable();
        Hook2?.Dispose();
    }

    /*internal delegate nint AddonAirShipExploration_ReceiveEvent(nint a1, ushort a2, int a3, AtkEvent* a4, VoyageInputData* a5);
    [Signature("40 53 48 83 EC 20 0F B7 C2 49 8B D9 83 C0 F1", DetourName = nameof(Detour))]
    internal Hook<AddonAirShipExploration_ReceiveEvent> Hook;

    internal nint Detour(nint a1, ushort a2, int a3, AtkEvent* a4, VoyageInputData* a5)
    {
        PluginLog.Debug($"Detour: {a1:X16}, {a2}, {a3:X16}, {(nint)a4:X16}, {(nint)a5:X16}");
        PluginLog.Debug($"  Data: {(nint)a5->unk_8:X16}({*a5->unk_8:X16}), {a5->unk_16}, {a5->unk_24} | {a5->RawDumpSpan.ToArray().Print()}");
        var span = new Span<byte>((void*)*a5->unk_8, 0x40).ToArray().Select(x => $"{x:X2}");
        PluginLog.Debug($"  Data 2, {a5->unk_8s->unk_4}, {a5->unk_8s->unk_8},  :{string.Join(" ", span)}");
        var ret = Hook.Original(a1, a2, a3, a4, a5);
        if (a2 == 0x23)
        {
            PluginLog.Debug($"  Event: {(nint)a4->Node:X16}, {(nint)a4->Target:X16}, {(nint)a4->Listener:X16}, {a4->Param}, {(nint)a4->NextEvent:X16}, {a4->Type}, {a4->Unk29}, {a4->Flags}");
            DuoLog.Information($"{a1:X}, {a2:X}, {a3:X}, {(nint)a4:X}, {(nint)a5:X}/{*(nint*)a5:X} / {ret:X16}");
            var data = (nint)a5;
            var d1 = *(int*)(data + 16);
            var d2 = *(byte*)(data + 24);
            var d3 = *(nint*)(data + 168);
            DuoLog.Information($"    {d1:X}, {d2:X}, {d3:X}");
        }
        return ret;
    }*/

    [StructLayout(LayoutKind.Explicit)]
    public struct Str1
    {
        [FieldOffset(16)] public int Unk0;
        [FieldOffset(24)] public byte Unk1;
        [FieldOffset(0)] public Str2* Unk2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Str2
    {
        [FieldOffset(168)] public Str3* Unk0;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Str3
    {
        [FieldOffset(156)] public int Unk0;
    }

    internal delegate void Del(nint a1, nint a2, Str1* a3);
    //[Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B 78 10", DetourName = nameof(Detour))]
    internal Hook<Del> Hook;

    internal void Detour(nint a1, nint a2, Str1* a3)
    {
        Hook.Original(a1, a2, a3);
        var addr = *(nint*)(*(nint*)a3 + 168);
        PluginLog.Debug($"{(nint)a1:X16},{(nint)a2:X16},{(nint)a3:X16}\n{a3->Unk0}, {a3->Unk1}, {(nint)a3->Unk2}, {(nint)(a3->Unk2->Unk0):X16}\n{addr:X16}, {(a3->Unk2->Unk0->Unk0):X16}");
        //return ret;
    }

    internal delegate byte Del2(nint a1);
    //[Signature("48 85 C9 74 0D 8B 81 ?? ?? ?? ?? C1 E8 15", DetourName = nameof(Detour2))]
    internal Hook<Del2> Hook2;

    internal byte Detour2(nint a1)
    {
        var ret = Hook2.Original(a1);
        PluginLog.Debug($"{(nint)a1:X16}, {ret:X16} / {*(int*)(a1 + 156)}");
        return ret;
    }

    internal void Use(int num)
    {
        if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
        {
            var dummyEvent = stackalloc AtkEvent[] { new() };
            var str3 = stackalloc Str3[] { new() { Unk0 = 0x0FFFFFFF } };
            var str2 = stackalloc Str2[] { new() { Unk0 = str3 } };
            var inputData = stackalloc Str1[] {
                new()
                {
                    Unk0 = num,
                    Unk1 = 0,
                    Unk2 = str2,
                }
            };
            Detour((nint)addon, (nint)dummyEvent, inputData);
        }
    }
}
