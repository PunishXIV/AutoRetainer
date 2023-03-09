using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer
{
    [Serializable]
    public class AdditionalRetainerData
    {
        public bool EntrustDuplicates = false;
        public bool WithdrawGil = false;
        public int WithdrawGilPercent = 100;
    }
}
