using Dalamud.Game;

namespace AutoRetainer.Internal.SERetainer;

public sealed class StaticRetainerContainer : SeAddressBase
{
    public StaticRetainerContainer(ISigScanner sigScanner)
        : base(sigScanner, "48 8B E9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 4E")
    { }
}
