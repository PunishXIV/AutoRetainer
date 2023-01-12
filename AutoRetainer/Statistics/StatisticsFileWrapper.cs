using ECommons.Configuration;

namespace AutoRetainer.Statistics;

internal class StatisticsFileWrapper
{
    internal ulong CID;
    internal string RetainerName;
    internal string FileName => $"{CID:X16}_{RetainerName}.statistic.json";
    internal StatisticsFile File;

    internal StatisticsFileWrapper(ulong CID, string RetainerName)
    {
        this.CID = CID;
        this.RetainerName = RetainerName;
        File = EzConfig.LoadConfiguration<StatisticsFile>(FileName);
        if(CID == Svc.ClientState.LocalContentId)
        {
            File.PlayerName = Svc.ClientState.LocalPlayer.Name.ToString() + "@" + Svc.ClientState.LocalPlayer.HomeWorld.GameData.Name.ToString();
        }
        File.RetainerName = RetainerName;
    }

    internal void Add(StatisticsRecord record)
    {
        File.Records.Add(record);
        Save();
    }

    internal void Save()
    {
        EzConfig.SaveConfiguration(File, FileName);
    }
}
