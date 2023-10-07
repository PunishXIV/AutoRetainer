using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainerAPI.Configuration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public enum GCDeliveryType
    {
        Disabled,Hide_Armoury_Chest_Items,Hide_Gear_Set_Items,Show_All_Items
    }
}
