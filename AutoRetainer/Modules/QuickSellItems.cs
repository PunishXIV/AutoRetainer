using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using PInvoke;
using System.Windows.Forms;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Modules;
#pragma warning disable CS0649
public unsafe class QuickSellItems : IDisposable
{
    internal delegate void* OpenInventoryContext(AgentInventoryContext* agent, InventoryType inventory, ushort slot, int a4, ushort a5, byte a6);
    [Signature("83 B9 ?? ?? ?? ?? ?? 7E 11", DetourName = nameof(OpenInventoryContextDetour), Fallibility = Fallibility.Fallible)]
    internal Hook<OpenInventoryContext> openInventoryContextHook;

    public InventoryType[] CanSellFrom = {
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
    };

    private string retainerSellText;
    private string entrustToRetainerText;
    private string retrieveFromRetainerText;
    private string putUpForSaleText;

    public QuickSellItems()
    {
        //5480	Have Retainer Sell Items
        retainerSellText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(5480)?.Text?.RawString ?? "Have Retainer Sell Items";
        //97	Entrust to Retainer
        entrustToRetainerText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(97)?.Text?.RawString ?? "Entrust to Retainer";
        //98	Retrieve from Retainer
        retrieveFromRetainerText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(98)?.Text?.RawString ?? "Retrieve from Retainer";
        //99	Put Up for Sale
        putUpForSaleText = Svc.Data.GetExcelSheet<Addon>()?.GetRow(99)?.Text?.RawString ?? "Put Up for Sale";
        SignatureHelper.Initialise(this);
        UiHelper.Setup();
        Toggle();
    }

    public void Enable()
    {
        if (openInventoryContextHook?.IsEnabled == false)
        {
            openInventoryContextHook?.Enable();
            PluginLog.Information("QuickSellItems enabled");
        }
    }

    internal static bool IsReadyToUse()
    {
        if (!Svc.Condition[ConditionFlag.OccupiedSummoningBell]) return false;
        if (!Svc.Targets.Target.IsRetainerBell()) return false;
        if (!Svc.Objects.Any(x => x.ObjectKind == ObjectKind.Retainer)) return false;
        { if (TryGetAddonByName<AtkUnitBase>("RetainerSellList", out var addon) && IsAddonReady(addon)) return true; }
        { if (TryGetAddonByName<AtkUnitBase>("RetainerGrid0", out var addon) && IsAddonReady(addon)) return true; }
        { if (TryGetAddonByName<AtkUnitBase>("RetainerGrid1", out var addon) && IsAddonReady(addon)) return true; }
        { if (TryGetAddonByName<AtkUnitBase>("RetainerGrid2", out var addon) && IsAddonReady(addon)) return true; }
        { if (TryGetAddonByName<AtkUnitBase>("RetainerGrid3", out var addon) && IsAddonReady(addon)) return true; }
        { if (TryGetAddonByName<AtkUnitBase>("RetainerGrid4", out var addon) && IsAddonReady(addon)) return true; }
        { if (TryGetAddonByName<AtkUnitBase>("RetainerCrystalGrid", out var addon) && IsAddonReady(addon)) return true; }
        return false;
    }

    internal bool GetAction(out List<string> text)
    {
        text = new();
        if (CSFramework.Instance()->WindowInactive) return false;
        if (Utils.IsKeyPressed(P.config.SellKey))
        {
            text.Add(retainerSellText);
        }
        if (Utils.IsKeyPressed(P.config.RetrieveKey))
        {
            text.Add(retrieveFromRetainerText);
        }
        if (Utils.IsKeyPressed(P.config.EntrustKey))
        {
            text.Add(entrustToRetainerText);
        }
        if (Utils.IsKeyPressed(P.config.SellMarketKey))
        {
            text.Add(putUpForSaleText);
        }
        return text.Count > 0;
    }

    private void* OpenInventoryContextDetour(AgentInventoryContext* agent, InventoryType inventoryType, ushort slot, int a4, ushort a5, byte a6)
    {
        var retVal = openInventoryContextHook.Original(agent, inventoryType, slot, a4, a5, a6);
        InternalLog.Verbose($"Inventory hook: {inventoryType}, {slot}");
        try
        {
            if (CanSellFrom.Contains(inventoryType) && IsReadyToUse() && GetAction(out var text))
            {
                var inventory = InventoryManager.Instance()->GetInventoryContainer(inventoryType);
                if (inventory != null)
                {
                    var itemSlot = inventory->GetInventorySlot(slot);
                    if (itemSlot != null)
                    {
                        var itemId = itemSlot->ItemID;
                        var item = Svc.Data.GetExcelSheet<Item>()?.GetRow(itemId);
                        if (item != null)
                        {
                            var addonId = agent->AgentInterface.GetAddonID();
                            if (addonId == 0) return retVal;
                            var addon = Common.GetAddonByID(addonId);
                            if (addon == null) return retVal;

                            for (var i = 0; i < agent->ContextItemCount; i++)
                            {
                                var contextItemParam = agent->EventParamsSpan[agent->ContexItemStartIndex + i];
                                if (contextItemParam.Type != ValueType.String) continue;
                                var contextItemName = contextItemParam.ValueString();

                                if (text.Contains(contextItemName))
                                {
                                    if (Bitmask.IsBitSet(agent->ContextItemDisabledMask, i))
                                    {
                                        P.DebugLog($"QRA found {i}:{contextItemName} but it's disabled");
                                        continue;
                                    }
                                    Common.GenerateCallback(addon, 0, i, 0U, 0, 0);
                                    agent->AgentInterface.Hide();
                                    UiHelper.Close(addon);
                                    P.DebugLog($"QRA Selected {i}:{contextItemName}");
                                    return retVal;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ex.Log();
        }

        return retVal;
    }

    public void Disable()
    {
        if (openInventoryContextHook?.IsEnabled == true)
        {
            openInventoryContextHook?.Disable();
            PluginLog.Information("QuickSellItems disabled");
        }
    }

    public void Toggle()
    {
        if (P.config.SellKey == Keys.None && P.config.RetrieveKey == Keys.None && P.config.EntrustKey == Keys.None && P.config.SellMarketKey == Keys.None)
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
