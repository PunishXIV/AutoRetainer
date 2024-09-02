using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Configuration;
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
}
