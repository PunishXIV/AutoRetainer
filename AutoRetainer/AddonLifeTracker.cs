using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer;

internal unsafe static class AddonLifeTracker
{
    static Dictionary<string, long> values = new();

    internal static void Tick()
    {
        Track("RetainerList");
        Track("SelectString");
        Track("RetainerTaskResult");
        Track("RetainerTaskAsk");
    }

    internal static void Reset()
    {
        values = new();
    }

    internal static long GetAge(string name)
    {
        if(values.TryGetValue(name, out var n))
        {
            return Environment.TickCount64 - n;
        }
        return 0;
    }

    static void Track(string name)
    {
        if (TryGetAddonByName<AtkUnitBase>(name, out var addon) && addon->IsVisible)
        {
            if (!values.ContainsKey(name))
            {
                values[name] = Environment.TickCount64;
            }
        }
        else
        {
            values.Remove(name);
        }
    }
}
