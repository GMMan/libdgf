using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Mesh
{
    [Flags]
    public enum TdbFlags : byte
    {
        None = 0,
        SkipBoundingBox = 1,
    }
}
