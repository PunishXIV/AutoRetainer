using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Internal;
public static unsafe class GameRetainerManager
{
		public static bool Ready => RetainerManager.Instance()->Ready != 0;
		public static Retainer[] Retainers => RetainerManager.Instance()->Retainers.ToArray().Where(x => x.RetainerId != 0 && x.Name[0] != 0).Select(x => new Retainer(x)).ToArray();
		public static int Count => Retainers.Length;

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


				public Retainer(RetainerManager.Retainer handle)
				{
						this.Handle = handle;
						this.Name = handle.Name.Read();
				}
		}
}
