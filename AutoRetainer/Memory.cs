using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer
{
    internal unsafe class Memory : IDisposable
    {
        delegate void FaceTargetDelegate(ActionManager* actionManager, Vector3* position, ulong a3);
        [Signature("E8 ?? ?? ?? ?? 81 FE ?? ?? ?? ?? 74 2E", Fallibility = Fallibility.Fallible)]
        FaceTargetDelegate FaceTarget;

        delegate ulong InteractWithObjectDelegate(TargetSystem* system, GameObject* obj, bool los);
        Hook<InteractWithObjectDelegate> InteractWithObjectHook;

        internal Memory()
        {
            SignatureHelper.Initialise(this, true);
        }

        internal void Turn(Vector3 pos)
        {
            FaceTarget(ActionManager.Instance(), &pos, 0xE0000000);
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
