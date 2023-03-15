using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.GcHandin
{
    internal static class AutoGCHandinUI
    {
        internal static void Draw()
        {
            ImGui.Checkbox("Enable Automatic GC Handin", ref P.config.EnableAutoGCHandin);
            ImGui.Checkbox("Tray notification upon handin completion (requires NotificationMaster)", ref P.config.GCHandinNotify);
        }
    }
}
