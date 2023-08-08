namespace AutoRetainer.Internal;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct AirshipExplorationInputData
{
    [FieldOffset(16)] public int Unk0;
    [FieldOffset(24)] public byte Unk1;
    [FieldOffset(0)] public AirshipExplorationInputData2* Unk2;
}