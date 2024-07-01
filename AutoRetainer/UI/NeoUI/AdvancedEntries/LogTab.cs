namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class LogTab : NeoUIEntry
{
    public override string Path => "Advanced/Log";

    public override void Draw()
    {
        InternalLog.PrintImgui();
    }
}
