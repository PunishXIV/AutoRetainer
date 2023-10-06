using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.Interop;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings;

internal static class SettingsMain
{
    static Dictionary<WorkshopFailAction, string> WorkshopFailActionNames = new()
    {
        {WorkshopFailAction.StopPlugin, "Halt all plugin operation" },
        {WorkshopFailAction.ExcludeVessel, "Exclude deployable from operation" },
        {WorkshopFailAction.ExcludeChar, "Exclude captain from multi mode rotation" },
    };

    internal static void Draw()
    {
        ImGuiEx.EzTabBar("GeneralSettings",
            ("General", TabGeneral, null, true),
            ("Multi Mode", TabMulti, null, true),
            //("Contingency", TabCont, null, true),
            //("Login Overlay", TabLoginOverlay, null, true),
            ("Character Order", TabCharaOrder, null, true),
            ("Exclusions", TabExclusions, null, true),
            ("Other", TabOther, null, true)
            );
    }

    static void TabLoginOverlay()
    {
        ImGui.Checkbox($"Display Login Overlay", ref C.LoginOverlay);
        ImGui.SetNextItemWidth(150f);
        if (ImGui.SliderFloat($"Login overlay scale multiplier", ref C.LoginOverlayScale.ValidateRange(0.1f, 5f), 0.2f, 2f)) P.LoginOverlay.bWidth = 0;
        ImGui.SetNextItemWidth(150f);
        if (ImGui.SliderFloat($"Login overlay button padding", ref C.LoginOverlayBPadding.ValidateRange(0.5f, 5f), 1f, 1.5f)) P.LoginOverlay.bWidth = 0;
    }

    static void TabCharaOrder()
    {
        ImGuiEx.TextWrapped($"Here you can sort your characters. This will affect order in which they will be processed by Multi Mode as well as how they will appear in plugin interface and login overlay.");
        for (int index = 0; index < C.OfflineData.Count; index++)
        {
            if (C.OfflineData[index].World.IsNullOrEmpty()) continue;
            ImGui.PushID($"c{index}");
            if (ImGui.ArrowButton("##up", ImGuiDir.Up) && index > 0)
            {
                try
                {
                    (C.OfflineData[index - 1], C.OfflineData[index]) = (C.OfflineData[index], C.OfflineData[index - 1]);
                }
                catch (Exception e)
                {
                    e.Log();
                }
            }
            ImGui.SameLine();
            if (ImGui.ArrowButton("##down", ImGuiDir.Down) && index < C.OfflineData.Count - 1)
            {
                try
                {
                    (C.OfflineData[index + 1], C.OfflineData[index]) = (C.OfflineData[index], C.OfflineData[index + 1]);
                }
                catch (Exception e)
                {
                    e.Log();
                }
            }
            ImGui.SameLine();
            ImGuiEx.TextV(Censor.Character(C.OfflineData[index].Name, C.OfflineData[index].World));
            ImGui.PopID();
        }
    }

    static void TabCont()
    {
        ImGuiEx.TextWrapped("Here you can apply various fallback actions to perform in the case of some common failure states or potential operation errors.");
            ImGuiEx.Text($"Ceruleum Tanks Expended");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of insufficient Ceruleum Tanks to deploy vessel on a new voyage.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Ceruleum Tanks Expended", ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames);

            ImGuiEx.Text($"Unable to Repair Deployable");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of insufficient Magitek Repair Materials to repair a vessel.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Unable to Repair Deployable", ref C.FailureNoRepair, WorkshopFailActionNames);

            ImGuiEx.Text($"Inventory at Capacity");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of the captain's inventory having insufficient space to receive voyage rewards.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Inventory at Capacity", ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames);

            ImGuiEx.Text($"Critical Operation Failure");
            ImGuiComponents.HelpMarker("Applies selected fallback action in the case of any unknown or miscellaneous error.");
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth(-20);
            ImGuiEx.EnumCombo("##Critical Operation Failure", ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames);

                ImGuiEx.Text($"Jailed by the GM");
                ImGuiComponents.HelpMarker("Applies selected fallback action in the case if you got jailed by the GM while plugin is running.");
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.SetNextItemFullWidth(-20);
                ImGui.BeginDisabled();
                if(ImGui.BeginCombo("##jailsel", "Terminate the game")) { ImGui.EndCombo(); }
                ImGui.EndDisabled();
                ImGuiGroup.EndGroupBox();
    }

