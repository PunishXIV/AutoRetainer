using Dalamud.Memory;

namespace AutoRetainer.Modules.Voyage
{
    [StructLayout(LayoutKind.Explicit, Size = Offsets.Submersible.TimerSize)]
    internal unsafe struct SubmersibleTimer
    {
        [FieldOffset(Offsets.Submersible.TimerTimeStamp)]
        internal uint TimeStamp;

        [FieldOffset(Offsets.Submersible.TimerRawName)]
        internal fixed byte RawName[Offsets.Submersible.TimerRawNameSize];

        internal string Name
        {
            get
            {
                fixed (byte* name = RawName)
                {
                    return MemoryHelper.ReadStringNullTerminated((IntPtr)name);
                }
            }
        }

    }
}
