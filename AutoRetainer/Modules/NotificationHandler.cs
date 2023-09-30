using Dalamud.Game.Text.SeStringHandling;
using ECommons.ChatMethods;

namespace AutoRetainer.Modules;

internal static class NotificationHandler
{
    internal static bool CurrentState = false;
    internal static bool IsNotified = false;
    internal static bool IsHidden = false;
    internal static void Tick()
    {
        var currentState = GetNotifyState();
        if (currentState != CurrentState)
        {
            CurrentState = currentState;
            if (currentState)
            {
                Svc.Chat.Print(new()
                {
                    Message = new SeStringBuilder().AddUiForeground("[AutoRetainer] Some of the retainers have completed their ventures!", (ushort)UIColor.Green).Build()
                });
                IsHidden = false;
                IsNotified = true;
            }
            else
            {
                IsNotified = false;
                IsHidden = false;
            }
        }
    }

    internal static bool GetNotifyState()
    {
        if (C.NotifyIncludeAllChara)
        {
            foreach (var x in C.OfflineData)
            {
                if (!C.NotifyIgnoreNoMultiMode || x.Enabled)
                {
                    foreach (var r in x.RetainerData)
                    {
                        if (r.HasVenture && r.GetVentureSecondsRemaining() <= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        else
        {
            if (Svc.ClientState.LocalContentId != 0 && C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var x))
            {
                foreach (var r in x.RetainerData)
                {
                    if (r.HasVenture && r.GetVentureSecondsRemaining() <= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
