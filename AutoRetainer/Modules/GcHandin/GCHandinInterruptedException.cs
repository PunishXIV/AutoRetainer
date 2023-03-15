using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.GcHandin
{
    internal class GCHandinInterruptedException : Exception
    {
        public GCHandinInterruptedException(string message) : base(message)
        {
        }
    }
}
