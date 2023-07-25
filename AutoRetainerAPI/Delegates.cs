using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainerAPI
{
    public class Delegates
    {
        public delegate void OnSendRetainerToVentureDelegate(string retainerName);
        public delegate void OnRetainerPostprocessTaskDelegate(string retainerName);
        public delegate void OnRetainerReadyToPostprocessDelegate(string retainerName);
        public delegate void OnRetainerSettingsDrawDelegate(ulong CID, string retainerName);
        public delegate void OnRetainerPostVentureTaskDrawDelegate(ulong CID, string retainerName);
        public delegate void OnRetainerListTaskButtonsDrawDelegate();
        public delegate void OnUITabDrawDelegate();
    }
}
