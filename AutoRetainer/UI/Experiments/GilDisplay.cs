using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.ExcelServices;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Experiments
{
    public static class GilDisplay
    {
        public static void Draw()
        {
            ImGui.SetNextItemWidth(200f);
            ImGui.InputInt("Ignore characters/retainers with gil less than", ref C.MinGilDisplay.ValidateRange(0, int.MaxValue));
            ImGuiComponents.HelpMarker($"Ignored retainer gil still contributes to character/DC total. Character is ignored if their gil AND all retainers' gil is less than this value. Ignored characters do not contribute to DC total.");
            ImGui.Checkbox("Only display character total", ref C.GilOnlyChars);
            Dictionary<ExcelWorldHelper.Region, List<OfflineCharacterData>> data = [];
            foreach(var x in C.OfflineData)
            {
                if(ExcelWorldHelper.TryGetWorldByName(x.World, out var world))
                {
                    if (!data.ContainsKey((ExcelWorldHelper.Region)world.DataCenter.Value.Region))
                    {
                        data[(ExcelWorldHelper.Region)world.DataCenter.Value.Region] = [];
                    }
                    data[(ExcelWorldHelper.Region)world.DataCenter.Value.Region].Add(x);
                }
            }
            foreach(var x in data)
            {
                ImGuiEx.Text($"{x.Key}:");
                var dcTotal = 0L;
                foreach(var c in x.Value)
                {
                    var charTotal = c.Gil + c.RetainerData.Sum(s => s.Gil);
                    if (charTotal > C.MinGilDisplay)
                    {
                        if(!C.GilOnlyChars) ImGuiEx.Text($"    {Censor.Character(c.Name, c.World)}: {c.Gil:N0}");
                        foreach (var r in c.RetainerData)
                        {
                            if (r.Gil > C.MinGilDisplay && !C.GilOnlyChars)
                            {
                                ImGuiEx.Text($"        {Censor.Retainer(r.Name)}: {r.Gil:N0}");
                            }
                        }
                        ImGuiEx.Text(ImGuiColors.DalamudViolet, $"    {Censor.Character(c.Name, c.World)} total: {charTotal:N0}");
                        dcTotal += charTotal;
                        ImGui.Separator();
                    }
                }
                ImGuiEx.Text(ImGuiColors.DalamudOrange, $"Data center total ({x.Key}): {dcTotal:N0}");
                ImGui.Separator();
                ImGui.Separator();
            }
        }
    }
}
