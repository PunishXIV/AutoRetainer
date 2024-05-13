using Dalamud.Memory;

namespace AutoRetainer.Modules.Voyage;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Submersible.StatusSize)]
internal unsafe struct SubmersibleStatus
{
		[FieldOffset(Offsets.Submersible.StatusTimeStamp)]
		internal uint TimeStamp;

		[FieldOffset(Offsets.Submersible.StatusRawName)]
		internal fixed byte RawName[Offsets.Submersible.StatusRawNameSize];

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
