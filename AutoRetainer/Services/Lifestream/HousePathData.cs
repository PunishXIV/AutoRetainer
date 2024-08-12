using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Services.Lifestream;
[Serializable]
public class HousePathData
{
    public int ResidentialDistrict;
    public int Ward;
    public int Plot;
    public List<Vector3> PathToEntrance = [];
    public List<Vector3> PathToWorkshop = [];
    public bool IsPrivate;
    public ulong CID;
    public bool EnableHouseEnterModeOverride = false;
    public int EnterModeOverride = 0;
}
