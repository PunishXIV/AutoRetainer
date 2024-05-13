namespace AutoRetainer.Modules.Voyage;

internal static class Offsets
{
		internal static class Submersible
		{
				internal const int TimerSize = 0x24;
				internal const int TimerTimeStamp = 0x00;
				internal const int TimerRawName = 0x08;
				internal const int TimerRawNameSize = 0x10;

				internal const int StatusSize = 0x3C;
				internal const int StatusTimeStamp = 0x08;
				internal const int StatusRawName = 0x16;
				internal const int StatusRawNameSize = 0x10;
		}

		internal static class Airship
		{
				internal const int TimerSize = 0x24;
				internal const int TimerTimeStamp = 0x00;
				internal const int TimerRawName = 0x06;
				internal const int TimerRawNameSize = 0x10;

				internal const int StatusSize = 0x24;
				internal const int StatusTimeStamp = 0x08;
				internal const int StatusRawName = 0x10;
				internal const int StatusRawNameSize = 0x10;
		}
}
