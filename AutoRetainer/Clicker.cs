using AutoRetainer.Multi;
using ClickLib.Clicks;
using ECommons;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace AutoRetainer;

internal unsafe class Clicker
{
    static long NextClickAt = 0;
    internal static ActionType lastAction = ActionType.None;

    internal static bool IsClickAllowed()
    {
        return Environment.TickCount64 > NextClickAt;
    }

    internal static void RecordClickTime(int time = 750)
    {
        if (MultiMode.Enabled)
        {
            Scheduler.turbo = false;
        }
        if (Scheduler.turbo)
        {
            time = 100;
        }
        else
        {
            time = (int)(((float)new Random().Next((int)(time), (int)(time * 1.5))) / (((float)P.config.Speed)/100f));
        }
        time.ValidateRange(100, int.MaxValue);
        NextClickAt = Environment.TickCount64 + time;
    }

    internal static void SelectRetainerByName(string name)
    {
        if (name.IsNullOrEmpty())
        {
            throw new Exception($"Name can not be null or empty");
        }
        if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
        {
            var list = (AtkComponentNode*)retainerList->UldManager.NodeList[2];
            for(var i = 1u;i<P.retainerManager.Count + 1; i++)
            {
                var retainerEntry = (AtkComponentNode*)list->Component->UldManager.NodeList[i];
                var text = (AtkTextNode*)retainerEntry->Component->UldManager.NodeList[13];
                var nodeName = text->NodeText.ToString();
                PluginLog.Verbose($"Retainer {i} text {nodeName}");
                if(name == nodeName)
                {
                    PluginLog.Verbose($"Selecting {nodeName}");
                    if (IsClickAllowed())
                    {
                        VerifyClick(ActionType.SelectRetainer);
                        RecordClickTime();
                        ClickRetainerList.Using((IntPtr)retainerList).Select(list, retainerEntry, i - 1);
                        if (P.config.Verbose) Notify.Success($"Selected retainer {i} {nodeName}");
                    }
                    else
                    {
                        PluginLog.Error("Click isn't allowed yet");
                        if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
                    }
                }
            }
        }
    }

    static void VerifyClick(ActionType type)
    {
        if(type == lastAction)
        {
            P.DisablePlugin();
            var t = $"[{P.Name}] Emergency shutdown due to attempt to execute multiple actions of the same type";
            MultiMode.Enabled = false;
            Svc.Chat.PrintError(t);
            throw new Exception(t);
        }
        else
        {
            lastAction = type;
        }
    }

    internal static void SelectVentureMenu()
    {
        if(TryGetAddonByName<AddonSelectString>("SelectString", out var select) && IsAddonReady(&select->AtkUnitBase))
        {
            var textNode = ((AtkTextNode*)select->AtkUnitBase.UldManager.NodeList[3]);
            var text = textNode->NodeText.ToString();
            //PluginLog.Information($"Text: {text}, col={textNode->TextColor.R:X2} {textNode->TextColor.G:X2} {textNode->TextColor.B:X2} {textNode->TextColor.A:X2}");
            if (Utils.TryParseRetainerName(text, out _))
            {
                var step1 = (AtkTextNode*)select->AtkUnitBase
                    .UldManager.NodeList[2]
                    ->GetComponent()->UldManager.NodeList[6]
                    ->GetComponent()->UldManager.NodeList[3];
                if(!step1->NodeText.ToString().EqualsAny(Utils.GetAddonText(2385), P.config.EnableAssigningQuickExploration? Utils.GetAddonText(2386) : "-"))
                {
                    PluginLog.Error("SelectVentureMenu mismatch");
                    return;
                }
                if (!IsSelectItemEnabled(step1))
                {
                    PluginLog.Error("SelectVentureMenu item disabled");
                    return;
                }
                if (IsClickAllowed())
                {
                    VerifyClick(ActionType.SelectStringVenture);
                    RecordClickTime();
                    ClickSelectString.Using((IntPtr)select).SelectItem6();
                    if (P.config.Verbose) Notify.Success($"Clicked venture");
                }
                else
                {
                    PluginLog.Error("Click isn't allowed yet");
                    if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
                }
            }
            else
            {
                PluginLog.Error("SelectVentureMenu checks not passed");
            }
        }
    }

