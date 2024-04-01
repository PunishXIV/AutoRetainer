﻿
#nullable enable


using ClickLib.Clicks;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Automation;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Diagnostics;

namespace AutoRetainer.Modules.Multi;

//Part of code is authored by Caraxi https://github.com/Caraxi/AutoLogin

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

    internal void Logoff()
    {
        actionQueue.Clear();
        actionQueue.Enqueue(Logout);
        actionQueue.Enqueue(SelectYesLogout);
    }

    internal void Login(string WorldName, string characterName, uint charaWorld, int serviceAccount)
    {
        BailoutManager.IsLogOnTitleEnabled = false;
        RecordLastData(WorldName, characterName, charaWorld, serviceAccount);
        this.charaWorld = charaWorld;
        var world = Svc.Data.Excel.GetSheet<World>()?.FirstOrDefault(w => w.Name.ToDalamudString().TextValue.Equals(WorldName, StringComparison.InvariantCultureIgnoreCase));

        if (world == null)
        {
            PluginLog.Error($"'{WorldName}' is not a valid world name.");
            return;
        }

        /*if (characterName >= 8)
        {
            PluginLog.Error("Invalid Character Index. Must be between 0 and 7.");
            return;
        }*/

        if (serviceAccount < 0 || serviceAccount >= 10)
        {
            PluginLog.Error("Invalid Service account Index. Must be between 0 and 9.");
            return;
        }

        tempDc = world.DataCenter.Row;
        tempWorld = world.RowId;
        tempCharacter = characterName;
        tempServiceAccount = serviceAccount;
        actionQueue.Clear();
        if (!Utils.IsCN)
        {
            actionQueue.Enqueue(OpenDataCenterMenu);
            actionQueue.Enqueue(SelectDataCentre);
            actionQueue.Enqueue(SelectServiceAccount);
        }
        else
        {
            actionQueue.Enqueue(Utils.IsTitleScreenReady);
            actionQueue.Enqueue(TitleScreenClickStart);
        }
        actionQueue.Enqueue(SelectWorld);
        actionQueue.Enqueue(VariableDelay(10));
        actionQueue.Enqueue(SelectCharacter);
        actionQueue.Enqueue(SelectYes);
        actionQueue.Enqueue(Delay5s);
        actionQueue.Enqueue(ClearTemp);
    }

    internal string LastCharacter;
    internal string LastWorld;
    internal uint LastCharaWorld;
    internal int LastServiceAccount;

    void RecordLastData(string WorldName, string characterName, uint charaWorld, int serviceAccount)
    {
        LastCharacter = characterName;
        LastWorld = WorldName;
        LastCharaWorld = charaWorld;
        LastServiceAccount = serviceAccount;
    }

    internal void SwapCharacter(string WorldName, string characterName, uint charaWorld, int serviceAccount)
    {
        BailoutManager.IsLogOnTitleEnabled = false;
        RecordLastData(WorldName, characterName, charaWorld, serviceAccount);
        this.charaWorld = charaWorld;
        var world = Svc.Data.Excel.GetSheet<World>()?.FirstOrDefault(w => w.Name.ToDalamudString().TextValue.Equals(WorldName, StringComparison.InvariantCultureIgnoreCase));

        if (world == null)
        {
            PluginLog.Error($"'{WorldName}' is not a valid world name.");
            return;
        }

        /*if (characterName >= 8)
        {
            PluginLog.Error("Invalid Character Index. Must be between 0 and 7.");
            return;
        }*/

        if (serviceAccount < 0 || serviceAccount >= 10)
        {
            PluginLog.Error("Invalid Service account Index. Must be between 0 and 9.");
            return;
        }

        tempDc = world.DataCenter.Row;
        tempWorld = world.RowId;
        tempCharacter = characterName;
        tempServiceAccount = serviceAccount;
        actionQueue.Clear();
        actionQueue.Enqueue(VariableDelay(5));
        actionQueue.Enqueue(Logout);
        actionQueue.Enqueue(SelectYesLogout);
        actionQueue.Enqueue(VariableDelay(5));
        if (!Utils.IsCN)
        {
            actionQueue.Enqueue(OpenDataCenterMenu);
            actionQueue.Enqueue(SelectDataCentre);
            actionQueue.Enqueue(SelectServiceAccount);
        }
        else
        {
            actionQueue.Enqueue(Utils.IsTitleScreenReady);
            actionQueue.Enqueue(TitleScreenClickStart);
        }
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

    private void OnFrameworkUpdate(object _)
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

        if (sw.ElapsedMilliseconds > 60000)
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

    bool OpenDataCenterMenu()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_TitleMenu", 1);
        if (addon == null || addon->IsVisible == false) return false;
        Callback.Fire(addon, false, 13);
        var nextAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (nextAddon == null) return false;
        return true;
    }

    bool SelectServiceAccount()
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

    bool TitleScreenClickStart()
    {
        if (!Utils.IsTitleScreenReady())
        {
            FrameThrottler.Throttle($"TitleScreenClickStart", 15, true);
            return true;
        }
        if (Utils.IsTitleScreenReady() && TryGetAddonByName<AtkUnitBase>("_TitleMenu", out var title) && IsAddonReady(title) && FrameThrottler.Throttle("TitleScreenClickStart"))
        {
            PluginLog.Debug($"[DCChange] Clicking start");
            Callback.Fire(title, true, (int)1);
            FrameThrottler.Throttle($"TitleScreenClickStart", 15, true);
            return false;
        }
        else
        {
            FrameThrottler.Throttle($"TitleScreenClickStart", 15, true);
        }
        return false;
    }

    bool SelectDataCentre()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (addon == null || tempDc == null) return false;
        Callback.Fire(addon, false, 2, (int)tempDc);
        return true;
    }

    bool SelectWorld()
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
            Callback.Fire(addon, false, 10, 0, i);
            return true;
        }

        if (checkedWorldCount > 0) actionQueue.Clear();
        return false;
    }

    bool SelectCharacter()
    {
        // Select Character
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_CharaSelectListMenu", 1);
        if (addon == null || tempCharacter == null) return false;
        if (!AgentLobby.Instance()->AgentInterface.IsAgentActive()) return false;
        if (AgentLobby.Instance()->TemporaryLocked) return false;
        if (Utils.TryGetCharacterIndex(tempCharacter, charaWorld, out var index))
        {
            Callback.Fire(addon, false, (int)18, (int)0, (int)index);
            var nextAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", 1);
            return nextAddon != null;
        }
        return false;
    }

    bool SelectYesLogout()
    {
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText());
        if (addon == null || !IsAddonReady(addon)) return false;
        ClickSelectYesNo.Using((nint)addon).Yes();
        return true;
    }

    bool SelectYes()
    {
        var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", 1);
        if (addon == null) return false;
        Callback.Fire(addon, false, 0);
        UiHelper.Close(addon, true);
        return true;
    }


    bool Delay5s()
    {
        Delay = 300;
        return true;
    }


    bool Delay1s()
    {
        Delay = 60;
        return true;
    }

    bool Logout()
    {
        var isLoggedIn = Svc.Condition.Any();
        if (!isLoggedIn) return true;

        Chat.Instance.SendMessage("/logout");
        return true;
    }

    bool ClearTemp()
    {
        tempWorld = null;
        tempDc = null;
        tempCharacter = null;
        tempServiceAccount = 0;
        return true;
    }


    internal uint? tempDc = null;
    internal uint? tempWorld = null;
    internal string? tempCharacter = null;
    internal uint charaWorld = 0;
    internal int tempServiceAccount = 0;

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
            //tempCharacter = "";

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
}
