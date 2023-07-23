using AutoRetainerAPI.Configuration;
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
            if (C.EnableAutoGCHandin)
            {
                foreach (var x in C.OfflineData)
                {
                    if (x.EnableGCArmoryHandin)
                    {
                        x.GCDeliveryType = GCDeliveryType.Hide_Gear_Set_Items;
                    }
                    else
                    {
                        x.GCDeliveryType = GCDeliveryType.Hide_Armoury_Chest_Items;
                    }
                }
                DuoLog.Warning($"GC Handin settings migrated");
            }
            EzConfig.Save();
        }
    }
}
