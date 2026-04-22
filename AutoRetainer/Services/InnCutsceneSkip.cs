using AutoRetainerAPI.Configuration;
using ECommons.EzHookManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.Services;

public unsafe class InnCutsceneSkip
{
    delegate nint IsEnterTerritoryEventLogin(nint a1, nint a2);
    [EzHook("48 83 EC 58 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24", false)]
    EzHook<IsEnterTerritoryEventLogin> IsEnterTerritoryEventLoginHook;

    nint IsEnterTerritoryEventLoginDetour(nint a1, nint a2)
    {
        try
        {
            if(C.CutsceneSkipMode == CutsceneSkipMode.Always)
            {
                PluginLog.Information($"Inn cutscene skipped because CutsceneSkipMode is set to {C.CutsceneSkipMode}");
                return 1;
            }
            if(C.CutsceneSkipMode == CutsceneSkipMode.When_Multi_Mode_is_on && MultiMode.Enabled)
            {
                PluginLog.Information($"Inn cutscene skipped because CutsceneSkipMode is set to {C.CutsceneSkipMode}");
                return 1;
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        return IsEnterTerritoryEventLoginHook.Original(a1, a2);
    }

    private InnCutsceneSkip()
    {
        EzSignatureHelper.Initialize(this);
        RefreshAccordingToConfig();
    }

    public void RefreshAccordingToConfig()
    {
        if(C.CutsceneSkipMode == CutsceneSkipMode.Never)
        {
            if(IsEnterTerritoryEventLoginHook.IsEnabled)
            {
                IsEnterTerritoryEventLoginHook.Pause();
            }
        }
        else
        {
            if(!IsEnterTerritoryEventLoginHook.IsEnabled)
            {
                IsEnterTerritoryEventLoginHook.Enable();
            }
        }
    }
}
