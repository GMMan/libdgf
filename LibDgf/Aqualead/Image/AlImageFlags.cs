using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Aqualead.Image
{
    [Flags]
    public enum AlImageFlags
    {
        IsUseAlpha = 1 << 0,
        PaletteAdd64K = 1 << 4,
        PaletteAdd128K = 1 << 5,
    }
}
