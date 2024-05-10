using AutoRetainer.UI.Experiments.Inventory;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class InventoryManagementTab : NeoUIEntry
{
		public override string Path => "Inventory Management";
		public override void Draw()
		{
				InventoryManagement.Draw();
		}
}
