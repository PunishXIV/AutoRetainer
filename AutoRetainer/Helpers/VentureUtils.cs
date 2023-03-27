using Dalamud.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Helpers
{
    internal static unsafe class VentureUtils
    {
        internal static void SelectVentureByID()
        {

        }

        internal static List<string> GetAvailableVentureNames()
        {
            List<string> ret = new();
            var data = CSFramework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.GetStringArrayData(95);
            if (data != null)
            {
                for (int i = 0; i < data->AtkArrayData.Size; i++)
                {
                    if (i % 4 != 1) continue;
                    var item = data->StringArray[i];
                    if (item != null)
                    {
                        var str = MemoryHelper.ReadSeStringNullTerminated((nint)item);
                        ret.Add(str.ExtractText());
                    }
                }
            }
            return ret;
        }
    }
}
