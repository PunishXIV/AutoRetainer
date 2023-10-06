using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainerAPI.Configuration
{
    public class OfflineVesselData
    {
        public string Name;
        public uint ReturnTime;

        public OfflineVesselData() { }

        public OfflineVesselData(string name, uint returnTime)
        {
            Name = name;
            ReturnTime = returnTime;
        }

        public OfflineVesselData(uint returnTime, string name)
        {
            Name = name;
            ReturnTime = returnTime;
        }
    }
}
