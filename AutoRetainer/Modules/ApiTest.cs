using AutoRetainerAPI;
using ECommons.Automation;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static class ApiTest
    {
        internal static bool Enabled = false;
        internal static TaskManager TaskManager;

        internal static void Init()
        {
            P.API.OnRetainerPostprocessStep += API_OnRetainerPostprocessTask;
            P.API.OnRetainerReadyToPostprocess += API_OnRetainerReadyToPostprocess;
            TaskManager = new();
        }

        private static void API_OnRetainerReadyToPostprocess(string retainerName)
        {
            if (!Enabled) return;
            P.API.RequestPostprocess();
        }

        private static void API_OnRetainerPostprocessTask(string retainerName)
        {
            TaskManager.Enqueue(() =>
            {
                if (GenericHelpers.IsKeyPressed(System.Windows.Forms.Keys.Back))
                {
                    return true;
                }
                return false;
            }, int.MaxValue);
            TaskManager.Enqueue(P.API.FinishPostProcess);
        }
    }
}
