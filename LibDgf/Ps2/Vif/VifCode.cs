using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public struct VifCode
    {
        public uint Value;

        public bool Interrupt
        {
            get
            {
                return (Cmd & VifCodeCmd.Interrupt) != 0;
            }
        }

        public VifCodeCmd Cmd
        {
            get
            {
                return (VifCodeCmd)(byte)(Value >> 24);
            }
        }

        public VifCodeCmd CmdWithoutInterrupt
        {
            get
            {
                return Cmd & ~VifCodeCmd.Interrupt;
            }
        }

        public byte Num
        {
            get
            {
                return (byte)(Value >> 16);
            }
        }

        public ushort Immediate
        {
            get
            {
                return (ushort)Value;
            }
        }

        public bool IsUnpack => (Cmd & VifCodeCmd.Unpack) == VifCodeCmd.Unpack;
    }
}
