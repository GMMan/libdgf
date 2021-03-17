using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public struct VuFloat
    {
        public uint Packed;

        public static implicit operator double(VuFloat f)
        {
            ulong sign = (f.Packed >> 31) & 1;
            ulong exponent = (f.Packed >> 23) & 0xff;
            ulong mantissa = f.Packed & 0x7fffff;
            ulong doubleValue;
            if (exponent == 0)
            {
                doubleValue = sign << 63;
            }
            else
            {
                doubleValue = (sign << 63) | ((exponent + 1023 - 127) << 52) | (mantissa << 29);
            }
            return BitConverter.ToDouble(BitConverter.GetBytes(doubleValue), 0);
        }

        public override string ToString()
        {
            return ((double)this).ToString();
        }
    }
}
