using AutoRetainer.UI;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;
using ECommons.Events;
using PunishLib;
using ECommons.Automation;
using ECommons.Configuration;
using Dalamud.Interface.Style;
using Dalamud.Utility;
using AutoRetainer.Scheduler.Tasks;
using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using ECommons.ExcelServices.TerritoryEnumeration;
using System.Diagnostics;
using AutoRetainer.Modules.Statistics;
using AutoRetainer.Internal;
using AutoRetainer.UI.Overlays;
using ECommons.Throttlers;
using AutoRetainerAPI.Configuration;
using AutoRetainerAPI;
using ECommons.GameHelpers;
using AutoRetainer.Modules.Voyage;
using Dalamud.Game.Network;
using System.Threading;
using ECommons.ExcelServices;
using NotificationMasterAPI;
using ECommons.EzSharedDataManager;
using AutoRetainer.UI.Experiments.Inventory;
using ECommons.Reflection;
using Dalamud.Interface.Internal.Notifications;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AutoRetainer;

public unsafe class AutoRetainer : IDalamudPlugin
{
    public string Name => "AutoRetainer";
    internal static AutoRetainer P;
    internal static Config C => P.config;
    private Config config;
    internal WindowSystem ws;
    internal ConfigGui configGui;
    internal bool IsInteractionAutomatic = false;
    internal QuickSellItems quickSellItems;
    internal TaskManager TaskManager;
    internal TaskManager ODMTaskManager;
    internal Memory Memory;
    internal bool WasEnabled = false;
    internal bool IsCloseActionAutomatic = false;
    internal long LastMovementAt;
    internal Vector3 LastPosition;
    internal bool IsNextToBell;
    internal bool ConditionWasEnabled = false;
    internal VenturePlanner VenturePlanner;
    internal VentureBrowser VentureBrowser;
    internal LogWindow LogWindow;
    internal AutoRetainerApi API;
    internal LoginOverlay LoginOverlay;
    internal MarketCooldownOverlay MarketCooldownOverlay;
    internal SubmarineUnlockPlanUI SubmarineUnlockPlanUI;
    internal SubmarinePointPlanUI SubmarinePointPlanUI;
    internal DuplicateBlacklistSelector DuplicateBlacklistSelector;

    internal long Time => C.UseServerTime ? CSFramework.GetServerTime() : DateTimeOffset.Now.ToUnixTimeSeconds();

    internal StyleModel Style;
    internal bool StylePushed = false;
    internal RetainerListOverlay RetainerListOverlay;
    internal uint LastVentureID = 0;
    internal uint ListUpdateFrame = 0;

    internal bool LogOpcodes = false;
    internal int LastLoadedItems = 0;
    internal NotificationMasterApi NotificationMasterApi;
    internal long[] TimeLaunched;
    internal ContextMenuManager ContextMenuManager;
    public bool ReadOnly = false;

    internal static OfflineCharacterData Data => Utils.GetCurrentCharacterData();

    public AutoRetainer(DalamudPluginInterface pi)
    {
        //PluginLoader.CheckAndLoad(pi, "https://love.puni.sh/plugins/AutoRetainer/blacklist.txt", delegate
        {
            ECommonsMain.Init(pi, this, Module.DalamudReflector);
            PunishLibMain.Init(pi, Name, PunishOption.DefaultKoFi); // Default button
            P = this;
            var cnt = Singleton.GetFFXIVCNT();
            PluginLog.Information($"FFXIV instances: {cnt}");
            if (Singleton.AcquireLock() || cnt <= 1)
            {
                new TickScheduler(Load);
            }
            else
            {
                new SingletonNotifyWindow();
            }
        }
        //);
    }

