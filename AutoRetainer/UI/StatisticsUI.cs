using AutoRetainer.Modules.Statistics;
using AutoRetainer.UI.Experiments;
using ECommons.Configuration;
using Lumina.Excel.GeneratedSheets;
using System.IO;

namespace AutoRetainer.UI;

internal static class StatisticsUI
{
    internal static Dictionary<string, Dictionary<string, Dictionary<uint, StatisticsData>>> Data = new();
    internal static Dictionary<string, uint> CharTotal = new();
    internal static Dictionary<string, uint> RetTotal = new();
    internal static Dictionary<(string Char, string Ret), HashSet<long>> VentureTimestamps = new();
    static string Filter = "";

    internal static void Draw()
    {
        ImGuiEx.EzTabBar("statstab", [
                ("Ventures", DrawVentures, null, true),
                ("Owned gil", S.GilDisplay.Draw, null, true),
                ("Owned FC points", S.FCData.Draw, null, true),
            ]);
    }

    internal static void DrawVentures()
    {
        if (Data.Count == 0)
        {
            Load();
        }
        if (ImGui.Button("Reload"))
        {
            Load();
        }
        ImGui.SameLine();
        ImGui.Checkbox("Show HQ and non-HQ together", ref C.StatsUnifyHQ);
        ImGui.SameLine();
        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##search", "Filter items...", ref Filter, 100);
        int cindex = 0;
        foreach (var cData in Data)
        {
            int rindex = 0;
            var display = false;
            if (CharTotal[cData.Key] != 0)
            {
                if (ImGui.CollapsingHeader($"{Censor.Character(cData.Key)} | Total Ventures: {CharTotal.GetSafe(cData.Key)}###chara{cData.Key}"))
                {
                    display = true;
                }
            }
            CharTotal[cData.Key] = 0;
            foreach (var x in cData.Value)
            {
                var array = x.Value.Where(c => Filter == string.Empty || $"{Svc.Data.GetExcelSheet<Item>().GetRow(c.Key).Name}".Contains(Filter, StringComparison.OrdinalIgnoreCase));
                var num = (uint)GetVentureCount(cData.Key, x.Key);
                CharTotal[cData.Key] += num;
                if (display && num != 0)
                {
                    ImGui.Dummy(new(10, 1));
                    ImGui.SameLine();
                    if (ImGui.CollapsingHeader($"{Censor.Retainer(x.Key)} | Ventures: {num}###{cData.Key}ret{x.Key}"))
                    {
                        foreach (var c in array)
                        {
                            var iName = $"{Svc.Data.GetExcelSheet<Item>().GetRow(c.Key).Name}";
                            ImGuiEx.Text($"             {iName}: {(C.StatsUnifyHQ ? c.Value.Amount + c.Value.AmountHQ : $"{c.Value.Amount}/{c.Value.AmountHQ}")}");
                        }
                    }
                }
            }
        }
    }

    internal static void Load()
    {
        Data.Clear();
        VentureTimestamps.Clear();
        try
        {
            foreach (var x in Directory.GetFiles(Svc.PluginInterface.GetPluginConfigDirectory()))
            {
                if (x.EndsWith(".statistic.json"))
                {
                    var file = EzConfig.LoadConfiguration<StatisticsFile>(x);
                    foreach (var z in file.Records)
                    {
                        AddData(file.PlayerName, file.RetainerName, z.ItemId, z.IsHQ, z.Amount, z.Timestamp);
                    }
                }
            }
            foreach (var x in Data)
            {
                uint ctotal = 0;
                foreach (var z in x.Value)
                {
                    uint cnt = 0;
                    foreach (var c in z.Value.Values)
                    {
                        cnt += c.Amount + c.AmountHQ;
                    }
                    RetTotal[z.Key] = cnt;
                    ctotal += cnt;
                }
                CharTotal[x.Key] = ctotal;
            }
        }
        catch (Exception e)
        {
            e.Log();
            Notify.Error($"Error: {e.Message}");
        }
    }

    static int GetVentureCount(string character)
    {
        var ret = 0;
        foreach (var x in VentureTimestamps)
        {
            if (x.Key.Char == character)
            {
                ret += x.Value.Count;
            }
        }
        return ret;
    }

    static int GetVentureCount(string character, string retainer)
    {
        if (VentureTimestamps.TryGetValue((character, retainer), out var h))
        {
            return h.Count;
        }
        return 0;
    }

    static void AddData(string character, string retainer, uint item, bool hq, uint amount, long timestamp)
    {
        if (!Data.TryGetValue(character, out var cData))
        {
            cData = new();
            Data.Add(character, cData);
        }
        if (!cData.TryGetValue(retainer, out var rData))
        {
            rData = new();
            cData.Add(retainer, rData);
        }
        if (!rData.TryGetValue(item, out var iData))
        {
            iData = new();
            rData.Add(item, iData);
        }
        if (!VentureTimestamps.ContainsKey((character, retainer)))
        {
            VentureTimestamps[(character, retainer)] = new();
        }
        VentureTimestamps[(character, retainer)].Add(timestamp);
        if (hq)
        {
            iData.AmountHQ += amount;
        }
        else
        {
            iData.Amount += amount;
        }
    }
}
