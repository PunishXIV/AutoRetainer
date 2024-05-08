using AutoRetainer.UI.NeoUI.AdvancedEntries;
using AutoRetainer.UI.NeoUI.MultiModeEntries;
using AutoRetainer.UI.NeoUI.StatisticsEntries;
using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public class NeoWindow : Window
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

				new LoginOverlay(),
				new MiscTab(),

				new LogTab(),
				new ExpertTab(),
				new DebugTab(),

				
				];

		internal ConfigFileSystem<ConfigFileSystemEntry> FileSystem;

		public NeoWindow() : base("AutoRetainer Configuration")
		{
				this.SetMinSize();
				FileSystem = new([.. Tabs.Where(x => x.ShouldDisplay())]);
				FileSystem.DrawButton = (x) =>
				{
						if (ImGui.Button("by Puni.sh", x))
						{
								ShellStart("https://puni.sh/");
						}
				};
		}

		public void Reload()
		{
				FileSystem.Reload();
		}

		public override void Draw()
		{
				NuiBuilder.Filter = FileSystem.Selector?.Filter ?? "";
				FileSystem.Draw(150f);
		}
}
