using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.PluginData;
public unsafe sealed class InventoryManagementSettings
{
    public bool IMEnableCofferAutoOpen = false;
    public bool IMEnableAutoVendor = false;
    public bool IMEnableContextMenu = false;
    public bool IMSkipVendorIfRetainer = false;
    public List<uint> IMAutoVendorHard = [];
    public List<uint> IMAutoVendorHardIgnoreStack = [];
    public List<uint> IMAutoVendorSoft = [];
    public List<uint> IMProtectList = [];
    public int IMAutoVendorHardStackLimit = 20;
    public bool IMDry = false;
    public bool IMEnableItemDesynthesis = false;
    public bool IMEnableNpcSell = false;
    public bool AllowSellFromArmory = false;
}