namespace AutoRetainer.Configuration;

[Serializable]
public class OfflineCharacterData
{
    public ulong CID = 0;
    public string Name = "Unknown";
    public int Index = 0;
    public string World = "";
    public bool Enabled = false;
    public List<OfflineRetainerData> RetainerData = new();
    public bool Preferred = false;
    public uint Ventures = 0;
    public uint InventorySpace = 0;
    public uint VentureCoffers = 0;
    public int ServiceAccount = 0;
    public bool EnableGCArmoryHandin = false; //todo: remove
    public bool ShouldSerializeEnableGCArmoryHandin() => false;
    public GCDeliveryType GCDeliveryType = GCDeliveryType.Disabled;

    internal uint CharaIndex
    {
        get
        {
            if (Index == 0)
            {
                throw new Exception("Index must not be 0");
            }
            return (uint)(Index - 1);
        }
    }

    public override string ToString()
    {
        return P.config.Verbose ? $"{Name}@{World}" : $"{Name}@{World}";
    }
}
