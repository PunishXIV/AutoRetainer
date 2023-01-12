namespace AutoRetainer.Offline;

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
        return $"{Name}@{World} #{CID:X16}";
    }
}
