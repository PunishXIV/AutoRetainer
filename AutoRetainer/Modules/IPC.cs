using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static class IPC
    {
        internal static bool Suppressed = false;

        internal static void Init()
        {
            P.DebugLog("IPC init");
            Svc.PluginInterface.GetIpcProvider<object>("AutoRetainer.Init").RegisterAction(() => { });
            Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").RegisterFunc(GetSuppressed);
            Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").RegisterAction(SetSuppressed);
            Svc.PluginInterface.GetIpcProvider<uint, object>("AutoRetainer.SetVenture").RegisterAction(SetVenture);
            Svc.PluginInterface.GetIpcProvider<ulong, OfflineCharacterData>("AutoRetainer.GetOfflineCharacterData").RegisterFunc(GetOCD);
            Svc.PluginInterface.GetIpcProvider<OfflineCharacterData, object>("AutoRetainer.WriteOfflineCharacterData").RegisterAction(SetOCD);
            Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData>("AutoRetainer.GetAdditionalRetainerData").RegisterFunc(GetARD);
            Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData, object>("AutoRetainer.WriteAdditionalRetainerData").RegisterAction(SetARD);
            Svc.PluginInterface.GetIpcProvider<List<ulong>>("AutoRetainer.GetRegisteredCIDs").RegisterFunc(GetRegisteredCIDs);
        }

        internal static void Shutdown()
        {
            P.DebugLog("IPC Shutdown");
            Svc.PluginInterface.GetIpcProvider<object>("AutoRetainer.Init").UnregisterAction();
            Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").UnregisterAction();
            Svc.PluginInterface.GetIpcProvider<uint, object>("AutoRetainer.SetVenture").UnregisterAction();
            Svc.PluginInterface.GetIpcProvider<ulong, OfflineCharacterData>("AutoRetainer.GetOfflineCharacterData").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<OfflineCharacterData, object>("AutoRetainer.WriteOfflineCharacterData").UnregisterAction();
            Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData>("AutoRetainer.GetAdditionalRetainerData").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<ulong, string, AdditionalRetainerData, object>("AutoRetainer.WriteAdditionalRetainerData").UnregisterAction();
            Svc.PluginInterface.GetIpcProvider<List<ulong>>("AutoRetainer.GetRegisteredCIDs").UnregisterFunc();
        }

        static List<ulong> GetRegisteredCIDs()
        {
            return C.OfflineData.Where(x => !C.Blacklist.Any(z => z.CID == x.CID) && !x.Name.EqualsAny("Unknown", "")).Select(x => x.CID).ToList();
        }

        static OfflineCharacterData GetOCD(ulong CID)
        {
            return C.OfflineData.FirstOrDefault(x => x.CID == CID);
        }

        static void SetOCD(OfflineCharacterData OCD)
        {
            C.OfflineData.RemoveAll(x => x.CID == OCD.CID);
            C.OfflineData.Add(OCD);
        }

        static AdditionalRetainerData GetARD(ulong cid, string name)
        {
            return Utils.GetAdditionalData(cid, name);
        }

        static void SetARD(ulong cid, string name, AdditionalRetainerData data)
        {
            C.AdditionalData[Utils.GetAdditionalDataKey(cid, name)] = data;
        }

        static void SetVenture(uint VentureID)
        {
            SchedulerMain.VentureOverride = VentureID;
            P.DebugLog($"Received venture override to {VentureID} / {VentureUtils.GetVentureName(VentureID)} via IPC");
        }

        static bool GetSuppressed()
        {
            return Suppressed;
        }

        static void SetSuppressed(bool s)
        {
            Suppressed = s;
        }
    }
}
