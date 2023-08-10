using AutoRetainerAPI.Configuration;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static AutoRetainer.Modules.Voyage.Offsets;

namespace AutoRetainer.Modules.Voyage
{
    internal unsafe class VoyageMemory
    {
        const string AirshipTimers = "E8 ?? ?? ?? ?? 33 D2 48 8D 4C 24 ?? 41 B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 54 24 ?? 48 8B CB E8 ?? ?? ?? ?? 48 8B 0D";
        const string AirshipStatus = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8D 99 ?? ?? ?? ?? C6 81";
        const string SubmersibleTimers = "E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? E8 ?? ?? ?? ?? 84 C0 75";
        const string SubmersibleStatus = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 0F 10 02 4C 8D 81";

        delegate void PacketHandler(IntPtr manager, IntPtr data);

        [Signature(SubmersibleTimers, DetourName = nameof(SubmersibleTimersDetour))]
        Hook<PacketHandler> _submersibleTimersHook;

        [Signature(SubmersibleStatus, DetourName = nameof(SubmersibleStatusListDetour))]
        Hook<PacketHandler> _submersibleStatusListHook;

        [Signature(AirshipTimers, DetourName = nameof(AirshipTimersDetour))]
        private Hook<PacketHandler> _airshipTimersHook;

        [Signature(AirshipStatus, DetourName = nameof(AirshipStatusListDetour))]
        private Hook<PacketHandler> _airshipStatusListHook;

        VoyageMemory()
        {
            SignatureHelper.Initialise(this);
            _submersibleStatusListHook?.Enable();
            _submersibleTimersHook?.Enable();
            _airshipTimersHook?.Enable();
            _airshipStatusListHook?.Enable();
        }

        internal static VoyageMemory Instance { get; private set; }

        public static void Init()
        {
            if(Instance != null)
            {
                throw new Exception("Already initialized!");
            }
            Instance = new();
        }

        public static void Dispose() => Instance?.DisposeInternal();

        void DisposeInternal()
        {
            _submersibleStatusListHook?.Disable();
            _submersibleTimersHook?.Disable();
            _airshipTimersHook?.Disable();
            _airshipStatusListHook?.Disable();
            _submersibleStatusListHook?.Dispose();
            _submersibleTimersHook?.Dispose();
            _airshipTimersHook?.Dispose();
            _airshipStatusListHook?.Dispose();
        }

        void SubmersibleTimersDetour(IntPtr manager, IntPtr data)
        {
            try
            {
                var timer = (SubmersibleTimer*)data;
                var temp = new List<OfflineVesselData>();
                for (byte i = 0; i < 4; ++i)
                {
                    if (timer[i].RawName[0] == 0) break;
                    temp.Add(new(timer[i].TimeStamp, timer[i].Name));
                }
                if(temp.Count > 0)
                {
                    Utils.GetCurrentCharacterData().OfflineSubmarineData = temp;
                    Notify.Info($"Updated airship data from SubmersibleTimer");
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
                _submersibleTimersHook.Original(manager, data);
        }

        void SubmersibleStatusListDetour(IntPtr manager, IntPtr data)
        {
            try
            {
                var status = (SubmersibleStatus*)data;
                var temp = new List<OfflineVesselData>();
                for (byte i = 0; i < 4; ++i)
                {
                    if (status[i].RawName[0] == 0) break;
                    temp.Add(new(status[i].TimeStamp, status[i].Name));
                }
                if (temp.Count > 0)
                {
                    Utils.GetCurrentCharacterData().OfflineSubmarineData = temp;
                    Notify.Info($"Updated airship data from SubmersibleStatus");
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
                _submersibleStatusListHook.Original(manager, data);
        }


        private unsafe void AirshipTimersDetour(IntPtr manager, IntPtr data)
        {
            try
            {
                var timer = (AirshipTimer*)data;
                var temp = new List<OfflineVesselData>();
                for (byte i = 0; i < 4; ++i)
                {
                    if (timer[i].RawName[0] == 0) break;
                    temp.Add(new(timer[i].TimeStamp, timer[i].Name));
                }
                if (temp.Count > 0)
                {
                    Utils.GetCurrentCharacterData().OfflineAirshipData = temp;
                    Notify.Info($"Updated airship data from AirshipTimer");
                }
            }
            catch(Exception e)
            {
                e.Log();
            }
            _airshipTimersHook.Original(manager, data);
        }

        private unsafe void AirshipStatusListDetour(IntPtr manager, IntPtr data)
        {
            try
            {
                var status = (AirshipStatus*)data;
                var temp = new List<OfflineVesselData>();
                for (byte i = 0; i < 4; ++i)
                {
                    if (status[i].RawName[0] == 0) break;
                    temp.Add(new(status[i].TimeStamp, status[i].Name));
                }
                if (temp.Count > 0)
                {
                    Utils.GetCurrentCharacterData().OfflineAirshipData = temp;
                    Notify.Info($"Updated airship data from AirshipStatus");
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
            _airshipStatusListHook.Original(manager, data);
        }

    }
}
