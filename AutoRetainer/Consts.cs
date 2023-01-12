using Dalamud;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer;

internal static class Consts
{
    internal static string RetainerAskCategoryText => Svc.ClientState.ClientLanguage switch
    {
        ClientLanguage.Japanese => "依頼するリテイナーベンチャーを選んでください",
        ClientLanguage.German => "Wähle eine Unternehmung, auf die du den Gehilfen schicken möchtest.",
        ClientLanguage.French => "Choisissez un type de tâche :",
        _ => "Select a category."
    };

    internal static string RetainerQuickExplorationText => Svc.ClientState.ClientLanguage switch
    {
        ClientLanguage.Japanese => "ほりだしもの依頼　（必要ベンチャースクリップ：2枚）",
        ClientLanguage.German => "Schneller Streifzug (2 Wertmarken)",
        ClientLanguage.French => "Tâche improvisée (2 jetons)",
        _ => "Quick Exploration."
    };

    internal static string BellName
    {
        get => Svc.Data.GetExcelSheet<EObjName>().GetRow(2000401).Singular.ToString();
    }
}
