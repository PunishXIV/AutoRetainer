
#nullable enable


using ClickLib.Clicks;
using Dalamud.Game;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.CodeDom;
using System.Diagnostics;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Modules.Multi;

//Part of code is authored by Caraxi https://github.com/Caraxi/AutoLogin

internal unsafe static class AutoLogin
{
    static TaskManager AutoLoginTaskManager;

    internal static void Initialize()
    {
        AutoLoginTaskManager = new()
        {
            TimeLimitMS = 60 * 1000
        };
    }

    internal static bool IsRunning => AutoLoginTaskManager.IsBusy;
    internal static void Abort() => AutoLoginTaskManager.Abort();

    internal static void Logoff()
    {
        AutoLoginTaskManager.Abort();
        AutoLoginTaskManager.Enqueue(Logout);
        AutoLoginTaskManager.Enqueue(SelectYesLogout);
    }

    internal static void Login(string WorldName, uint characterIndex, int serviceAccount)
    {
        var world = Svc.Data.Excel.GetSheet<World>()?.FirstOrDefault(w => w.Name.ToDalamudString().TextValue.Equals(WorldName, StringComparison.InvariantCultureIgnoreCase));

        if (world == null)
        {
            PluginLog.Error($"'{WorldName}' is not a valid world name.");
            return;
        }

        if (characterIndex >= 8)
        {
            PluginLog.Error("Invalid Character Index. Must be between 0 and 7.");
            return;
        }

        if (serviceAccount < 0 || serviceAccount >= 10)
        {
            PluginLog.Error("Invalid Service account Index. Must be between 0 and 9.");
            return;
        }

        tempDc = world.DataCenter.Row;
        tempWorld = world.RowId;
        tempCharacter = characterIndex;
        tempServiceAccount = serviceAccount;
        if (AutoLoginTaskManager.IsBusy) throw new InvalidOperationException($"AutoLoginTaskManager is busy");
        AutoLoginTaskManager.Enqueue(OpenDataCenterMenu);
        AutoLoginTaskManager.Enqueue(SelectDataCentre);
        AutoLoginTaskManager.Enqueue(SelectServiceAccount);
        AutoLoginTaskManager.Enqueue(SelectWorld);
        AutoLoginTaskManager.DelayNext("AutoLoginDelay", 10, true);
        AutoLoginTaskManager.Enqueue(SelectCharacter);
        AutoLoginTaskManager.Enqueue(SelectYes);
        AutoLoginTaskManager.DelayNext("AutoLoginDelay", 5, true);
        AutoLoginTaskManager.Enqueue(ClearTemp);
    }

    internal static void SwapCharacter(string WorldName, uint characterIndex, int serviceAccount)
    {

        var world = Svc.Data.Excel.GetSheet<World>()?.FirstOrDefault(w => w.Name.ToDalamudString().TextValue.Equals(WorldName, StringComparison.InvariantCultureIgnoreCase));

        if (world == null)
        {
            PluginLog.Error($"'{WorldName}' is not a valid world name.");
            return;
        }

        if (characterIndex >= 8)
        {
            PluginLog.Error("Invalid Character Index. Must be between 0 and 7.");
            return;
        }

        if (serviceAccount < 0 || serviceAccount >= 10)
        {
            PluginLog.Error("Invalid Service account Index. Must be between 0 and 9.");
            return;
        }

        tempDc = world.DataCenter.Row;
        tempWorld = world.RowId;
        tempCharacter = characterIndex;
        tempServiceAccount = serviceAccount;
        if (AutoLoginTaskManager.IsBusy) throw new InvalidOperationException($"AutoLoginTaskManager is busy");
        AutoLoginTaskManager.DelayNext("AutoLoginDelay", 5, true);
        AutoLoginTaskManager.Enqueue(Logout);
        AutoLoginTaskManager.Enqueue(SelectYesLogout);
        AutoLoginTaskManager.DelayNext("AutoLoginDelay", 5, true);
        AutoLoginTaskManager.Enqueue(OpenDataCenterMenu);
        AutoLoginTaskManager.Enqueue(SelectDataCentre);
        AutoLoginTaskManager.Enqueue(SelectServiceAccount);
        AutoLoginTaskManager.Enqueue(SelectWorld);
        AutoLoginTaskManager.DelayNext("AutoLoginDelay", 10, true);
        AutoLoginTaskManager.Enqueue(SelectCharacter);
        AutoLoginTaskManager.Enqueue(SelectYes);
        AutoLoginTaskManager.DelayNext("AutoLoginDelay", 5, true);
        AutoLoginTaskManager.Enqueue(ClearTemp);
    }

