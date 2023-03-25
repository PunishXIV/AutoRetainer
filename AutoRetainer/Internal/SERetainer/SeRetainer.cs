using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;

namespace AutoRetainer.Internal.SERetainer;

[StructLayout(LayoutKind.Explicit, Size = Size)]
public unsafe struct SeRetainer
{
    public const int Size = 0x48;

    [FieldOffset(0x00)]
    public ulong RetainerID;

    [FieldOffset(0x08)]
    private fixed byte _name[0x20];

    [FieldOffset(0x29)]
    public byte ClassJob;

    [FieldOffset(0x2A)]
    public byte Level;

    [FieldOffset(0x2C)]
    public uint Gil;

    [FieldOffset(0x38)]
    public uint VentureID;

    [FieldOffset(0x3C)]
    public uint VentureCompleteTimeStamp;

    public bool Available
        => ClassJob != 0;

    public DateTime VentureComplete
        => Utils.DateFromTimeStamp(VentureCompleteTimeStamp);

    public SeString Name
    {
        get
        {
            fixed (byte* name = _name)
            {
                return MemoryHelper.ReadSeStringNullTerminated((IntPtr)name);
            }
        }
    }
}
