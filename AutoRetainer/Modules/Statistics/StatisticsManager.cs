using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.Events;

namespace AutoRetainer.Modules.Statistics;

internal static class StatisticsManager
{
    internal static List<StatisticsFileWrapper> Files = new();
    internal static void Init()
    {
        Svc.Chat.ChatMessage += Chat_ChatMessage;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
    }

    internal static void Dispose()
    {
        Svc.Chat.ChatMessage -= Chat_ChatMessage;
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        Files.Clear();
    }

    private static void ClientState_TerritoryChanged(object sender, ushort e)
    {
        Files.Clear();
    }

    private static void Chat_ChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (C.RecordStats && Svc.Condition[ConditionFlag.OccupiedSummoningBell] && ProperOnLogin.PlayerPresent && (ushort)type == 2110 && Svc.Targets.Target.IsRetainerBell() && Utils.TryGetCurrentRetainer(out var retName))
        {
            var textProcessed = false;
            uint amount = 1;
            foreach (var x in message.Payloads)
            {
                {
                    if (!textProcessed && x.Type == PayloadType.RawText)
                    {
                        if (x.ToString().TryMatch(@"([0-9]+)", out var match) && uint.TryParse(match.Groups[1].ToString(), out var amt))
                        {
                            P.DebugLog($"Amount parsed: {amt}");
                            amount = amt;
                        }
                        else
                        {
                            P.DebugLog($"Single item {x}");
                        }
                        textProcessed = true;
                    }
                }
                {
                    if (x is ItemPayload p)
                    {
                        GetWrapper(Svc.ClientState.LocalContentId, retName).Add(new()
                        {
                            ItemId = p.ItemId,
                            IsHQ = p.IsHQ,
                            Timestamp = P.Time,
                            Amount = amount,
                            VentureID = P.LastVentureID
                        }) ;
                        break;
                    }
                }
            }
        }
    }

    internal static StatisticsFileWrapper GetWrapper(ulong CID, string Name)
    {
        if (Files.TryGetFirst(x => x.CID == CID && x.RetainerName == Name, out var data))
        {
            return data;
        }
        else
        {
            data = new StatisticsFileWrapper(CID, Name);
            Files.Add(data);
            return data;
        }
    }
}
