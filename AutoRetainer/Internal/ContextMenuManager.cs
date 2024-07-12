using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using ECommons.ChatMethods;
using ECommons.ExcelServices;
using ECommons.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Xml.Linq;
using UIColor = ECommons.ChatMethods.UIColor;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Internal;

internal unsafe class ContextMenuManager
{
    private SeString Prefix = new SeStringBuilder().AddUiForeground(" ", 539).Build();
    private SeIconChar DalamudPrefix = (SeIconChar)ushort.MaxValue;

    public ContextMenuManager()
    {
        Svc.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ContextMenu", OnContextMenuUpdate);
    }

    private void OnContextMenuUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AddonContextMenu*)args.Addon;
        var numEntries = addon->AtkValues[0].UInt;
        for (int i = 0; i < numEntries; i++)
        {
            var entry = addon->AtkValues[7 + i];
            if(entry.Type.EqualsAny(ValueType.String, ValueType.ManagedString, ValueType.String8))
            {
                var seString = MemoryHelper.ReadSeStringNullTerminated((nint)entry.String);
                for (int x = 0; x < seString.Payloads.Count; x++)
                {
                    {
                        if (seString.Payloads[x] is TextPayload payload && payload.Text.Length >= 1 && payload.Text[0] == (char)DalamudPrefix)
                        {
                            seString.Payloads.RemoveAt(x);
                            seString.Payloads.Add(new TextPayload("\0"));
                            break;
                        }
                    }
                }
                MemoryHelper.WriteSeString((nint)entry.String, seString);
            }
        }
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        if (!C.IMEnableContextMenu) return;
        if (args.MenuType == ContextMenuType.Inventory && args.Target is MenuTargetInventory inv && inv.TargetItem != null)
				{
            var id = inv.TargetItem.Value.ItemId;
            if (id != 0)
            {
                if (C.IMProtectList.Contains(id))
                {
                    args.AddMenuItem(new()
                    {
                        Prefix = DalamudPrefix,
                        Name = new SeStringBuilder().Append(Prefix).AddText("= Item has been protected =").Build(),
                        OnClicked = (a) =>
                        {
                            if (IsKeyPressed([LimitedKeys.LeftControlKey, LimitedKeys.RightControlKey]) && IsKeyPressed([LimitedKeys.RightShiftKey, LimitedKeys.LeftShiftKey]))
                            {
                                var t = $"Item {ExcelItemHelper.GetName(id)} removed from protection list";
                                Notify.Success(t);
                                ChatPrinter.Red("[AutoRetainer] " + t);
                                C.IMProtectList.Remove(id);
                            }
                            else
                            {
                                Notify.Error($"Hold both CTRL+SHIFT while clicking to remove protection from item");
                            }
                        }
                    });
                }
                else
                {
                    var data = Svc.Data.GetExcelSheet<Item>().GetRow(id);
                    if (C.IMAutoVendorSoft.Contains(id))
                    {
                        args.AddMenuItem(new()
                        {
                            Prefix = DalamudPrefix,
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from soft vendor list", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                C.IMAutoVendorSoft.Remove(id);
                                Notify.Info($"Item {ExcelItemHelper.GetName(id)} removed from soft vendor list");
                            }
                        });
                    }
                    else if (data.PriceLow > 0)
                    {
                        args.AddMenuItem(new()
                        {
                            Prefix = DalamudPrefix,
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to soft vendor list", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                C.IMAutoVendorHard.Remove(id);
                                C.IMAutoVendorSoft.Add(id);
                                Notify.Success($"Item {ExcelItemHelper.GetName(id)} added to soft vendor list");
                            }
                        });
                    }

                    if (C.IMAutoVendorHard.Contains(id))
                    {
                        args.AddMenuItem(new()
                        {
                            Prefix = DalamudPrefix,
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from hard vendor list", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                C.IMAutoVendorHard.Remove(id);
                                Notify.Success($"Item {ExcelItemHelper.GetName(id)} removed from hard vendor list");
                            }
                        });
                    }
                    else if (data.PriceLow > 0)
                    {
                        args.AddMenuItem(new()
                        {
                            Prefix = DalamudPrefix,
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to hard vendor list", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                C.IMAutoVendorSoft.Remove(id);
                                C.IMAutoVendorHard.Add(id);
                                Notify.Success($"Item {ExcelItemHelper.GetName(id)} added to hard vendor list");
                            }
                        });
                    }
                    args.AddMenuItem(new()
                    {
                        Prefix = DalamudPrefix,
                        Name = new SeStringBuilder().Append(Prefix).AddText("Protect item from auto actions").Build(),
                        OnClicked = (a) =>
                        {
                            C.IMAutoVendorHard.Remove(id);
                            C.IMAutoVendorSoft.Remove(id);
                            C.IMProtectList.Add(id);
                            Notify.Success($"{ExcelItemHelper.GetName(id)} added to protection list");
                        }
                    });
                }
            }
        }
    }

    public void Dispose()
    {
        Svc.ContextMenu.OnMenuOpened -= ContextMenu_OnMenuOpened;
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "ContextMenu", OnContextMenuUpdate);
    }
}
