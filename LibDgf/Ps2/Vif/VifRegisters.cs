using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public class VifRegisters
    {
        public uint[] R { get; } = new uint[4];
        public uint[] C { get; } = new uint[4];
        public uint Cycle { get; set; }
        public uint Mask { get; set; }
        public uint Mode { get; set; }
        public uint ITop { get; set; }
        public uint ITopS { get; set; }
        public uint Base { get; set; }
        public uint Ofst { get; set; }
        public uint Top { get; set; }
        public uint TopS { get; set; }
        public uint Mark { get; set; }
        public uint Num { get; set; }
        public VifCode Code { get; set; }

        // Just this flag because the other ones are not that interesting
        public bool Stat_Dbf { get; set; }

        public byte CycleCl
        {
            get
            {
                return (byte)Cycle;
            }
            set
            {
                Cycle = (Cycle & 0xffffff00) | value;
            }
        }

        public byte CycleWl
        {
            get
            {
                return (byte)(Cycle >> 8);
            }
            set
            {
                Cycle = (Cycle & 0xffff00ff) | ((uint)value << 8);
            }
        }

        public VifMode ModeMod
        {
            get
            {
                return (VifMode)(Mode & 3);
            }
            set
            {
                Mode = (uint)value & 3;
            }
        }

        public void DoubleBufferSwap()
        {
            ITop = ITopS;
            Top = TopS;
            TopS = Base + (Stat_Dbf ? Ofst : 0);
            Stat_Dbf = !Stat_Dbf;
        }
    }
}
