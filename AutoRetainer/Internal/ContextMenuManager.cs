using Dalamud.Game.Text.SeStringHandling;
using ECommons.ChatMethods;
using ECommons.ExcelServices;
using ECommons.Interop;
using Lumina.Excel.GeneratedSheets;
using UIColor = ECommons.ChatMethods.UIColor;

namespace AutoRetainer.Internal;

internal class ContextMenuManager
{
		private SeString Prefix = new SeStringBuilder().AddUiForeground(" ", 539).Build();

		public ContextMenuManager()
		{
				//ContextMenu.OnOpenInventoryContextMenu += ContextMenu_OnOpenInventoryContextMenu;
		}

		public void Dispose()
		{
				//ContextMenu.Dispose();
		}

		/*private void ContextMenu_OnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
		{
				if (!C.IMEnableContextMenu) return;
				var id = args.ItemId % 1000000;
				if (id != 0)
				{
						if (C.IMProtectList.Contains(id))
						{
								args.AddCustomItem(new(new SeStringBuilder().Append(Prefix).AddText("= Item has been protected =").Build(), delegate
								{
										if (IsKeyPressed([LimitedKeys.LeftControlKey, LimitedKeys.RightControlKey]) && IsKeyPressed([LimitedKeys.RightShiftKey, LimitedKeys.LeftShiftKey]))
										{
												var t = $"Item {ExcelItemHelper.GetName(id)} removed from protection list";
												Notify.Success(t);
												ChatPrinter.Red("[AutoRetainer] " + t);
												C.IMProtectList.Remove(id);
										}
										else
										{
												Notify.Error($"Hold both CTRL+SHIFT while clicking to remove protection from item");
										}
								}));
						}
						else
						{
								var data = Svc.Data.GetExcelSheet<Item>().GetRow(id);
								if (C.IMAutoVendorSoft.Contains(id))
								{
										args.AddCustomItem(new(new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from soft vendor list", (ushort)UIColor.Orange).Build(), delegate
										{
												C.IMAutoVendorSoft.Remove(id);
												Notify.Info($"Item {ExcelItemHelper.GetName(id)} removed from soft vendor list");
										}));
								}
								else if (data.PriceLow > 0)
								{
										args.AddCustomItem(new(new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to soft vendor list", (ushort)UIColor.Yellow).Build(), delegate
										{
												C.IMAutoVendorHard.Remove(id);
												C.IMAutoVendorSoft.Add(id);
												Notify.Success($"Item {ExcelItemHelper.GetName(id)} added to soft vendor list");
										}));
								}

								if (C.IMAutoVendorHard.Contains(id))
								{
										args.AddCustomItem(new(new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from hard vendor list", (ushort)UIColor.Orange).Build(), delegate
										{
												C.IMAutoVendorHard.Remove(id);
												Notify.Success($"Item {ExcelItemHelper.GetName(id)} removed from hard vendor list");
										}));
								}
								else if (data.PriceLow > 0)
								{
										args.AddCustomItem(new(new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to hard vendor list", (ushort)UIColor.Yellow).Build(), delegate
										{
												C.IMAutoVendorSoft.Remove(id);
												C.IMAutoVendorHard.Add(id);
												Notify.Success($"Item {ExcelItemHelper.GetName(id)} added to hard vendor list");
										}));
								}
								args.AddCustomItem(new(new SeStringBuilder().Append(Prefix).AddText("Protect item from auto actions").Build(), delegate
								{
										C.IMAutoVendorHard.Remove(id);
										C.IMAutoVendorSoft.Remove(id);
										C.IMProtectList.Add(id);
										Notify.Success($"{ExcelItemHelper.GetName(id)} added to protection list");
								}));
						}
				}
		}*/
}
