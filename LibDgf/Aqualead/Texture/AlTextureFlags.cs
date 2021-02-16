using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Aqualead.Texture
{
    [Flags]
    public enum AlTextureFlags : byte
    {
        IsSpecial = 1 << 1,
        IsIndirect = 1 << 2,
        IsOverrideDimensions = 1 << 3
    }
}
