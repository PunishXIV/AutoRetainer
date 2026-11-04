using AutoRetainerAPI.Configuration;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using ECommons.ExcelServices.Sheets;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;

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
    internal const string CharP = "";
    internal const string StrDCV = "";

    internal const string IconRefresh = "\uf2f9";
    internal const string IconMultiMode = "\uf021";
    internal const string IconDuplicate = "\uf24d";
    internal const string IconGil = "\uf51e";
    internal const string IconPlanner = "\uf0ae";
    internal const string IconSettings = "\uf013";
    internal const string IconWarning = "\uf071";

    internal const string IconAnchor = "\uf13d";
    internal const string IconLevelup = "\ue098";
    internal const string IconResend = "\ue4bb";
    internal const string IconUnlock = "\uf13e";
    internal const string IconRepeat = "\uf363";
    internal const string IconPath = "\uf55b";
    internal const string IconFire = "\uf06d";

    internal static string LogOutAndExitGame => Svc.Data.GetExcelSheet<Addon>().GetRow(116).Text.GetText(true).Cleanup();

    internal static readonly ReadOnlyDictionary<UnlockMode, string> UnlockModeNames = new(new Dictionary<UnlockMode, string>()
    {
        { UnlockMode.MultiSelect, "Pick max amount of destinations" },
        { UnlockMode.SpamOne, "Spam one destination" },
        { UnlockMode.WhileLevelling, "Include one unlock destination while levelling" },
    });

    internal static readonly (string Normal, string GameFont) Digits = ("0123456789", "");

    //The summoning bell's own menu, in whatever language the client runs.
    private const string RetainerBellDialogue = "custom/000/CmnDefRetainerCall_00010";

    private static string BellText(uint row) => Svc.Data.GetExcelSheet<QuestDialogueText>(name: RetainerBellDialogue)?.GetRowOrDefault(row)?.Value.GetText() ?? "";

    //Everything a log message says before its first macro, which is fixed text in any language.
    internal static string LogMessageOpening(uint row)
    {
        var text = Svc.Data.GetExcelSheet<LogMessage>()?.GetRowOrDefault(row)?.Text.ToDalamudString();
        if(text == null) return "";
        var opening = "";
        foreach(var payload in text.Payloads)
        {
            if(payload is not TextPayload literal) break;
            opening += literal.Text;
        }
        return opening.Trim();
    }

    //196	TASK_CATEGORY_TREASURE	Field Exploration.
    //198	TASK_CATEGORY_MINER_2	Highland Exploration.
    //200	TASK_CATEGORY_BOTANIST_2	Woodland Exploration.
    //202	TASK_CATEGORY_FISHER_2	Waterside Exploration.
    internal static string[] FieldExplorationNames => field ??=
    [
        BellText(196),
        BellText(198),
        BellText(200),
        BellText(202),
        //zh-TW: row ids unverified on that client, so it stays a literal
        "平地探索委託（需要2枚探險幣）",
        "山岳探索委託（需要2枚探險幣）",
        "森林探索委託（需要2枚探險幣）",
        "水岸探索委託（需要2枚探險幣）",
    ];

    //195	TASK_CATEGORY_NORMAL	Hunting.
    //197	TASK_CATEGORY_MINER_1	Mining.
    //199	TASK_CATEGORY_BOTANIST_1	Botany.
    //201	TASK_CATEGORY_FISHER_1	Fishing.
    internal static string[] HuntingVentureNames => field ??=
    [
        BellText(195),
        BellText(197),
        BellText(199),
        BellText(201),
        //zh-TW: row ids unverified on that client, so it stays a literal
        "狩獵籌集委託（需要1枚探險幣）",
        "採礦籌集委託（需要1枚探險幣）",
        "採伐籌集委託（需要1枚探險幣）",
        "捕魚籌集委託（需要1枚探險幣）",
    ];

    //402	TASK_CATEGORY_FORTUNE	Quick Exploration.
    internal static string[] QuickExploration => field ??= [BellText(402), /*zh-TW: row id unverified, stays a literal*/ "自由尋寶委託（需要2枚探險幣）"];

    internal static string[] Entrance => field ??= MenuEntries(
        Svc.Data.GetExcelSheet<EObjName>()?.GetRowOrDefault(2002737)?.Singular.GetText(), "進入房屋");

    internal static string ApartmentEntrance => Svc.Data.GetExcelSheet<EObjName>().GetRow(2007402).Singular.ToString();

    internal static bool ConfirmHouseEntrance(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Warp>()?.GetRowOrDefault(131148)?.Question)
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["要進入這間房屋嗎？"]);

    //194	ASK_CATEGORY	Select a category.
    internal static string[] RetainerAskCategoryText => field ??= [BellText(194), /*zh-TW: row id unverified, stays a literal*/ "請選擇要委託的探險"];

    internal static string[] BellName => [Svc.Data.GetExcelSheet<EObjName>().GetRow(2000401).Singular.GetText(), "リテイナーベル"];

    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Go to your apartment
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	自分の部屋に移動する
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Die eigene Wohnung betreten
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Aller dans votre appartement

    internal static string[] GoToYourApartment => field ??= MenuEntries(
        DialogueText("custom/003/HouFixMansionEntrance_00359", 0)?.GetText(),
        DialogueText("custom/003/HouFixMansionEntrance_00359", 4)?.GetText(), "移動到自己的房間");

    private const string AirshipDialogue = "custom/002/CmnDefCompanyCommanderBoard_00258";
    private const string SubmersibleDialogue = "custom/004/HouFixCompanySubmarine_00447";

    private static ReadOnlySeString? DialogueText(string sheet, uint row) => Svc.Data.GetExcelSheet<QuestDialogueText>(name: sheet)?.GetRowOrDefault(row)?.Value;

    private static string AddonText(uint row) => Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(row)?.Text.GetText() ?? "";

    private static string ManagementText(uint row) => Svc.Data.GetExcelSheet<CustomTalk>()?.GetRowOrDefault(row)?.MainOption.GetText() ?? "";

    // An empty entry must not match the first menu option.
    private static string[] MenuEntries(params string[] entries) => entries.Where(x => !string.IsNullOrEmpty(x)).ToArray();

    // Match each fixed text part. Parameters can change the displayed numbers and item names.
    private static bool MatchesTemplate(string text, ReadOnlySeString? template)
    {
        if(template == null) return false;
        var parts = template.Value.ToDalamudString().Payloads.OfType<TextPayload>()
            .Select(x => x.Text?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return parts.Length > 0 && parts.All(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    // Keep zh-TW literals until the row IDs are verified on that client.
    internal static string[] SkipCutsceneStr => field ??= MenuEntries(AddonText(281), "要跳過這段過場動畫嗎？");
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Move to the company workshop
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	地下工房に移動する
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Die Ge<SoftHyphen/>sell<SoftHyphen/>schaftswerkstätte betreten
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Aller dans l'atelier de compagnie
    internal static string[] EnterWorkshop => field ??= MenuEntries(DialogueText("custom/001/CmnDefHousingPersonalRoomEntrance_00178", 11)?.GetText(), "移動到部隊工房");

    internal static string[] AirshipManagement => field ??= MenuEntries(ManagementText(721154), "管理飛空艇");
    internal static string[] SubmarineManagement => field ??= MenuEntries(ManagementText(721343), "管理潛水艇");
    internal static string[] CancelVoyage => field ??= MenuEntries(AddonText(2), "取消");
    internal static string[] NothingVoyage => field ??= MenuEntries(DialogueText(AirshipDialogue, 5)?.GetText(), DialogueText(SubmersibleDialogue, 6)?.GetText(), "取消");
    internal static string[] DeployOnSubaquaticVoyage => field ??= MenuEntries(DialogueText(SubmersibleDialogue, 8)?.GetText(), "出發");
    internal static string[] ViewPrevVoyageLog => field ??= MenuEntries(DialogueText(AirshipDialogue, 8)?.GetText(), DialogueText(SubmersibleDialogue, 9)?.GetText(), "上次的遠航報告");
    internal static string[] VoyageQuitEntry => field ??= MenuEntries(AddonText(5), "取消");
    internal static string[] ChangeSubmersibleComponents => field ??= MenuEntries(DialogueText(SubmersibleDialogue, 10)?.GetText(), "更換配件");
    internal static bool RegisterSub(string text) => MatchesTemplate(text, DialogueText(SubmersibleDialogue, 3))
        || text.StartsWithAny(["登記新的潛水艇"]);

    internal static bool PanelAirship(string text) => MatchesTemplate(text, DialogueText(AirshipDialogue, 1))
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["請選擇飛空艇。"]);
    internal static bool PanelSubmersible(string text) => MatchesTemplate(text, DialogueText(SubmersibleDialogue, 2))
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["請選擇潛水艇。"]);

    //2004353	entrance to additional chambers	0	entrances to additional chambers	0	1	1	0	0
    internal static string[] AdditionalChambersEntrance =>
    [
        Svc.Data.GetExcelSheet<EObjName>().GetRow(2004353).Singular.GetText(),
        Regex.Replace(Svc.Data.GetExcelSheet<EObjName>().GetRow(2004353).Singular.GetText(), @"\[.*?\]", "")
    ];

    //2005274	voyage control panel	0	voyage control panels	0	0	1	0	0
    internal static string PanelName => Svc.Data.GetExcelSheet<EObjName>().GetRow(2005274).Singular.GetText();

    //4160	60	9	0	False	Unable to retrieve extracted items. Insufficient inventory/crystal inventory space.
    internal static string VoyageInventoryError => Svc.Data.GetExcelSheet<LogMessage>().GetRow(4160).Text.ToDalamudString().GetText();

    internal static bool UnableToVisitWorld(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<LogMessage>()?.GetRowOrDefault(5800)?.Text);

    //4169	60	9	0	False	Unable to repair vessel component without the required <SheetEn(Item,3,IntegerParameter(1),1,1)/>.
    //4272	60	9	0	False Unable to repair vessel.Insufficient<SheetEn(Item,3,IntegerParameter(1),3,1)/>.
    //4169	60	9	0	False	修理に必要な<Sheet(Item,IntegerParameter(1),0)/>を持っていません。
    //4272	60	9	0	False	修理に必要な<Sheet(Item,IntegerParameter(1),0)/>が足りません。
    //4169	60	9	0	False	未持有修理所必需的<Sheet(Item,IntegerParameter(1),0)/>。
    //4272	60	9	0	False	沒有修理所必需的<Sheet(Item,IntegerParameter(1),0)/>。
    //4272	60	9	0	False	Du hast nicht genug <SheetDe(Item,5,IntegerParameter(1),2,4,1)/> für die Reparatur.
    //4169	60	9	0	False	Für die Reparatur ist <SheetDe(Item,1,IntegerParameter(1),1,1,1)/> erforderlich.
    //4169	60	9	0	False	Réparation impossible. Vous n'avez pas <SheetFr(Item,2,IntegerParameter(1),1,1)/> nécessaire.
    //4272	60	9	0	False	Vous n'avez pas <SheetFr(Item,2,IntegerParameter(1),1,1)/> nécessaire à la réparation.

    internal static bool UnableToRepairVessel(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<LogMessage>()?.GetRowOrDefault(4169)?.Text)
        || MatchesTemplate(text, Svc.Data.GetExcelSheet<LogMessage>()?.GetRowOrDefault(4272)?.Text)
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["修理所必需的"]);

    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	パーツの修理
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	Bauteile reparieren
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	Réparer des éléments
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	修理配件
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	パーツの修理
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	Bauteile reparieren
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	Réparer des éléments
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	修理配件

    internal static string[] WorkshopRepair => field ??= MenuEntries(DialogueText(AirshipDialogue, 10)?.GetText(), DialogueText(SubmersibleDialogue, 11)?.GetText(), "修理配件");

    //Use <If(Equal(IntegerParameter(4),1))>your last <SheetEn(Item,3,IntegerParameter(2),1,1)/><Else/><Value>IntegerParameter(3)</Value> of your <Value>IntegerParameter(4)</Value> <SheetEn(Item,3,IntegerParameter(2),2,1)/></If> to repair your vessel's <SheetEn(Item,3,IntegerParameter(1),1,1)/>?
    //6587	<If(Equal(IntegerParameter(3),1))><Clickable(<SheetDe(Item,2,IntegerParameter(2),1,4,1)/>)/><Else/><Value>IntegerParameter(3)</Value> <SheetDe(Item,5,IntegerParameter(2),2,4,1)/></If> (Besitz: <Value>IntegerParameter(4)</Value>) benutzen, um <SheetDe(Item,2,IntegerParameter(1),1,4,1)/> zu reparieren?
    //6587	Utiliser <If(Equal(IntegerParameter(3),1))><SheetFr(Item,1,IntegerParameter(2),1,1)/><Else/><Value>IntegerParameter(3)</Value> <SheetFr(Item,12,IntegerParameter(2),2,1)/></If> pour réparer <SheetFr(Item,2,IntegerParameter(1),1,1)/> de votre appareil<Indent/>? (<Value>IntegerParameter(4)</Value> possédé<If(LessThanOrEqualTo(IntegerParameter(4),1))><Else/>s</If>)
    /*6587	下記のアイテムを修理しますか？
    <Sheet(Item,IntegerParameter(1),0)/>
    消費:<Sheet(Item,IntegerParameter(2),0)/>×<Value>IntegerParameter(3)</Value>(所持数 <Value>IntegerParameter(4)</Value>)
    */

    internal static bool WorkshopRepairConfirm(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(6587)?.Text)
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["要修理下列部件嗎", "要修理下列元件嗎", "要修理下列組件嗎"]);

    // Use the components selected and <If(Equal(IntegerParameter(1),1))>the following item<Else/><Value>IntegerParameter(1)</Value> of the following items</If> to outfit and register your submersible?
    /* 6886 Das Tauchboot mit den gewählten Bauteilen registrieren?
     Verbraucht <Value>IntegerParameter(1)</Value> <If(Equal(IntegerParameter(1),1))>Exemplar<Else/>Exemplare</If> des folgenden Gegenstands:
    */
    // 6886 Utiliser les éléments choisis et <If(Equal(IntegerParameter(1),1))>l'objet suivant<Else/><Value>IntegerParameter(1)</Value> des objets suivants</If> pour équiper et enregistrer le sous-marin<Indent/>?
    /*選択したパーツアイテムと以下のアイテムを
       <Value>IntegerParameter(1)</Value>枚消費して潜水艦を登録します。
       よろしいですか？
    */

    internal static bool WorkshopRegisterConfirm(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(6886)?.Text)
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["張下列道具登記新的潛水艇嗎"]);

    //Your retainer will be unable to process item buyback requests once recalled. Are you sure you wish to proceed?
    //215	TEXT_CMNDEFRETAINERCALL_00010_ASK_RETURN_WITH_BUYBACK	Wenn du deinen Gehilfen wegschickst, kannst du die von ihm verkauften Gegenstände nicht mehr zurückkaufen. Möchtest du trotzdem fortfahren?
    //215	TEXT_CMNDEFRETAINERCALL_00010_ASK_RETURN_WITH_BUYBACK	Renvoyer le servant effacera la liste de rachat. Confirmer<Indent/>?

    //215	ASK_RETURN_WITH_BUYBACK	Your retainer will be unable to process item buyback requests once recalled.
    internal static string[] WillBeUnableToProcessBuyback => field ??= [BellText(215)];

    //3290	<Sheet(Item,IntegerParameter(1),0)/>×<Value>IntegerParameter(2)</Value>を、<Format(IntegerParameter(3),FF022C)/>枚の軍票と交換します。
    //よろしいですか？
    //3290	<Format(IntegerParameter(3),FF022E)/> Staatstaler gegen <If(Equal(IntegerParameter(2),1))><SheetDe(Item,1,IntegerParameter(1),1,4,1)/><Else/><Format(IntegerParameter(2),FF022E)/> <SheetDe(Item,5,IntegerParameter(1),2,4,1)/></If> eintauschen?
    //3290	Acheter <Value>IntegerParameter(2)</Value> <SheetFr(Item,12,IntegerParameter(1),IntegerParameter(2),1)/> pour <Format(IntegerParameter(3),FF05021D0103)/> sceau<If(LessThanOrEqualTo(IntegerParameter(3),1))><Else/>x</If><Indent/>?

    internal static bool GCSealExchangeConfirm(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(3290)?.Text)
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["要交換嗎"]);

    internal static bool DiscardItem(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(110)?.Text)
        || MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(153)?.Text)
        || text.ContainsAny(StringComparison.OrdinalIgnoreCase, ["確定要捨棄"]);

    private static string[] FuelNames => field ??= ItemNames(10155);

    private static string[] ItemNames(uint row)
    {
        var item = Svc.Data.GetExcelSheet<Item>()?.GetRowOrDefault(row);
        return MenuEntries(item?.Singular.GetText(), item?.Plural.GetText(), item?.Name.GetText());
    }

    internal static bool FuelPurchaseConfirm(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(3473)?.Text)
        && FuelNames.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));

    internal static bool RetainerMenu(string text) => MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(2376)?.Text)
        || MatchesTemplate(text, Svc.Data.GetExcelSheet<Addon>()?.GetRowOrDefault(2377)?.Text);
}
