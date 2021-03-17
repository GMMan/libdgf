using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public enum VifCodeCmd : byte
    {
        Nop = 0b000_0000,
        StCycl = 0b000_0001,
        Offset = 0b000_0010,
        Base = 0b000_0011,
        Itop = 0b000_0100,
        StMod = 0b000_0101,
        MskPath3 = 0b000_0110,
        Mark = 0b000_0111,
        FlushE = 0b001_0000,
        Flush = 0b001_0001,
        FlushA = 0b001_0011,
        MsCal = 0b001_0100,
        MsCnt = 0b001_0111,
        MsCalF = 0b001_0101,
        StMask = 0b010_0000,
        StRow = 0b011_0000,
        StCol = 0b011_0001,
        Mpg = 0b100_1010,
        Direct = 0b101_0000,
        DirectHl = 0b101_0001,
        Unpack = 0b11_00000,
        Interrupt = 0b1000_0000
    }
}
