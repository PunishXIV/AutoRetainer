using Dalamud.Memory;

namespace AutoRetainer.Modules.Voyage
{
    [StructLayout(LayoutKind.Explicit, Size = Offsets.Airship.TimerSize)]
    internal unsafe struct AirshipTimer
    {
        [FieldOffset(Offsets.Airship.TimerTimeStamp)]
        internal uint TimeStamp;

        [FieldOffset(Offsets.Airship.TimerRawName)]
        internal fixed byte RawName[Offsets.Airship.TimerRawNameSize];

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
