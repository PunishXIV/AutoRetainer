using AutoRetainer.UI.MainWindow;
using Dalamud.Plugin.Services;
using ECommons.LazyDataHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Services;
public class TitleScreenButton : IDisposable
{
    IReadOnlyTitleScreenMenuEntry TitleScreenMenuEntryButton;
    private TitleScreenButton()
    {
        if(C.UseTitleScreenButton)
        {
            Svc.Framework.Update += RegisterTitleIcon;
        }
    }

    private void RegisterTitleIcon(object f)
    {
        if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "autoretainer.png"), out var icon))
        {
            Svc.Framework.Update -= RegisterTitleIcon;
            var duplicate = icon.CreateWrapSharingLowLevelResource();
            Purgatory.Add(duplicate.Dispose);
            TitleScreenMenuEntryButton = Svc.TitleScreenMenu.AddEntry(Svc.PluginInterface.Manifest.Name, duplicate, () => P.AutoRetainerWindow.IsOpen = true);
        }
    }

    public void Dispose()
    {
        if(TitleScreenMenuEntryButton != null)
        {
            Svc.TitleScreenMenu.RemoveEntry(TitleScreenMenuEntryButton);
        }
    }
}
