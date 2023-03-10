using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer
{
    internal unsafe class Memory : IDisposable
    {
        delegate ulong InteractWithObjectDelegate(TargetSystem* system, GameObject* obj, bool los);
        Hook<InteractWithObjectDelegate> InteractWithObjectHook;

        internal Memory()
        {
            SignatureHelper.Initialise(this, true);
        }

        internal ulong InteractWithObjectDetour(TargetSystem* system, GameObject* obj, bool los)
        {
            DuoLog.Information($"Interacted with {MemoryHelper.ReadSeStringNullTerminated((nint)obj->Name)}, los={los}");
            return InteractWithObjectHook.Original(system, obj, los);
        }

        internal void InstallInteractHook()
        {
            if(InteractWithObjectHook == null ) 
            {
                InteractWithObjectHook = Hook<InteractWithObjectDelegate>.FromAddress((nint)TargetSystem.Addresses.InteractWithObject.Value, InteractWithObjectDetour);
                InteractWithObjectHook.Enable();
            }
        }

        public void Dispose()
        {
            InteractWithObjectHook?.Disable();
            InteractWithObjectHook?.Dispose();
        }
    }
}
