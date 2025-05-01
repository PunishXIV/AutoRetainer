using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Readers;

internal unsafe class ReaderAirShipExplorationDetail(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
{
    internal string Fuel => ReadString(1);
    internal string Distance => ReadString(2);

    internal bool CanDeploy
    {
        get
        {
            var f = Fuel.Split("/");
            var d = Distance.Split("/");
            try
            {
                var curF = int.Parse(f[0]);
                var maxF = int.Parse(f[1]);
                var curD = int.Parse(d[0]);
                var maxD = int.Parse(d[1]);
                return curF > 0 && maxF > 0 && curD > 0 && maxD > 0 && curF <= maxF && curD <= maxD;
            }
            catch(Exception e)
            {
                e.LogDebug();
            }
            return false;
        }
    }
}
