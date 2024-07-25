using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Configuration;
public class ARDiscardMiniConfig : IEzConfig
{
    public List<uint> DiscardingItems;
    public List<uint> BlacklistedItems;
}
