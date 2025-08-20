using AutoRetainer.UI.NeoUI.AdvancedEntries;
using AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
using AutoRetainer.UI.NeoUI.Experiments;
using AutoRetainer.UI.NeoUI.InventoryManagementEntries;
using AutoRetainer.UI.NeoUI.MultiModeEntries;
using ECommons.Configuration;
using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
//ott
namespace AutoRetainer.UI.NeoUI;
public sealed class NeoWindow : Window
{
    private readonly NeoUIEntry[] Tabs =
    [
        new MainSettings(),
        new UserInterface(),
        new DeployablesTab(),
        new RetainersTab(),
        new Keybinds(),

        new MultiModeCommon(),
        new MultiModeRetainers(),
        new MultiModeDeployables(),
        new MultiModeContingency(),
        new CharaOrder(),
        new MultiModeFPSLimiter(),
        new MultiModeLockout(),

        ..ConfigFileSystemHelpers.CreateInstancesOf<InventoryManagementBase>(),

        new LoginOverlay(),
        new MiscTab(),

        ..ConfigFileSystemHelpers.CreateInstancesOf<ExperimentUIEntry>(),

        new LogTab(),
        new ExpertTab(),
        new CharacterSync(),

        ..ConfigFileSystemHelpers.CreateInstancesOf<DebugSectionBase>(),
    ];

    internal ConfigFileSystem FileSystem;

    public NeoUIEntry Selected;

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

        /*ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##selConf", Selected?.Path ?? "Select section", ImGuiComboFlags.HeightLarge))
        {
            foreach(var x in Tabs)
            {
                if(ImGui.Selectable(x.Path, ReferenceEquals(x, Selected)))
                {
                    Selected = x;
                }
                if(ReferenceEquals(x, Selected) && ImGui.IsWindowAppearing()) ImGui.SetScrollHereY();
            }
            ImGui.EndCombo();
        }
        Selected?.Draw();*/
    }

    public override void OnClose()
    {
        EzConfig.Save();
    }
    /*
    public static class ConfigFileSystemHelpers
    {
        public static IEnumerable<T?> CreateInstancesOf<T>()
        {
            var instances = typeof(T).Assembly.GetTypes().Where(x => !x.IsAbstract && typeof(T).IsAssignableFrom(x)).Select(x => (T?)Activator.CreateInstance(x, true));
            foreach(var i in instances)
            {
                yield return i;
            }
        }
    }
    */
}
