using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.GcHandin
{
    internal class AutoGCHandinOverlay : Window
    {
        public AutoGCHandinOverlay() : base("AutoRetainer GC Handin overlay", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.AlwaysAutoResize, true)
        {
            this.RespectCloseHotkey = false;
        }

        public override void PreDraw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        public override void Draw()
        {
            ImGui.Checkbox("Auto-handin items", ref AutoGCHandin.Operation);
        }

        public override void PostDraw()
        {
            ImGui.PopStyleVar();
        }
    }
}
