using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.Statistics;
public sealed class FcDataManager
{
    private FcDataManager() { }

    public void Draw()
    {
        ImGui.Checkbox($"Update every 30 hours", ref C.UpdateStaleFCData);
        ImGui.SameLine();
        if (ImGuiEx.Button("Update", Player.Interactable))
        {
            S.FCPointsUpdater.ScheduleUpdateIfNeeded(true);
        }
        ImGui.SameLine();
        ImGui.Checkbox($"Show only wallet FC", ref C.DisplayOnlyWalletFC);
        if (ImGui.BeginTable("FCData", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn($"Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"Characters");
            ImGui.TableSetupColumn($"Gil");
            ImGui.TableSetupColumn($"FC points");
            ImGui.TableSetupColumn($"##control");
            ImGui.TableHeadersRow();

            var totalGil = 0L;
            var totalPoint = 0L;

            var i = 0;
            foreach (var x in C.FCData)
            {
                if (x.Key == 0) continue;
                if (!x.Value.GilCountsTowardsChara && C.DisplayOnlyWalletFC) continue;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(C.NoNames ? $"Free company {++i}" : x.Value.Name);

                ImGui.TableNextColumn();
                foreach (var c in C.OfflineData.Where(z => z.FCID == x.Key))
                {
                    ImGuiEx.Text(x.Value.HolderChara == c.CID && x.Value.GilCountsTowardsChara?EColor.GreenBright:null, Censor.Character(c.Name, c.World));
										if (ImGuiEx.HoveredAndClicked("Left click - Relog to this character"))
										{
												Svc.Commands.ProcessCommand($"/ays relog {c.Name}@{c.World}");
										}
                    if (x.Value.GilCountsTowardsChara)
                    {
                        if (ImGuiEx.HoveredAndClicked("Right click - set as gil holder", ImGuiMouseButton.Right))
                        {
                            x.Value.HolderChara = c.CID;
                        }
                    }
								}

                ImGui.TableNextColumn();
                if (x.Value.LastGilUpdate != -1 && x.Value.LastGilUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.Gil:N0}");
                    totalGil += x.Value.Gil;
										ImGuiEx.Tooltip($"Last updated {UpdatedWhen(x.Value.LastGilUpdate)}");
                }
                else
                {
                    ImGuiEx.Text($"Unknown");
                }

                ImGui.TableNextColumn();
                if (x.Value.FCPointsLastUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.FCPoints:N0}");
                    totalPoint += x.Value.FCPoints;
										ImGuiEx.Tooltip($"Last updated {UpdatedWhen(x.Value.FCPointsLastUpdate)}");
                }
                else
                {
                    ImGuiEx.Text($"Unknown");
                }

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox($"\uf555##FC{x.Key}", ref x.Value.GilCountsTowardsChara, EColor.Green);
                ImGui.PopFont();
                ImGuiEx.Tooltip("Mark this free company as Wallet FC. Gil Display tab will include money of this FC.");
            }

            ImGui.TableNextRow();
						ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, EColor.GreenDark.ToUint());
						ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, EColor.GreenDark.ToUint());
            ImGui.TableNextColumn();
            ImGuiEx.Text($"TOTAL");
						ImGui.TableNextColumn();
						ImGui.TableNextColumn();
						ImGuiEx.Text($"{totalGil:N0}");
						ImGui.TableNextColumn();
						ImGuiEx.Text($"{totalPoint:N0}");

						ImGui.EndTable();
        }


        string UpdatedWhen(long time)
        {
            var diff = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
            if (diff < 1000L * 60) return "just now";
            if (diff < 1000L * 60 * 60) return $"{(int)(diff / 1000 / 60)} minute(s) ago";
            if (diff < 1000L * 60 * 60 * 60) return $"{(int)(diff / 1000 / 60 / 60)} hour(s) ago";
            return $"{(int)(diff / 1000 / 60 / 60 / 24)} day(s) ago";
        }
    }

    public OfflineCharacterData GetHolderChara(ulong fcid, FCData data)
    {
        if(C.OfflineData.TryGetFirst(x => x.FCID == fcid && x.CID == data.HolderChara, out var chara))
        {
            return chara;
        }
        else if(C.OfflineData.TryGetFirst(x => x.FCID == fcid, out var fchara))
        {
            data.HolderChara = fchara.CID;
            return fchara;
        }
        return null;
    }
}
