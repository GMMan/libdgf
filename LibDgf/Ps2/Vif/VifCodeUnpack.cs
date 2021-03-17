using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public struct VifCodeUnpack
    {
        private uint Value;

        public bool Interrupt
        {
            get
            {
                return (Value & 0x80000000) != 0;
            }
        }

        public bool Mask
        {
            get
            {
                return (Value & 0x10000000) != 0;
            }
        }

        public VifUnpackVnType Vn
        {
            get
            {
                return (VifUnpackVnType)((Value >> 26) & 0x3);
            }
        }

        public VifUnpackVlType Vl
        {
            get
            {
                return (VifUnpackVlType)((Value >> 24) & 0x3);
            }
        }

        public byte Num
        {
            get
            {
                return (byte)(Value >> 16);
            }
        }

        public bool Flag
        {
            get
            {
                return (Value & 0x8000) != 0;
            }
        }

        public bool Unsigned
        {
            get
            {
                return (Value & 0x4000) != 0;
            }
        }

        public ushort Address
        {
            get
            {
                return (ushort)(Value & 0x3ff);
            }
        }

        public static implicit operator VifCode(VifCodeUnpack v)
        {
            return new VifCode { Value = v.Value | ((uint)VifCodeCmd.Unpack << 24) };
        }

        public static explicit operator VifCodeUnpack(VifCode v)
        {
            if ((v.Cmd & VifCodeCmd.Unpack) != VifCodeCmd.Unpack)
                throw new ArgumentException("Not an UNPACK code", nameof(v));
            return new VifCodeUnpack { Value = v.Value };
        }
    }
}
