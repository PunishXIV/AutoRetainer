using AutoRetainer.Helpers;
using AutoRetainer.Scheduler.Handlers;
using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks
{
    internal unsafe static class TaskInteractWithNearestBell
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(PlayerWorldHandlers.SelectNearestBell);
            P.TaskManager.Enqueue(() => { P.IsInteractionAutomatic = true; return true; });
            P.TaskManager.Enqueue(PlayerWorldHandlers.InteractWithTargetedBell);
        }
    }
}
