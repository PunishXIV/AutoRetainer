namespace AutoRetainer.PluginData;
[Serializable]
public class EntrustPlan
{
    public Guid Guid = Guid.NewGuid();
    public string Name = "";
    public bool Duplicates = false;
    public bool DuplicatesMultiStack = false;
    public List<EntrustCategoryConfiguration> EntrustCategories = [];
    public List<uint> EntrustItems = [];
    public Dictionary<uint, int> EntrustItemsAmountToKeep = [];
    public bool AllowEntrustFromArmory = false;
    public bool ManualPlan = false;
    public bool ExcludeProtected = false;
}
