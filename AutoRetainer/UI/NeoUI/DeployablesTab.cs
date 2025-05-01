using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainerAPI.Configuration;
using ECommons.Events;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using VesselDescriptor = (ulong CID, string VesselName);

namespace AutoRetainer.UI.NeoUI;
public class DeployablesTab : NeoUIEntry
{
    public override string Path => "Deployables";

    private static int MinLevel = 0;
    private static int MaxLevel = 0;
    private static string Conf = "";
    private static bool InvertConf = false;

    public override NuiBuilder Builder { get; init; }

    public DeployablesTab()
    {
        Builder = new NuiBuilder()
        .Section("General")
        .Checkbox($"Resend vessels when accessing the Voyage Control Panel", () => ref C.SubsAutoResend2)
        .Checkbox($"Finalize all vessels before resending them", () => ref C.FinalizeBeforeResend)
        .Checkbox($"Hide Airships from Deployables UI", () => ref C.HideAirships)

        .Section("Alert Settings")
        .Checkbox($"Less than possible vessels enabled", () => ref C.AlertNotAllEnabled)
        .Checkbox($"Enabled vessel isn't deployed", () => ref C.AlertNotDeployed)
        .Widget("Unoptimal submersible configuration alerts:", (z) =>
        {
            foreach(var x in C.UnoptimalVesselConfigurations)
            {
                ImGuiEx.Text($"Rank {x.MinRank}-{x.MaxRank}, {(x.ConfigurationsInvert ? "NOT " : "")} {x.Configurations.Print()}");
                if(ImGuiEx.HoveredAndClicked("Ctrl+click to delete", default, true))
                {
                    var t = x.GUID;
                    new TickScheduler(() => C.UnoptimalVesselConfigurations.RemoveAll(x => x.GUID == t));
                }
            }

            ImGuiEx.TextV($"Rank:");
            ImGui.SameLine();
            ImGuiEx.SetNextItemWidthScaled(60f);
            ImGui.DragInt("##rank1", ref MinLevel, 0.1f);
            ImGui.SameLine();
            ImGuiEx.Text($"-");
            ImGui.SameLine();
            ImGuiEx.SetNextItemWidthScaled(60f);
            ImGui.DragInt("##rank2", ref MaxLevel, 0.1f);
            ImGuiEx.TextV($"Configurations:");
            ImGui.SameLine();
            ImGui.Checkbox($"NOT", ref InvertConf);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100f.Scale());
            ImGui.InputText($"##conf", ref Conf, 3000);
            ImGui.SameLine();
            if(ImGui.Button("Add"))
            {
                C.UnoptimalVesselConfigurations.Add(new()
                {
                    Configurations = Conf.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    MinRank = MinLevel,
                    MaxRank = MaxLevel,
                    ConfigurationsInvert = InvertConf
                });
            }
        })
        .Section("Mass configuration change")
        .Widget(MassConfigurationChangeWidget)
        .Section("Registration, component and plan automation")
        .Widget(AutomatedSubPlannerWidget);
    }

    private HashSet<VesselDescriptor> SelectedVessels = [];
    private int MassMinLevel = 0;
    private int MassMaxLevel = 120;
    private VesselBehavior MassBehavior = VesselBehavior.Finalize;
    private UnlockMode MassUnlockMode = UnlockMode.WhileLevelling;
    private SubmarineUnlockPlan SelectedUnlockPlan;
    private SubmarinePointPlan SelectedPointPlan;

    private void MassConfigurationChangeWidget()
    {
        ImGuiEx.Text($"Select submersibles:");
        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo($"##sel", $"Selected {SelectedVessels.Count}", ImGuiComboFlags.HeightLarge))
        {
            ref var search = ref Ref<string>.Get("Search");
            ImGui.InputTextWithHint("##searchSubs", "Character search", ref search, 100);
            foreach(var x in C.OfflineData)
            {
                if(x.ExcludeWorkshop) continue;
                if(search.Length > 0 && !$"{x.Name}@{x.World}".Contains(search, StringComparison.OrdinalIgnoreCase)) continue;
                if(x.OfflineSubmarineData.Count > 0)
                {
                    ImGui.PushID(x.CID.ToString());
                    ImGuiEx.CollectionCheckbox(Censor.Character(x.Name, x.World), x.OfflineSubmarineData.Select(v => (x.CID, v.Name)), SelectedVessels);
                    ImGui.Indent();
                    foreach(var v in x.OfflineSubmarineData)
                    {
                        ImGuiEx.CollectionCheckbox($"{v.Name}", (x.CID, v.Name), SelectedVessels);
                    }
                    ImGui.Unindent();
                    ImGui.PopID();
                }
            }
            ImGui.EndCombo();
        }
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)'\uf057', "Deselect All"))
        {
            SelectedVessels.Clear();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)'\uf055', "Select All"))
        {
            SelectedVessels.Clear();
            foreach(var x in C.OfflineData) foreach(var v in x.OfflineSubmarineData) SelectedVessels.Add((x.CID, v.Name));
        }
        ImGui.Separator();
        ImGuiEx.TextV("By level:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        ImGui.DragInt("##minlevel", ref MassMinLevel, 0.1f);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        ImGui.DragInt("##maxlevel", ref MassMaxLevel, 0.1f);
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "Add vessels by level to selection"))
        {
            foreach(var x in C.OfflineData)
            {
                foreach(var v in x.OfflineSubmarineData)
                {
                    var adata = x.GetAdditionalVesselData(v.Name, VoyageType.Submersible);
                    if(adata.Level.InRange(MassMinLevel, MassMaxLevel, true))
                    {
                        SelectedVessels.Add((x.CID, v.Name));
                    }
                }
            }
        }
        ImGui.Separator();
        ImGuiEx.Text("Actions:");

        ImGui.Separator();
        ImGui.SetNextItemWidth(150f);
        ImGuiEx.EnumCombo("##behavior", ref MassBehavior);
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)'\uf018', "Set behavior"))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var vdata = odata.GetOfflineVesselData(x.VesselName, VoyageType.Submersible);
                    var adata = odata.GetAdditionalVesselData(x.VesselName, VoyageType.Submersible);
                    adata.VesselBehavior = MassBehavior;
                    num++;
                }
            }
            Notify.Success($"Affected {num} submarines");
        }

        ImGui.Separator();
        ImGui.SetNextItemWidth(150f);
        ImGuiEx.EnumCombo("##unlockmode", ref MassUnlockMode, Lang.UnlockModeNames);
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)'\uf09c', "Set unlock mode"))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var vdata = odata.GetOfflineVesselData(x.VesselName, VoyageType.Submersible);
                    var adata = odata.GetAdditionalVesselData(x.VesselName, VoyageType.Submersible);
                    adata.UnlockMode = MassUnlockMode;
                    num++;
                }
            }
            Notify.Success($"Affected {num} submarines");
        }

        ImGui.Separator();

        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo("##uplan", "Unlock plan: " + (SelectedUnlockPlan?.Name ?? "not selected", ImGuiComboFlags.HeightLarge)))
        {
            foreach(var plan in C.SubmarineUnlockPlans)
            {
                if(ImGui.Selectable($"{plan.Name}##{plan.GUID}"))
                {
                    SelectedUnlockPlan = plan;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)'\uf3c1', "Set unlock plan", SelectedUnlockPlan != null))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var vdata = odata.GetOfflineVesselData(x.VesselName, VoyageType.Submersible);
                    var adata = odata.GetAdditionalVesselData(x.VesselName, VoyageType.Submersible);
                    adata.SelectedUnlockPlan = SelectedUnlockPlan.GUID.ToString();
                    num++;
                }
            }
            Notify.Success($"Affected {num} submarines");
        }
        ImGui.Separator();

        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo("##uplan2", "Point plan: " + (VoyageUtils.GetPointPlanName(SelectedPointPlan) ?? "not selected"), ImGuiComboFlags.HeightLarge))
        {
            foreach(var plan in C.SubmarinePointPlans)
            {
                if(ImGui.Selectable($"{VoyageUtils.GetPointPlanName(plan)}##{plan.GUID}"))
                {
                    SelectedPointPlan = plan;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)'\uf55b', "Set point plan", SelectedPointPlan != null))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var vdata = odata.GetOfflineVesselData(x.VesselName, VoyageType.Submersible);
                    var adata = odata.GetAdditionalVesselData(x.VesselName, VoyageType.Submersible);
                    adata.SelectedPointPlan = SelectedPointPlan.GUID.ToString();
                    num++;
                }
            }
            Notify.Success($"Affected {num} submarines");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Check, "Enable selected submersibles"))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    if(odata.EnabledSubs.Add(x.VesselName))
                    {
                        num++;
                    }
                }
            }
            Notify.Success($"Affected {num} submarines");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Times, "Disable selected submersibles"))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    if(odata.EnabledSubs.Remove(x.VesselName))
                    {
                        num++;
                    }
                }
            }
            Notify.Success($"Affected {num} submarines");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.CheckCircle, "Enable deployables multi mode for owners of selected submersibles"))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null && !odata.WorkshopEnabled)
                {
                    odata.WorkshopEnabled = true;
                    num++;
                }
            }
            Notify.Success($"Affected {num} characters");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.TimesCircle, "Disable deployables multi mode for owners of selected submersibles"))
        {
            var num = 0;
            foreach(var x in SelectedVessels)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null && odata.WorkshopEnabled)
                {
                    odata.WorkshopEnabled = false;
                    num++;
                }
            }
            Notify.Success($"Affected {num} characters");
        }
    }

    private void AutomatedSubPlannerWidget()
    {
        ImGui.Checkbox("Enable automatic sub registration", ref C.EnableAutomaticSubRegistration);
        ImGui.Checkbox("Enable automatic components and plan change", ref C.EnableAutomaticComponentsAndPlanChange);
        ImGuiEx.Text("Ranges:");
        for (var index = C.LevelAndPartsData.Count - 1; index >= 0; index--)
        {
            LevelAndPartsData? entry = C.LevelAndPartsData[index];
            if (ImGui.CollapsingHeader($"{entry.GetPlanBuild()}: {entry.MinLevel} - {entry.MaxLevel} ###{entry.GUID}"))
            {
                ImGui.Separator();
                ImGui.Text("Level range:");
                ImGui.SameLine();
                ImGuiEx.SetNextItemWidthScaled(60f);
                ImGui.PushID("##minlvl");
                ImGui.DragInt($"##minlvl{entry.GUID}", ref entry.MinLevel, 0.1f);
                ImGui.PopID();
                ImGui.SameLine();
                ImGuiEx.Text($"-");
                ImGuiEx.SetNextItemWidthScaled(60f);
                ImGui.SameLine();
                ImGui.PushID("##maxlvl");
                ImGui.DragInt($"##maxlvl{entry.GUID}", ref entry.MaxLevel, 0.1f);
                ImGui.PopID();

                ImGui.Text("Hull:");
                ImGui.SameLine(60f);
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo($"##hull{entry.GUID}", ref entry.Part1);

                ImGui.Text("Stern:");
                ImGui.SameLine(60f);
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo($"##stern{entry.GUID}", ref entry.Part2);

                ImGui.Text("Bow:");
                ImGui.SameLine(60f);
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo($"##bow{entry.GUID}", ref entry.Part3);

                ImGui.Text("Bridge:");
                ImGui.SameLine(60f);
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo($"##bridge{entry.GUID}", ref entry.Part4);

                ImGui.Text("Behavior:");
                ImGui.SameLine(60f);
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo($"##behavior{entry.GUID}", ref entry.VesselBehavior);
                ImGui.Text("Plan:");
                ImGui.SameLine(60f);
                if (entry.VesselBehavior == VesselBehavior.Unlock)
                {
                    ImGui.SetNextItemWidth(150f);
                    if (ImGui.BeginCombo($"##unlockplan{entry.GUID}", C.SubmarineUnlockPlans.Any(x => x.GUID == entry.SelectedUnlockPlan)
                                                                              ? C.SubmarineUnlockPlans.First(x => x.GUID == entry.SelectedUnlockPlan)
                                                                                 .Name
                                                                              : "Non selected", ImGuiComboFlags.HeightLarge))
                    {
                        foreach (var plan in C.SubmarineUnlockPlans)
                        {
                            if (ImGui.Selectable($"{plan.Name}##{entry.GUID}"))
                            {
                                entry.SelectedUnlockPlan = plan.GUID;
                            }
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.Text("Mode:");
                    ImGui.SameLine(60f);
                    ImGui.SetNextItemWidth(150f);
                    ImGuiEx.EnumCombo($"##unlockmode{entry.GUID}", ref entry.UnlockMode);
                }
                else if (entry.VesselBehavior == VesselBehavior.Use_plan)
                {
                    ImGui.SetNextItemWidth(150f);
                    if (ImGui.BeginCombo($"##pointplan{entry.GUID}", C.SubmarinePointPlans.Any(x => x.GUID == entry.SelectedPointPlan)
                                                                             ? C.SubmarinePointPlans.First(x => x.GUID == entry.SelectedPointPlan)
                                                                                .Name
                                                                             : "Non selected", ImGuiComboFlags.HeightLarge))
                    {
                        foreach (var plan in C.SubmarinePointPlans)
                        {
                            if (ImGui.Selectable($"{plan.Name}##{entry.GUID}"))
                            {
                                entry.SelectedPointPlan = plan.GUID;
                            }
                        }

                        ImGui.EndCombo();
                    }
                }

                ImGui.Separator();
                ImGui.Checkbox($"Different setup for first Submersible###firstSubDifferent{entry.GUID}", ref entry.FirstSubDifferent);
                if (entry.FirstSubDifferent)
                {
                    ImGui.Text("First Sub Behavior:");
                    ImGui.SameLine(150f);
                    ImGui.SetNextItemWidth(150f);
                    ImGuiEx.EnumCombo($"##firstSubBehavior{entry.GUID}", ref entry.FirstSubVesselBehavior);
                    ImGui.Text("First Sub Plan:");
                    ImGui.SameLine(150f);
                    if (entry.FirstSubVesselBehavior == VesselBehavior.Unlock)
                    {
                        ImGui.SetNextItemWidth(150f);
                        if (ImGui.BeginCombo($"##firstSubUnlockplan{entry.GUID}", C.SubmarineUnlockPlans.Any(x => x.GUID == entry.FirstSubSelectedUnlockPlan)
                                                     ? C.SubmarineUnlockPlans.First(x => x.GUID == entry.FirstSubSelectedUnlockPlan)
                                                        .Name
                                                     : "Non selected", ImGuiComboFlags.HeightLarge))
                        {
                            foreach (var plan in C.SubmarineUnlockPlans)
                            {
                                if (ImGui.Selectable($"{plan.Name}##firstSub{entry.GUID}"))
                                {
                                    entry.FirstSubSelectedUnlockPlan = plan.GUID;
                                }
                            }

                            ImGui.EndCombo();
                        }

                        ImGui.Text("First Sub Mode:");
                        ImGui.SameLine(150f);
                        ImGui.SetNextItemWidth(150f);
                        ImGuiEx.EnumCombo($"##firstSubUnlockmode{entry.GUID}", ref entry.FirstSubUnlockMode);
                    }
                    else if (entry.FirstSubVesselBehavior == VesselBehavior.Use_plan)
                    {
                        ImGui.SetNextItemWidth(150f);
                        if (ImGui.BeginCombo($"##firstSubPointplan{entry.GUID}", C.SubmarinePointPlans.Any(x => x.GUID == entry.FirstSubSelectedPointPlan)
                                                     ? C.SubmarinePointPlans.First(x => x.GUID == entry.FirstSubSelectedPointPlan)
                                                        .Name
                                                     : "Non selected", ImGuiComboFlags.HeightLarge))
                        {
                            foreach (var plan in C.SubmarinePointPlans)
                            {
                                if (ImGui.Selectable($"{plan.Name}##firstSub{entry.GUID}"))
                                {
                                    entry.FirstSubSelectedPointPlan = plan.GUID;
                                }
                            }

                            ImGui.EndCombo();
                        }
                    }
                }

                ImGui.NewLine();
                if (ImGui.Button($"Delete##{entry.GUID}"))
                {
                    C.LevelAndPartsData.RemoveAt(index);
                }
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Add"))
        {
            C.LevelAndPartsData.Insert(0, new());
        }
    }
}
