using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using NotificationMasterAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.UI
{
    internal static class SharedUI
    {
        internal static void DrawEntranceConfig(ref HouseEntrance entrance, string name)
        {
            if (ImGui.Button("Register closest entrance"))
            {
                var door = Utils.GetNearestEntrance(out _, true);
                if (HousingUtils.TryGetCurrentDescriptor(out var d) && door != null)
                {
                    entrance = new()
                    {
                        Descriptor = d,
                        Entrance = door.Position,
                    };
                }
                else
                {
                    Notify.Error($"Please stand inside your plot and close to the entrance");
                }
            }
            if(entrance != null)
            {
                ImGuiEx.TextWrapped($"Currently registered: {entrance}");
                if (ImGui.Button("Unregister"))
                {
                    entrance = null;
                }
            }
        }

        internal static void DrawMultiModeHeader(OfflineCharacterData data)
        {
            var b = true;
            ImGui.CollapsingHeader($"{Censor.Character(data.Name)} Configuration##conf", ref b, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
            if (b == false)
            {
                ImGui.CloseCurrentPopup();
            }
        }

        internal static void DrawServiceAccSelector(OfflineCharacterData data)
        {
            ImGui.SetNextItemWidth(150);
            if (ImGui.BeginCombo("##sindex", $"Service Account {data.ServiceAccount + 1}"))
            {
                for (var i = 1; i <= 10; i++)
                {
                    if (ImGui.Selectable($"Service Account {i}"))
                    {
                        data.ServiceAccount = i - 1;
                    }
                }
                ImGui.EndCombo();
            }
        }

        internal static void DrawPreferredCharacterUI(OfflineCharacterData data)
        {
            if (ImGui.Checkbox("Preferred Character", ref data.Preferred))
            {
                foreach (var z in C.OfflineData)
                {
                    if (z.CID != data.CID)
                    {
                        z.Preferred = false;
                    }
                }
            }
            ImGuiComponents.HelpMarker("When operating in multi mode, if there are no other characters with imminent ventures to collect, it will relog back to your preferred character.");
        }

        internal static void DrawExcludeReset(OfflineCharacterData data, out ulong deleteData)
        {
            deleteData = 0;
            if (ImGuiEx.ButtonCtrl("Exclude Character"))
            {
                C.Blacklist.Add((data.CID, data.Name));
            }
            ImGuiComponents.HelpMarker("Excluding this character will immediately reset it's settings, remove it from this list and exclude all retainers from being processed. You can still run manual tasks on it's retainers. You can cancel this action in settings.");
            if (ImGuiEx.ButtonCtrl("Reset character data"))
            {
                deleteData = data.CID;
            }
            ImGuiComponents.HelpMarker("Character's saved data will be removed without excluding it. Character data will be regenerated once you log back into this character.");
        }
    }
}
