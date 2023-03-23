using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Configuration
{
    internal static class Migrator
    {
        internal static void MigrateGC()
        {
            if (P.config.EnableAutoGCHandin)
            {
                foreach (var x in P.config.OfflineData)
                {
                    if (x.EnableGCArmoryHandin)
                    {
                        x.GCDeliveryType = GCDeliveryType.Allow_Inventory_and_Armory_Chest;
                    }
                    else
                    {
                        x.GCDeliveryType = GCDeliveryType.Allow_Inventory_Only;
                    }
                }
                DuoLog.Warning($"GC Handin settings migrated");
            }
            EzConfig.Save();
        }
    }
}
