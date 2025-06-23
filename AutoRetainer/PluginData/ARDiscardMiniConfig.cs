using ECommons.Configuration;

namespace AutoRetainer.PluginData;
public class ARDiscardMiniConfig : IEzConfig
{
    public List<uint> DiscardingItems;
    public List<uint> BlacklistedItems;
}
