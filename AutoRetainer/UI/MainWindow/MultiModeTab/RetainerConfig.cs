using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using NotificationMasterAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public static unsafe class RetainerConfig
{
    public static void Draw(OfflineRetainerData ret, OfflineCharacterData data, AdditionalRetainerData adata)
    {
        ImGui.CollapsingHeader($"{Censor.Retainer(ret.Name)} - {Censor.Character(data.Name)} Configuration  ##conf", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
        ImGuiEx.Text($"Additional Post-venture Tasks:");
        //ImGui.Checkbox($"Entrust Duplicates", ref adata.EntrustDuplicates);
        var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == adata.EntrustPlan);
        ImGuiEx.TextV($"Entrust Items:");
        if(!C.EnableEntrustManager) ImGuiEx.HelpMarker("Globally disabled in settings", EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString());
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo($"##select", selectedPlan?.Name ?? "Disabled", ImGuiComboFlags.HeightLarge))
        {
            if(ImGui.Selectable("Disabled")) adata.EntrustPlan = Guid.Empty;
            for(var i = 0; i < C.EntrustPlans.Count; i++)
            {
                var plan = C.EntrustPlans[i];
                ImGui.PushID(plan.Guid.ToString());
                if(ImGui.Selectable(plan.Name, plan == selectedPlan))
                {
                    adata.EntrustPlan = plan.Guid;
                }
                ImGui.PopID();
            }
            ImGui.EndCombo();
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "Copy entrust plan to..."))
        {
            ImGui.OpenPopup($"CopyEntrustPlanTo");
        }
        if(ImGui.BeginPopup("CopyEntrustPlanTo"))
        {
            if(ImGui.Selectable("To all other retainers of this character"))
            {
                var cnt = 0;
                foreach(var x in data.RetainerData)
                {
                    cnt++;
                    Utils.GetAdditionalData(data.CID, x.Name).EntrustPlan = adata.EntrustPlan;
                }
                Notify.Info($"Changed {cnt} retainers");
            }
            if(ImGui.Selectable("To all other retainers without entrust plan of this character"))
            {
                foreach(var x in data.RetainerData)
                {
                    var cnt = 0;
                    if(!C.EntrustPlans.Any(s => s.Guid == adata.EntrustPlan))
                    {
                        Utils.GetAdditionalData(data.CID, x.Name).EntrustPlan = adata.EntrustPlan;
                        cnt++;
                    }
                    Notify.Info($"Changed {cnt} retainers");
                }
            }
            if(ImGui.Selectable("To all other retainers of ALL characters"))
            {
                var cnt = 0;
                foreach(var offlineData in C.OfflineData)
                {
                    foreach(var x in offlineData.RetainerData)
                    {
                        Utils.GetAdditionalData(offlineData.CID, x.Name).EntrustPlan = adata.EntrustPlan;
                        cnt++;
                    }
                }
                Notify.Info($"Changed {cnt} retainers");
            }
            if(ImGui.Selectable("To all other retainers without entrust plan of ALL characters"))
            {
                var cnt = 0;
                foreach(var offlineData in C.OfflineData)
                {
                    foreach(var x in offlineData.RetainerData)
                    {
                        var a = Utils.GetAdditionalData(data.CID, x.Name);
                        if(!C.EntrustPlans.Any(s => s.Guid == a.EntrustPlan))
                        {
                            a.EntrustPlan = adata.EntrustPlan;
                            cnt++;
                        }
                    }
                }
                Notify.Info($"Changed {cnt} retainers");
            }
            ImGui.EndPopup();
        }
        ImGui.Checkbox($"Withdraw/Deposit Gil", ref adata.WithdrawGil);
        if(adata.WithdrawGil)
        {
            if(ImGui.RadioButton("Withdraw", !adata.Deposit)) adata.Deposit = false;
            if(ImGui.RadioButton("Deposit", adata.Deposit)) adata.Deposit = true;
            ImGuiEx.SetNextItemWidthScaled(200f);
            ImGui.InputInt($"Amount, %", ref adata.WithdrawGilPercent.ValidateRange(1, 100), 1, 10);
        }
        ImGui.Separator();
        Svc.PluginInterface.GetIpcProvider<ulong, string, object>(ApiConsts.OnRetainerSettingsDraw).SendMessage(data.CID, ret.Name);
        if(C.Verbose)
        {
            if(ImGui.Button("Fake ready"))
            {
                ret.VentureEndsAt = 1;
            }
            if(ImGui.Button("Fake unready"))
            {
                ret.VentureEndsAt = P.Time + 60 * 60;
            }
        }
    }
}
