using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.GcHandin;
[StructLayout(LayoutKind.Explicit, Size = 152)]
public unsafe struct GCExpectEntry
{
    [FieldOffset(112)] public int Unk112;
    [FieldOffset(116)] public uint Unk116;
    [FieldOffset(120)] public uint Seals;
    [FieldOffset(132)] public uint ItemID;
    [FieldOffset(136)] public uint Unk136;
    [MarshalAs(UnmanagedType.I1)][FieldOffset(145)] public bool Unk145;
}
