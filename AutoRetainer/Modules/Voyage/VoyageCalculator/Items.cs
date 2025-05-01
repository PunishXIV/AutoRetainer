using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public enum Items : uint
{
    Tanks = 10155,
    Kits = 10373,
    DiveCredits = 22317,

    // Frames
    SharkClassBoFrame = 26508,
    SharkClassBrFrame = 26509,
    SharkClassHuFrame = 26510,
    SharkClassStrFrame = 26511,

    UnkiuClassBoFrame = 26512,
    UnkiuClassBrFrame = 26513,
    UnkiuClassHuFrame = 26514,
    UnkiuClassStrFrame = 26515,

    WhaleClassBoFrame = 26516,
    WhaleClassBrFrame = 26517,
    WhaleClassHuFrame = 26518,
    WhaleClassStrFrame = 26519,

    CoelacanthClassBoFrame = 26520,
    CoelacanthClassBrFrame = 26521,
    CoelacanthClassHuFrame = 26522,
    CoelacanthClassStrFrame = 26523,

    SyldraClassBoFrame = 26524,
    SyldraClassBrFrame = 26525,
    SyldraClassHuFrame = 26526,
    SyldraClassStrFrame = 26527,

    // Parts
    SharkClassBow = 21792,
    SharkClassBridge = 21793,
    SharkClassHull = 21794,
    SharkClassStern = 21795,

    UnkiuClassBow = 21796,
    UnkiuClassBridge = 21797,
    UnkiuClassHull = 21798,
    UnkiuClassStern = 21799,

    WhaleClassBoPart = 22526,
    WhaleClassBridge = 22527,
    WhaleClassHull = 22528,
    WhaleClassStern = 22529,

    CoelacanthClassBow = 23903,
    CoelacanthClassBridge = 23904,
    CoelacanthClassHull = 23905,
    CoelacanthClassStern = 23906,

    SyldraClassBow = 24344,
    SyldraClassBridge = 24345,
    SyldraClassHull = 24346,
    SyldraClassStern = 24347,

    ModSharkClassBow = 24348,
    ModSharkClassBridge = 24349,
    ModSharkClassHull = 24350,
    ModSharkClassStern = 24351,

    ModUnkiuClassBow = 24352,
    ModUnkiuClassBridge = 24353,
    ModUnkiuClassHull = 24354,
    ModUnkiuClassStern = 24355,

    ModWhaleClassBow = 24356,
    ModWhaleClassBridge = 24357,
    ModWhaleClassHull = 24358,
    ModWhaleClassStern = 24359,

    ModCoelacanthClassBow = 24360,
    ModCoelacanthClassBridge = 24361,
    ModCoelacanthClassHull = 24362,
    ModCoelacanthClassStern = 24363,

    ModSyldraClassBow = 24364,
    ModSyldraClassBridge = 24365,
    ModSyldraClassHull = 24366,
    ModSyldraClassStern = 24367,
}

internal static class ImportantItemsMethods
{
    private static ExcelSheet<Item> Item = null!;
    public static void Initialize() => Item = Svc.Data.GetExcelSheet<Item>()!;

    public static Item GetItem(this Items item) => Item.GetRow((uint)item)!;
    public static int GetPartId(this Items item) => PartIdToItemId.First(d => d.Value == (uint)item).Key;

    public static readonly Dictionary<ushort, uint> PartIdToItemId = new()
{
    // Shark
    { 1, 21792 }, // Bow
    { 2, 21793 }, // Bridge
    { 3, 21794 }, // Hull
    { 4, 21795 }, // Stern

    // Ubiki
    { 5, 21796 },
    { 6, 21797 },
    { 7, 21798 },
    { 8, 21799 },

    // Whale
    { 9, 22526 },
    { 10, 22527 },
    { 11, 22528 },
    { 12, 22529 },

    // Coelacanth
    { 13, 23903 },
    { 14, 23904 },
    { 15, 23905 },
    { 16, 23906 },

    // Syldra
    { 17, 24344 },
    { 18, 24345 },
    { 19, 24346 },
    { 20, 24347 },

    // Modified same order
    { 21, 24348 },
    { 22, 24349 },
    { 23, 24350 },
    { 24, 24351 },

    { 25, 24352 },
    { 26, 24353 },
    { 27, 24354 },
    { 28, 24355 },

    { 29, 24356 },
    { 30, 24357 },
    { 31, 24358 },
    { 32, 24359 },

    { 33, 24360 },
    { 34, 24361 },
    { 35, 24362 },
    { 36, 24363 },

    { 37, 24364 },
    { 38, 24365 },
    { 39, 24366 },
    { 40, 24367 }
};
}
