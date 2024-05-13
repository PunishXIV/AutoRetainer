namespace AutoRetainer.Internal;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct AirshipExplorationInputData
{
		[FieldOffset(0)] public AirshipExplorationInputData2* Unk2;
		[FieldOffset(16)] public int Unk0;
		[FieldOffset(24)] public byte Unk1;
}