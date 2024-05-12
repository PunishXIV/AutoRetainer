using ECommons.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Experiments;
public sealed class FCData
{
		private FCData() { }

    public void Draw()
    {
				ImGui.Checkbox($"Periodically update FC points data", ref C.UpdateStaleFCData);
				ImGui.SameLine();
				if (ImGui.Button("Update for current character"))
				{
            S.FCPointsUpdater.ScheduleUpdateIfNeeded(true);
				}
				ImGui.SameLine();
				ImGui.Checkbox($"Show only FC marked as own money", ref C.DisplayOnlyWalletFC);
				if (ImGui.BeginTable("FCData", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn($"Name", ImGuiTableColumnFlags.WidthStretch);
						ImGui.TableSetupColumn($"Characters");
						ImGui.TableSetupColumn($"Gil");
						ImGui.TableSetupColumn($"FC points");
						ImGui.TableSetupColumn($"##control");
            ImGui.TableHeadersRow();

						var i = 0;
						foreach (var x in C.FCData)
						{
								if (x.Key == 0) continue;
								if (!x.Value.GilCountsTowardsChara && C.DisplayOnlyWalletFC) continue;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(C.NoNames?$"Free company {++i}": x.Value.Name);

								ImGui.TableNextColumn();
								foreach(var c in C.OfflineData.Where(z => z.FCID == x.Key))
								{
										ImGuiEx.Text(Censor.Character(c.Name, c.World));
										if(ImGuiEx.HoveredAndClicked("Relog to this character"))
										{
												Svc.Commands.ProcessCommand($"/ays relog {c.Name}@{c.World}");
										}
								}

								ImGui.TableNextColumn();
								if (x.Value.LastGilUpdate != -1 && x.Value.LastGilUpdate != 0)
								{
										ImGuiEx.Text($"{x.Value.Gil:N0}");
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
						}

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
}