    static bool? OpenDataCenterMenu()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_TitleMenu", 1);
        if (addon == null || addon->IsVisible == false) return false;
        GenerateCallback(addon, 12);
        var nextAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (nextAddon == null) return false;
        return true;
    }

    static bool? SelectServiceAccount()
    {
        var dcMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (dcMenu != null) UiHelper.Close(dcMenu, true);
        if (TryGetAddonByName<AtkUnitBase>("_CharaSelectWorldServer", out _))
        {
            return true;
        }
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase)
            && addon->AtkUnitBase.UldManager.NodeListCount >= 4)
        {
            var text = MemoryHelper.ReadSeString(&addon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText).ExtractText();
            var compareTo = Svc.Data.GetExcelSheet<Lobby>()?.GetRow(11)?.Text.ToString();
            if (text == compareTo)
            {
                PluginLog.Information($"Selecting service account");
                ClickSelectString.Using((nint)addon).SelectItem((ushort)tempServiceAccount);
                return true;
            }
            else
            {
                PluginLog.Information($"Found different SelectString: {text}");
                return false;
            }
        }
        return false;
    }

    static bool? SelectDataCentre()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (addon == null || tempDc == null) return false;
        GenerateCallback(addon, 2, (int)tempDc);
        return true;
    }

    static bool? SelectWorld()
    {
        // Select World
        var dcMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (dcMenu != null) UiHelper.Close(dcMenu, true);
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_CharaSelectWorldServer", 1);
        if (addon == null) return false;

        var stringArray = CSFramework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder.StringArrays[1];
        if (stringArray == null || tempWorld == null) return false;

        var world = Svc.Data.Excel.GetSheet<World>()?.GetRow(tempWorld.Value);
        if (world is not { IsPublic: true }) return false;

        var checkedWorldCount = 0;

        for (var i = 0; i < 16; i++)
        {
            var n = stringArray->StringArray[i];
            if (n == null) continue;
            var s = MemoryHelper.ReadStringNullTerminated(new IntPtr(n));
            if (s.Trim().Length == 0) continue;
            checkedWorldCount++;
            if (s != world.Name.RawString) continue;
            GenerateCallback(addon, 9, 0, i);
            return true;
        }

        if (checkedWorldCount > 0) return null;
        return false;
    }

    static bool? SelectCharacter()
    {
        // Select Character
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_CharaSelectListMenu", 1);
        if (addon == null || tempCharacter == null) return false;
        GenerateCallback(addon, 17, 0, tempCharacter);
        var nextAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", 1);
        return nextAddon != null;
    }

    static bool? SelectYesLogout()
    {
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText());
        if (addon == null) return false;
        ClickSelectYesNo.Using((nint)addon).Yes();
        return true;
    }

    static bool? SelectYes()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", 1);
        if (addon == null) return false;
        ClickSelectYesNo.Using((nint)addon).Yes();
        return true;
    }


    static bool? Logout()
    {
        var isLoggedIn = Svc.Condition.Any();
        if (!isLoggedIn) return true;

        Chat.Instance.SendMessage("/logout");
        return true;
    }

    static bool? ClearTemp()
    {
        tempWorld = null;
        tempDc = null;
        tempCharacter = null;
        tempServiceAccount = 0;
        return true;
    }


    static private uint? tempDc = null;
    static private uint? tempWorld = null;
    static private uint? tempCharacter = null;
    static private int tempServiceAccount = 0;

    static void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
    {
        if (unitBase == null) throw new Exception("Null UnitBase");
        var atkValues = (AtkValue*)Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
        if (atkValues == null) return;
        try
        {
            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i];
                switch (v)
                {
                    case uint uintValue:
                        atkValues[i].Type = ValueType.UInt;
                        atkValues[i].UInt = uintValue;
                        break;
                    case int intValue:
                        atkValues[i].Type = ValueType.Int;
                        atkValues[i].Int = intValue;
                        break;
                    case float floatValue:
                        atkValues[i].Type = ValueType.Float;
                        atkValues[i].Float = floatValue;
                        break;
                    case bool boolValue:
                        atkValues[i].Type = ValueType.Bool;
                        atkValues[i].Byte = (byte)(boolValue ? 1 : 0);
                        break;
                    case string stringValue:
                        {
                            atkValues[i].Type = ValueType.String;
                            var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                            var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                            Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                            Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                            atkValues[i].String = (byte*)stringAlloc;
                            break;
                        }
                    default:
                        throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                }
            }

            unitBase->FireCallback(values.Length, atkValues);
        }
        finally
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (atkValues[i].Type == ValueType.String)
                {
                    Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                }
            }
            Marshal.FreeHGlobal(new IntPtr(atkValues));
        }
    }
}
