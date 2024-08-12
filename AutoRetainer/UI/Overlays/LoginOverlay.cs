using AutoRetainer.Internal;
using Dalamud.Interface.GameFonts;

namespace AutoRetainer.UI.Overlays;

internal unsafe class LoginOverlay : Window
{
    internal float bWidth = 0f;
    private string Search = "";

    public LoginOverlay() : base("AutoRetainer login overlay", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoFocusOnAppearing, true)
    {
        P.WindowSystem.AddWindow(this);
        RespectCloseHotkey = false;
        IsOpen = true;
    }

    public override bool DrawConditions()
    {
        return C.LoginOverlay && Utils.CanAutoLogin();
    }

    public override void Draw()
    {
        var num = 1;
        ImGuiEx.LineCentered(() =>
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.InputTextWithHint("##search", "Search...", ref Search, 50);
        });
        ImGui.SetWindowFontScale(C.LoginOverlayScale);
        //ImGui.PushFont(Svc.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.MiedingerMid18)).ImFont);
        foreach(var x in C.OfflineData.Where(x => !x.Name.IsNullOrEmpty() && !x.ExcludeOverlay))
        {
            if(Search != "" && !$"{x.Name}@{x.World}".Contains(Search, StringComparison.OrdinalIgnoreCase)) continue;
            var n = Censor.Character(x.Name, x.World);
            var dim = ImGuiHelpers.GetButtonSize(n) * C.LoginOverlayScale;
            if(dim.X > bWidth)
            {
                bWidth = dim.X;
            }
            if(ImGui.Button(n, new(bWidth * C.LoginOverlayBPadding, dim.Y * C.LoginOverlayBPadding)))
            {
                MultiMode.Relog(x, out _, RelogReason.Overlay);
                //AutoLogin.Instance.Login(x.CurrentWorld, x.Name, ExcelWorldHelper.GetWorldByName(x.World).RowId, x.ServiceAccount);
            }
        }
        //ImGui.PopFont();
        ImGuiEx.LineCentered("LoginCenter", delegate
        {
            if(ImGui.Checkbox("Multi Mode", ref MultiMode.Enabled))
            {
                MultiMode.OnMultiModeEnabled();
            }
        });
    }
}
