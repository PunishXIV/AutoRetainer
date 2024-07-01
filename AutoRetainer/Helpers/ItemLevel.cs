using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Helpers;

internal static unsafe class ItemLevel
{
		//This code is authored by Caraxi https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Tweaks/UiAdjustment/ExamineItemLevel.cs

		private static readonly uint[] canHaveOffhand = [2, 6, 8, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32];
		private static readonly uint[] ignoreCategory = [105];

		internal static int? Calculate(out int gathering, out int perception)
		{
				gathering = 0;
				perception = 0;
				var container = InventoryManager.Instance()->GetInventoryContainer(InventoryType.RetainerEquippedItems);
				if (container == null) return null;
				var sum = 0U;
				var c = 12;
				for (var i = 0; i < 13; i++)
				{
						if (i == 5) continue;
						var slot = container->GetInventorySlot(i);
						if (slot == null) continue;
						var id = slot->ItemId;
						var item = Svc.Data.Excel.GetSheet<ECommons.ExcelServices.Sheets.ExtendedItem>()?.GetRow(id);
						if (item == null) continue;
						if (ignoreCategory.Contains(item.ItemUICategory.Row))
						{
								if (i == 0) c -= 1;
								c -= 1;
								continue;
						}

						if (i == 0 && !canHaveOffhand.Contains(item.ItemUICategory.Row))
						{
								sum += item.LevelItem.Row;
								i++;
						}

						var bonusGathering = 0;

						var bonusPerception = 0;

						for (var j = 0; j < item.BaseParam.Length; j++)
						{
								var baseParam = item.BaseParam[j];
								if (baseParam.BaseParam.Value?.RowId == 72)
								{
										bonusGathering += baseParam.Value;
								}
								if (baseParam.BaseParam.Value?.RowId == 73)
								{
										bonusPerception += baseParam.Value;
								}
						}
						if (slot->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))
						{
								for (var j = 0; j < item.BaseParamSpecial.Length; j++)
								{
										var baseParam = item.BaseParamSpecial[j];
										if (baseParam.BaseParam.Value?.RowId == 72)
										{
												bonusGathering += baseParam.Value;
										}
										if (baseParam.BaseParam.Value?.RowId == 73)
										{
												bonusPerception += baseParam.Value;
										}
								}
						}
						for (int j = 0; j < 5; j++)
						{
								var materia = slot->Materia[j];
								var materiag = slot->MateriaGrades[j];
								if (materia != 0)
								{
										var m = Svc.Data.GetExcelSheet<Materia>().GetRow(materia);
										if (m != null && m.BaseParam.Value?.RowId == 72)
										{
												bonusGathering += m.Value[materiag];
										}
										if (m != null && m.BaseParam.Value?.RowId == 73)
										{
												bonusPerception += m.Value[materiag];
										}
								}
						}
						gathering += bonusGathering;
						perception += bonusPerception;

						sum += item.LevelItem.Row;
				}

				var avgItemLevel = sum / c;
				return (int)avgItemLevel;
		}
}
