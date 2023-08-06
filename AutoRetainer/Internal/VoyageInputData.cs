using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Internal
{
    [StructLayout(LayoutKind.Explicit, Size = 200)]
    internal unsafe struct VoyageInputData
    {
        [FieldOffset(0)] internal fixed byte RawDump[200];
        [FieldOffset(16)] internal int unk_16;
        [FieldOffset(24)] internal byte unk_24;
        [FieldOffset(168)] internal nint unk_168;


        internal readonly Span<byte> RawDumpSpan
        {
            get
            {
                fixed (byte* ptr = RawDump)
                {
                    return new Span<byte>(ptr, sizeof(VoyageInputData));
                }
            }
        }
    }

}
