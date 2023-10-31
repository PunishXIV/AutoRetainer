using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Configuration
{
    [Serializable]
    public class UnoptimalVesselConfiguration
    {
        [NonSerialized] internal string GUID = Guid.NewGuid().ToString();
        public int MinRank = 1;
        public int MaxRank = 100;
        public string[] Configurations = [];
        public bool ConfigurationsInvert = false;
    }
}
