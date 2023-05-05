using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Handlers
{
    internal unsafe static class GCHandlers
    {
        internal static bool? SetMaxVenturesExchange()
        {
            if(TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog", out var addon))
            {
                var numeric = (AtkComponentNumericInput*)addon->UldManager.NodeList[8]->GetComponent();
                numeric->SetValue(500);
                return true;
            }
            return false;
        }
    }
}
