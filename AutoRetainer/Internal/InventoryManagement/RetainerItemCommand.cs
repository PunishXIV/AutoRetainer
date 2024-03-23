using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Internal.InventoryManagement
{
    public enum RetainerItemCommand : long
    {
        RetrieveFromRetainer = 0,
        EntrustToRetainer = 1,
        HaveRetainerSellItem = 5,
    }
}
