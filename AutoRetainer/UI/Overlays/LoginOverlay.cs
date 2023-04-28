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
            return  P.config.LoginOverlay && Utils.CanAutoLogin();
        }

        public override void Draw()
        {
            var num = 1;
            ImGui.SetWindowFontScale(P.config.LoginOverlayScale);
            ImGui.PushFont(Svc.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.MiedingerMid18)).ImFont);
            foreach(var x in P.config.OfflineData.Where(x => !x.Name.IsNullOrEmpty()))
            {
                var n = Censor.Character(x.Name, x.World);
                var dim = ImGuiHelpers.GetButtonSize(n);
                if(dim.X > bWidth)
                {
                    bWidth = dim.X;
                }
                if (ImGui.Button(n, new(bWidth * 1.35f, dim.Y * 1.35f)))
                {
                    AutoLogin.Instance.Login(x.World, x.Name, x.ServiceAccount);
                }
            }
            ImGui.PopFont();
            ImGuiEx.ImGuiLineCentered("LoginCenter", delegate
            {
                if(ImGui.Checkbox("Multi Mode", ref MultiMode.Enabled))
                {
                    MultiMode.OnMultiModeEnabled();
                }
            });
        }
    }
}
