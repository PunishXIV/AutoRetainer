using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Internal;
public unsafe static class GameRetainerManager
{
    public static bool Ready => RetainerManager.Instance()->Ready != 0;
    public static Retainer[] Retainers => RetainerManager.Instance()->RetainersSpan.ToArray().Where(x => x.RetainerID != 0 && *x.Name != 0).Select(x => new Retainer(x)).ToArray();
    public static int Count => Retainers.Length;

    public class Retainer
    {
        public RetainerManager.Retainer Handle;
        public string Name;
        public uint VentureID => Handle.VentureID;
        public bool Available => Handle.ClassJob != 0 && Handle.Available;
        public DateTime VentureComplete => Utils.DateFromTimeStamp(Handle.VentureComplete);
        public ulong RetainerID => Handle.RetainerID;
        public uint Gil => Handle.Gil;
        public uint VentureCompleteTimeStamp => Handle.VentureComplete;
        public int MarkerItemCount => Handle.MarkerItemCount;
        public uint MarketExpire => Handle.MarketExpire;
        public int Level => Handle.Level;
        public uint ClassJob => Handle.ClassJob;
        public RetainerManager.RetainerTown Town => Handle.Town;


        public Retainer(RetainerManager.Retainer handle)
        {
            this.Handle = handle;
            this.Name = MemoryHelper.ReadStringNullTerminated((nint)handle.Name);
        }
    }
}
