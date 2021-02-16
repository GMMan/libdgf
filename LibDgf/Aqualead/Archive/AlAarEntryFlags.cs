using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Aqualead.Archive
{
    [Flags]
    public enum AlAarEntryFlags : byte
    {
        IsResident = 1 << 0,
        IsPrepare = 1 << 1,
        Unknown2 = 1 << 6,
        IsUseName = 1 << 7
    }
}
