using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Txm
{
    public enum TxmPixelFormat : byte
    {
        PSMCT32 = 0x00,
        PSMCT24 = 0x01,
        PSMCT16 = 0x02,
        PSMCT16S = 0x0a,
        PSMT8 = 0x13,
        PSMT4 = 0x14,
        PSMT8H = 0x1b,
        PSMT4HL = 0x24,
        PSMT4HH = 0x2c,
        PSMZ32 = 0x30,
        PSMZ24 = 0x31,
        PSMZ16 = 0x32,
        PSMZ16S = 0x3a,
        None = 0xff
    }
}
