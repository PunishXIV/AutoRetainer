using AutoRetainer.Internal;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Helpers
{
    internal unsafe static class VoyageUtils
    {
        internal static bool TryGetNearestVoyagePanel(out GameObject obj)
        {
            //Data ID: 2007820
            if(Svc.Objects.TryGetFirst(x => x.DataId == 2007820 && x.IsTargetable, out var o))
            {
                obj = o;
                return true;
            }
            obj = default;
            return false;
        }

        internal static void PickRoutePoint(int which)
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
            {
                var Event = stackalloc AtkEvent[1]
                {
                    new AtkEvent()
                    {
                        Node = null,
                        Target = (AtkEventTarget*)addon->UldManager.NodeList[132],
                        Listener = &addon->AtkEventListener,
                        Param = 0,
                        NextEvent = null,
                        Type = AtkEventType.ListItemToggle,
                        Unk29 = 0,
                        Flags = 206,
                    }
                };
                var Data = stackalloc VoyageInputData[1]
                {
                    new VoyageInputData()
                    {
                        unk_16 = which,
                        unk_24 = 0,
                        unk_168 = 0
                    }
                };
                //P.Memory.Detour((nint)addon, 0x23, 0, Event, Data);
            }
        }
    }
}
