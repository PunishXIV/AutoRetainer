using AutoRetainer.UI;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;
using ECommons.Events;
using PunishLib;
using PunishLib.Sponsor;
using ECommons.Automation;
using ECommons.Configuration;
using Dalamud.Interface.Style;
using Dalamud.Utility;
using AutoRetainer.Scheduler.Tasks;
using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using ECommons.MathHelpers;
using PInvoke;
using ECommons.ExcelServices.TerritoryEnumeration;
using System.Diagnostics;
using AutoRetainer.Modules.Statistics;
using AutoRetainer.Internal;
using AutoRetainer.UI.Overlays;
using ECommons.Throttlers;

namespace AutoRetainer;

public unsafe class AutoRetainer : IDalamudPlugin
{
    public string Name => "AutoRetainer";
    internal static AutoRetainer P;
    internal Config config;
    internal WindowSystem ws;
    internal ConfigGui configGui;
    internal RetainerManager retainerManager;
    internal bool IsInteractionAutomatic = false;
    internal QuickSellItems quickSellItems;
    internal TaskManager TaskManager;
    internal Memory Memory;
    internal bool WasEnabled = false;
    internal bool IsCloseActionAutomatic = false;
    internal long LastMovementAt;
    internal Vector3 LastPosition;
    internal bool IsNextToBell;

    internal long Time => P.config.UseServerTime ? FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.GetServerTime() : DateTimeOffset.Now.ToUnixTimeSeconds();

    internal StyleModel Style;
    internal bool StylePushed = false;

