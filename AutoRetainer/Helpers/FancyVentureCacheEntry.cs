namespace AutoRetainer.Helpers;

internal record struct FancyVentureCacheEntry(string Entry, bool Avail, string Left)
{
    internal ulong CreationFrame = Svc.PluginInterface.UiBuilder.FrameCount;

    internal bool IsValid => Svc.PluginInterface.UiBuilder.FrameCount == CreationFrame;
}
