using ECommons.Configuration;

namespace AutoRetainer.Configuration;
public class ARDiscardMiniConfig : IEzConfig
{
    public List<uint> DiscardingItems;
    public List<uint> BlacklistedItems;
}
