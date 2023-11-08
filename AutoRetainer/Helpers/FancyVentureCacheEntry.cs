using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Helpers
{
    internal record struct FancyVentureCacheEntry(string Entry, bool Avail, string Left, string Right)
    {
        internal ulong CreationFrame = Svc.PluginInterface.UiBuilder.FrameCount;

        internal bool IsValid => Svc.PluginInterface.UiBuilder.FrameCount == CreationFrame;
    }
}
