using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public unsafe sealed class CharacterSync : NeoUIEntry
{
    public override string Path => "Advanced/Character Synchronization";

    private List<string> ToDelete = [];

    public override void Draw()
    {
        if(ToDelete.Count > 0)
        {
            if(ImGuiEx.BeginDefaultTable(["Name", "##control"]))
            {
                foreach(var item in ToDelete)
                {
                    var ocd = C.OfflineData.FirstOrDefault(x => x.NameWithWorld == item);
                    if(ocd != null)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGuiEx.Text($"{ocd.NameWithWorld}");
                        ImGui.TableNextColumn();
                        if(ImGui.SmallButton("Exclude from list"))
                        {
                            new TickScheduler(() => ToDelete.Remove(item));
                        }
                    }
                    else
                    {
                        new TickScheduler(() => ToDelete.Remove(item));
                    }
                }
                ImGui.EndTable();
            }
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "Delete listed characters from AutoRetainer", enabled: ImGuiEx.Ctrl))
            {
                C.OfflineData.RemoveAll(x => ToDelete.Contains(x.NameWithWorld));
            }
            ImGuiEx.Tooltip("Hold CTRL and click");
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Ban, "Cancel"))
            {
                ToDelete.Clear();
            }
            return;
        }

        ImGuiEx.TextWrapped($"Prune deleted characters in a single click.");
        var jbInstalled = Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "JustBackup" && x.IsLoaded);
        if(!jbInstalled)
        {
            ImGuiEx.TextWrapped(EColor.RedBright, "To continue, you need to install JustBackup plugin.");
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.WindowMaximize, "Open Plugin Installer"))
            {
                Svc.PluginInterface.OpenPluginInstallerTo(PluginInstallerOpenKind.AllPlugins, "JustBackup");
            }
            return;
        }
        ImGuiEx.TextWrapped($"""
            1. Create a backup by typing /justbackup, ensure it has succeeded and saved into a secure location.
            2. Open your character list on FFXIV Lodestone.
            """);
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.ExternalLinkSquareAlt, "Open character list now"))
        {
            ShellStart("https://eu.finalfantasyxiv.com/lodestone/account/select_character/");
        }
        ImGuiEx.TextWrapped($"3. Make sure you are logged with the correct account and copy entire page's content by pressing CTRL+A then CTRL+C");
        ImGuiEx.TextWrapped($"4. Once finished, click the following button:");
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Paste, "Prepare Character Cleanup"))
        {
            Parse();
        }
    }

    void Parse()
    {
        try
        {
            var lines = Paste().Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var isParsing = false;
            List<string> charas = [];
            for(var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if(line == "Character")
                {
                    isParsing = true;
                }
                else if(line == "Update Character List")
                {
                    isParsing = false;
                }
                if(isParsing)
                {
                    if(!line.Contains('[') && !line.Contains(']') && line.Contains(' '))
                    {
                        var chara = line;
                        var world = lines[i + 1].Split(' ')[0];
                        var n = $"{chara}@{world}".Trim();
                        if(n != "")
                        {
                            charas.Add(n);
                        }
                    }
                }
            }
            if(charas.Count == 0)
            {
                Notify.Error("Did not read any characters");
            }
            else
            {
                ToDelete = [.. C.OfflineData.Select(x => x.NameWithWorld).Where(x => !charas.Contains(x))];
                PluginLog.Debug($"To Delete: \n{ToDelete.Print("\n")}");
            }
        }
        catch(Exception e)
        {
            e.Log();
            Notify.Error("Could not parse character list");
        }
    }
}