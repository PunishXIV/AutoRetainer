using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Handlers
{
    internal unsafe static class RetainerListHandlers
    {
        internal static bool SelectRetainerByName(string name)
        {
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
                    PluginLog.Verbose($"Retainer {i} text {nodeName}");
                    if (name == nodeName)
                    {
                        if (Utils.GenericThrottle)
                        {
                            PluginLog.Verbose($"Selecting {nodeName}");
                            ClickRetainerList.Using((IntPtr)retainerList).Select(list, retainerEntry, i - 1);
                            if (P.config.Verbose) Notify.Success($"Selected retainer {i} {nodeName}");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        internal static bool CloseRetainerList()
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
                    retainerList->FireCallback(1, v);
                    PluginLog.Verbose($"Closing retainer window");
                    return true;
                }
            }
            return false;
        }
    }
}
