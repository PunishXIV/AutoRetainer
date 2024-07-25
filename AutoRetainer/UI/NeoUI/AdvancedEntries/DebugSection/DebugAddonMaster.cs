using ECommons.ExcelServices;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public unsafe class DebugAddonMaster : DebugSectionBase
{
    public override void Draw()
    {
        if (ImGui.CollapsingHeader("RestainerList"))
        {
            if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && IsAddonReady(addon))
            {
                var r = new AddonMaster.RetainerList(addon);
                foreach (var x in r.Retainers)
                {
                    ImGuiEx.Text($"{x.Name}, {x.IsActive}");
                    if (ImGuiEx.HoveredAndClicked())
                    {
                        x.Select();
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("_TitleMenu"))
        {
            if(TryGetAddonMaster<AddonMaster._TitleMenu>(out var m) && m.IsAddonReady)
            {
                ImGuiEx.Text($"Ready: {m.IsReady}");
                if (ImGui.Button("Start")) m.Start();
                if (ImGui.Button("DataCenter")) m.DataCenter();
                if (ImGui.Button("Exit")) m.Exit();
            }
        }

        if (ImGui.CollapsingHeader("TitleDCWorldMap"))
        {
            if(TryGetAddonMaster<AddonMaster.TitleDCWorldMap>(out var m) && m.IsAddonReady)
            {
                foreach(var x in AddonMaster.TitleDCWorldMap.PublicDC)
                {
                    if (ImGui.Button(Svc.Data.GetExcelSheet<WorldDCGroupType>().GetRow((uint)x).Name))
                    {
                        m.Select(x);
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("_CharaSelectWorldServer"))
        {
            if(TryGetAddonMaster<AddonMaster._CharaSelectWorldServer>(out var m))
            {
                foreach(var x in m.Worlds)
                {
                    if (ImGui.Button(x.Name))
                    {
                        x.Select();
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("_CharaSelectListMenu"))
        {
            if (TryGetAddonMaster<AddonMaster._CharaSelectListMenu>(out var m) && m.IsAddonReady)
            {
                if (ImGui.Button("World##w"))
                {
                    m.SelectWorld();
                }
                //PluginLog.Information($"Chars: {m.Characters.Print("\n")}");
                ImGuiEx.Text($"{AgentLobby.Instance()->LobbyUpdateStage}");
                ImGuiEx.Text($"{AgentLobby.Instance()->HoveredCharacterContentId}");
                foreach (var x in m.Characters)
                {
                    if (ImGui.Button(x.ToString() + "/select"))
                    {
                        x.Select();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button(x.ToString() + "/login"))
                    {
                        x.Login();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button(x.ToString() + "/context"))
                    {
                        x.OpenContextMenu();
                    }
                    if (x.IsSelected)
                    {
                        ImGuiEx.Text($"Selected");
                    }
                }
            }
        }
    }
}
