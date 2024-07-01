namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public abstract class DebugUIEntry : NeoUIEntry
{
    public override string Path => $"Advanced/Debug/{GetType().Name.Replace("Debug", "")}";
    public override bool ShouldDisplay()
    {
        return C.Verbose;
    }
}
