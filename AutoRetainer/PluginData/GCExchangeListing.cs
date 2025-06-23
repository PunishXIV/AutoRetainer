using Lumina.Excel.Sheets;

namespace AutoRetainer.PluginData;
public unsafe sealed class GCExchangeListing : IEquatable<GCExchangeListing>
{
    public GCExchangeCategoryTab Category;
    public GCExchangeRankTab Rank
    {
        get
        {
            if(MinPurchaseRank <= 4)
            {
                return GCExchangeRankTab.Low;
            }
            else if(MinPurchaseRank <= 8)
            {
                return GCExchangeRankTab.Medium;
            }
            else
            {
                return GCExchangeRankTab.High;
            }
        }
    }
    public uint ItemID;
    public uint Seals;
    public uint MinPurchaseRank;
    public Item Data => Svc.Data.GetExcelSheet<Item>().GetRow(ItemID);

    public override bool Equals(object obj)
    {
        return Equals(obj as GCExchangeListing);
    }

    public bool Equals(GCExchangeListing other)
    {
        return other is not null &&
               Category == other.Category &&
               ItemID == other.ItemID &&
               Seals == other.Seals &&
               MinPurchaseRank == other.MinPurchaseRank;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Category, ItemID, Seals, MinPurchaseRank);
    }

    public static bool operator ==(GCExchangeListing left, GCExchangeListing right)
    {
        return EqualityComparer<GCExchangeListing>.Default.Equals(left, right);
    }

    public static bool operator !=(GCExchangeListing left, GCExchangeListing right)
    {
        return !(left == right);
    }
}