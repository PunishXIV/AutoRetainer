namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public abstract class InventoryManagementBase : NeoUIEntry
{
    public abstract string Name { get; }
    public sealed override string Path => $"Inventory Management/{Name}";
}
