
#nullable enable


using ClickLib.Clicks;
using Dalamud.Game;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Diagnostics;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Multi;

internal unsafe class AutoLogin
{
    static AutoLogin? instance = null;
    internal static AutoLogin Instance
    {
        get
        {
            instance ??= new();
            return instance;
        }
    }

    internal static void Dispose()
    {
        if (instance != null)
        {
            instance.DisposeInternal();
            instance = null;
        }
    }

    void DisposeInternal()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
        PluginLog.Information("Autologin module disposed");
    }

    AutoLogin()
    {
        Svc.Framework.Update += OnFrameworkUpdate;
        PluginLog.Information("Autologin module initialized");
    }

    internal void SwapCharacter(string WorldName, uint characterIndex, int serviceAccount)
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

        if(serviceAccount < 0 || serviceAccount >= 10)
        {
            PluginLog.Error("Invalid Service account Index. Must be between 0 and 9.");
            return;
        }

        tempDc = world.DataCenter.Row;
        tempWorld = world.RowId;
        tempCharacter = characterIndex;
        tempServiceAccount = serviceAccount;
        actionQueue.Clear();
        actionQueue.Enqueue(VariableDelay(5));
        actionQueue.Enqueue(Logout);
        actionQueue.Enqueue(SelectYesLogout);
        actionQueue.Enqueue(VariableDelay(5));
        actionQueue.Enqueue(OpenDataCenterMenu);
        actionQueue.Enqueue(SelectDataCentre);
        actionQueue.Enqueue(SelectServiceAccount);
        actionQueue.Enqueue(SelectWorld);
        actionQueue.Enqueue(VariableDelay(10));
        actionQueue.Enqueue(SelectCharacter);
        actionQueue.Enqueue(SelectYes);
        actionQueue.Enqueue(Delay5s);
        actionQueue.Enqueue(ClearTemp);
    }

    private readonly Stopwatch sw = new();
    private uint Delay = 0;

    private Func<bool> VariableDelay(uint frameDelay)
    {
        return () =>
        {
            Delay = frameDelay;
            return true;
        };
    }

    internal void Abort()
    {
        actionQueue.Clear();
    }

    private void OnFrameworkUpdate(Framework framework)
    {
        if (actionQueue.Count == 0)
        {
            if (sw.IsRunning) sw.Stop();
            return;
        }
        if (!sw.IsRunning) sw.Restart();

        /*if (Svc.KeyState[VirtualKey.SHIFT])
        {
            Svc.PluginInterface.UiBuilder.AddNotification("AutoLogin Cancelled.", "AutoLogin", NotificationType.Warning);
            actionQueue.Clear();
        }*/

        if (Delay > 0)
        {
            Delay -= 1;
            return;
        }



        if (sw.ElapsedMilliseconds > 20000)
        {
            actionQueue.Clear();
            return;
        }

        try
        {
            var hasNext = actionQueue.TryPeek(out var next);
            if (hasNext)
            {
                if (next!())
                {
                    actionQueue.Dequeue();
                    sw.Reset();
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Log($"Failed: {ex.Message}");
        }
    }

    private readonly Queue<Func<bool>> actionQueue = new();
    internal bool IsRunning => actionQueue.Count != 0;

    public bool OpenDataCenterMenu()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_TitleMenu", 1);
        if (addon == null || addon->IsVisible == false) return false;
        GenerateCallback(addon, 12);
        var nextAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (nextAddon == null) return false;
        return true;
    }

    public bool SelectServiceAccount()
    {
        var dcMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (dcMenu != null) dcMenu->Hide(true);
        if(TryGetAddonByName<AtkUnitBase>("_CharaSelectWorldServer", out _))
        {
            return true;
        }
        if (GenericHelpers.TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase)
            && addon->AtkUnitBase.UldManager.NodeListCount >= 4)
        {
            var text = MemoryHelper.ReadSeString(&addon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText).ExtractText();
            var compareTo = Svc.Data.GetExcelSheet<Lobby>()?.GetRow(11)?.Text.ToString();
            if(text == compareTo)
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

    public bool SelectDataCentre()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (addon == null || tempDc == null) return false;
        GenerateCallback(addon, 2, (int)tempDc);
        return true;
    }

    public bool SelectWorld()
    {
        // Select World
        var dcMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (dcMenu != null) dcMenu->Hide(true);
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_CharaSelectWorldServer", 1);
        if (addon == null) return false;

        var stringArray = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder.StringArrays[1];
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

        if (checkedWorldCount > 0) actionQueue.Clear();
        return false;
    }

    public bool SelectCharacter()
    {
        // Select Character
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_CharaSelectListMenu", 1);
        if (addon == null || tempCharacter == null) return false;
        GenerateCallback(addon, 17, 0, tempCharacter);
        var nextAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", 1);
        return nextAddon != null;
    }

    public bool SelectYesLogout()
    {
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText());
        if (addon == null) return false;
        GenerateCallback(addon, 0);
        addon->Hide(true);
        return true;
    }

    public bool SelectYes()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", 1);
        if (addon == null) return false;
        GenerateCallback(addon, 0);
        addon->Hide(true);
        return true;
    }


    public bool Delay5s()
    {
        Delay = 300;
        return true;
    }


    public bool Delay1s()
    {
        Delay = 60;
        return true;
    }

    public bool Logout()
    {
        var isLoggedIn = Svc.Condition.Any();
        if (!isLoggedIn) return true;

        Chat.Instance.SendMessage("/logout");
        return true;
    }

    public bool ClearTemp()
    {
        tempWorld = null;
        tempDc = null;
        tempCharacter = null;
        tempServiceAccount = 0;
        return true;
    }


    private uint? tempDc = null;
    private uint? tempWorld = null;
    private uint? tempCharacter = null;
    private int tempServiceAccount = 0;

    internal void DrawUI()
    {
        if (ImGui.Button("Clear Queue"))
        {
            actionQueue.Clear();
        }

        if (ImGui.Button("Test Step: Open Data Centre Menu")) actionQueue.Enqueue(OpenDataCenterMenu);
        if (ImGui.Button($"Test Step: Select Data Center [{tempDc}]")) actionQueue.Enqueue(SelectDataCentre);

        if (ImGui.Button($"Test Step: SELECT WORLD [{tempWorld}]"))
        {
            actionQueue.Clear();
            actionQueue.Enqueue(SelectWorld);
        }

        if (ImGui.Button($"Test Step: SELECT CHARACTER [{tempCharacter}]"))
        {
            actionQueue.Clear();
            actionQueue.Enqueue(SelectCharacter);
        }

        if (ImGui.Button("Test Step: SELECT YES"))
        {
            actionQueue.Clear();
            actionQueue.Enqueue(SelectYes);
        }

        if (ImGui.Button("Logout"))
        {
            actionQueue.Clear();
            actionQueue.Enqueue(Logout);
            actionQueue.Enqueue(SelectYes);
            actionQueue.Enqueue(Delay5s);
        }



        if (ImGui.Button("Swap Character"))
        {
            tempDc = 9;
            tempWorld = 87;
            tempCharacter = 0;

            actionQueue.Enqueue(Logout);
            actionQueue.Enqueue(SelectYes);
            actionQueue.Enqueue(OpenDataCenterMenu);
            actionQueue.Enqueue(SelectDataCentre);
            actionQueue.Enqueue(SelectWorld);
            actionQueue.Enqueue(SelectCharacter);
            actionQueue.Enqueue(SelectYes);
            actionQueue.Enqueue(Delay5s);
            actionQueue.Enqueue(ClearTemp);
        }

        if (ImGui.Button("Full Run"))
        {
            actionQueue.Clear();
            actionQueue.Enqueue(OpenDataCenterMenu);
            actionQueue.Enqueue(SelectDataCentre);
            actionQueue.Enqueue(SelectWorld);
            actionQueue.Enqueue(SelectCharacter);
            actionQueue.Enqueue(SelectYes);
        }

        ImGui.Text("Current Queue:");
        foreach (var l in actionQueue.ToList())
        {
            ImGui.Text($"{l.Method.Name}");
        }
    }


    public static void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
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