    internal static void SelectQuickVenture()
    {
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var select) && IsAddonReady(&select->AtkUnitBase))
        {
            var textNode = ((AtkTextNode*)select->AtkUnitBase.UldManager.NodeList[3]);
            var text = textNode->NodeText.ToString();
            if (text.Equals(Consts.RetainerAskCategoryText))
            {
                var step1 = (AtkTextNode*)select->AtkUnitBase
                    .UldManager.NodeList[2]
                    ->GetComponent()->UldManager.NodeList[3]
                    ->GetComponent()->UldManager.NodeList[3];
                if (!step1->NodeText.ToString().Equals(Consts.RetainerQuickExplorationText))
                {
                    PluginLog.Error("SelectQuickVenture mismatch");
                    return;
                }
                if (!IsSelectItemEnabled(step1))
                {
                    PluginLog.Error("SelectQuickVenture item disabled");
                    return;
                }
                if (IsClickAllowed())
                {
                    VerifyClick(ActionType.SelectStringVentureCategory);
                    RecordClickTime();
                    ClickSelectString.Using((IntPtr)select).SelectItem3();
                    if (P.config.Verbose) Notify.Success($"Clicked quick exploration");
                }
                else
                {
                    PluginLog.Error("Click isn't allowed yet");
                    if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
                }
            }
        }
    }

    internal static void ClickReassign(bool reassign = true)
    {
        if(TryGetAddonByName<AddonRetainerTaskResult>("RetainerTaskResult", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            var ventures = Utils.GetVenturesAmount();
            PluginLog.Verbose($"Ventures: {ventures}");
            if (ventures < 2)
            {
                PluginLog.Error("Not enough ventures");
                P.DisablePlugin();
                return;
            }
            if (!addon->ReassignButton->IsEnabled)
            {
                PluginLog.Error("Button disabled");
                return;
            }
            if (IsClickAllowed())
            {
                VerifyClick(ActionType.ReassignVenture);
                RecordClickTime();
                if (reassign)
                {
                    ClickRetainerTaskResult.Using((IntPtr)addon).Reassign();
                }
                else
                {
                    ClickRetainerTaskResult.Using((IntPtr)addon).Confirm();
                }
                if (P.config.Verbose) Notify.Success($"Clicked reassign");
            }
            else
            {
                PluginLog.Error("Click isn't allowed yet");
                if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
            }
        }
    }

    internal static void ClickRetainerTaskAsk()
    {
        if (TryGetAddonByName<AddonRetainerTaskAsk>("RetainerTaskAsk", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            var ventures = InventoryManager.Instance()->GetInventoryItemCount(21072);
            PluginLog.Verbose($"Ventures: {ventures}");
            if (ventures < 2)
            {
                PluginLog.Error("Not enough ventures");
                P.DisablePlugin();
                return;
            }
            if (!addon->AssignButton->IsEnabled)
            {
                PluginLog.Error("Button disabled");
                return;
            }
            if (IsClickAllowed())
            {
                VerifyClick(ActionType.ConfirmVenture);
                RecordClickTime();
                ClickLib.Clicks.ClickRetainerTaskAsk.Using((IntPtr)addon).Assign();
                if (P.config.Verbose) Notify.Success($"Clicked assign");
            }
            else
            {
                PluginLog.Error("Click isn't allowed yet");
                if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
            }
        }
    }

    internal static void SelectQuit()
    {
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var select) && IsAddonReady(&select->AtkUnitBase))
        {
            var textNode = ((AtkTextNode*)select->AtkUnitBase.UldManager.NodeList[3]);
            var text = textNode->NodeText.ToString();
            //PluginLog.Information($"Text: {text}, col={textNode->TextColor.R:X2} {textNode->TextColor.G:X2} {textNode->TextColor.B:X2} {textNode->TextColor.A:X2}");
            if (Utils.TryParseRetainerName(text, out _))
            {
                int? click = null;
                for(var i = 0; i < select->PopupMenu.PopupMenu.EntryCount; i++)
                {
                    if (Marshal.PtrToStringUTF8((IntPtr)select->PopupMenu.PopupMenu.EntryNames[i]).Equals(Utils.GetAddonText(2383)))
                    {
                        click = i;
                        break;
                    }
                }
                if(click == null)
                {
                    PluginLog.Error("Quit not found");
                    if (P.config.Verbose) Notify.Error("Quit not found");
                    return;
                }
                if (IsClickAllowed())
                {
                    VerifyClick(ActionType.SelectStringQuit);
                    RecordClickTime();
                    ClickSelectString.Using((IntPtr)select).SelectItem((ushort)click.Value);
                    if (P.config.Verbose) Notify.Success($"Clicked quit");
                }
                else
                {
                    PluginLog.Error("Click isn't allowed yet");
                    if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
                }
            }
            else
            {
                PluginLog.Error("SelectQuit checks not passed");
            }
        }
    }

    internal static void InteractWithNearestBell(out bool success)
    {
        success = false;
        if (IsClickAllowed())
        {
            foreach(var x in Svc.Objects)
            {
                if((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル"))
                {
                    if(Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < Utils.GetValidInteractionDistance(x) && ((GameObject*)x.Address)->GetIsTargetable())
                    {
                        if (IsClickAllowed())
                        {
                            if (x.Address == Svc.Targets.Target?.Address)
                            {
                                VerifyClick(ActionType.BellInteract);
                                RecordClickTime();
                                P.NoConditionEvent = true;
                                TargetSystem.Instance()->InteractWithObject((GameObject*)x.Address, false);
                                success = true;
                                if (P.config.Verbose) Notify.Success($"Interacted with bell");
                            }
                            else
                            {
                                Svc.Targets.SetTarget(x);
                                RecordClickTime(500);
                            }
                            break;
                        }
                        else
                        {
                            PluginLog.Error("Click isn't allowed yet");
                            if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
                        }
                    }
                }
            }
            if (P.config.Verbose) Notify.Success($"Interacted with nearest bell");
        }
        else
        {
            PluginLog.Error("Click isn't allowed yet");
            if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
        }
    }

    internal static bool ClickClose()
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
        {
            if (IsClickAllowed())
            {
                VerifyClick(ActionType.CloseRetainerWindow);
                RecordClickTime();
                P.NoConditionEvent = true;
                var v = stackalloc AtkValue[1]
                {
                    new()
                    {
                        Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                        Int = -1
                    }
                };
                retainerList->FireCallback(1, v);
                if (P.config.Verbose) Notify.Success($"Closing retainer window");
                return true;
            }
            else
            {
                PluginLog.Error("Click isn't allowed yet");
                if (P.config.Verbose) Notify.Error("Click isn't allowed yet");
            }
        }
        return false;
    }
}
