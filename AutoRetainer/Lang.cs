using Dalamud;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer;

internal static class Lang
{
    internal const string CharPlant = "";
    internal const string CharLevel = "";
    internal const string CharItemLevel = "";
    internal const string CharDice = "";
    internal const string CharDeny = "";
    internal const string CharQuestion = "";
    internal const string CharLevelSync = "";

    internal const string IconRefresh = "\uf2f9";
    internal const string IconMultiMode = "\uf021";
    internal const string IconDuplicate = "\uf24d";
    internal const string IconGil = "\uf51e";
    internal const string IconPlanner = "\uf0ae";
    internal const string IconSettings = "\uf013";
    internal const string IconWarning = "\uf071";

    internal static readonly (string Normal, string GameFont) Digits = ("0123456789", "");

    internal static readonly string[] FieldExplorationNames = new string[]
    {
        "Field Exploration.",
        "Highland Exploration.",
        "Woodland Exploration.",
        "Waterside Exploration.",
        "探索依頼：平地　　（必要ベンチャースクリップ：2枚）",
        "探索依頼：山岳　　（必要ベンチャースクリップ：2枚）",
        "探索依頼：森林　　（必要ベンチャースクリップ：2枚）",
        "探索依頼：水辺　　（必要ベンチャースクリップ：2枚）",
        "Felderkundung (2 Wertmarken)",
        "Hochlanderkundung (2 Wertmarken)",
        "Forsterkundung (2 Wertmarken)",
        "Gewässererkundung (2 Wertmarken)",
        "Exploration en plaine (2 jetons)",
        "Exploration en montagne (2 jetons)",
        "Exploration en forêt (2 jetons)",
        "Exploration en rivage (2 jetons)"
    };

    internal static readonly string[] HuntingVentureNames = new string[]
    {
        "Hunting.",
        "Mining.",
        "Botany.",
        "Fishing.",
        "調達依頼：渉猟　　（必要ベンチャースクリップ：1枚）",
        "調達依頼：採掘　　（必要ベンチャースクリップ：1枚）",
        "調達依頼：園芸　　（必要ベンチャースクリップ：1枚）",
        "調達依頼：漁猟　　（必要ベンチャースクリップ：1枚）",
        "Beutezug (1 Wertmarke)",
        "Mineraliensuche (1 Wertmarke)",
        "Ernteausflug (1 Wertmarke)",
        "Fischzug (1 Wertmarke)",
        "Travail de chasse (1 jeton)",
        "Travail de mineur (1 jeton)",
        "Travail de botaniste (1 jeton)",
        "Travail de pêche (1 jeton)"
    };

    internal static readonly string[] QuickExploration = new string[]
    {
        "Quick Exploration.",
        "ほりだしもの依頼　（必要ベンチャースクリップ：2枚）",
        "Schneller Streifzug (2 Wertmarken)",
        "Tâche improvisée (2 jetons)"
    };

    internal static readonly string[] Entrance = new string[]
    {
        "ハウスへ入る",
        "Eingang",
        "Entrée",
        "Entrance"
    };

    internal static readonly string[] ConfirmHouseEntrance = new string[]
    {
        "「ハウス」へ入りますか？",
        "Das Gebäude betreten?",
        "Entrer dans la maison ?",
        "Enter the estate hall?"
    };

    internal static string RetainerAskCategoryText => Svc.ClientState.ClientLanguage switch
    {
        ClientLanguage.Japanese => "依頼するリテイナーベンチャーを選んでください",
        ClientLanguage.German => "Wähle eine Unternehmung, auf die du den Gehilfen schicken möchtest.",
        ClientLanguage.French => "Choisissez un type de tâche :",
        _ => "Select a category."
    };

    internal static string BellName
    {
        get => Svc.Data.GetExcelSheet<EObjName>().GetRow(2000401).Singular.ToString();
    }
}
