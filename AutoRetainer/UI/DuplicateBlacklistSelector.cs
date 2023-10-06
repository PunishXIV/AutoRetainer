using AutoRetainerAPI.Configuration;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace AutoRetainer.UI
{
    internal class DuplicateBlacklistSelector() : Window("Entrust duplicates blacklist")
    {
        internal OfflineCharacterData SelectedData;
        internal Dictionary<uint, string> Items = Svc.Data.GetExcelSheet<Item>().Where(x => x.StackSize > 1 && x.RowId > 99).ToDictionary(x => x.RowId, x => x.Name.ToDalamudString().ExtractText());
        string Filter = "";
        public override void Draw()
        {
            if(ImGui.BeginCombo("##ocdsel", SelectedData == null?"Select...":$"{Censor.Character(SelectedData.Name, SelectedData.World)}"))
            {
                C.OfflineData.Where(x => !x.ExcludeRetainer).Each((x) =>
                {
                    if(ImGui.Selectable($"{Censor.Character(x.Name, x.World)}"))
                    {
                        SelectedData = x;
                    }
                });
                ImGui.EndCombo();
            }
            ImGui.Separator();
            ImGuiEx.EzTableColumns("cols", new System.Action[] {delegate
            {
                ImGuiEx.SetNextItemFullWidth();
                ImGui.InputTextWithHint($"##fltr", "Filter...", ref Filter, 100);
                if (ImGui.BeginChild("all"))
                {
                    foreach(var x in Items)
                    {
                        if(Filter.Length > 0 && !x.Value.Contains(Filter, StringComparison.OrdinalIgnoreCase)) continue;
                        if(SelectedData.TransferItemsBlacklist.Contains(x.Key)) continue;
                        if (ImGui.Selectable($"{x.Value}##{x.Key}"))
                        {
                            SelectedData.TransferItemsBlacklist.Add(x.Key);
                        }
                    }
                    ImGui.EndChild();
                }
            }, delegate
            {
                ImGuiEx.Text($"Item transfer blacklist:");
                if(ImGui.Button("Copy to clipboard"))
                {
                    ImGui.SetClipboardText(JsonConvert.SerializeObject(SelectedData.TransferItemsBlacklist));
                }
                ImGui.SameLine();
                if(ImGuiEx.ButtonCtrl("Paste from clipboard"))
                {
                    try
                    {
                        SelectedData.TransferItemsBlacklist = JsonConvert.DeserializeObject<List<uint>>(ImGui.GetClipboardText());
                    }
                    catch(Exception e)
                    {
                        DuoLog.Error($"Error while importing from clipboard: {e.Message}");
                        e.Log();
                    }
                }
                if (ImGui.BeginChild("blacklist"))
                {
                    var toRem = -1;
                    foreach(var x in SelectedData.TransferItemsBlacklist)
                    {
                        if (ImGui.Selectable($"{Items[x]}##{x}"))
                        {
                            toRem = (int)x;
                        }
                    }
                    if(toRem > -1)
                    {
                        SelectedData.TransferItemsBlacklist.Remove((uint)toRem);
                    }
                    ImGui.EndChild();
                }
            } });
        }
    }
}
