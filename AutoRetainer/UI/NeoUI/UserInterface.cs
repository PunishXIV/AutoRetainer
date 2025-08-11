using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class UserInterface : NeoUIEntry
{
    public override string Path => "User Interface";

    public override NuiBuilder Builder => new NuiBuilder()

        .Section("User Interface")
        .Checkbox("Anonymise Retainers", () => ref C.NoNames, "Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).")
        .Checkbox("Display Quick Menu in Retainer UI", () => ref C.UIBar)
        .Checkbox("Display Extended Retainer Info", () => ref C.ShowAdditionalInfo, "Displays retainer item level/gathering/perception and the name of their current venture in the main UI.")
        .Widget("Do not close AutoRetainer windows on ESC key press", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
        })
        .Checkbox("Display only most significant icon in status bar", () => ref C.StatusBarMSI)
        .SliderInt(120f, "Status bar icon size", () => ref C.StatusBarIconWidth, 32, 128)
        .Checkbox("Open AutoRetainer window on game start", () => ref C.DisplayOnStart)
        //.Checkbox("Skip item sell/trade confirmation while plugin is active", () => ref C.SkipItemConfirmations)
        .Checkbox("Enable title screen button (requires plugin restart)", () => ref C.UseTitleScreenButton)
        .Checkbox("Hide character search", () => ref C.NoCharaSearch)
        .Checkbox("Don't flash background of characters that are complete", () => ref C.NoGradient)
        .Checkbox("Do not warn about second game instance running from same directory", () => ref C.No2ndInstanceNotify, "This will automatically skip AutoRetainer's loading on second instance of the game and you will have no way of loading it until you disable this option in primary instance")

        .Section("Character sorting in Retainer tab")
        .Checkbox("Enable", () => ref C.EnableRetainerSort)
        .TextWrapped("This is purely visual order and does not affects character processing in any way.")
        .Widget(() => UIUtils.DrawSortableEnumList("rorder", C.RetainersVisualOrders))

        .Section("Character sorting in Deployables tab")
        .Checkbox("Enable", () => ref C.EnableDeployablesSort)
        .TextWrapped("This is purely visual order and does not affects character processing in any way.")
        .Widget(() => UIUtils.DrawSortableEnumList("dorder", C.DeployablesVisualOrders));



}