using AutoRetainerAPI.Configuration;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Helpers
{
    public static unsafe class HousingUtils
    {
        public static bool TryGetCurrentDescriptor(out HouseDescriptor Descriptor)
        {
            try
            {
                var h = HousingManager.Instance();
                Descriptor = new(Svc.ClientState.TerritoryType, h->GetCurrentWard(), h->GetCurrentPlot());
                return true;
            }
            catch(ArgumentOutOfRangeException)
            {
                Descriptor = default;
                return false;
            }
        }

        public static HouseDescriptor GetCurrentDescriptor()
        {

            var h = HousingManager.Instance();
            return new(Svc.ClientState.TerritoryType, h->GetCurrentWard(), h->GetCurrentPlot(), true);
        }

        public static bool IsInThisHouse(this HouseDescriptor Descriptor)
        {
            if (TryGetCurrentDescriptor(out var d) && d == Descriptor) return true;
            return false;
        }
    }
}
