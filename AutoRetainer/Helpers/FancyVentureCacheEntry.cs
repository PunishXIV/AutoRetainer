namespace AutoRetainer.Helpers;

internal record struct FancyVentureCacheEntry(string Entry, bool Avail, string Left, string Right)
{
		internal ulong CreationFrame = Svc.PluginInterface.UiBuilder.FrameCount;

		internal bool IsValid => Svc.PluginInterface.UiBuilder.FrameCount == CreationFrame;
}
