using Dalamud.Memory;

namespace AutoRetainer.Modules.Voyage;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Airship.StatusSize)]
internal unsafe struct AirshipStatus
{
		[FieldOffset(Offsets.Airship.StatusTimeStamp)]
		internal uint TimeStamp;

		[FieldOffset(Offsets.Airship.StatusRawName)]
		internal fixed byte RawName[Offsets.Airship.StatusRawNameSize];

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
