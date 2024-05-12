using AutoRetainer.UI.NeoUI.AdvancedEntries;
using AutoRetainer.UI.NeoUI.InventoryManagementEntries;
using AutoRetainer.UI.NeoUI.MultiModeEntries;
using AutoRetainer.UI.NeoUI.StatisticsEntries;
using ECommons.Singletons;
using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;

namespace AutoRetainer.UI.NeoUI;
public sealed class NeoWindow : Window
{
		private readonly NeoUIEntry[] Tabs = [
				new MainSettings(),
				new DeployablesTab(),

				new MultiModeCommon(),
				new MultiModeRetainers(),
				new MultiModeDeployables(),
				new MultiModeContingency(),
				new CharaOrder(),
				new Exclusions(),

				new VentureStats(),
				new GilStats(),
				new FCPointsStats(),
				new InventoryManagementTab(),

				new LoginOverlay(),
				new MiscTab(),

				new LogTab(),
				new ExpertTab(),
				new DebugTab(),
				];

		internal ConfigFileSystem<ConfigFileSystemEntry> FileSystem;

		public NeoWindow() : base("AutoRetainer Configuration")
		{
				P.WindowSystem.AddWindow(this);
				this.SetMinSize();
				FileSystem = new(() => [.. Tabs.Where(x => x.ShouldDisplay())]);
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
