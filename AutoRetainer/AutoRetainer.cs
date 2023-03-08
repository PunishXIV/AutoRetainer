using AutoRetainer.UI;
using AutoRetainer.Multi;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;
using AutoRetainer.QSI;
using AutoRetainer.Offline;
using AutoRetainer.Statistics;
using AutoRetainer.GcHandin;
using ECommons.Events;
using PunishLib;
using PunishLib.Sponsor;
using ECommons.Automation;
using ECommons.Configuration;
using Dalamud.Interface.Style;
using Dalamud.Utility;
using AutoRetainer.NewScheduler.Tasks;

namespace AutoRetainer;

public class AutoRetainer : IDalamudPlugin
{
    public string Name => "AutoRetainer";
    internal static AutoRetainer P;
    internal Config config;
    internal WindowSystem ws;
    internal ConfigGui configGui;
    internal RetainerManager retainerManager;
    private bool Enabled = false;
    internal bool NoConditionEvent = false;
    internal QuickSellItems quickSellItems;
    internal TaskManager TaskManager;
    internal Memory Memory;

    internal long Time => P.config.UseServerTime ? FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.GetServerTime() : DateTimeOffset.Now.ToUnixTimeSeconds();

    internal StyleModel Style;
    internal bool StylePushed = false;

    public AutoRetainer(DalamudPluginInterface pi)
    {
        ECommonsMain.Init(pi, this, Module.DalamudReflector);
        PunishLibMain.Init(pi, this);
	      SponsorManager.SetSponsorInfo("https://ko-fi.com/spetsnaz");
        P = this;
        new TickScheduler(delegate
        {
            EzConfig.Migrate<Config>();
            config = EzConfig.Init<Config>();
            retainerManager = new(Svc.SigScanner);
            ws = new();
            configGui = new();
            TaskManager = new() { AbortOnTimeout = true };
            Memory = new();
            Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
            Svc.ClientState.Logout += Logout;
            Svc.Condition.ConditionChange += ConditionChange;
            EzCmd.Add("/autoretainer", CommandHandler, "Open plugin interface");
            EzCmd.Add("/ays", CommandHandler, "Open plugin interface");
            Svc.Toasts.ErrorToast += Toasts_ErrorToast;
            Svc.Toasts.Toast += Toasts_Toast;
            Svc.Framework.Update += Tick;
            quickSellItems = new();
            StatisticsManager.Init();
            AutoGCHandin.Init();

            ws.AddWindow(new MultiModeOverlay());
            MultiMode.Init();

            Style = StyleModel.Deserialize("DS1H4sIAAAAAAAACqVYS3ObOhT+L6w9HZ4CvGuS22bRdDJNOu29OxkrNjUxFGP3kcl/75F0jiSwO3MBb4Swvu+8jyRePO4tgzf+wlt5yxfvq7fM5ORfNb4uvMJb+vLFGkeBq3xc5b9JYNUTcCy8Db7eImOJc77Ch28IZggOlYgd/lvh+IyrYlzF1Kr9AKvf1hffNvg2wreRevsd1FLGtfAAohfegR46NPuIKpxw/IHjTxx/GesTx/rfZz6R4ji/qEVRg50v3qP42RlY5kd5GDAEZ3KSpAvvPznLE5axMAV5X5RbgUJib8oDX1VibThY4ueZHyMHS7M4CmLkUJMojR2OL+V+Xf+42ljVmfqlZIGZKobAT7IsjBKX4npbVuuJDNpF93VzbFyGjLAZouSDWp7HsP6qbteitcvzOAhSRqGwUy0wVh7RjoNA+UHO/MywPGw5OGCG8u9a/iwc5QMZNZYRg5Qm/Y4MoSYkcxKtqqW5rU+idcIZpkok2Rb24hklIdgaIldsWd4WXXmyJRoZFyiSyKSSYokpzxQLQ4Eyv8quErNSAxkG6kzmua6rijcHxz3jqe7E/njFWzdgVCzGr4ELeChaELvqQcZniWF538o2OD5XgnOayblygWtyxlAPEMXujre7v/ShLJdlFyCeyS4UMleXqoRa7PlmrDWGYaYpV8euq2mXgQWhkhUi3KxX6CRM4tx3C1Cjh5GJQQEAEkkCNR+nxhuyBcRUf7lW13INzEnkWp/2CEYcWkB0TnIruNsrR3WnSJJEeWRoZnSnzJDMjM+DaHjLu3qiTbb/G6IZVtGO6tLNaL5O3D6JQ/lbvG/LZnq3sBwzW4Ulmhm8x2nND9IHW6rmmBMxWT5JrnlmhKrH83n/VBdHd2MaZVyeyj4yoJrp6Zu62JX7zX0rTqWw55tYNizQBmlSfeQ0h8xeP0tNqSDZP89N98vZBqmfhWSWI/++qrsP5V4c7I4QUNOyne8SYBjcnOKne6f2A2nMdF4kA6Lb8tDVGzgMWZZe38yk82LyHqPZZY4zhfrndHST2RLk8dOtQghqJXTr6x0TTWpc8J7C4AG1a+u9xUXkRfmA+X0Z+KHcbO3dQu4NOlgmCc9wnybfBPwByduKJKsbIBCoEZBq1AiGd5gHUYmiE+4dYkyaRnppKPO05Zubtm4eebsR3ejYw0qT8h/56RYcWPWcmMCxBUSbwEsXUJuxWWGcChz6fgW1MyT7u1/SAfKmfHY8Q1cjyjfqIdL8u3rNK437f6AoeZVXargzeEvvmndNcyyKcu/BPV9fT/nkKlyNObEE9I1h7HlJA9fjT2saaNtrxqIwN+0arEkZ1Ukegzj3RvA09cK+mdpCthMKQyNLG4aeS/tFkOk90sHRJxrJKTcy6Q0dPwkzoUhVHji43cSGWxmcqSptoNn2tIFmHzVI2+IJQxXpxmw/8WJST639xhqkdTafZEi+Rp67Ar5PTcsSeyI2XtK43tkj143b1dXu0WOCBkDYF+Gf1z+6Z+sXPRQAAA==");
        });
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
                PluginLog.Debug($"Recorded venture start time = {offlineRetainerData.VentureBeginsAt}");
            }
        }
    }

    private void CommandHandler(string command, string arguments)
    {
        if (arguments.EqualsIgnoreCase("debug"))
        {
            config.Verbose = !config.Verbose;
            DuoLog.Information($"Debug mode {(config.Verbose ? "enabled" : "disabled")}");
            return;
        }
        else if (arguments.EqualsIgnoreCase("ss"))
        {
            config.SS = !config.SS;
            DuoLog.Information($"Super Secret mode {(config.SS ? "enabled" : "disabled")}");
            if (config.SS) DuoLog.Warning($"Super Secret settings contain features that may be incomplete, incompatible with certain plugins, may cause damage or other unwanted effects to your character, account and game as a whole. Disabling Super Secret mode will not automatically disable previously enabled Super Secret options; you must disable them first before enabling it.");
            return;
        }
        else if(arguments.StartsWith("relog "))
        {
            var target = P.config.OfflineData.Where(x => $"{x.Name}@{x.World}" == arguments[6..]).FirstOrDefault();
            if(target != null)
            {
                if(!AutoLogin.Instance.IsRunning) AutoLogin.Instance.SwapCharacter(target.World, target.CharaIndex, target.ServiceAccount);
            }
            else
            {
                Notify.Error($"Could not find target character");
            }
        }
        else
            configGui.IsOpen = !configGui.IsOpen;
    }

    private void Tick(Framework framework)
    {
        if (P.IsEnabled() && P.retainerManager.Ready && Svc.ClientState.LocalPlayer != null)
        {
            Scheduler.Tick();
            if (!P.config.SelectedRetainers.ContainsKey(Svc.ClientState.LocalContentId))
            {
                P.config.SelectedRetainers[Svc.ClientState.LocalContentId] = new();
            }
        }
        OfflineDataManager.Tick();
        AutoGCHandin.Tick();
        MultiMode.Tick();
        NotificationHandler.Tick();
    }

    private void Toasts_ErrorToast(ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
    {
        var text = message.ExtractText();
        //10350	60	8	0	False	You have no applicable items to entrust.

        if (text == Svc.Data.GetExcelSheet<LogMessage>().GetRow(10350).Text.ToDalamudString().ExtractText())
        {
            TaskEntrustDuplicates.NoDuplicates = true;
        }
        if (IsEnabled())
        {
            if (message.ToString().Equals(Svc.Data.GetExcelSheet<LogMessage>().GetRow(1308).Text.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Clicker.lastAction = ActionType.None;
                PluginLog.Warning("Detected error 1308");
            }
            else if (message.ToString().Equals(Svc.Data.GetExcelSheet<LogMessage>().GetRow(648).Text.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                message = $"{message} AutoRetainer is shutting down.";
                DisablePlugin();
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
        Safe(DisablePlugin);
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
            GcHandin.YesAlready.EnableIfNeeded();
            Multi.YesAlready.EnableIfNeeded();
        });
        Safe(StatisticsManager.Dispose);
        Safe(TaskManager.Dispose);
        Safe(Memory.Dispose);
        PunishLibMain.Dispose();
        ECommonsMain.Dispose();
    }

    internal HashSet<string> GetSelectedRetainers(ulong cid)
    {
        if (!config.SelectedRetainers.ContainsKey(cid))
        {
            config.SelectedRetainers.Add(cid, new());
        }
        return config.SelectedRetainers[cid];
    }

    private void ConditionChange(ConditionFlag flag, bool value)
    {
        if (NoConditionEvent)
        {
            NoConditionEvent = false;
            if(!MultiMode.Enabled) return;
        }
        if(flag == ConditionFlag.OccupiedSummoningBell)
        {
            Clicker.lastAction = ActionType.None;
            if (value)
            {
                if ((config.AutoEnableDisable || MultiMode.Enabled) && Svc.Targets.Target.IsRetainerBell() && Svc.ClientState.LocalPlayer?.HomeWorld.Id == Svc.ClientState.LocalPlayer?.CurrentWorld.Id)
                {
                    if (!ImGui.GetIO().KeyShift || MultiMode.Enabled)
                    {
                        Scheduler.Bans.Clear();
                        if (config.TurboMode)
                        {
                            Scheduler.turbo = true;
                        }
                        EnablePlugin();
                        if(P.config.OpenOnEnable) configGui.IsOpen = true;
                    }
                    else
                    {
                        Notify.Info("Requested to suppress enabling");
                    }
                }
                Clicker.RecordClickTime(1500);
            }
            else
            {
                if (config.AutoEnableDisable || MultiMode.Enabled)
                {
                    DisablePlugin();
                    if(!MultiMode.Enabled) configGui.IsOpen = false;
                }
            }
        }
    }

    void Logout(object _, object __)
    {
        DisablePlugin();

        if (!AutoLogin.Instance.IsRunning)
        {
            MultiMode.LastLongin = 0;
        }

    }

    internal bool IsEnabled()
    {
        return this.Enabled;
    }

    internal void EnablePlugin()
    {
        if (P.Enabled) return;
        AddonLifeTracker.Reset();
        P.Enabled = true;
    }

    internal void DisablePlugin()
    {
        if (!P.Enabled) return;
        P.Enabled = false;
        Scheduler.turbo = false;
    }
}