    public void Load()
    {
        EzConfig.Migrate<Config>();
        config = EzConfig.Init<Config>();
        Migrator.MigrateGC();
        ws = new();
        VenturePlanner = new();
        ws.AddWindow(VenturePlanner);
        VentureBrowser = new();
        ws.AddWindow(VentureBrowser);
        LogWindow = new();
        ws.AddWindow(LogWindow);
        configGui = new();
        MarketCooldownOverlay = new();
        ws.AddWindow(MarketCooldownOverlay);
        DuplicateBlacklistSelector = new();
        ws.AddWindow(DuplicateBlacklistSelector);
        TaskManager = new() { AbortOnTimeout = true, TimeLimitMS = 20000 };
        Memory = new();
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
        Svc.ClientState.Logout += Logout;
        Svc.Condition.ConditionChange += ConditionChange;
        EzCmd.Add("/autoretainer", CommandHandler, "Open plugin interface\n/autoretainer e|enable → Enable plugin\n/autoretainer d|disable - Disable plugin\n/autoretainer t|toggle - toggle plugin\n/autoretainer m|multi - toggle MultiMode\n/autoretainer relog Character Name@WorldName - relog to the targeted character if configured\n/autoretainer b|browser - open venture browser\n/autoretainer expert - toggle expert settings\n/autoretainer debug - toggle debug menu and verbose output\n/autoretainer shutdown <hours> [minutes] [seconds] - schedule a game shutdown in this amount of time");
        EzCmd.Add("/ays", CommandHandler);
        Svc.Toasts.ErrorToast += Toasts_ErrorToast;
        Svc.Toasts.Toast += Toasts_Toast;
        Svc.Framework.Update += Tick;
        quickSellItems = new();
        StatisticsManager.Init();
        AutoGCHandin.Init();
        IPC.Init();
        Utils.FixKeys();
        VoyageMain.Init();

        ws.AddWindow(new MultiModeOverlay());
        RetainerListOverlay = new RetainerListOverlay();
        ws.AddWindow(RetainerListOverlay);
        LoginOverlay = (new LoginOverlay());
        ws.AddWindow(LoginOverlay);
        SubmarineUnlockPlanUI = new();
        ws.AddWindow(SubmarineUnlockPlanUI);
        SubmarinePointPlanUI = new();
        ws.AddWindow(SubmarinePointPlanUI);
        MultiMode.Init();
        NotificationMasterApi = new(Svc.PluginInterface);
        ODMTaskManager = new()
        {
            TimeLimitMS = 60 * 1000,
            AbortOnTimeout = true,
        };

        Safety.Check();

        Style = StyleModel.Deserialize("DS1H4sIAAAAAAAACqVYS3ObOhT+L6w9HZ4CvGuS22bRdDJNOu29OxkrNjUxFGP3kcl/75F0jiSwO3MBb4Swvu+8jyRePO4tgzf+wlt5yxfvq7fM5ORfNb4uvMJb+vLFGkeBq3xc5b9JYNUTcCy8Db7eImOJc77Ch28IZggOlYgd/lvh+IyrYlzF1Kr9AKvf1hffNvg2wreRevsd1FLGtfAAohfegR46NPuIKpxw/IHjTxx/GesTx/rfZz6R4ji/qEVRg50v3qP42RlY5kd5GDAEZ3KSpAvvPznLE5axMAV5X5RbgUJib8oDX1VibThY4ueZHyMHS7M4CmLkUJMojR2OL+V+Xf+42ljVmfqlZIGZKobAT7IsjBKX4npbVuuJDNpF93VzbFyGjLAZouSDWp7HsP6qbteitcvzOAhSRqGwUy0wVh7RjoNA+UHO/MywPGw5OGCG8u9a/iwc5QMZNZYRg5Qm/Y4MoSYkcxKtqqW5rU+idcIZpkok2Rb24hklIdgaIldsWd4WXXmyJRoZFyiSyKSSYokpzxQLQ4Eyv8quErNSAxkG6kzmua6rijcHxz3jqe7E/njFWzdgVCzGr4ELeChaELvqQcZniWF538o2OD5XgnOayblygWtyxlAPEMXujre7v/ShLJdlFyCeyS4UMleXqoRa7PlmrDWGYaYpV8euq2mXgQWhkhUi3KxX6CRM4tx3C1Cjh5GJQQEAEkkCNR+nxhuyBcRUf7lW13INzEnkWp/2CEYcWkB0TnIruNsrR3WnSJJEeWRoZnSnzJDMjM+DaHjLu3qiTbb/G6IZVtGO6tLNaL5O3D6JQ/lbvG/LZnq3sBwzW4Ulmhm8x2nND9IHW6rmmBMxWT5JrnlmhKrH83n/VBdHd2MaZVyeyj4yoJrp6Zu62JX7zX0rTqWw55tYNizQBmlSfeQ0h8xeP0tNqSDZP89N98vZBqmfhWSWI/++qrsP5V4c7I4QUNOyne8SYBjcnOKne6f2A2nMdF4kA6Lb8tDVGzgMWZZe38yk82LyHqPZZY4zhfrndHST2RLk8dOtQghqJXTr6x0TTWpc8J7C4AG1a+u9xUXkRfmA+X0Z+KHcbO3dQu4NOlgmCc9wnybfBPwByduKJKsbIBCoEZBq1AiGd5gHUYmiE+4dYkyaRnppKPO05Zubtm4eebsR3ejYw0qT8h/56RYcWPWcmMCxBUSbwEsXUJuxWWGcChz6fgW1MyT7u1/SAfKmfHY8Q1cjyjfqIdL8u3rNK437f6AoeZVXargzeEvvmndNcyyKcu/BPV9fT/nkKlyNObEE9I1h7HlJA9fjT2saaNtrxqIwN+0arEkZ1Ukegzj3RvA09cK+mdpCthMKQyNLG4aeS/tFkOk90sHRJxrJKTcy6Q0dPwkzoUhVHji43cSGWxmcqSptoNn2tIFmHzVI2+IJQxXpxmw/8WJST639xhqkdTafZEi+Rp67Ar5PTcsSeyI2XtK43tkj143b1dXu0WOCBkDYF+Gf1z+6Z+sXPRQAAA==");

        API = new();
        ApiTest.Init();
        FPSManager.UnlockChillFrames();
        Svc.GameNetwork.NetworkMessage += GameNetwork_NetworkMessage;
        Utils.ResetEscIgnoreByWindows();
        Svc.PluginInterface.UiBuilder.Draw += FPSLimiter.FPSLimit;
        AutoCutsceneSkipper.Init(MiniTA.ProcessCutsceneSkip);
        EzSharedData.TryGet("AutoRetainer.Started", out TimeLaunched, CreationMode.CreateAndKeep, [DateTimeOffset.Now.ToUnixTimeMilliseconds()]);
        if (!C.NightModePersistent) C.NightMode = false;
        ContextMenuManager = new();
        PluginLog.Information($"AutoRetainer v{P.GetType().Assembly.GetName().Version} is ready.");
        if (!EzSharedData.TryGet<object>("AutoRetainer.WasLoaded", out _) && C.MultiAutoStart)
        {
            MultiMode.PerformAutoStart();
        }
    }

