using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Helpers;

internal static unsafe class ItemLevel
{
    //Parts of this code is authored by Caraxi https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Tweaks/UiAdjustment/ExamineItemLevel.cs

    private static readonly uint[] canHaveOffhand = [2, 6, 8, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32];
    private static readonly uint[] ignoreCategory = [105];

    internal static int? Calculate(out int gathering, out int perception)
    {
        gathering = 0;
        perception = 0;
        var container = InventoryManager.Instance()->GetInventoryContainer(InventoryType.RetainerEquippedItems);
        if(container == null) return null;
        var sum = 0U;
        var c = 12;
        for(var i = 0; i < 13; i++)
        {
            if(i == 5) continue;
            var slot = container->GetInventorySlot(i);
            if(slot == null) continue;
            var id = slot->ItemId;
            if(!Svc.Data.GetExcelSheet<Item>().TryGetRow(id, out var item)) continue;
            if(ignoreCategory.ContainsNullable(item.ItemUICategory.RowId))
            {
                if(i == 0) c -= 1;
                c -= 1;
                continue;
            }

            if(i == 0 && !canHaveOffhand.ContainsNullable(item.ItemUICategory.RowId))
            {
                sum += item.LevelItem.RowId;
                i++;
            }

            gathering += slot->GetStat(BaseParamEnum.Gathering);
            perception += slot->GetStat(BaseParamEnum.Perception);

            sum += item.LevelItem.RowId;
        }

        var avgItemLevel = sum / c;
        return (int)avgItemLevel;
    }
}
