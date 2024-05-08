using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class CharaOrder : NeoUIEntry
{
    public override string Path => "Multi Mode/Character Order";

		static string Search = "";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
                    .Section("Character Order")
                    .Widget("Here you can sort your characters. This will affect order in which they will be processed by Multi Mode as well as how they will appear in plugin interface and login overlay.", (x) =>
                    {
                        ImGuiEx.TextWrapped($"Here you can sort your characters. This will affect order in which they will be processed by Multi Mode as well as how they will appear in plugin interface and login overlay.");
												ImGui.SetNextItemWidth(150f);
												ImGui.InputText($"Search", ref Search, 50);
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
														if (ImGuiEx.IconButton(FontAwesomeIcon.AngleDoubleDown))
														{
																try
																{
																		var moveIndex = index;
																		MoveItemToPosition(C.OfflineData, (x) => x == C.OfflineData[moveIndex], C.OfflineData.Count - 1);
																}
																catch (Exception e)
																{
																		e.Log();
																}
														}
														ImGui.SameLine();
														if (ImGuiEx.IconButton(FontAwesomeIcon.AngleDoubleUp))
														{
																try
																{
																		var moveIndex = index;
																		MoveItemToPosition(C.OfflineData, (x) => x == C.OfflineData[moveIndex], 0);
																}
																catch (Exception e)
																{
																		e.Log();
																}
														}
														ImGui.SameLine();
                            ImGuiEx.TextV((Search != "" && C.OfflineData[index].Name.Contains(Search, StringComparison.OrdinalIgnoreCase))?ImGuiColors.ParsedGreen:null, Censor.Character(C.OfflineData[index].Name, C.OfflineData[index].World));
                            ImGui.PopID();
                        }
                    });
}
