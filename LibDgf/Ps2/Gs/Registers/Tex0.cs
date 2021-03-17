using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Gs.Registers
{
    public struct Tex0
    {
        public ulong Packed;

        public ushort Tbp0
        {
            get
            {
                return (ushort)(Packed & 0x3fff);
            }
        }

        public byte Tbw
        {
            get
            {
                return (byte)((Packed >> 14) & 0x3f);
            }
        }

        public byte Psm
        {
            get
            {
                return (byte)((Packed >> 20) & 0x3f);
            }
        }

        public byte Tw
        {
            get
            {
                return (byte)((Packed >> 26) & 0xf);
            }
        }

        public byte Th
        {
            get
            {
                return (byte)((Packed >> 30) & 0xf);
            }
        }

        public byte Tcc
        {
            get
            {
                return (byte)((Packed >> 34) & 0x1);
            }
        }

        public byte Tfx
        {
            get
            {
                return (byte)((Packed >> 35) & 0x3);
            }
        }

        public ushort Cbp
        {
            get
            {
                return (byte)((Packed >> 37) & 0x3ff);
            }
        }

        public byte Cpsm
        {
            get
            {
                return (byte)((Packed >> 51) & 0xf);
            }
        }

        public byte Csm
        {
            get
            {
                return (byte)((Packed >> 55) & 0x1);
            }
        }

        public byte Csa
        {
            get
            {
                return (byte)((Packed >> 56) & 0x1f);
            }
        }

        public byte Cld
        {
            get
            {
                return (byte)((Packed >> 61) & 0x7);
            }
        }
    }
}
