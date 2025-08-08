using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Interop;
using ECommons.MathHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Modules;
#pragma warning disable CS0649
public unsafe class QuickSellItems : IDisposable
{
    //TODO: just remake this
    [Signature("83 B9 ?? ?? ?? ?? ?? 7E 11 39 91", DetourName = nameof(OpenInventoryContextDetour), Fallibility = Fallibility.Fallible)]
    internal Hook<AgentInventoryContext.Delegates.OpenForItemSlot> openInventoryContextHook;

    public InventoryType[] CanSellFrom = [
        InventoryType.Inventory1,
        InventoryType.Inventory2,
        InventoryType.Inventory3,
        InventoryType.Inventory4,
        InventoryType.ArmoryMainHand,
        InventoryType.ArmoryHead,
        InventoryType.ArmoryBody,
        InventoryType.ArmoryHands,
        InventoryType.ArmoryLegs,
        InventoryType.ArmoryFeets,
        InventoryType.ArmoryEar,
        InventoryType.ArmoryNeck,
        InventoryType.ArmoryWrist,
        InventoryType.ArmoryRings,
        InventoryType.ArmoryOffHand,
        InventoryType.RetainerPage1,
        InventoryType.RetainerPage2,
        InventoryType.RetainerPage3,
        InventoryType.RetainerPage4,
        InventoryType.RetainerPage5,
        InventoryType.RetainerPage6,
        InventoryType.RetainerPage7,
    ];

    private string retainerSellText;
    private string entrustToRetainerText;
    private string retrieveFromRetainerText;
    private string putUpForSaleText;

    public QuickSellItems()
    {
        //5480	Have Retainer Sell Items
        retainerSellText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(5480).Text.ToString() ?? "Have Retainer Sell Items";
        //97	Entrust to Retainer
        entrustToRetainerText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(97).Text.ToString() ?? "Entrust to Retainer";
        //98	Retrieve from Retainer
        retrieveFromRetainerText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(98).Text.ToString() ?? "Retrieve from Retainer";
        //99	Put Up for Sale
        putUpForSaleText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(99).Text.ToString() ?? "Put Up for Sale";
        Svc.Hook.InitializeFromAttributes(this);
        Toggle();

    }

    public void Enable()
    {
        if(openInventoryContextHook?.IsEnabled == false)
        {
            openInventoryContextHook?.Enable();
            PluginLog.Information("QuickSellItems enabled");
        }
    }

    internal static bool IsReadyToUse()
    {
        if(!Svc.Condition[ConditionFlag.OccupiedSummoningBell]) return false;
        if(!Svc.Targets.Target.IsRetainerBell()) return false;
        if(!Svc.Objects.Any(x => x.ObjectKind == ObjectKind.Retainer)) return false;
        { if(TryGetAddonByName<AtkUnitBase>("RetainerSellList", out var addon) && IsAddonReady(addon)) return true; }
        { if(TryGetAddonByName<AtkUnitBase>("RetainerGrid0", out var addon) && IsAddonReady(addon)) return true; }
        { if(TryGetAddonByName<AtkUnitBase>("RetainerGrid1", out var addon) && IsAddonReady(addon)) return true; }
        { if(TryGetAddonByName<AtkUnitBase>("RetainerGrid2", out var addon) && IsAddonReady(addon)) return true; }
        { if(TryGetAddonByName<AtkUnitBase>("RetainerGrid3", out var addon) && IsAddonReady(addon)) return true; }
        { if(TryGetAddonByName<AtkUnitBase>("RetainerGrid4", out var addon) && IsAddonReady(addon)) return true; }
        { if(TryGetAddonByName<AtkUnitBase>("RetainerCrystalGrid", out var addon) && IsAddonReady(addon)) return true; }
        return false;
    }

    internal bool GetAction(out List<string> text)
    {
        text = [];
        if(CSFramework.Instance()->WindowInactive) return false;
        if(IsKeyPressed(C.SellKey))
        {
            text.Add(retainerSellText);
        }
        if(IsKeyPressed(C.RetrieveKey))
        {
            text.Add(retrieveFromRetainerText);
        }
        if(IsKeyPressed(C.EntrustKey))
        {
            text.Add(entrustToRetainerText);
        }
        if(IsKeyPressed(C.SellMarketKey))
        {
            text.Add(putUpForSaleText);
        }
        return text.Count > 0;
    }

    private void OpenInventoryContextDetour(AgentInventoryContext* agent, InventoryType inventoryType, int slot, int a4, uint a5)
    {
        openInventoryContextHook.Original(agent, inventoryType, slot, a4, a5);
        InternalLog.Verbose($"Inventory hook: {inventoryType}, {slot}");
        try
        {
            if(CanSellFrom.Contains(inventoryType) && IsReadyToUse() && GetAction(out var text) && TryGetAddonMaster<AddonMaster.ContextMenu>(out var m) && m.IsAddonReady)
            {
                var item = InventoryManager.Instance()->GetInventoryContainer(inventoryType)->Items[slot];
                var data = Svc.Data.GetExcelSheet<Item>().GetRowOrDefault(item.ItemId);
                if(data.HasValue)
                {
                    foreach(var x in m.Entries)
                    {
                        if(x.Enabled && x.Text.EqualsAny(text))
                        {
                            x.Select();
                            break;
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            ex.Log();
        }
    }

    public void Disable()
    {
        if(openInventoryContextHook?.IsEnabled == true)
        {
            openInventoryContextHook?.Disable();
            PluginLog.Information("QuickSellItems disabled");
        }
    }

    public void Toggle()
    {
        if(C.SellKey == LimitedKeys.None && C.RetrieveKey == LimitedKeys.None && C.EntrustKey == LimitedKeys.None && C.SellMarketKey == LimitedKeys.None)
        {
            Disable();
        }
        else
        {
            Enable();
        }
    }

    public void Dispose()
    {
        openInventoryContextHook?.Dispose();
    }
}
