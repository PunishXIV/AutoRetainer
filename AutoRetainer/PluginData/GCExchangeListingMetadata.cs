using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrandCompany = ECommons.ExcelServices.GrandCompany;

namespace AutoRetainer.PluginData;
public sealed unsafe class GCExchangeListingMetadata : IEquatable<GCExchangeListingMetadata>
{
    public HashSet<GrandCompany> Companies = [];

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
        return Equals(obj as GCExchangeListingMetadata);
    }

    public bool Equals(GCExchangeListingMetadata other)
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

    public static bool operator ==(GCExchangeListingMetadata left, GCExchangeListingMetadata right)
    {
        return EqualityComparer<GCExchangeListingMetadata>.Default.Equals(left, right);
    }

    public static bool operator !=(GCExchangeListingMetadata left, GCExchangeListingMetadata right)
    {
        return !(left == right);
    }
}