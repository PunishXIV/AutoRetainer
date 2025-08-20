using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.StaticData;
public sealed class NPCDescriptor : IEquatable<NPCDescriptor>
{
    public uint DataID;
    public float InteractRadius;

    public NPCDescriptor()
    {
    }

    public NPCDescriptor(uint dataID, float interactRadius)
    {
        DataID = dataID;
        InteractRadius = interactRadius;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as NPCDescriptor);
    }

    public bool Equals(NPCDescriptor other)
    {
        return other is not null &&
               DataID == other.DataID &&
               InteractRadius == other.InteractRadius;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DataID, InteractRadius);
    }

    public static bool operator ==(NPCDescriptor left, NPCDescriptor right)
    {
        return EqualityComparer<NPCDescriptor>.Default.Equals(left, right);
    }

    public static bool operator !=(NPCDescriptor left, NPCDescriptor right)
    {
        return !(left == right);
    }

    public bool IsWithinInteractRadius()
    {
        var obj = Svc.Objects.FirstOrDefault(x => x.DataId == this.DataID && x.IsTargetable);
        if(obj != null)
        {
            return Player.DistanceTo(obj) < this.InteractRadius;
        }
        return false;
    }
}