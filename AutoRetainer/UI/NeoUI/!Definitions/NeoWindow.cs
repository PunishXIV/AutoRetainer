using AutoRetainer.UI.NeoUI.AdvancedEntries;
using AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
using AutoRetainer.UI.NeoUI.Experiments;
using AutoRetainer.UI.NeoUI.InventoryManagementEntries;
using AutoRetainer.UI.NeoUI.MultiModeEntries;
using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;

namespace AutoRetainer.UI.NeoUI;
public sealed class NeoWindow : Window
{
    private readonly NeoUIEntry[] Tabs =
    [
        new MainSettings(),
        new DeployablesTab(),
        new Keybinds(),

        new MultiModeCommon(),
        new MultiModeRetainers(),
        new MultiModeDeployables(),
        new MultiModeContingency(),
        new CharaOrder(),
        new Exclusions(),
        new MultiModeFPSLimiter(),

        new InventoryManagementTab(),

        new LoginOverlay(),
        new MiscTab(),

        ..ConfigFileSystemHelpers.CreateInstancesOf<ExperimentUIEntry>(),

        new LogTab(),
        new ExpertTab(),

        ..ConfigFileSystemHelpers.CreateInstancesOf<DebugUIEntry>(),
    ];

    internal ConfigFileSystem<ConfigFileSystemEntry> FileSystem;

    public NeoWindow() : base("AutoRetainer Configuration")
    {
        P.WindowSystem.AddWindow(this);
        this.SetMinSize();
        FileSystem = new(() => Tabs);
    }

    public void Reload()
    {
        FileSystem.Reload();
    }

    public override void Draw()
    {
        FileSystem.Draw(null);
    }
}
