using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Services;
public class LifestreamIPC
{
    private LifestreamIPC()
    {
        EzIPC.Init(this, "Lifestream", SafeWrapper.AnyException);
    }

    [EzIPC] public Func<uint, byte, bool> Teleport;
    [EzIPC] public Func<bool> TeleportToHome;
    [EzIPC] public Func<bool> TeleportToFC;
    [EzIPC] public Func<bool> TeleportToApartment;
    [EzIPC] public Func<bool> IsBusy;
}
