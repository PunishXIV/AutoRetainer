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
            ws.AddWindow(new NotifyOverlay());
            MultiMode.Init();

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
                offlineRetainerData.VentureBeginsAt = DateTimeOffset.Now.ToUnixTimeSeconds();
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
        else
            configGui.IsOpen = !configGui.IsOpen;
    }

    private void Tick(Framework framework)
    {
        OfflineDataManager.Tick();
        AutoGCHandin.Tick();
        MultiMode.Tick();
        NotificationHandler.Tick();
    }

    private void Toasts_ErrorToast(ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
    {
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
                        configGui.IsOpen = true;
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
