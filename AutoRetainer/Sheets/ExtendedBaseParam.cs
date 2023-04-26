using Lumina.Data;
using Lumina.Excel;
using Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Sheets
{
    //This code is authored by Caraxi https://github.com/Caraxi/SimpleTweaksPlugin/
    [Sheet("BaseParam")]
    public class ExtendedBaseParam : Lumina.Excel.GeneratedSheets.BaseParam
    {
        public readonly ushort[] EquipSlotCategoryPct = new ushort[22];

        public override void PopulateData(RowParser parser, GameData gameData, Language language)
        {
            base.PopulateData(parser, gameData, language);
            for (var i = 1; i < EquipSlotCategoryPct.Length; i++)
            {
                EquipSlotCategoryPct[i] = parser.ReadColumn<ushort>(i + 3);
            }
        }
    }
}
