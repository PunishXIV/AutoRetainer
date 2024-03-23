using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Internal;
public class Singleton
{
    public static string LockFileName => Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "session.lock");
    static FileStream Stream;
    public static bool AcquireLock()
    {
        try
        {
            Stream = new FileStream(LockFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch(Exception e)
        {
            PluginLog.Warning($"Could not acquire lock.");
            e.LogWarning();
            return false;
        }
    }

    public static int GetFFXIVCNT()
    {
        int ret = 0;
        var reference = Process.GetCurrentProcess().ProcessName;
        try
        {
            foreach(var p in Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName.EqualsIgnoreCase(reference)) ret++;
                }
                catch(Exception ex)
                {
                    ex.LogInfo();
                }
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        return ret;
    }

    public static void ReleaseLock()
    {
        try
        {
            Stream.Dispose();
        }
        catch(Exception e)
        {
            e.LogInfo();
        }
    }
}
