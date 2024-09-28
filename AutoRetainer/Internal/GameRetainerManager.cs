using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Internal;
public static unsafe class GameRetainerManager
{
    public static bool Ready => RetainerManager.Instance()->Ready != 0;
    public static List<Retainer> Retainers
    {
        get
        {
            var ret = new List<Retainer>();
            var m = RetainerManager.Instance();
            for (int i = 0; i < m->Retainers.Length; i++)
            {
                var x = m->Retainers[i];
                if(x.RetainerId != 0 && x.Name[0] != 0)
                {
                    ret.Add(new(x)
                    {
                        DisplayOrder = m->DisplayOrder[i]
                    });
                }
            }
            return [.. ret.OrderBy(x => x.DisplayOrder)];
        }
    }
    public static int Count => Retainers.Count;

    public class Retainer
    {
        public RetainerManager.Retainer Handle;
        public string Name;
        public uint VentureID => Handle.VentureId;
        public bool Available => Handle.ClassJob != 0 && Handle.Available;
        public DateTime VentureComplete => Utils.DateFromTimeStamp(Handle.VentureComplete);
        public ulong RetainerID => Handle.RetainerId;
        public uint Gil => Handle.Gil;
        public uint VentureCompleteTimeStamp => Handle.VentureComplete;
        public int MarkerItemCount => Handle.MarketItemCount;
        public uint MarketExpire => Handle.MarketExpire;
        public int Level => Handle.Level;
        public uint ClassJob => Handle.ClassJob;
        public RetainerManager.RetainerTown Town => Handle.Town;
        public int DisplayOrder = 0;


        public Retainer(RetainerManager.Retainer handle)
        {
            Handle = handle;
            Name = handle.Name.Read();
        }
    }
}