    public AutoRetainer(DalamudPluginInterface pi)
    {
        //PluginLoader.CheckAndLoad(pi, "https://love.puni.sh/plugins/AutoRetainer/blacklist.txt", delegate
        {
            ECommonsMain.Init(pi, this, Module.DalamudReflector);
            PunishLibMain.Init(pi, this);
            SponsorManager.SetSponsorInfo("https://ko-fi.com/spetsnaz");
            P = this;
            new TickScheduler(delegate
            {
                EzConfig.Migrate<Config>();
                config = EzConfig.Init<Config>();
                Migrator.MigrateGC();
                retainerManager = new(Svc.SigScanner);
                ws = new();
                configGui = new();
                TaskManager = new() { AbortOnTimeout = true, TimeLimitMS = 20000 };
                Memory = new();
                Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
                Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
                Svc.ClientState.Logout += Logout;
                Svc.Condition.ConditionChange += ConditionChange;
                EzCmd.Add("/autoretainer", CommandHandler, "Open plugin interface\n/autoretainer e|enable → Enable plugin\n/autoretainer d|disable - Disable plugin\n/autoretainer t|toggle - toggle plugin\n/autoretainer m|multi - toggle MultiMode\n/autoretainer relog Character Name@WorldName - relog to the targeted character if configured\n/autoretainer expert - toggle expert settings\n/autoretainer debug - toggle debug menu and verbose output");
                EzCmd.Add("/ays", CommandHandler);
                Svc.Toasts.ErrorToast += Toasts_ErrorToast;
                Svc.Toasts.Toast += Toasts_Toast;
                Svc.Framework.Update += Tick;
                quickSellItems = new();
                StatisticsManager.Init();
                AutoGCHandin.Init();

                ws.AddWindow(new MultiModeOverlay());
                ws.AddWindow(new RetainerListOverlay());
                MultiMode.Init();

                Safety.Check();

                Style = StyleModel.Deserialize("DS1H4sIAAAAAAAACqVYS3ObOhT+L6w9HZ4CvGuS22bRdDJNOu29OxkrNjUxFGP3kcl/75F0jiSwO3MBb4Swvu+8jyRePO4tgzf+wlt5yxfvq7fM5ORfNb4uvMJb+vLFGkeBq3xc5b9JYNUTcCy8Db7eImOJc77Ch28IZggOlYgd/lvh+IyrYlzF1Kr9AKvf1hffNvg2wreRevsd1FLGtfAAohfegR46NPuIKpxw/IHjTxx/GesTx/rfZz6R4ji/qEVRg50v3qP42RlY5kd5GDAEZ3KSpAvvPznLE5axMAV5X5RbgUJib8oDX1VibThY4ueZHyMHS7M4CmLkUJMojR2OL+V+Xf+42ljVmfqlZIGZKobAT7IsjBKX4npbVuuJDNpF93VzbFyGjLAZouSDWp7HsP6qbteitcvzOAhSRqGwUy0wVh7RjoNA+UHO/MywPGw5OGCG8u9a/iwc5QMZNZYRg5Qm/Y4MoSYkcxKtqqW5rU+idcIZpkok2Rb24hklIdgaIldsWd4WXXmyJRoZFyiSyKSSYokpzxQLQ4Eyv8quErNSAxkG6kzmua6rijcHxz3jqe7E/njFWzdgVCzGr4ELeChaELvqQcZniWF538o2OD5XgnOayblygWtyxlAPEMXujre7v/ShLJdlFyCeyS4UMleXqoRa7PlmrDWGYaYpV8euq2mXgQWhkhUi3KxX6CRM4tx3C1Cjh5GJQQEAEkkCNR+nxhuyBcRUf7lW13INzEnkWp/2CEYcWkB0TnIruNsrR3WnSJJEeWRoZnSnzJDMjM+DaHjLu3qiTbb/G6IZVtGO6tLNaL5O3D6JQ/lbvG/LZnq3sBwzW4Ulmhm8x2nND9IHW6rmmBMxWT5JrnlmhKrH83n/VBdHd2MaZVyeyj4yoJrp6Zu62JX7zX0rTqWw55tYNizQBmlSfeQ0h8xeP0tNqSDZP89N98vZBqmfhWSWI/++qrsP5V4c7I4QUNOyne8SYBjcnOKne6f2A2nMdF4kA6Lb8tDVGzgMWZZe38yk82LyHqPZZY4zhfrndHST2RLk8dOtQghqJXTr6x0TTWpc8J7C4AG1a+u9xUXkRfmA+X0Z+KHcbO3dQu4NOlgmCc9wnybfBPwByduKJKsbIBCoEZBq1AiGd5gHUYmiE+4dYkyaRnppKPO05Zubtm4eebsR3ejYw0qT8h/56RYcWPWcmMCxBUSbwEsXUJuxWWGcChz6fgW1MyT7u1/SAfKmfHY8Q1cjyjfqIdL8u3rNK437f6AoeZVXargzeEvvmndNcyyKcu/BPV9fT/nkKlyNObEE9I1h7HlJA9fjT2saaNtrxqIwN+0arEkZ1Ukegzj3RvA09cK+mdpCthMKQyNLG4aeS/tFkOk90sHRJxrJKTcy6Q0dPwkzoUhVHji43cSGWxmcqSptoNn2tIFmHzVI2+IJQxXpxmw/8WJST639xhqkdTafZEi+Rp67Ar5PTcsSeyI2XtK43tkj143b1dXu0WOCBkDYF+Gf1z+6Z+sXPRQAAA==");
            });
        }
        //);
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
                && P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var offlineData) 
                && offlineData.RetainerData.TryGetFirst(x => x.Name == ret, out var offlineRetainerData))
            {
                offlineRetainerData.VentureBeginsAt = P.Time;
                P.DebugLog($"Recorded venture start time = {offlineRetainerData.VentureBeginsAt}");
            }
            //4578	57	33	0	False	Gil earned from market sales has been entrusted to your retainer.<If(Equal(IntegerParameter(1),1))>
            //The amount earned exceeded your retainer's gil limit. Excess gil has been discarded.<Else/></If>
            if (text.StartsWith(Svc.Data.GetExcelSheet<LogMessage>().GetRow(4578).Text.ToDalamudString().ExtractText(true)))
            {
                TaskWithdrawGil.forceCheck = true;
                P.DebugLog($"Forcing to check for gil");
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
        }
        else if (arguments.StartsWith("relog "))
        {
            var target = P.config.OfflineData.Where(x => $"{x.Name}@{x.World}" == arguments[6..]).FirstOrDefault();
            if (target != null)
            {
                if (!AutoLogin.Instance.IsRunning) AutoLogin.Instance.SwapCharacter(target.World, target.CharaIndex, target.ServiceAccount);
            }
            else
            {
                Notify.Error($"Could not find target character");
            }
        }
        else
        {
            configGui.IsOpen = !configGui.IsOpen;
        }
    }

    private void Tick(Framework framework)
    {
        if (SchedulerMain.PluginEnabled && Svc.ClientState.LocalPlayer != null)
        {
            SchedulerMain.Tick();
            if (!P.config.SelectedRetainers.ContainsKey(Svc.ClientState.LocalContentId))
            {
                P.config.SelectedRetainers[Svc.ClientState.LocalContentId] = new();
            }
        }
        if (SchedulerMain.PluginEnabled || P.TaskManager.IsBusy || Svc.Condition[ConditionFlag.OccupiedSummoningBell])
        {
            if (TryGetAddonByName<AddonTalk>("Talk", out var addon) && addon->AtkUnitBase.IsVisible)
            {
                ClickTalk.Using((IntPtr)addon).Click();
            }
        }
        OfflineDataManager.Tick();
        AutoGCHandin.Tick();
        MultiMode.Tick();
        NotificationHandler.Tick();
        YesAlready.Tick();
        if(SchedulerMain.PluginEnabled || MultiMode.Enabled)
        {
            if(Svc.ClientState.TerritoryType == Prisons.Mordion_Gaol)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
        IsNextToBell = false;
        if (P.config.RetainerSense && Svc.ClientState.LocalPlayer != null)
        {
            if(!IsOccupied() && !P.config.OldRetainerSense && !TaskManager.IsBusy && !MultiMode.Active && !Svc.Condition[ConditionFlag.InCombat] && !Svc.Condition[ConditionFlag.BoundByDuty] && Utils.IsAnyRetainersCompletedVenture())
            {
                var bell = Utils.GetReachableRetainerBell();
                if (bell == null || LastPosition != Svc.ClientState.LocalPlayer.Position)
                {
                    LastPosition = Svc.ClientState.LocalPlayer.Position;
                    LastMovementAt = Environment.TickCount64;
                }
                if(bell != null)
                {
                    IsNextToBell = true;
                }
                if(Environment.TickCount64 - LastMovementAt > P.config.RetainerSenseThreshold)
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
            if (message.ToString().StartsWithAny("Unable to execute command. Character is currently visiting the", "他のデータセンター", "Der Vorgang kann nicht ausgeführt werden, da der Charakter gerade das Datenzentrum", "Impossible d'exécuter cette commande. Le personnage se trouve dans un autre centre de traitement de données"))
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
            Safe(this.quickSellItems.Disable);
            Safe(this.quickSellItems.Dispose);
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
            Svc.ClientState.Logout -= Logout;
            Svc.Condition.ConditionChange -= ConditionChange;
            Svc.Framework.Update -= Tick;
            Svc.Toasts.ErrorToast -= Toasts_ErrorToast;
            Svc.Toasts.Toast -= Toasts_Toast;
            Safe(delegate
            {
                YesAlready.EnableIfNeeded();
            });
            Safe(StatisticsManager.Dispose);
            Safe(TaskManager.Dispose);
            Safe(Memory.Dispose);
            PunishLibMain.Dispose();
            ECommonsMain.Dispose();
        }
        //PluginLoader.Dispose();
    }

    internal HashSet<string> GetSelectedRetainers(ulong cid)
    {
        if (!config.SelectedRetainers.ContainsKey(cid))
        {
            config.SelectedRetainers.Add(cid, new());
        }
        return config.SelectedRetainers[cid];
    }

    internal void DebugLog(string message)
    {
        PluginLog.Information(message);
    }

    private void ConditionChange(ConditionFlag flag, bool value)
    {
        if(flag == ConditionFlag.OccupiedSummoningBell)
        {
            if (Svc.Targets.Target.IsRetainerBell()) {
                if (value)
                {
                    if (MultiMode.Active)
                    {
                        WasEnabled = false;
                        if (IsInteractionAutomatic)
                        {
                            IsInteractionAutomatic = false;
                            SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode);
                        }
                    }
                    else
                    {
                        var bellBehavior = Utils.IsAnyRetainersCompletedVenture() ? P.config.OpenBellBehaviorWithVentures : P.config.OpenBellBehaviorNoVentures;
                        if(bellBehavior != OpenBellBehavior.Pause_AutoRetainer && (Bitmask.IsBitSet(User32.GetKeyState((int)P.config.Suppress), 15) && !CSFramework.Instance()->WindowInactive))
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
                        P.DebugLog($"Enabling plugin because WasEnabled is true");
                        SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
                        WasEnabled = false;
                    }
                    else if(!IsCloseActionAutomatic && P.config.AutoDisable && !MultiMode.Enabled)
                    {
                        P.DebugLog($"Disabling plugin because AutoDisable is on");
                        SchedulerMain.DisablePlugin();
                    }
                }
            }
            IsCloseActionAutomatic = false;
        }
    }

    void Logout(object _, object __)
    {
        SchedulerMain.DisablePlugin();

        if (!AutoLogin.Instance.IsRunning)
        {
            MultiMode.LastLogin = 0;
        }

    }
}
