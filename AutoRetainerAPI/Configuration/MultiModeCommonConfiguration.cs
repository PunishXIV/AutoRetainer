using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainerAPI.Configuration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class MultiModeCommonConfiguration
    {
        public bool MultiWaitForAll = false;
        public int AdvanceTimer = 60;
        public bool WaitForAllLoggedIn = false;
    }
}
