using AutoRetainer.NewScheduler.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler.Handlers
{
    internal unsafe static class RetainerListHandlers
    {
        internal static bool? SelectRetainerByName(string name)
        {
            TaskWithdrawGil.forceCheck = false;
            if (name.IsNullOrEmpty())
            {
                throw new Exception($"Name can not be null or empty");
            }
            if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
            {
                var list = (AtkComponentNode*)retainerList->UldManager.NodeList[2];
                for (var i = 1u; i < P.retainerManager.Count + 1; i++)
                {
                    var retainerEntry = (AtkComponentNode*)list->Component->UldManager.NodeList[i];
                    var text = (AtkTextNode*)retainerEntry->Component->UldManager.NodeList[13];
                    var nodeName = text->NodeText.ToString();
                    //P.DebugLog($"Retainer {i} text {nodeName}");
                    if (name == nodeName)
                    {
                        if (Utils.GenericThrottle)
                        {
                            P.DebugLog($"Selecting {nodeName}");
                            ClickRetainerList.Using((IntPtr)retainerList).Select(list, retainerEntry, i - 1);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        internal static bool? CloseRetainerList()
        {
            if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
            {
                if (Utils.GenericThrottle)
                {
                    var v = stackalloc AtkValue[1]
                    {
                        new()
                        {
                            Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                            Int = -1
                        }
                    };
                    P.IsCloseActionAutomatic = true;
                    retainerList->FireCallback(1, v);
                    P.DebugLog($"Closing retainer window");
                    return true;
                }
            }
            return false;
        }
    }
}
