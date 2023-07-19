using AutoRetainerAPI.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal unsafe static class Safety
    {
        internal const string Value = "ForceEnableSafetyFeatures";

        internal static void Set(bool enabled)
        {
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).CreateSubKey("Software").CreateSubKey("AutoRetainer").SetValue(Value, enabled?1:0, RegistryValueKind.DWord);
        }

        internal static bool Get()
        {
            try
            {
                var reg = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey("Software")?.OpenSubKey("AutoRetainer")?.GetValue(Value);
                if (reg == null) return false;
                return (int)reg == 1;
            }
            catch (Exception e)
            {
                InternalLog.Verbose($"{e.Message}\n{e.StackTrace}");
            }
            return false;
        }

        internal static void Check()
        {
            if (P.config.UnsafeProtection || Get())
            {
                foreach(var x in P.config.OfflineData)
                {
                    if(x.GCDeliveryType == GCDeliveryType.Show_All_Items)
                    {
                        x.GCDeliveryType = GCDeliveryType.Hide_Armoury_Chest_Items;
                        Notify.Info($"Unsafe option removed: character {Censor.Character(x.Name)} - {nameof(GCDeliveryType.Show_All_Items)}");
                    }
                }
            }
        }
    }
}
