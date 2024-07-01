namespace AutoRetainer.Configuration;

[Serializable]
public class UnoptimalVesselConfiguration
{
    [NonSerialized] internal string GUID = Guid.NewGuid().ToString();
    public int MinRank = 1;
    public int MaxRank = 100;
    public string[] Configurations = [];
    public bool ConfigurationsInvert = false;
}
