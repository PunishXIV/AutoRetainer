namespace AutoRetainer.Structs;

[StructLayout(LayoutKind.Sequential, Size = SeRetainer.Size * 10 + 12)]
public unsafe struct RetainerContainer
{
    public fixed byte Retainers[SeRetainer.Size * 10];
    public fixed byte DisplayOrder[10];
    public byte Ready;
    public byte RetainerCount;
}
