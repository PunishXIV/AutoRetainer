using ECommons.ExcelServices;
using FFXIVClientStructs;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class CharaOrder : NeoUIEntry
{
    public override string Path => "Multi Mode/Exclusions and Order";

    private static string Search = "";
    private static ImGuiEx.RealtimeDragDrop DragDrop = new("CharaOrder");

    public override bool NoFrame { get; set; } = true;

    public override void Draw()
    {
        C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
        var b = new NuiBuilder()
        .Section("Character Order")
        .Widget("Here you can sort your characters. This will affect order in which they will be processed by Multi Mode as well as how they will appear in plugin interface and login overlay.", (x) =>
        {
            ImGuiEx.TextWrapped($"Here you can sort your characters. This will affect order in which they will be processed by Multi Mode as well as how they will appear in plugin interface and login overlay.");
            ImGui.SetNextItemWidth(150f);
            ImGui.InputText($"Search", ref Search, 50);
            DragDrop.Begin();
            if(ImGui.BeginTable("CharaOrderTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##ctrl");
                ImGui.TableSetupColumn("Character", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Toggles");
                ImGui.TableHeadersRow();

                for(var index = 0; index < C.OfflineData.Count; index++)
                {
                    var chr = C.OfflineData[index];
                    var idx = index;
                    ImGui.PushID(chr.Identity);
                    ImGui.TableNextRow();
                    DragDrop.SetRowColor(chr.Identity);
                    ImGui.TableNextColumn();
                    DragDrop.NextRow();
                    DragDrop.DrawButtonDummy(chr.Identity, (x) =>
                    {
                        MoveItemToPosition(C.OfflineData, (s) => s.Identity == x, idx);
                    });
                    ImGui.TableNextColumn();
                    if(chr.World.IsNullOrEmpty()) continue;
                    ImGuiEx.TextV((Search != "" && ($"{chr.Name}@{chr.World}").Contains(Search, StringComparison.OrdinalIgnoreCase)) ? ImGuiColors.ParsedGreen : (Search == ""?null:ImGuiColors.DalamudGrey3), Censor.Character(chr.Name, chr.World));
                    ImGui.TableNextColumn();
                    if(ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Users, ref chr.ExcludeRetainer, inverted:true))
                    {
                        chr.Enabled = false;
                        C.SelectedRetainers.Remove(chr.CID);
                    }
                    ImGuiEx.Tooltip("Enable retainers");
                    ImGui.SameLine();
                    if(ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Ship, ref chr.ExcludeWorkshop, inverted:true))
                    {
                        chr.WorkshopEnabled = false;
                        chr.EnabledSubs.Clear();
                        chr.EnabledAirships.Clear();
                    }
                    ImGuiEx.Tooltip("Enable deployables");
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.DoorOpen, ref chr.ExcludeOverlay, inverted: true);
                    ImGuiEx.Tooltip("Display on login overlay");
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Coins, ref chr.NoGilTrack, inverted: true);
                    ImGuiEx.Tooltip("Count gil on this character towards total");
                    ImGui.SameLine();
                    if(ImGuiEx.IconButton("\uf057", enabled:ImGuiEx.Ctrl))
                    {
                        C.Blacklist.Add((chr.CID, chr.Name));
                    }
                    ImGuiEx.Tooltip($"Hold CTRL and click to delete stored character data and prevent it from being ever created again, effectively excluding it from being processed by AutoRetainer entirely in any ways.");

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
            DragDrop.End();
        });


        if(C.Blacklist.Count != 0)
        {
            b = b.Section("Excluded Characters")
                .Widget(() =>
                {
                    for(var i = 0; i < C.Blacklist.Count; i++)
                    {
                        var d = C.Blacklist[i];
                        ImGuiEx.TextV($"{d.Name} ({d.CID:X16})");
                        ImGui.SameLine();
                        if(ImGui.Button($"Delete##bl{i}"))
                        {
                            C.Blacklist.RemoveAt(i);
                            C.SelectedRetainers.Remove(d.CID);
                            break;
                        }
                    }
                });
        }

        b.Draw();
    }
}
