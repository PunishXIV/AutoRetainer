#nullable enable
namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public class ListComparer : IEqualityComparer<List<uint>>
{
    public bool Equals(List<uint>? x, List<uint>? y)
    {
        if (x == null)
            return false;
        if (y == null)
            return false;

        return x.Count == y.Count && !x.Except(y).Any();
    }

    public int GetHashCode(List<uint> obj)
    {
        var hash = 19;
        foreach (var element in obj.OrderBy(x => x))
        {
            hash = (hash * 31) + element.GetHashCode();
        }

        return hash;
    }
}
