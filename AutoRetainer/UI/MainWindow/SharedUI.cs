using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.GameHelpers;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow;

    internal static class SharedUI
    {
        internal static void DrawExcludedNotification(bool retainer, bool workshop)
        {
            if (Player.CID == 0) return;
            var col = GradientColor.Get(ImGuiColors.DalamudYellow, ImGuiColors.DalamudRed, 750);
            if (C.Blacklist.Any(x => x.CID == Player.CID))
            {
                ImGuiEx.ImGuiLineCentered("ExclWarning1", () => ImGuiEx.Text(col, "Your current character is excluded from AutoRetainer!"));
                ImGuiEx.ImGuiLineCentered("ExclWarning2", () => ImGuiEx.Text(col, "Go to settings - exclusions to change it."));
            }
            else
            {
                if (retainer && Data.ExcludeRetainer)
                {
                    ImGuiEx.ImGuiLineCentered("ExclWarning1", () => ImGuiEx.Text(col, "Your current character is excluded from retainer list!"));
                    ImGuiEx.ImGuiLineCentered("ExclWarning2", () => ImGuiEx.Text(col, "Go to settings - exclusions to change it."));
                }
                if (workshop && Data.ExcludeWorkshop)
                {
                    ImGuiEx.ImGuiLineCentered("ExclWarning3", () => ImGuiEx.Text(col, "Your current character is excluded from deployable list!"));
                    ImGuiEx.ImGuiLineCentered("ExclWarning2", () => ImGuiEx.Text(col, "Go to settings - exclusions to change it."));
                }
            }
        }

        internal static void DrawEntranceConfig(this OfflineCharacterData data, ref HouseEntrance entrance)
        {
            if (ImGui.Button("Register Entrance"))
            {
                if (data != Data)
                {
                    Notify.Error("You are not logged in on this character");
                }
                else
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
            }
            ImGuiComponents.HelpMarker("If your estate entrance is not the closest to the teleport destination you can override it manually here by standing next to the door and clicking the register button.");
            if (entrance != null)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    entrance = null;
                }
                ImGuiEx.Tooltip($"Currently registered:\n{entrance}");
            }
        }

        internal static void DrawMultiModeHeader(OfflineCharacterData data, string overrideTitle = null)
        {
            var b = true;
            ImGui.CollapsingHeader($"{Censor.Character(data.Name)} {overrideTitle ?? "Configuration"}##conf", ref b, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
            if (b == false)
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.Dummy(new(500, 1));
        }

        internal static void DrawServiceAccSelector(OfflineCharacterData data)
        {
            ImGuiEx.Text($"Service Account Selection");
            ImGuiEx.SetNextItemWidthScaled(150);
            if (ImGui.BeginCombo("##Service Account Selection", $"Service Account {data.ServiceAccount + 1}"))
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

        internal static void DrawExcludeReset(OfflineCharacterData data)
        {
            if (ImGuiGroup.BeginGroupBox("Character Data Expunge/Reset"))
            {
                if (ImGuiEx.ButtonCtrl("Exclude Character"))
                {
                    C.Blacklist.Add((data.CID, data.Name));
                }
                ImGuiComponents.HelpMarker("Excluding this character will immediately reset it's settings, remove it from this list and exclude all retainers from being processed. You can still run manual tasks on it's retainers. You can cancel this action in settings.");
                if (ImGuiEx.ButtonCtrl("Reset character data"))
                {
                    new TickScheduler(() => C.OfflineData.RemoveAll(x => x.CID == data.CID));
                }
                ImGuiComponents.HelpMarker("Character's saved data will be removed without excluding it. Character data will be regenerated once you log back into this character.");
                ImGuiGroup.EndGroupBox();
            }
        }
    }
