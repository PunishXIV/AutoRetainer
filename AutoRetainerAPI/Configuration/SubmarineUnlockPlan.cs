using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AutoRetainerAPI.Configuration
{
    public class SubmarineUnlockPlan
    {
        public string GUID = Guid.NewGuid().ToString();
        public string Name = string.Empty;
        public List<uint> ExcludedRoutes = new();
        public bool Delete = false;
        public bool UnlockSubs = true;

        public bool ShouldSerializeDelete() => false;

        public void CopyFrom(SubmarineUnlockPlan other)
        {
            this.Name = other.Name;
            this.ExcludedRoutes = other.ExcludedRoutes;
            this.UnlockSubs = other.UnlockSubs;
        }
    }
}
