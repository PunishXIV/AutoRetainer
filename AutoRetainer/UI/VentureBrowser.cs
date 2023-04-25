using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI
{
    internal class VentureBrowser : Window
    {
        OfflineRetainerData SelectedRetainer = null;
        OfflineCharacterData SelectedCharacter = null;
        List<VentureBrowserData> Data = new();
        string search = "";
        int minLevel = 1;
        int maxLevel = 90;
        bool GatherBuddyPresent = false;
        public VentureBrowser() : base("Venture Browser")
        {
        }

        public override void OnClose()
        {
            Data.Clear();
        }

        public override void OnOpen()
        {
            GatherBuddyPresent = Svc.PluginInterface.PluginNames.Contains("GatherBuddy");
        }

        public override void Draw()
        {
            ImGuiEx.SetNextItemFullWidth();
            if (ImGui.BeginCombo("##selectRet", SelectedCharacter != null?$"{Censor.Character(SelectedCharacter.Name, SelectedCharacter.World)} - {Censor.Retainer(SelectedRetainer.Name)} - {SelectedRetainer.Level} {ExcelJobHelper.GetJobNameById(SelectedRetainer.Job)}" : "Select a retainer..."))
            {
                foreach (var x in P.config.OfflineData)
                {
                    foreach (var r in x.RetainerData)
                    {
                        if (ImGui.Selectable($"{Censor.Character(x.Name, x.World)} - {Censor.Retainer(r.Name)} - Lv{r.Level} {ExcelJobHelper.GetJobNameById(r.Job)}"))
                        {
                            SelectedRetainer = r;
                            SelectedCharacter = x;
                            Data.Clear();
                        }
                    }
                }
                ImGui.EndCombo();
            }

            if (SelectedRetainer != null && SelectedCharacter != null)
            {
                ImGuiEx.InputWithRightButtonsArea("VBrowser", delegate
                {
                    ImGui.InputTextWithHint("##search", "Filter...", ref search, 100);
                }, delegate
                {
                    ImGuiEx.TextV($"{Lang.CharLevel}:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(50f);
                    ImGui.DragInt("##minL", ref minLevel, 1, 1, 90);
                    ImGui.SameLine();
                    ImGuiEx.Text($"-");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(50f);
                    ImGui.DragInt("##maxL", ref maxLevel, 1, 1, 90);
                });
                var adata = Utils.GetAdditionalData(SelectedCharacter.CID, SelectedRetainer.Name);
                if(adata.Gathering == -1 || adata.Perception == -1 || adata.Ilvl == -1 || SelectedRetainer.Level == 0)
                {
                    ImGuiEx.TextWrapped($"Data is absent for this retainer. Access retainer bell and select that retainer to populate data.");
                }
                else
                {
                    if (Data.Count == 0)
                    {
                        foreach (var v in VentureUtils.GetHunts(SelectedRetainer.Job))
                        {
                            var parts = VentureUtils.GetFancyVentureNameParts(v, SelectedCharacter, SelectedRetainer, out var avail);
                            VentureBrowserData ventureBrowserData = new()
                            {
                                VentureName = parts.Name,
                                VentureLevel = v.RetainerLevel,
                                AvailableByGear = VentureUtils.IsDoL(SelectedRetainer.Job) ? adata.Gathering >= v.RequiredGathering : adata.Ilvl >= v.RequiredItemLevel,
                                Gathered = SelectedCharacter.UnlockedGatheringItems.Contains(VentureUtils.GetGatheringItemByItemID(v.GetVentureItemId())),
                                CurrentIndex = parts.YieldRate,
                                IsDol = VentureUtils.IsDoL(SelectedRetainer.Job),
                                IlvlGathering = VentureUtils.IsDoL(SelectedRetainer.Job) ? v.RequiredGathering : v.RequiredItemLevel,
                            };
                            (ventureBrowserData.Requirements, ventureBrowserData.Amounts) = VentureUtils.GetVentureAmounts(v, SelectedRetainer);
                            Data.Add(ventureBrowserData);
                        }
                    }
                    if(ImGui.BeginTable("##VentureBrowserTable", 9, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableSetupColumn(Lang.CharLevel);
                        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn(Data.FirstOrDefault()?.IsDol == true?Lang.CharPlant:Lang.CharItemLevel);
                        ImGui.TableSetupColumn("☆☆☆☆");
                        ImGui.TableSetupColumn("★☆☆☆");
                        ImGui.TableSetupColumn("★★☆☆");
                        ImGui.TableSetupColumn("★★★☆");
                        ImGui.TableSetupColumn("★★★★");
                        ImGui.TableSetupColumn("Unlocked");
                        ImGui.TableHeadersRow();

                        foreach (var x in Data.Where(x => x.VentureName.Contains(search, StringComparison.OrdinalIgnoreCase) && x.VentureLevel >= minLevel && x.VentureLevel <= maxLevel))
                        {
                            var n = x.IsDol ? Lang.CharP : Lang.CharItemLevel;
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGuiEx.Text(SelectedRetainer.Level >= x.VentureLevel?ImGuiColors.ParsedGreen:ImGuiColors.DalamudRed, $"{x.VentureLevel}");
                            ImGui.TableNextColumn();
                            ImGuiEx.Text($"{x.VentureName}");
                            ImGui.TableNextColumn();
                            ImGuiEx.Text(x.AvailableByGear? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, $"{x.IlvlGathering}");
                            ImGui.TableNextColumn();

                            var col = x.AvailableByGear && x.CurrentIndex == 1 ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudWhite;
                            ImGuiEx.Text(col, $"{n} < {x.Requirements[1]}");
                            var t = $"{x.Amounts[0].FancyDigits()}";
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() / 2 - ImGui.CalcTextSize(t).X / 2);
                            ImGuiEx.Text(col, t);
                            ImGui.TableNextColumn();

                            col = x.AvailableByGear && x.CurrentIndex == 2 ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudWhite;
                            ImGuiEx.Text(col, $"{n} {x.Requirements[1]} - {x.Requirements[2] - 1}");
                            t = $"{x.Amounts[1].FancyDigits()}";
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() / 2 - ImGui.CalcTextSize(t).X / 2);
                            ImGuiEx.Text(col, t);
                            ImGui.TableNextColumn();

                            col = x.AvailableByGear && x.CurrentIndex == 3 ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudWhite;
                            ImGuiEx.Text(col, $"{n} {x.Requirements[2]} - {x.Requirements[3] - 1}");
                            t = $"{x.Amounts[2].FancyDigits()}";
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() / 2 - ImGui.CalcTextSize(t).X / 2);
                            ImGuiEx.Text(col, t);
                            ImGui.TableNextColumn();

                            col = x.AvailableByGear && x.CurrentIndex == 4 ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudWhite;
                            ImGuiEx.Text(col, $"{n} {x.Requirements[3]} - {x.Requirements[4] - 1}");
                            t = $"{x.Amounts[3].FancyDigits()}";
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() / 2 - ImGui.CalcTextSize(t).X / 2);
                            ImGuiEx.Text(col, t);
                            ImGui.TableNextColumn();

                            col = x.AvailableByGear && x.CurrentIndex == 5 ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudWhite;
                            ImGuiEx.Text(col, $"{n} {x.Requirements[4]}+");
                            t = $"{x.Amounts[4].FancyDigits()}";
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() / 2 - ImGui.CalcTextSize(t).X / 2);
                            ImGuiEx.Text(col, t);
                            ImGui.TableNextColumn();

                            if (x.IsDol)
                            {
                                ImGuiEx.Text(x.Gathered?ImGuiColors.ParsedGreen:ImGuiColors.DalamudRed, x.Gathered?"Yes":"No");
                                if(!x.Gathered && GatherBuddyPresent)
                                {
                                    if (ImGui.SmallButton($"Gather##{x.VentureName}"))
                                    {
                                        Svc.Commands.ProcessCommand($"/gather {x.VentureName}");
                                    }
                                }
                            }
                            else
                            {
                                ImGuiEx.Text($"Always");
                            }
                        }
                        

                        ImGui.EndTable();
                    }
                }
            }
        }

        internal class VentureBrowserData
        {
            internal string VentureName;
            internal int[] Requirements = new int[5];
            internal int[] Amounts = new int[5];
            internal int CurrentIndex;
            internal bool Gathered;
            internal bool AvailableByGear;
            internal int VentureLevel;
            internal bool IsDol;
            internal int IlvlGathering;
        }
    }
}
