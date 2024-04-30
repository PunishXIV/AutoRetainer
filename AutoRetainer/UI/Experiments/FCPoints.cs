using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Experiments;
public static class FCPoints
{
    public static void Draw()
    {
        foreach(var x in C.OfflineData)
        {
            if(x.FCPoints > 0)
            {
                ImGuiEx.Text($"{Censor.Character(x.Name, x.World)}: {x.FCPoints:N0}, updated {UpdatedWhen(x.FCPointsLastUpdate)}");
            }
        }

        string UpdatedWhen(long time)
        {
            var diff = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
            if (diff < 1000L * 60) return "just now";
            if (diff < 1000L * 60 * 60) return $"{(int)(diff / 1000 / 60)} minute(s) ago";
            if (diff < 1000L * 60 * 60 * 60) return $"{(int)(diff / 1000 / 60 / 60)} hour(s) ago";
            return $"{(int)(diff / 1000 / 60 / 60 / 24)} day(s) ago";
        }
    }
}
