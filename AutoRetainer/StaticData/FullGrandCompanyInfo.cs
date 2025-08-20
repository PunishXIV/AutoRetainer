using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.StaticData;
public sealed class FullGrandCompanyInfo
{
    private static readonly float StandardInteractionRadius = 7f;

    public uint TerritoryType { get; init; }
    public NPCDescriptor SealShop { get; init; }
    public NPCDescriptor DeliveryMissions { get; init; }
    public NPCDescriptor FCAdministrator { get; init; }
    public NPCDescriptor FCActionShop { get; init; }

    public bool IsReadyToExchange()
    {
        if(Svc.ClientState.TerritoryType != TerritoryType) return false;
        return SealShop.IsWithinInteractRadius() && DeliveryMissions.IsWithinInteractRadius();
    }

    public static readonly FullGrandCompanyInfo ImmortalFlames = new()
    {
        TerritoryType = 130,
        SealShop = new(1002390, StandardInteractionRadius),
        DeliveryMissions = new(1002391, StandardInteractionRadius),
        FCAdministrator = new(1002392, StandardInteractionRadius),
        FCActionShop = new(1003925, StandardInteractionRadius),
    };

    public static readonly FullGrandCompanyInfo TwinAdder = new()
    {
        TerritoryType = 132,
        SealShop = new(1002393, StandardInteractionRadius),
        DeliveryMissions = new(1002394, StandardInteractionRadius),
        FCAdministrator = new(1002395, StandardInteractionRadius),
        FCActionShop = new(1000165, StandardInteractionRadius),
    };

    public static readonly FullGrandCompanyInfo Maelstrom = new()
    {
        TerritoryType = 128,
        SealShop = new(1002387, StandardInteractionRadius),
        DeliveryMissions = new(1002388, StandardInteractionRadius),
        FCAdministrator = new(1003247, StandardInteractionRadius),
        FCActionShop = new(1002389, StandardInteractionRadius),
    };

    private FullGrandCompanyInfo()
    {
    }
}