using AutoRetainer.Modules.EzIPCManagers;
using AutoRetainer.UI.Experiments;
using AutoRetainer.UI.NeoUI;
using ECommons.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer;
public static class AutoRetainerServiceManager
{
		public static NeoWindow NeoWindow { get; private set; }
		public static EzIPCManager EzIPCManager { get; private set; }
		public static FCPointsUpdater FCPointsUpdater { get; private set; }
		public static FCData FCData { get; private set; }
		public static GilDisplay GilDisplay { get; private set; }

}
