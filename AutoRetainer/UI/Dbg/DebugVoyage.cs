using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal static unsafe class DebugVoyage
    {
        internal static void Draw()
        {
            var data = HousingManager.Instance()->WorkshopTerritory->Submersible.DataListSpan;
            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                ImGuiEx.Text($"Sub: {MemoryHelper.ReadSeStringNullTerminated((nint)d.Name).ExtractText()}, returns at {d.GetReturnTime()}, current: {d.CurrentExp}");
            }
        }
    }
}