    static void TabGeneral()
    {
        if(ImGuiGroup.BeginGroupBox("Settings"))
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderInt("Time Desynchronization Compensation", ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0);
            ImGuiComponents.HelpMarker("Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC. ");
            //ImGui.Checkbox($"Enable SuperSonic(tm) Blazing fast operation speed", ref C.UseFrameDelay);
            ImGui.SetNextItemWidth(100f);
            if (!C.UseFrameDelay)
            {
                ImGuiEx.SliderIntAsFloat("Interaction Delay, seconds", ref C.Delay.ValidateRange(10, 1000), 20, 1000);
            }
            else
            {
                ImGui.SliderInt("Interaction Delay, frames", ref C.FrameDelay.ValidateRange(2, 500), 2, 12);
            }
            ImGuiComponents.HelpMarker("The lower this value is the faster plugin will use actions. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it. ");
            ImGuiGroup.EndGroupBox();
        };
        if (ImGuiGroup.BeginGroupBox("Operation"))
        {
            if (ImGui.RadioButton("Assign + Reassign", C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
            ImGuiComponents.HelpMarker("Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.");
            if (ImGui.RadioButton("Collect", !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
            ImGuiComponents.HelpMarker("Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.");
            if (ImGui.RadioButton("Reassign", !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
            ImGuiComponents.HelpMarker("Only reassign ventures that retainers are undertaking.");

            var d = MultiMode.GetAutoAfkOpt() != 0;
            if (d) ImGui.BeginDisabled();
            ImGui.Checkbox("RetainerSense", ref C.RetainerSense);
            ImGuiComponents.HelpMarker($"AutoRetainer will automatically enable itself when the player is within interaction range of a Summoning Bell. You must remain stationary or the activation will be cancelled.");
            if (d)
            {
                ImGui.EndDisabled();
                ImGuiComponents.HelpMarker("Using RetainerSense requires Auto-afk option to be turned off.");
            }
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.SliderIntAsFloat("Activation Time", ref C.RetainerSenseThreshold, 1000, 100000);
            ImGuiGroup.EndGroupBox();
        };
        if(ImGuiGroup.BeginGroupBox("User Interface"))
        {
            ImGui.Checkbox("Anonymise Retainers", ref C.NoNames);
            ImGuiComponents.HelpMarker("Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).");
            ImGui.Checkbox($"Display Quick Menu in Retainer UI", ref C.UIBar);
            ImGui.Checkbox($"Opt out of custom Dalamud theme", ref C.NoTheme);
            ImGui.Checkbox($"Display Extended Retainer Info", ref C.ShowAdditionalInfo);
            ImGuiComponents.HelpMarker("Displays retainer item level/gathering/perception and the name of their current venture in the main UI.");
            if(ImGui.Checkbox("Do not close AutoRetainer windows on ESC key press", ref C.IgnoreEsc))
            {
                Utils.ResetEscIgnoreByWindows();
            }
            ImGui.Separator();
            TabLoginOverlay();
            ImGui.Separator();
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt($"Retainer list: remaining inventory slots warning", ref C.UIWarningRetSlotNum.ValidateRange(2,1000));
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt($"Retainer list: remaining ventures warning", ref C.UIWarningRetVentureNum.ValidateRange(2,1000));
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt($"Deployables list: remaining inventory slots warning", ref C.UIWarningDepSlotNum.ValidateRange(2, 1000));
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt($"Deployables list: remaining fuel warning", ref C.UIWarningDepTanksNum.ValidateRange(20, 1000));
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt($"Deployables list: remaining repair kit warning", ref C.UIWarningDepRepairNum.ValidateRange(5, 1000));
            ImGuiGroup.EndGroupBox();
        }
        if (ImGuiGroup.BeginGroupBox("Keybinds"))
        {
            DrawKeybind("Temporarily prevents AutoRetainer from being automatically enabled when using a Summoning Bell", ref C.Suppress);
            DrawKeybind("Temporarily set the Collect Operation mode, preventing ventures from being assigned for the current cycle", ref C.TempCollectB);
            ImGuiGroup.EndGroupBox();
        };
    }

    static void TabMulti()
    {
        if(ImGuiGroup.BeginGroupBox("Common settings"))
        {
            ImGui.Checkbox($"Housing Bell Support", ref C.MultiAllowHET);
            ImGui.PushStyleColor(ImGuiCol.TextDisabled, ImGuiColors.DalamudOrange);
            ImGuiComponents.HelpMarker("A Summoning Bell must be within range of the spawn point once the home is entered, or a workshop must be purchased.");
            ImGui.PopStyleColor();
            ImGui.Checkbox($"Upon activating Multi Mode, attempt to enter nearby house", ref C.MultiHETOnEnable);
            ImGui.Checkbox($"Enforce Full Character Rotation", ref C.CharEqualize);
            ImGuiComponents.HelpMarker("Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.");
            ImGui.Checkbox($"Wait on login screen", ref C.MultiWaitOnLoginScreen);
            ImGuiComponents.HelpMarker($"If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.");
            ImGui.Checkbox("Synchronise Retainers (one time)", ref MultiMode.Synchronize);
            ImGuiComponents.HelpMarker("AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.");
            ImGuiGroup.EndGroupBox();
        }
        
        if (ImGuiGroup.BeginGroupBox("Retainers"))
        {
            ImGui.Checkbox("Wait For Venture Completion", ref C.MultiModeRetainerConfiguration.MultiWaitForAll);
            ImGuiComponents.HelpMarker("AutoRetainer will wait for all retainers to return before cycling to the next character in multi mode operation.");
            ImGui.SetNextItemWidth(60);
            ImGui.DragInt("Advance Relog Threshold", ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
            ImGui.SetNextItemWidth(100);
            ImGui.SliderInt("Minimum inventory slots to continue operation", ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30);
            ImGuiGroup.EndGroupBox();
        }

        if (ImGuiGroup.BeginGroupBox("Deployables"))
        {
            ImGui.PushID("workshop");
            ImGui.Checkbox("Wait For Voyage Completion", ref C.MultiModeWorkshopConfiguration.MultiWaitForAll);
            ImGuiComponents.HelpMarker("AutoRetainer will wait for all deployables to return before cycling to the next character in multi mode operation.");
            ImGui.SetNextItemWidth(60);
            ImGui.DragInt("Advance Relog Threshold", ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300);
            ImGui.Checkbox("Wait even when already logged in", ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn);
            ImGuiGroup.EndGroupBox();
            ImGui.PopID();
        }

        if (ImGuiGroup.BeginGroupBox("Contingency"))
        {
            TabCont();
            ImGui.EndGroup();
        }
    }

    static void TabOther()
    {

        if (ImGuiGroup.BeginGroupBox("Quick Retainer Action"))
        {
            QRA("Sell Item", ref C.SellKey);
            QRA("Entrust Item", ref C.EntrustKey);
            QRA("Retrieve Item", ref C.RetrieveKey);
            QRA("Put up For Sale", ref C.SellMarketKey);
            ImGuiGroup.EndGroupBox();
        };
        if (ImGuiGroup.BeginGroupBox("Statistics"))
        {
            ImGui.Checkbox($"Record Venture Statistics", ref C.RecordStats);
            ImGuiGroup.EndGroupBox();
        };
        if (ImGuiGroup.BeginGroupBox("Automatic Grand Company Expert Delivery")) {
            AutoGCHandinUI.Draw();
            ImGuiGroup.EndGroupBox();
        }
        
        if (ImGuiGroup.BeginGroupBox("Performance")) {
            if (Utils.IsBusy) ImGui.BeginDisabled();
            ImGui.Checkbox($"Remove minimized FPS restrictions while plugin is operating", ref C.UnlockFPS);
            ImGui.Checkbox($"- Also remove general FPS restriction", ref C.UnlockFPSUnlimited);
            ImGui.Checkbox($"- Also pause ChillFrames plugin", ref C.UnlockFPSChillFrames);
            ImGui.Checkbox($"Raise FFXIV process priority while plugin is operating", ref C.ManipulatePriority);
            ImGuiComponents.HelpMarker("May result other programs slowdown");
            if (Utils.IsBusy) ImGui.EndDisabled();
            ImGuiGroup.EndGroupBox();
        }
    }

    static void QRA(string text, ref LimitedKeys key)
    {
        if(DrawKeybind(text, ref key))
        {
            P.quickSellItems.Toggle();
        }
        ImGui.SameLine();
        ImGuiEx.Text("+ right click");
    }

    static string KeyInputActive = null;
    static bool DrawKeybind(string text, ref LimitedKeys key)
    {
        bool ret = false;
        ImGui.PushID(text);
        ImGuiEx.Text($"{text}:");
        ImGui.Dummy(new(20, 1));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f);
        if (ImGui.BeginCombo("##inputKey", $"{key}"))
        {
            if (text == KeyInputActive)
            {
                ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...");
                foreach (var x in Enum.GetValues<LimitedKeys>())
                {
                    if (IsKeyPressed(x))
                    {
                        KeyInputActive = null;
                        key = x;
                        ret = true;
                        break;
                    }
                }
            }
            else
            {
                if (ImGui.Selectable("Auto-detect new key", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    KeyInputActive = text;
                }
                ImGuiEx.Text($"Select key manually:");
                ImGuiEx.SetNextItemFullWidth();
                ImGuiEx.EnumCombo("##selkeyman", ref key);
            }
            ImGui.EndCombo();
        }
        else
        {
            if(text == KeyInputActive)
            {
                KeyInputActive = null;
            }
        }
        if (key != LimitedKeys.None)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                key = LimitedKeys.None;
                ret = true;
            }
        }
        ImGui.PopID();
        return ret;
    }

    static void TabExclusions()
    {
        C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
        if (ImGuiGroup.BeginGroupBox("Configure exclusions"))
        {
            foreach (var x in C.OfflineData)
            {
                if (ImGui.BeginTable("##excl", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerH))
                {
                    ImGui.TableSetupColumn("1", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("2");
                    ImGui.TableSetupColumn("3");
                    ImGui.TableSetupColumn("4");
                    ImGui.TableSetupColumn("5");
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"{Censor.Character(x.Name, x.World)}:");
                    ImGui.TableNextColumn();
                    if (ImGui.Checkbox("Retainers", ref x.ExcludeRetainer))
                    {
                        x.Enabled = false;
                        C.SelectedRetainers.Remove(x.CID);
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.Checkbox("Deployables", ref x.ExcludeWorkshop))
                    {
                        x.WorkshopEnabled = false;
                        x.EnabledSubs.Clear();
                        x.EnabledAirships.Clear();
                    }
                    ImGui.TableNextColumn();
                    ImGui.Checkbox("Login overlay", ref x.ExcludeOverlay);
                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton("\uf057"))
                    {
                        C.Blacklist.Add((x.CID, x.Name));
                    }
                    ImGuiEx.Tooltip($"This will delete stored character data and prevent it from being ever created again, effectively excluding it from all current and future functions.");
                    ImGui.SameLine();
                    ImGui.Dummy(new(20, 1));
                }
                ImGui.EndTable();
            }
            ImGuiGroup.EndGroupBox();
        }
        if (C.Blacklist.Any())
        {
            if (ImGuiGroup.BeginGroupBox("Excluded Characters"))
            {
                for (int i = 0; i < C.Blacklist.Count; i++)
                {
                    var d = C.Blacklist[i];
                    ImGuiEx.TextV($"{d.Name} ({d.CID:X16})");
                    ImGui.SameLine();
                    if (ImGui.Button($"Delete##bl{i}"))
                    {
                        C.Blacklist.RemoveAt(i);
                        C.SelectedRetainers.Remove(d.CID);
                        break;
                    }
                }
                ImGuiGroup.EndGroupBox();
            }
        }
    }
}
