using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;
using OtterGui.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public class NeoUIEntry : ConfigFileSystemEntry
{
		public virtual NuiBuilder Builder { get; init; }

		public virtual bool ShouldDisplay() => true;

		public override Vector4? GetColor()
		{
				if(P.NeoWindow.FileSystem?.Selector?.Filter.IsNullOrEmpty() == false)
				{
						if (Path.SplitDirectories().Last().Contains(P.NeoWindow.FileSystem.Selector.Filter, StringComparison.OrdinalIgnoreCase)) return ImGuiColors.ParsedGreen;
						return Builder?.ShouldDraw == true ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudGrey3;
				}
				return null;
		}

		public override void Draw()
		{
				Builder.Draw();
		}
}
