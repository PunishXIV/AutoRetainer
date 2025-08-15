using AutoRetainerAPI.Configuration;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
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

    internal static readonly string[] FieldExplorationNames =
    [
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
        "Exploration en rivage (2 jetons)",
        "平地探索委托（需要2枚探险币）",
        "山岳探索委托（需要2枚探险币）",
        "森林探索委托（需要2枚探险币）",
        "水岸探索委托（需要2枚探险币）",
        "平地探索委託（需要2枚探險幣）",
        "山岳探索委託（需要2枚探險幣）",
        "森林探索委託（需要2枚探險幣）",
        "水岸探索委託（需要2枚探險幣）",
        "탐색수행: 평지 (필요한 집사 급료: 2개)",
        "탐색수행: 산악 (필요한 집사 급료: 2개)",
        "탐색수행: 삼림 (필요한 집사 급료: 2개)",
        "탐색수행: 물가 (필요한 집사 급료: 2개)",
    ];

    internal static readonly string[] HuntingVentureNames =
    [
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
        "Travail de pêche (1 jeton)",
        "狩猎筹集委托（需要1枚探险币）",
        "采矿筹集委托（需要1枚探险币）",
        "采伐筹集委托（需要1枚探险币）",
        "捕鱼筹集委托（需要1枚探险币）",
        "狩獵籌集委託（需要1枚探險幣）",
        "採礦籌集委託（需要1枚探險幣）",
        "採伐籌集委託（需要1枚探險幣）",
        "捕魚籌集委託（需要1枚探險幣）",
        "조달수행: 사냥 (필요한 집사 급료: 1개)",
        "조달수행: 광부 (필요한 집사 급료: 1개)",
        "조달수행: 원예가 (필요한 집사 급료: 1개)",
        "조달수행: 어부 (필요한 집사 급료: 1개)",
    ];

    internal static readonly string[] QuickExploration =
    [
        "Quick Exploration.",
        "ほりだしもの依頼　（必要ベンチャースクリップ：2枚）",
        "Schneller Streifzug (2 Wertmarken)",
        "Tâche improvisée (2 jetons)",
        "自由探索委托（需要2枚探险币）",
        "自由探索委託（需要2枚探險幣）",
        "발굴수행 (필요한 집사 급료: 2개)",
    ];

    internal static readonly string[] Entrance =
    [
        "ハウスへ入る",
        "进入房屋",
        "進入房屋",
        "Eingang",
        "Entrée",
        "Entrance",
        "주택으로 들어가기",
    ];

    internal static string ApartmentEntrance => Svc.Data.GetExcelSheet<EObjName>().GetRow(2007402).Singular.ToString();

    internal static readonly string[] ConfirmHouseEntrance =
    [
        "「ハウス」へ入りますか？",
        "要进入这间房屋吗？",
        "要進入這間房屋嗎？",
        "Das Gebäude betreten?",
        "Entrer dans la maison ?",
        "Enter the estate hall?",
        "'주택'으로 들어가시겠습니까?",
    ];

    internal static readonly string[] RetainerAskCategoryText =
    [
        "依頼するリテイナーベンチャーを選んでください",
        "请选择要委托的探险",
        "請選擇要委託的探險",
        "Wähle eine Unternehmung, auf die du den Gehilfen schicken möchtest.",
        "Choisissez un type de tâche :",
        "Select a category.",
        "집사 수행의 종류를 선택하십시오.",
    ];

    internal static string[] BellName => [Svc.Data.GetExcelSheet<EObjName>().GetRow(2000401).Singular.GetText(), "リテイナーベル"];

    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Go to your apartment
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	自分の部屋に移動する
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Die eigene Wohnung betreten
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Aller dans votre appartement

    internal static readonly string[] GoToYourApartment =
    [
        "Go to your apartment",
        "自分の部屋に移動する",
        "移动到自己的房间",
        "移動到自己的房間",
        "Die eigene Wohnung betreten",
        "Aller dans votre appartement",
        "자신의 방으로 이동",
    ];

    internal static readonly string[] SkipCutsceneStr =
    [
        "Skip cutscene?",
        "要跳过这段过场动画吗？",
        "要跳過這段過場動畫嗎？",
        "Videosequenz überspringen?",
        "Passer la scène cinématique ?",
        "このカットシーンをスキップしますか？",
        "영상을 건너뛰시겠습니까?",
    ];
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Move to the company workshop
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	地下工房に移動する
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Die Ge<SoftHyphen/>sell<SoftHyphen/>schaftswerkstätte betreten
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Aller dans l'atelier de compagnie
    internal static readonly string[] EnterWorkshop = ["Move to the company workshop", "地下工房に移動する", "移动到部队工房", "移動到部隊工房", "Die Gesellschaftswerkstätte betreten", "Aller dans l'atelier de compagnie", "지하공방으로 이동"];

    internal static readonly string[] AirshipManagement = ["Airship Management", "飛空艇の管理", "管理飞空艇", "管理飛空艇", "Luftschiff verwalten", "Contrôle aérien", "비공정 관리"];
    internal static readonly string[] SubmarineManagement = ["Submersible Management", "潜水艦の管理", "管理潜水艇", "管理潛水艇", "Tauchboot verwalten", "Contrôle sous-marin", "잠수함 관리"];
    internal static readonly string[] CancelVoyage = ["Cancel", "キャンセル", "取消", "Abbrechen", "Annuler", "취소"];
    internal static readonly string[] NothingVoyage = ["Nothing.", "やめる", "取消", "Nichts", "Annuler", "그만두기"];
    internal static readonly string[] DeployOnSubaquaticVoyage = ["Deploy submersible on subaquatic voyage", "ボイジャー出港", "出发", "出發", "Auf Erkundung gehen", "Expédier le sous-marin", "탐사 출항"];
    internal static readonly string[] ViewPrevVoyageLog = ["View previous voyage log", "前回のボイジャー報告", "上次的远航报告", "上次的遠航報告", "Bericht der letzten Erkundung", "Consulter le journal de la précédente expédition", "이전 탐사 보고서"];
    internal static readonly string[] VoyageQuitEntry = ["Quit", "やめる", "取消", "Beenden", "Annuler", "그만두기"];
    internal static readonly string[] ChangeSubmersibleComponents = ["Change submersible components", "パーツの変更", "Bauteile austauschen", "Changer les éléments", "부품 변경"]; // Missing Chinese
    internal static readonly string[] RegisterSub = ["Outfit and register a submersible.", "潜水艦の新規登録", "Registrierung eines neuen Tauchboots", "Enregistrement d'un sous-marin", "새 잠수함 등록"]; // Missing Chinese

    internal static readonly string[] PanelAirship = ["Select an airship.", "飛空艇を選択してください。", "请选择飞空艇。", "請選擇飛空艇。", "Wähle ein Luftschiff.", "Choisissez un aéronef.", "비공정을 선택하십시오."];
    internal static readonly string[] PanelSubmersible = ["Select a submersible.", "潜水艦を選択してください。", "请选择潜水艇。", "請選擇潛水艇。", "Wähle ein Tauchboot.", "Choisissez un sous-marin.", "잠수함을 선택하십시오."];

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

    internal static string[] UnableToVisitWorld = ["Unable to execute command. Character is currently visiting the", "他のデータセンター", "无法进行该操作，其他玩家正在操作该潜水艇。", "無法進行該操作，其他玩家正在操作該潛水艇。", "Der Vorgang kann nicht ausgeführt werden, da der Charakter gerade das Datenzentrum", "Impossible d'exécuter cette commande. Le personnage se trouve dans un autre centre de traitement de données", "다른 데이터 센터"];

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

    internal static readonly string[] UnableToRepairVessel = ["修理に必要な", "修理所必需的", "Unable to repair vessel", "Du hast nicht genug", "Für die Reparatur ist", "Réparation impossible. Vous n'avez pas", "nécessaire à la réparation", "수리에 필요한"];

    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	パーツの修理
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	Bauteile reparieren
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	Réparer des éléments
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	修理配件
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	パーツの修理
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	Bauteile reparieren
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	Réparer des éléments
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	修理配件

    internal static readonly string[] WorkshopRepair =
    [
        "Repair submersible components",
        "Repair airship components",
        "パーツの修理",
        "Bauteile reparieren",
        "Réparer des éléments",
        "パーツの修理",
        "Bauteile reparieren",
        "Réparer des éléments",
        "修理配件",
        "부품 수리",
    ];

    //Use <If(Equal(IntegerParameter(4),1))>your last <SheetEn(Item,3,IntegerParameter(2),1,1)/><Else/><Value>IntegerParameter(3)</Value> of your <Value>IntegerParameter(4)</Value> <SheetEn(Item,3,IntegerParameter(2),2,1)/></If> to repair your vessel's <SheetEn(Item,3,IntegerParameter(1),1,1)/>?
    //6587	<If(Equal(IntegerParameter(3),1))><Clickable(<SheetDe(Item,2,IntegerParameter(2),1,4,1)/>)/><Else/><Value>IntegerParameter(3)</Value> <SheetDe(Item,5,IntegerParameter(2),2,4,1)/></If> (Besitz: <Value>IntegerParameter(4)</Value>) benutzen, um <SheetDe(Item,2,IntegerParameter(1),1,4,1)/> zu reparieren?
    //6587	Utiliser <If(Equal(IntegerParameter(3),1))><SheetFr(Item,1,IntegerParameter(2),1,1)/><Else/><Value>IntegerParameter(3)</Value> <SheetFr(Item,12,IntegerParameter(2),2,1)/></If> pour réparer <SheetFr(Item,2,IntegerParameter(1),1,1)/> de votre appareil<Indent/>? (<Value>IntegerParameter(4)</Value> possédé<If(LessThanOrEqualTo(IntegerParameter(4),1))><Else/>s</If>)
    /*6587	下記のアイテムを修理しますか？
    <Sheet(Item,IntegerParameter(1),0)/>
    消費:<Sheet(Item,IntegerParameter(2),0)/>×<Value>IntegerParameter(3)</Value>(所持数 <Value>IntegerParameter(4)</Value>)
    */

    internal static readonly string[] WorkshopRepairConfirm =
        [
            "repair",
            "下記のアイテムを修理しますか",
            "reparieren",
            "réparer",
            "要修理下列部件吗",
            "要修理下列部件嗎",
            "要修理下列元件嗎",
            "수리하시겠습니까?",
        ];

    // Use the components selected and <If(Equal(IntegerParameter(1),1))>the following item<Else/><Value>IntegerParameter(1)</Value> of the following items</If> to outfit and register your submersible?
    /* 6886 Das Tauchboot mit den gewählten Bauteilen registrieren?
     Verbraucht <Value>IntegerParameter(1)</Value> <If(Equal(IntegerParameter(1),1))>Exemplar<Else/>Exemplare</If> des folgenden Gegenstands:
    */
    // 6886 Utiliser les éléments choisis et <If(Equal(IntegerParameter(1),1))>l'objet suivant<Else/><Value>IntegerParameter(1)</Value> des objets suivants</If> pour équiper et enregistrer le sous-marin<Indent/>?
    /*選択したパーツアイテムと以下のアイテムを
       <Value>IntegerParameter(1)</Value>枚消費して潜水艦を登録します。
       よろしいですか？
    */

    internal static readonly string[] WorkshopRegisterConfirm =
    [
            "to outfit and register your submersible",
            "枚消費して潜水艦を登録します",
            "Das Tauchboot mit den gewählten Bauteilen registrieren",
            "pour équiper et enregistrer le sous-marin",
            "잠수함을 등록하시겠습니까",
            //"",
            //""    Missing chinese and korean (Addonsheet - 6886)
    ];

    //Your retainer will be unable to process item buyback requests once recalled. Are you sure you wish to proceed?
    //215	TEXT_CMNDEFRETAINERCALL_00010_ASK_RETURN_WITH_BUYBACK	Wenn du deinen Gehilfen wegschickst, kannst du die von ihm verkauften Gegenstände nicht mehr zurückkaufen. Möchtest du trotzdem fortfahren?
    //215	TEXT_CMNDEFRETAINERCALL_00010_ASK_RETURN_WITH_BUYBACK	Renvoyer le servant effacera la liste de rachat. Confirmer<Indent/>?

    internal static readonly string[] WillBeUnableToProcessBuyback = [
        "Your retainer will be unable to process item buyback requests once recalled. Are you sure you wish to proceed?",
        "リテイナーを帰すと売却依頼アイテムの買い戻しができなくなりますが、よろしいですか？",
        "Renvoyer le servant effacera la liste de rachat. Confirmer",
        "Wenn du deinen Gehilfen wegschickst, kannst du die von ihm verkauften Gegenstände nicht mehr zurückkaufen. Möchtest du trotzdem fortfahren",
        "让雇员返回后将无法购回委托卖掉的道具",
        "讓僱員返回後將無法購回委託賣掉的道具",
        "집사를 돌려보내면 판매 의뢰한 아이템을 재매입할 수 없게 됩니다. 계속하시겠습니까?",
        ];

    internal static readonly string[] LogInPartialText = ["Logging in with", "Log in with", "でログインします。", "einloggen?", "eingeloggt.", "Se connecter avec", "Vous allez vous connecter avec", "Souhaitez-vous vous connecter avec", "登入吗", "登入嗎", "登录吗", "접속하시겠습니까?"];

    //3290	<Sheet(Item,IntegerParameter(1),0)/>×<Value>IntegerParameter(2)</Value>を、<Format(IntegerParameter(3),FF022C)/>枚の軍票と交換します。
    //よろしいですか？
    //3290	<Format(IntegerParameter(3),FF022E)/> Staatstaler gegen <If(Equal(IntegerParameter(2),1))><SheetDe(Item,1,IntegerParameter(1),1,4,1)/><Else/><Format(IntegerParameter(2),FF022E)/> <SheetDe(Item,5,IntegerParameter(1),2,4,1)/></If> eintauschen?
    //3290	Acheter <Value>IntegerParameter(2)</Value> <SheetFr(Item,12,IntegerParameter(1),IntegerParameter(2),1)/> pour <Format(IntegerParameter(3),FF05021D0103)/> sceau<If(LessThanOrEqualTo(IntegerParameter(3),1))><Else/>x</If><Indent/>?

    internal static readonly string[] GCSealExchangeConfirm = ["Exchange", "よろしいですか？", "Staatstaler gegen", "Acheter", "要交换吗", "교환하시겠습니까", "要交換嗎"];

    internal static readonly string[] DiscardItem = ["Discard", "を捨てます。", "wegwerfen", "Jeter"];
}
