using Dalamud.Interface.GameFonts;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Overlays
{
    internal unsafe class LoginOverlay : Window
    {
        float bWidth = 0f;
        public LoginOverlay() : base("AutoRetainer login overlay", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar, true)
        {
            this.RespectCloseHotkey = false;
            this.IsOpen = true;
        }

        public override bool DrawConditions()
        {
            return !Svc.ClientState.IsLoggedIn && P.config.LoginOverlay && !P.TaskManager.IsBusy && !AutoLogin.Instance.IsRunning && TryGetAddonByName<AtkUnitBase>("Title", out var title) && title->IsVisible && !TryGetAddonByName<AtkUnitBase>("TitleDCWorldMap", out _) && !TryGetAddonByName<AtkUnitBase>("TitleConnect", out _);
        }

        public override void Draw()
        {
            var num = 1;
            ImGui.SetWindowFontScale(P.config.LoginOverlayScale);
            ImGui.PushFont(Svc.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.MiedingerMid18)).ImFont);
            foreach(var x in P.config.OfflineData.Where(x => !x.Name.IsNullOrEmpty() && x.Index != 0))
            {
                var n = Censor.Character(x.Name, x.World);
                var dim = ImGuiHelpers.GetButtonSize(n);
                if(dim.X > bWidth)
                {
                    bWidth = dim.X;
                }
                if (ImGui.Button(n, new(bWidth * 1.35f, dim.Y * 1.35f)))
                {
                    AutoLogin.Instance.Login(x.World, x.CharaIndex, x.ServiceAccount);
                }
            }
            ImGui.PopFont();
            ImGuiEx.ImGuiLineCentered("LoginCenter", delegate
            {
                ImGui.Checkbox("Multi Mode", ref MultiMode.Enabled);
            });
        }
    }
}
