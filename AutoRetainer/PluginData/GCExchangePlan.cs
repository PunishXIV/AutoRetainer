using AutoRetainerAPI.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.PluginData;
[Serializable]
public unsafe sealed class GCExchangePlan
{
    public string ID => GUID.ToString();
    public Guid GUID = Guid.NewGuid();
    public string Name = "";
    public List<GCExchangeItem> Items = [];
    public int RemainingSeals = 0;
    public bool FinalizeByPurchasing = false;

    public bool ShouldSerializeGUID() => GUID != Guid.Empty;
    public bool ShouldSerializeID() => false;
}