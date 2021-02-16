using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDgf.Dat
{
    public class DatEntry
    {
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public void Read(BinaryReader br)
        {
            Offset = br.ReadUInt32();
            Length = br.ReadUInt32();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Offset);
            bw.Write(Length);
        }
    }
}
