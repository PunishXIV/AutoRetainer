using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Services;
#pragma warning disable
public unsafe sealed class DataMigrator
{
    private DataMigrator()
    {
        MigrateInventoryManagement();
    }

    void MigrateInventoryManagement()
    {
        if(C.IMMigrated) return;
        PluginLog.Warning($"Starting inventory management settings migration");
        C.DefaultIMSettings.IMEnableCofferAutoOpen = C.IMEnableCofferAutoOpen;
        C.DefaultIMSettings.IMEnableAutoVendor = C.IMEnableAutoVendor;
        C.DefaultIMSettings.IMEnableContextMenu = C.IMEnableContextMenu;
        C.DefaultIMSettings.IMSkipVendorIfRetainer = C.IMSkipVendorIfRetainer;
        C.DefaultIMSettings.IMAutoVendorHard = C.IMAutoVendorHard;
        C.DefaultIMSettings.IMAutoVendorHardIgnoreStack = C.IMAutoVendorHardIgnoreStack;
        C.DefaultIMSettings.IMAutoVendorSoft = C.IMAutoVendorSoft;
        C.DefaultIMSettings.IMProtectList = C.IMProtectList;
        C.DefaultIMSettings.IMAutoVendorHardStackLimit = C.IMAutoVendorHardStackLimit;
        C.DefaultIMSettings.IMDry = C.IMDry;
        C.DefaultIMSettings.IMEnableItemDesynthesis = C.IMEnableItemDesynthesis;
        C.DefaultIMSettings.IMEnableNpcSell = C.IMEnableNpcSell;
        C.DefaultIMSettings.AllowSellFromArmory = C.AllowSellFromArmory;
        C.IMMigrated = true;
    }
}