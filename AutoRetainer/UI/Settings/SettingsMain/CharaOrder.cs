﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings.SettingsMain
{
    internal class CharaOrder
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped($"Here you can sort your characters. This will affect order in which they will be processed by Multi Mode as well as how they will appear in plugin interface and login overlay.");
            for (int index = 0; index < C.OfflineData.Count; index++)
            {
                if (C.OfflineData[index].World.IsNullOrEmpty()) continue;
                ImGui.PushID($"c{index}");
                if (ImGui.ArrowButton("##up", ImGuiDir.Up) && index > 0)
                {
                    try
                    {
                        (C.OfflineData[index - 1], C.OfflineData[index]) = (C.OfflineData[index], C.OfflineData[index - 1]);
                    }
                    catch (Exception e)
                    {
                        e.Log();
                    }
                }
                ImGui.SameLine();
                if (ImGui.ArrowButton("##down", ImGuiDir.Down) && index < C.OfflineData.Count - 1)
                {
                    try
                    {
                        (C.OfflineData[index + 1], C.OfflineData[index]) = (C.OfflineData[index], C.OfflineData[index + 1]);
                    }
                    catch (Exception e)
                    {
                        e.Log();
                    }
                }
                ImGui.SameLine();
                ImGuiEx.TextV(Censor.Character(C.OfflineData[index].Name, C.OfflineData[index].World));
                ImGui.PopID();
            }
        }
    }
}
