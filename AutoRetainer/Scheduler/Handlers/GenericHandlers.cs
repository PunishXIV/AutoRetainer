using ECommons.Throttlers;

namespace AutoRetainer.Scheduler.Handlers;

internal static class GenericHandlers
{
		internal static bool? Throttle(int ms)
		{
				return EzThrottler.Throttle("AutoRetainerWait", ms);
		}

		internal static bool? WaitFor(int ms)
		{
				return EzThrottler.Check("AutoRetainerWait");
		}
}
