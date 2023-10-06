using Dalamud.Interface.GameFonts;

namespace AutoRetainer.UI.Overlays
{
    internal unsafe class LoginOverlay : Window
    {
        internal float bWidth = 0f;
        public LoginOverlay() : base("AutoRetainer login overlay", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar, true)
        {
            this.RespectCloseHotkey = false;
            this.IsOpen = true;
        }

        public override bool DrawConditions()
        {
            return  C.LoginOverlay && Utils.CanAutoLogin();
        }

        public override void Draw()
        {
            var num = 1;
            ImGui.SetWindowFontScale(C.LoginOverlayScale);
            ImGui.PushFont(Svc.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.MiedingerMid18)).ImFont);
            foreach(var x in C.OfflineData.Where(x => !x.Name.IsNullOrEmpty() && !x.ExcludeOverlay))
            {
                var n = Censor.Character(x.Name, x.World);
                var dim = ImGuiHelpers.GetButtonSize(n) * C.LoginOverlayScale;
                if(dim.X > bWidth)
                {
                    bWidth = dim.X;
                }
                if (ImGui.Button(n, new(bWidth * C.LoginOverlayBPadding, dim.Y * C.LoginOverlayBPadding)))
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
