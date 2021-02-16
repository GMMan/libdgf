using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Aqualead.Archive
{
    [Flags]
    public enum AlAarArchiveFlags : byte
    {
        IsValid = 1 << 0,
        IsSorted = 1 << 5,
        IsUseNameHash = 1 << 6
    }
}
