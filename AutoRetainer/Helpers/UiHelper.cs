using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Helpers;

internal static unsafe class UiHelper
{
		public static bool Ready;
		private delegate byte AtkUnitBaseClose(AtkUnitBase* unitBase, byte a2);
		private static AtkUnitBaseClose _atkUnitBaseClose;

		public static void Setup()
		{
				_atkUnitBaseClose = Marshal.GetDelegateForFunctionPointer<AtkUnitBaseClose>(Svc.SigScanner.ScanText("40 53 48 83 EC 50 81 A1"));
				Ready = true;
		}

		public static void Close(AtkUnitBase* atkUnitBase, bool unknownBool = false)
		{
				if (!Ready) return;
				_atkUnitBaseClose(atkUnitBase, (byte)(unknownBool ? 1 : 0));
		}
}
