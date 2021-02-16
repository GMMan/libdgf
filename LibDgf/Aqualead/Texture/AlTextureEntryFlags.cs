using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Aqualead.Texture
{
    [Flags]
    public enum AlTextureEntryFlags : byte
    {
        IsHasName = 1 << 0,
        IsHasCenterPoint = 1 << 1,
        IsFiltered = 1 << 2
    }
}