    private void GameNetwork_NetworkMessage(nint dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
    {
        if (!LogOpcodes) return;
        DuoLog.Information($"{opCode:X16}, {direction}");
    }

    private void Toasts_Toast(ref Dalamud.Game.Text.SeStringHandling.SeString message, ref Dalamud.Game.Gui.Toast.ToastOptions options, ref bool isHandled)
    {
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && ProperOnLogin.PlayerPresent)
        {
            var text = message.ToString();
            //4330	57	33	0	False	リテイナーベンチャー「<Value>IntegerParameter(2)</Value> <Sheet(Item,IntegerParameter(1),0)/>」を依頼しました。
            //4330	57	33	0	False	Du hast deinen Gehilfen mit der Beschaffung von <SheetDe(Item,1,IntegerParameter(1),IntegerParameter(3),3,1)/> ( <Value>IntegerParameter(2)</Value>) beauftragt.
            //4330	57	33	0	False	Vous avez confié la tâche “<SheetFr(Item,12,IntegerParameter(1),2,1)/> ( <Value>IntegerParameter(2)</Value>)” à votre servant.
            if (text.StartsWithAny("You assign your retainer", "リテイナーベンチャー", "Du hast deinen Gehilfen mit", "Vous avez confié la tâche")
                && Utils.TryGetCurrentRetainer(out var ret) 
                && C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var offlineData) 
                && offlineData.RetainerData.TryGetFirst(x => x.Name == ret, out var offlineRetainerData))
            {
                offlineRetainerData.VentureBeginsAt = P.Time;
                DebugLog($"Recorded venture start time = {offlineRetainerData.VentureBeginsAt}");
            }
            //4578	57	33	0	False	Gil earned from market sales has been entrusted to your retainer.<If(Equal(IntegerParameter(1),1))>
            //The amount earned exceeded your retainer's gil limit. Excess gil has been discarded.<Else/></If>
            if (text.StartsWith(Svc.Data.GetExcelSheet<LogMessage>().GetRow(4578).Text.ToDalamudString().ExtractText(true)))
            {
                TaskWithdrawGil.forceCheck = true;
                DebugLog($"Forcing to check for gil");
            }
        }
    }

    private void CommandHandler(string command, string arguments)
    {
        if (arguments.EqualsIgnoreCase("debug"))
        {
            config.Verbose = !config.Verbose;
            DuoLog.Information($"Debug mode {(config.Verbose ? "enabled" : "disabled")}");
        }
        else if (arguments.EqualsIgnoreCase("expert"))
        {
            config.Expert = !config.Expert;
            DuoLog.Information($"Expert mode {(config.Expert ? "enabled" : "disabled")}");
        }
        else if(arguments.EqualsIgnoreCaseAny("e", "enable"))
        {
            SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
        }
        else if (arguments.EqualsIgnoreCaseAny("d", "disable"))
        {
            SchedulerMain.DisablePlugin();
        }
        else if (arguments.EqualsIgnoreCaseAny("t", "toggle"))
        {
            Svc.Commands.ProcessCommand(SchedulerMain.PluginEnabled ? "/ays d" : "/ays e");
        }
        else if (arguments.EqualsIgnoreCaseAny("m", "multi"))
        {
            MultiMode.Enabled = !MultiMode.Enabled;
            MultiMode.OnMultiModeEnabled();
        }
        else if (arguments.StartsWithAny(StringComparison.OrdinalIgnoreCase, "m ", "multi "))
        {
            var arg2 = arguments.Split(" ")[1];
            if(arg2.EqualsIgnoreCaseAny("d", "disable"))
            {
                if (MultiMode.Enabled) MultiMode.Enabled = false;
            }
            else if (arg2.EqualsIgnoreCaseAny("e", "enable"))
            {
                if (!MultiMode.Enabled)
                {
                    MultiMode.Enabled = true;
                    MultiMode.OnMultiModeEnabled();
                }
            }
        }
        else if (arguments.EqualsIgnoreCaseAny("b", "browser"))
        {
            VentureBrowser.IsOpen = !VentureBrowser.IsOpen;
        }
        else if (arguments.EqualsIgnoreCaseAny("l", "log"))
        {
            LogWindow.IsOpen = !LogWindow.IsOpen;
        }
        else if (arguments.StartsWith("relog "))
        {
            var target = C.OfflineData.Where(x => $"{x.Name}@{x.World}" == arguments[6..]).FirstOrDefault();
            if (target != null)
            {
                MultiMode.Relog(target, out _, RelogReason.Command);
                /*if (!AutoLogin.Instance.IsRunning)
                {
                    if (Svc.ClientState.IsLoggedIn)
                    {
                        AutoLogin.Instance.SwapCharacter(target.CurrentWorld, target.Name, ExcelWorldHelper.GetWorldByName(target.World).RowId, target.ServiceAccount);
                    }
                    else
                    {
                        AutoLogin.Instance.Login(target.CurrentWorld, target.Name, ExcelWorldHelper.GetWorldByName(target.World).RowId, target.ServiceAccount);
                    }
                }*/
            }
            else
            {
                Notify.Error($"Could not find target character");
            }
        }
        else if (arguments.EqualsIgnoreCase("het"))
        {
            HouseEnterTask.EnqueueTask();
        }
        else if (arguments.EqualsIgnoreCaseAny("deliver"))
        {
            GCContinuation.EnableDeliveringIfPossible();
        }
        else if (arguments.StartsWith("shutdown"))
        {
            var str = arguments.Split((char[])[' ', ',', ':', '-', '/', '.'], StringSplitOptions.RemoveEmptyEntries);
            if(str.Length <= 1)
            {
                Shutdown.ShutdownAt = 0;
                Shutdown.ForceShutdownAt = 0;
                Svc.Chat.Print("Shutdown timer cleared");
            }
            else
            {
                try
                {
                    var time = new TimeSpan();
                    time = time.Add(TimeSpan.FromHours(int.Parse(str[1])));
                    if (str.Length > 2) time = time.Add(TimeSpan.FromMinutes(int.Parse(str[2])));
                    if (str.Length > 3) time = time.Add(TimeSpan.FromSeconds(int.Parse(str[3])));
                    if (time.TotalSeconds < 10)
                    {
                        DuoLog.Error("Timer can't be less than 10 seconds");
                    }
                    else
                    {
                        Svc.Chat.Print($"Shutting down in {time}");
                        Shutdown.ShutdownAt = Environment.TickCount64 + (long)time.TotalMilliseconds;
                        Shutdown.ForceShutdownAt = Environment.TickCount64 + (long)time.TotalMilliseconds + 10 * 60 * 1000;
                    }
                }
                catch(Exception e)
                {
                    DuoLog.Error($"{e.Message}");
                    PluginLog.Error($"{e.StackTrace}");
                }
            }
        }
        else if (arguments.StartsWith("modifySoftVendorList"))
        {
            if(int.TryParse(arguments.Split(" ")[1], out var num))
            {
                if(num > 0)
                {
                    var id = (uint)num;
                    if (!C.IMAutoVendorSoft.Contains(id))
                    {
                        C.IMAutoVendorSoft.Add(id);
                        PluginLog.Warning($"External addition to soft vendor list: {ExcelItemHelper.GetName(id)}");
                    }
                }
                else if(num < 0)
                {
                    var id = (uint)-num;
                    if (C.IMAutoVendorSoft.Contains(id))
                    {
                        C.IMAutoVendorSoft.Remove(id);
                        PluginLog.Warning($"External removal from soft vendor list: {ExcelItemHelper.GetName(id)}");
                    }
                }
            }
        }
        else if (arguments.StartsWith("set"))
        {
            try
            {
                var field = arguments.Split(" ")[1];
                var value = arguments.Split(" ")[2];
                if (C.GetFoP(field).GetType() == typeof(bool))
                {
                    C.SetFoP(field, bool.Parse(value));
                    DuoLog.Information($"Set bool {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(int))
                {
                    C.SetFoP(field, int.Parse(value));
                    DuoLog.Information($"Set int {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(uint))
                {
                    C.SetFoP(field, uint.Parse(value));
                    DuoLog.Information($"Set uint {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(float))
                {
                    C.SetFoP(field, float.Parse(value));
                    DuoLog.Information($"Set float {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(double))
                {
                    C.SetFoP(field, double.Parse(value));
                    DuoLog.Information($"Set double {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(nint))
                {
                    C.SetFoP(field, nint.Parse(value));
                    DuoLog.Information($"Set nint {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(long))
                {
                    C.SetFoP(field, long.Parse(value));
                    DuoLog.Information($"Set long {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(ulong))
                {
                    C.SetFoP(field, ulong.Parse(value));
                    DuoLog.Information($"Set ulong {field}={value}");
                }
                else if (C.GetFoP(field).GetType() == typeof(string))
                {
                    C.SetFoP(field, value);
                    DuoLog.Information($"Set string {field}={value}");
                }
            }
            catch(Exception e)
            {
                e.LogDuo();
            }
        }
        else
        {
            configGui.IsOpen = !configGui.IsOpen;
        }
    }

    private void Tick(object _)
    {
        if (!IPC.Suppressed)
        {
            if (SchedulerMain.PluginEnabled && Svc.ClientState.LocalPlayer != null)
            {
                SchedulerMain.Tick();
                if (!C.SelectedRetainers.ContainsKey(Svc.ClientState.LocalContentId))
                {
                    C.SelectedRetainers[Svc.ClientState.LocalContentId] = new();
                }
            }
        }
        MiniTA.Tick();
        OfflineDataManager.Tick();
        AutoGCHandin.Tick();
        MultiMode.Tick();
        NotificationHandler.Tick();
        NewYesAlreadyManager.Tick();
        Artisan.ArtisanTick();
        FPSManager.Tick();
        PriorityManager.Tick();
        TextAdvanceManager.Tick();
        Shutdown.Tick();
        BailoutManager.Tick();
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && Utils.TryGetCurrentRetainer(out var name) && Utils.TryGetRetainerByName(name, out var retainer))
        {
            if (!retainer.VentureID.EqualsAny(0u, LastVentureID))
            {
                LastVentureID = retainer.VentureID;
                PluginLog.Debug($"Retainer {retainer.Name} current venture={LastVentureID}");
            }
        }
        else
        {
            if (LastVentureID != 0)
            {
                LastVentureID = 0;
                PluginLog.Debug($"Last venture ID reset");
            }
        }
        //if(C.RetryItemSearch) RetryItemSearch.Tick();
        if (SchedulerMain.PluginEnabled || MultiMode.Enabled || TaskManager.IsBusy)
        {
            if(Svc.ClientState.TerritoryType == Prisons.Mordion_Gaol)
            {
                Process.GetCurrentProcess().Kill();
            }
            if (Svc.Condition[ConditionFlag.OccupiedSummoningBell])
            {
                if (!ConditionWasEnabled)
                {
                    ConditionWasEnabled = true;
                    DebugLog($"ConditionWasEnabled = true");
                }
            }
        }
        IsNextToBell = false;
        if (C.RetainerSense && Svc.ClientState.LocalPlayer != null && Svc.ClientState.LocalPlayer.HomeWorld.Id == Svc.ClientState.LocalPlayer.CurrentWorld.Id)
        {
            if(!IPC.Suppressed && !IsOccupied() && !C.OldRetainerSense && !TaskManager.IsBusy && !Utils.MultiModeOrArtisan && !Svc.Condition[ConditionFlag.InCombat] && !Svc.Condition[ConditionFlag.BoundByDuty] && Utils.IsAnyRetainersCompletedVenture())
            {
                var bell = Utils.GetReachableRetainerBell(true);
                if (bell == null || LastPosition != Svc.ClientState.LocalPlayer.Position)
                {
                    LastPosition = Svc.ClientState.LocalPlayer.Position;
                    LastMovementAt = Environment.TickCount64;
                }
                if(bell != null)
                {
                    IsNextToBell = true;
                }
                if(Environment.TickCount64 - LastMovementAt > C.RetainerSenseThreshold)
                {
                    if (bell != null)
                    {
                        IsNextToBell = true;
                        if (EzThrottler.Throttle("RetainerSense", 30000))
                        {
                            TaskInteractWithNearestBell.Enqueue();
                            TaskManager.Enqueue(() => { SchedulerMain.EnablePlugin(PluginEnableReason.Auto); return true; });
                        }
                    }
                }
            }
        }
    }

    private void Toasts_ErrorToast(ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
    {
        if (P.TaskManager.IsBusy)
        {
            var text = message.ExtractText();
            if (text == Svc.Data.GetExcelSheet<LogMessage>().GetRow(10350).Text.ToDalamudString().ExtractText())
            {
                TaskEntrustDuplicates.NoDuplicates = true;
            }
        }
        if (!Svc.ClientState.IsLoggedIn)
        {
            //5800	60	8	0	False	Unable to execute command. Character is currently visiting the <Highlight>StringParameter(1)</Highlight> data center.
            //5800	60	8	0	False	他のデータセンター<Highlight>StringParameter(1)</Highlight>へ遊びに行っているため操作できません。
            //5800	60	8	0	False	Der Vorgang kann nicht ausgeführt werden, da der Charakter gerade das Datenzentrum <Highlight>StringParameter(1)</Highlight> bereist.
            //5800	60	8	0	False	Impossible d'exécuter cette commande. Le personnage se trouve dans un autre centre de traitement de données (<Highlight>StringParameter(1)</Highlight>).
            if (message.ToString().StartsWithAny(Lang.UnableToVisitWorld))
            {

                MultiMode.Enabled = false;
                AutoLogin.Instance.Abort();
            }
        }
    }

    public void Dispose()
    {
        //if (PluginLoader.IsLoaded)
        {
            Safe(() => Singleton.ReleaseLock());
            Safe(() => this.quickSellItems.Disable());
            Safe(() => this.quickSellItems.Dispose());
            Safe(() => Svc.PluginInterface.UiBuilder.Draw -= FPSLimiter.FPSLimit);
            Safe(() => Svc.PluginInterface.UiBuilder.Draw -= ws.Draw);
            Safe(() => Svc.ClientState.Logout -= Logout);
            Safe(() => Svc.Condition.ConditionChange -= ConditionChange);
            Safe(() => Svc.Framework.Update -= Tick);
            Safe(() => Svc.Toasts.ErrorToast -= Toasts_ErrorToast);
            Safe(() => Svc.Toasts.Toast -= Toasts_Toast);
            Safe(() => Svc.GameNetwork.NetworkMessage -= GameNetwork_NetworkMessage);
            Safe(() => NewYesAlreadyManager.Unlock());
            Safe(() => TextAdvanceManager.UnlockTA());
            Safe(() => StatisticsManager.Shutdown());
            Safe(() => AutoLogin.Dispose());
            Safe(() => Memory.Dispose());
            Safe(() => IPC.Shutdown());
            Safe(() => API.Dispose());
            Safe(() => FPSManager.ForceRestore());
            Safe(() => PriorityManager.RestorePriority());
            Safe(() => VoyageMain.Shutdown());
            Safe(() => ContextMenuManager.Dispose());
            PunishLibMain.Dispose();
            ECommonsMain.Dispose();
        }
        //PluginLoader.Dispose();
    }

    void AddVenture(string name, uint ventureId)
    {
        if (API.Ready && API.GetOfflineCharacterData(Player.CID).RetainerData.TryGetFirst(x => x.Name == name, out var rdata))
        {
            var adata = API.GetAdditionalRetainerData(Player.CID, rdata.Name);
            if(adata.VenturePlan.List.TryGetFirst(x => x.ID == ventureId, out var v))
            {
                v.Num += 1;
            }
            else
            {
                adata.VenturePlan.List.Add(new(ventureId, 1));
            }
            API.WriteAdditionalRetainerData(Player.CID, rdata.Name, adata);
        }
    }

    IEnumerable<string> ListRetainers()
    {
        if (API.Ready)
        {
            foreach (var x in API.GetOfflineCharacterData(Player.CID).RetainerData)
            {
                yield return x.Name;
            }
        }
    }

    internal HashSet<string> GetSelectedRetainers(ulong cid)
    {
        if (!config.SelectedRetainers.ContainsKey(cid))
        {
            config.SelectedRetainers.Add(cid, new());
        }
        return config.SelectedRetainers[cid];
    }

    internal static string LastLogMsg = string.Empty;
    internal static void DebugLog(string message)
    {
        //if (LastLogMsg != message)
        {
            PluginLog.Debug(message);
        }
    }

    private void ConditionChange(ConditionFlag flag, bool value)
    {
        if(flag == ConditionFlag.OccupiedSummoningBell)
        {
            OfflineDataManager.WriteOfflineData(true, true);
            if (!value)
            {
                ConditionWasEnabled = false;
                DebugLog("ConditionWasEnabled = false;");
            }
            if (Svc.Targets.Target.IsRetainerBell()) {
                if (value)
                {
                    if (Utils.MultiModeOrArtisan)
                    {
                        WasEnabled = false;
                        if (IsInteractionAutomatic)
                        {
                            IsInteractionAutomatic = false;
                            SchedulerMain.EnablePlugin(MultiMode.Enabled? PluginEnableReason.MultiMode : PluginEnableReason.Artisan);
                        }
                    }
                    else
                    {
                        var bellBehavior = Utils.IsAnyRetainersCompletedVenture() ? C.OpenBellBehaviorWithVentures : C.OpenBellBehaviorNoVentures;
                        if(bellBehavior != OpenBellBehavior.Pause_AutoRetainer && IsKeyPressed(C.Suppress) && !CSFramework.Instance()->WindowInactive)
                        {
                            bellBehavior = OpenBellBehavior.Do_nothing;
                            Notify.Info($"Open bell action cancelled");
                        }
                        if (SchedulerMain.PluginEnabled && bellBehavior == OpenBellBehavior.Pause_AutoRetainer)
                        {
                            WasEnabled = true;
                            SchedulerMain.DisablePlugin();
                        }
                        if (IsInteractionAutomatic)
                        {
                            IsInteractionAutomatic = false;
                            SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
                        }
                        else
                        {
                            if (bellBehavior == OpenBellBehavior.Enable_AutoRetainer)
                            {
                                SchedulerMain.EnablePlugin(PluginEnableReason.Access);
                            }
                            else if (bellBehavior == OpenBellBehavior.Disable_AutoRetainer)
                            {
                                SchedulerMain.DisablePlugin();
                            }
                        }
                    }
                }
            }
            else
            {
                if (Svc.Targets.Target.IsRetainerBell() || Svc.Targets.PreviousTarget.IsRetainerBell())
                {
                    if (WasEnabled)
                    {
                        DebugLog($"Enabling plugin because WasEnabled is true");
                        SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
                        WasEnabled = false;
                    }
                    else if(!IsCloseActionAutomatic && C.AutoDisable && !Utils.MultiModeOrArtisan)
                    {
                        DebugLog($"Disabling plugin because AutoDisable is on");
                        SchedulerMain.DisablePlugin();
                    }
                }
            }
            IsCloseActionAutomatic = false;
        }
        if(flag == ConditionFlag.Gathering)
        {
            VentureBrowser.Reset();
            OfflineDataManager.WriteOfflineData(true, true);
        }
    }

    void Logout()
    {
        if (Player.Available)
        {
            PluginLog.Verbose($"Writing logout offline data...");
            OfflineDataManager.WriteOfflineData(true, true);
        }
        SchedulerMain.DisablePlugin();

        if (!AutoLogin.Instance.IsRunning)
        {
            MultiMode.LastLogin = 0;
        }

    }
}
