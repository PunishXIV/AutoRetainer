using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class AccountWhitelist : NeoUIEntry
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"You may setup account whitelist. In the event you will log in using non-whitelisted account, AutoRetainer will not record any characters, retainers, or submarines.");
        if(C.WhitelistedAccounts.Count == 0)
        {
            ImGuiEx.TextWrapped(EColor.GreenBright, "Current whitelist status: Disabled. To enable, add some account to it.");
        }
        else
        {
            ImGuiEx.TextWrapped(EColor.YellowBright, "Current whitelist status: Enabled. To disable, remove all accounts from it.");
        }

        foreach(var x in C.WhitelistedAccounts)
        {
            ImGui.PushID(x.ToString());
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                new TickScheduler(() => C.WhitelistedAccounts.Remove(x));
            }
            ImGui.SameLine();
            ImGuiEx.TextV($"Account {x}");
            ImGui.PopID();
        }
    }
}