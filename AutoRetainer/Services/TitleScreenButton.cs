using ECommons.LazyDataHelpers;
using System.IO;

namespace AutoRetainer.Services;
public class TitleScreenButton : IDisposable
{
    private IReadOnlyTitleScreenMenuEntry TitleScreenMenuEntryButton;
    private TitleScreenButton()
    {
        if(C.UseTitleScreenButton)
        {
            Svc.Framework.Update += RegisterTitleIcon;
        }
    }

    private void RegisterTitleIcon(object f)
    {
        Svc.Framework.Update -= RegisterTitleIcon;
        var tex = Svc.Texture.GetFromGame(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "autoretainer.png"));
        TitleScreenMenuEntryButton = Svc.TitleScreenMenu.AddEntry(Svc.PluginInterface.Manifest.Name, tex, () => P.AutoRetainerWindow.IsOpen = true);
    }

    public void Dispose()
    {
        if(TitleScreenMenuEntryButton != null)
        {
            Svc.TitleScreenMenu.RemoveEntry(TitleScreenMenuEntryButton);
        }
    }
}
