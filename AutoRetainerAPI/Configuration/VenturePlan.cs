using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainerAPI.Configuration
{
    [Serializable]
    public class VenturePlan
    {
        [NonSerialized] public string GUID = Guid.NewGuid().ToString();
        public string Name = "";
        public List<PlannedVenture> List = new();
        public PlanCompleteBehavior PlanCompleteBehavior = PlanCompleteBehavior.Restart_plan;

        public List<uint> ListUnwrapped
        {
            get
            {
                var ret = new List<uint>();
                foreach (var v in List)
                {
                    for (int i = 0; i < v.Num; i++)
                    {
                        ret.Add(v.ID);
                    }
                }
                return ret;
            }
        }

        public bool ShouldSerializeListUnwrapped() => false;
    }
}
