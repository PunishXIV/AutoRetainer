using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public abstract class InventoryManagemenrBase : NeoUIEntry
{
    public abstract string Name { get; }
    public sealed override string Path => $"Inventory Management/{Name}";
}
