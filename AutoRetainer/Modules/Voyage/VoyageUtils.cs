using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage
{
    internal unsafe static class VoyageUtils
    {
        internal static string[] PanelName = new string[] { "Voyage Control Panel" };

        internal static bool IsVoyagePanel(this GameObject obj)
        {
            return obj?.Name.ToString().EqualsAny(PanelName) == true;
        }

        internal static bool IsVoyageCondition()
        {
            return Svc.Condition[ConditionFlag.OccupiedInEvent] || Svc.Condition[ConditionFlag.OccupiedInQuestEvent];
        }

        internal static bool IsInVoyagePanel()
        {
            if (IsVoyageCondition() && Svc.Targets.Target.IsVoyagePanel())
            {
                return true;
            }
            return false;
        }

        internal static bool TryGetNearestVoyagePanel(out GameObject obj)
        {
            //Data ID: 2007820
            if (Svc.Objects.TryGetFirst(x => x.Name.ToString().EqualsAny(PanelName) && x.IsTargetable, out var o))
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

        public static long GetRemainingSeconds(this OfflineVesselData data)
        {
            return data.ReturnTime - P.Time;
        }

        internal static void WriteOfflineData()
        {
            if (HousingManager.Instance()->WorkshopTerritory != null && C.OfflineData.TryGetFirst(x => x.CID == Player.CID, out var ocd))
            {
                {
                    var vessels = HousingManager.Instance()->WorkshopTerritory->Airship;
                    var temp = new List<OfflineVesselData>();
                    foreach (var x in vessels.DataListSpan)
                    {
                        var name = MemoryHelper.ReadSeStringNullTerminated((nint)x.Name).ExtractText();
                        if (name != "")
                        {
                            temp.Add(new() { Name = name, ReturnTime = x.ReturnTime });
                        }
                    }
                    if (temp.Count > 0)
                    {
                        Utils.GetCurrentCharacterData().OfflineAirshipData = temp;
                    }
                }
                {
                    var vessels = HousingManager.Instance()->WorkshopTerritory->Submersible;
                    var temp = new List<OfflineVesselData>();
                    foreach (var x in vessels.DataListSpan)
                    {
                        var name = MemoryHelper.ReadSeStringNullTerminated((nint)x.Name).ExtractText();
                        if (name != "")
                        {
                            temp.Add(new() { Name = name, ReturnTime = x.ReturnTime });
                        }
                    }
                    if (temp.Count > 0)
                    {
                        Utils.GetCurrentCharacterData().OfflineSubmarineData = temp;
                    }
                }
            }
        }

        internal static VoyageType? DetectAddonType(AtkUnitBase* addon)
        {
            var textptr = addon->UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText;
            var text = MemoryHelper.ReadSeString(&textptr).ExtractText();
            if (text.Contains("Select an airship."))
            {
                return VoyageType.Airship;
            }
            if (text.Contains("Select a submersible."))
            {
                return VoyageType.Submersible;
            }
            return null;
        }
    }
}
